using Emk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emk.Services
{
	public class EmkSendingService
    {
        private List<EmkSettings> _settings;

        public EmkSendingService(bool reloadSettings = false)
        {
            _settings = Factory.LoadSettings(reloadSettings);
        }

		public async Task Run(int? accountId = null)
		{
            if(!IsLicenseValid()) return;

			Log.Info("Начинаю отправку данных по пациентам...");
            var p = accountId == null
                ? await GetPatientsAndTreatDates()
                : await GetPatientsAndTreatDates(accountId.Value);
            if (p == null || !p.Any())
            {
                Log.Info("Записей лечения не найдено.");
                return;
            }

            await Send(p);
        }

        public async Task Update(int accountId)
        {
            if (! IsLicenseValid()) return;

            Log.Info("Начинаю поиск СМО с номером счета " + accountId);
            var smo = FindPatientAccount(accountId);
            if (smo == null)
            {
                Log.Error("СМО не найден.");
                return;
            }

            var set = _settings.FirstOrDefault(x => x.PracticeId == smo.PracticeId);
            if (set == default) {
                Log.Warning($"Не найдены настройки практики для пациента с ИД {smo.PatientId} (TreatDate:{smo.TreatDate}, Practice: {smo.PracticeId})");
                return;
            }

            if (!set.Enabled) {
                Log.Info($"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {smo.PatientId} (TreatDate:{smo.TreatDate}, Practice: {smo.PracticeId}) пропущен.");
                return;
            }

            var checkEsign = checkDocumentEsignAsync(accountId);
            var checkAccess = checkDocumentAccessAsync(accountId);
            var check = await Task.WhenAll(checkEsign, checkAccess);

            if(check.Any(a=> !a)) return;

            await new PixService(set).UpdatePatient(smo);
            var result = new EmkService(set).UpdateCase(smo).ConfigureAwait(false).GetAwaiter().GetResult();
            if (result == 0) Factory.GetEmkRepository.UpdateEsignFiles(smo).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private PatientAccount FindPatientAccount(int accountId) =>
            Factory.GetTreatRepository.GetPatientAccountByIdAsync(accountId).GetAwaiter().GetResult();

        private async Task Send(List<PatientAccount> list)
        {
            var group = list.GroupBy(x => new { x.PatientId, x.AccountId, x.ProviderId, x.DiagnoseCode });
            foreach (var smo in group)
            {
                Log.Info("-----------------");
                var i = smo.First();
                var set = _settings.FirstOrDefault(x => x.PracticeId == i.PracticeId);

                var check = await checkSend(set, i);
                if (check.Any(a=> !a)) continue;

                var pix = new PixService(set);
                var emk = new EmkService(set);

                var result = await pix.AddPatient(i) ? 0 : -1;
                if (result == 0)
                    result = await emk.AddCase(i, false);
                else
                    Log.Warning($"Случай:{i.AccountId} будет пропущен.");
                if (result == 0) Factory.GetEmkRepository.UpdateEsignFiles(i);

            }
        }

        private bool IsLicenseValid()
        {
            if (Factory.GetLicenseRepository.IsLicenseValid().ConfigureAwait(false).GetAwaiter().GetResult()) {
                Log.Warning("Отсутствует лицензия на использование обратитесь в техническую поддержку.");
                return false;
            }

            return true;
        }

        private async Task<List<PatientAccount>> GetPatientsAndTreatDates()
		{
#if DEBUG
            //foreach (var setting in _settings) {
            //    setting.DateInterval = 0;
            //}
#endif
            DateTime startDate;
			DateTime endDate;
			if (_settings.First().SendingType == SendingType.DaysBeforeNow) {
				startDate = DateTime.Now.AddDays(-_settings.First().DateInterval);
                endDate = DateTime.Now.AddDays(-_settings.First().DateInterval);
			}
			else {
				startDate = _settings.First().IntervalFrom.Date;
				endDate = _settings.First().IntervalTo.Date;
			}

            Log.Info($"Получаю пациентов и лечение с {startDate.ToShortDateString()} по {endDate.ToShortDateString()}");
            var pats = await Factory.GetTreatRepository.GetPatientAccountsAsync(startDate, endDate);
            //var pats = Factory.GetTreatRepository.GetPatientsTreats(startDate, endDate);
			Log.Info($"Получено {pats.Count}");
			return AddPostfixForDiagnose(pats);
        }

        private async Task<List<PatientAccount>> GetPatientsAndTreatDates(int accountId)
        {
#if DEBUG
            //foreach (var setting in _settings) {
            //    setting.DateInterval = 0;
            //}
#endif
            Log.Info($"Получаю пациентов и лечение по счету № {accountId}");
            var pats = await Factory.GetTreatRepository.GetPatientAccountByIdAsync(accountId);
            var result = new List<PatientAccount>();
            result.Add(pats);
            //var pats = Factory.GetTreatRepository.GetPatientsTreats(startDate, endDate);
            Log.Info($"Получено {pats}");
            return AddPostfixForDiagnose(result);
        }

        private List<PatientAccount> AddPostfixForDiagnose(List<PatientAccount> list)
        {
            if (list.Any(a => a != null))
            {
                foreach (var accId in list.Select(s => s.AccountId).Distinct())
                {
                    var group = list.Where(w => w.AccountId == accId)
                        .GroupBy(x => new {x.PatientId, x.AccountId, x.ProviderId});
                    foreach (var item in group.Distinct())
                    {
                        if (item.Select(s => new {s.PatientId, s.AccountId, s.ProviderId}).Distinct().Count() < 2)
                            continue;
                        var ch = 'a';
                        foreach (var acc in item.GroupBy(x => x.DiagnoseCode))
                        {
                            acc.First().SmoPostfix = ch.ToString();
                            ch++;
                        }
                    }
                }

                return list;
            }
            else
            {
                //Log.Info($"Вы ввели не существующий счет.");
                return null;
            }
        }

        //Проверка подписи электронного документа для случая лечения
        // если подписи нет, случай пропускаем и не добавляем в выгрузку "continue"
        private async Task<bool> checkDocumentEsignAsync(int accountId)
        {
            var result = await Factory.GetTreatRepository.GetCheckDocumentEsignAsync(accountId).ConfigureAwait(false);
            if(result.Any())
            {
                foreach (var item in result.Where(w => w != String.Empty))
                {
                    Log.Info(item);
                }

                return false;
            }

            return true;
        }

        //Проверка наличия или отсутствия документов для случаев лечения
        // если в EsignFiles нет записей по номеру счета i.AccountId, то эти случаи отправляем без проверки файлов
        // если в EsignFiles записи по номеру счета i.AccountId существуют, то проверяем наличие доступа к файлам по указанному пути из EsignFiles
        private async Task<bool> checkDocumentAccessAsync(int accountId)
        {
            var checkdocaccess = await Factory.GetTreatRepository.CheckDocumentAccessAsync(accountId).ConfigureAwait(false);
            if (checkdocaccess.Any(a => a != String.Empty))
            {
                foreach (var item in checkdocaccess.Where(w => w != String.Empty))
                {
                    Log.Info(item);
                }

                return false;
            }

            return true;
        }

        private async Task<bool> checkPracticIdAsync(PatientAccount pa)
        {
            //Проверка соответствия случая лечения и лечения пациента
            // если практика для случая лечения и лечения пациента не соответствуют, случай пропускаем и не добавляем в выгрузку "continue"

            var checkPracticId = await Factory.GetTreatRepository.GetCheckPracticIdAsync(pa.AccountId).ConfigureAwait(false);
            if (checkPracticId.Any(a => a != String.Empty))
            {
                foreach (var item in checkPracticId.Where(w => w != String.Empty))
                {
                    Log.Info(item);
                }

                return false;
            }

            return true;
        }

        private async Task<bool> checkPatientConsentTransPersDataAsync(PatientAccount pa)
        {
            if (await Factory.GetTreatRepository.CheckPatientConsentTransPersDataAsync(pa.PatientId) > 0)
            {
                Log.Info(
                    $"Пациент с идентификатором:{pa.PatientId} и № карты {pa.PatientsCartNum} не дал согласие на передачу персоналных данных. № счета:{pa.AccountId} исключен из пакета данных на передачу в EmkService.");
                return false;
            }

            return true;
        }

        private async Task<bool[]> checkSend(EmkSettings set, PatientAccount pa)
        {
            var checksettings = Task.Run(() => {
                // Проверка насроек практики для пациента
                if (set == default || set.IdLPU == Guid.Empty || set.Guid == Guid.Empty)
                {
                    Log.Warning(
                        $"Не найдены настройки практики для пациента с ИД {pa.PatientId} (TreatDate:{pa.TreatDate}, Practice: {pa.PracticeId})");
                    return false;
                }

                //Проверка возможности отправки данных для пациента
                if (!set.Enabled)
                {
                    Log.Info(
                        $"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {pa.PatientId} (TreatDate:{pa.TreatDate}, Practice: {pa.PracticeId}) пропущен.");
                    return false;
                }

                return true;
            });
            var checkPracticId = checkPracticIdAsync(pa);
            var checkPatientConsentTransPersData = checkPatientConsentTransPersDataAsync(pa);
            var checkEsign = checkDocumentEsignAsync(pa.AccountId);
            var checkAccess = checkDocumentAccessAsync(pa.AccountId);
            var check = await Task.WhenAll(checksettings, checkPracticId, checkEsign, checkAccess, checkPatientConsentTransPersData);

            return check;
        }
    }
}
