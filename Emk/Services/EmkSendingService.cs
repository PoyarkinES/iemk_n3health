using Emk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Emk.Repository;

namespace Emk.Services
{
	public class EmkSendingService
    {
        private List<EmkSettings> _settings;

        public EmkSendingService(bool reloadSettings = false)
        {
            _settings = Factory.LoadSettings(reloadSettings);
        }

		public void Run(int? accountId = null)
		{
            if(!IsLicenseValid()) return;

			Log.Info("Начинаю отправку данных по пациентам...");
            var p = accountId == null
                ? GetPatientsAndTreatDates()?.ToList()
                : GetPatientsAndTreatDates(accountId.Value)?.ToList();
            if (p == null || p.Count == 0)
            {
                Log.Info("Записей лечения не найдено.");
                return;
            }

            Send(p);
        }

        public void Update(int accountId)
        {
            if (!IsLicenseValid()) return;

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

            //Проверка подписи электронного документа для случая лечения
            // если подписи нет, случай пропускаем и не добавляем в выгрузку "continue"
            if (Factory.GetTreatRepository.GetCheckDocumentEsign(accountId).Any(a => a != String.Empty))
            {
                foreach (var item in Factory.GetTreatRepository.GetCheckDocumentEsign(accountId).Where(w => w != String.Empty))
                {
                    Log.Info(item);
                }

                return;
            }

            //Проверка наличия или отсутствия документов для случаев лечения
            // если в EsignFiles нет записей по номеру счета i.AccountId, то эти случаи отправляем без проверки файлов
            // если в EsignFiles записи по номеру счета i.AccountId существуют, то проверяем наличие доступа к файлам по указанному пути из EsignFiles
            string dir;
            var checkdocaccess = Factory.GetTreatRepository.GetCheckDocumentAccess(accountId, out dir).ToList();
            if (checkdocaccess.Any(a=> a != String.Empty))
            {
                foreach (var item in checkdocaccess.Where(w => w != String.Empty))
                {
                    Log.Info(item);
                }

                return;
            }

            new PixService(set).UpdatePatient(smo.PatientId);
            var result = new EmkService(set).UpdateCase(smo, dir);
            if (result == 0) Factory.GetEmkRepository.UpdateEsignFiles(smo);
        }

        private PatientAccount FindPatientAccount(int accountId) =>
            Factory.GetTreatRepository.GetPatientAccountById(accountId);

        private void Send(List<PatientAccount> list)
        {
            var group = list.GroupBy(x => new { x.PatientId, x.AccountId, x.ProviderId, x.DiagnoseCode });
            foreach (var smo in group)
            {
                Log.Info("-----------------");
                var i = smo.First();
                var set = _settings.FirstOrDefault(x => x.PracticeId == i.PracticeId);
                if (set != null)
                    set.AutoUpdate = new SettingsService().LoadSettings().AutoUpdate;

                // Проверка насроек практики для пациента
                if (set == default || set.IdLPU == Guid.Empty || set.Guid == Guid.Empty)
                {
                    Log.Warning(
                        $"Не найдены настройки практики для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId})");
                    continue;
                }

                //Проверка возможности отправки данных для пациента
                if (!set.Enabled)
                {
                    Log.Info(
                        $"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId}) пропущен.");
                    continue;
                }

                //Проверка соответствия случая лечения и лечения пациента
                // если практика для случая лечения и лечения пациента не соответствуют, случай пропускаем и не добавляем в выгрузку "continue"
                if (Factory.GetTreatRepository.GetCheckPracticId(i.AccountId).Any(a=>a != String.Empty))
                {
                    foreach (var item in Factory.GetTreatRepository.GetCheckPracticId(i.AccountId).Where(w=> w != String.Empty))
                    {
                        Log.Info(item);
                    }

                    continue;
                }

                //Проверка подписи электронного документа для случая лечения
                // если подписи нет, случай пропускаем и не добавляем в выгрузку "continue"
                if (Factory.GetTreatRepository.GetCheckDocumentEsign(i.AccountId).Any(a=>a != String.Empty))
                {
                    foreach (var item in Factory.GetTreatRepository.GetCheckDocumentEsign(i.AccountId).Where(w => w != String.Empty))
                    {
                        Log.Info(item);
                    }

                    continue;
                }

                //Проверка наличия или отсутствия документов для случаев лечения
                // если в EsignFiles нет записей по номеру счета i.AccountId, то эти случаи отправляем без проверки файлов
                // если в EsignFiles записи по номеру счета i.AccountId существуют, то проверяем наличие доступа к файлам по указанному пути из EsignFiles
                string dir;
                var checkdocaccess = Factory.GetTreatRepository.GetCheckDocumentAccess(i.AccountId, out dir).ToList();
                if (checkdocaccess.Any(a => a != String.Empty))
                {
                    foreach (var item in checkdocaccess.Where(w => w != String.Empty))
                    {
                        Log.Info(item);
                    }

                    continue;
                }

                if (Factory.GetTreatRepository.CheckPatientConsentTransPersData(i.PatientId) > 0)
                {
                    Log.Info(
                        $"Пациент с идентификатором:{i.PatientId} и № карты {i.PatientsCartNum} не дал согласие на передачу персоналных данных. № счета:{i.AccountId} исключен из пакета данных на передачу в EmkService.");
                    continue;
                }

                var pix = new PixService(set);
                var emk = new EmkService(set);

                var result = pix.AddPatient(i.PatientId) ? 0 : -1;
                if (result == 0)
                    result = emk.AddCase(i, false, dir);
                else
                    Log.Warning($"Случай:{i.AccountId} будет пропущен.");
                if (result == 0) Factory.GetEmkRepository.UpdateEsignFiles(i);

            }
        }

        private bool IsLicenseValid()
        {
            if (Factory.GetLicenseRepository.IsLicenseValid()) {
                Log.Warning("Отсутствует лицензия на использование обратитесь в техническую поддержку.");
                return false;
            }

            return true;
        }

        //private void Send(List<PatientTreat> p)
        //{

        //	var errId = 0;
        //	var errDesc = string.Empty;
        //	foreach (var i in p) {
        //		Log.Info("-----------------");
        //              //if(i.PatientId != 335815)
        //              //    continue;
        //              try
        //              {
        //                  var set = _settings.SingleOrDefault(x => x.PracticeId == i.PracticeId);
        //                  if (set == default)
        //                  {
        //                      Log.Warning($"Не найдены настройки практики для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId})");
        //                      continue;
        //                  }

        //                  if (!set.Enabled)
        //                  {
        //                      Log.Info($"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId}) пропущен.");
        //                      continue;
        //                  }

        //                  var pix = new PixService(set);
        //                  var emk = new EmkService(set);
        //                  pix.AddPatient(i.PatientId);
        //                  pix.UpdatePatient(i.PatientId);
        //                  emk.AddCase(i);
        //              }
        //              catch (Exception e)
        //              {
        //                  Log.Error(e.Message);
        //              }
        //	}
        //	Factory.CloseDbConnection();
        //}

        //private void SendEmk(List<PatientTreat> p)
        //{
        //	var emk = new EmkService(_settings);
        //	var errId = 0;
        //	var errDesc = string.Empty;
        //	foreach (var i in p) emk.AddCase(i.PatientId, i.TreatDate, ref errId, ref errDesc);
        //}

        private IEnumerable<PatientAccount> GetPatientsAndTreatDates()
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
            var pats = Factory.GetTreatRepository.GetPatientAccounts(startDate, endDate).ToList();
            //var pats = Factory.GetTreatRepository.GetPatientsTreats(startDate, endDate);
			Log.Info($"Получено {pats.Count}");
			return AddPostfixForDiagnose(pats);
        }

        private IEnumerable<PatientAccount> GetPatientsAndTreatDates(int accountId)
        {
#if DEBUG
            //foreach (var setting in _settings) {
            //    setting.DateInterval = 0;
            //}
#endif
            Log.Info($"Получаю пациентов и лечение по счету № {accountId}");
            var pats = Factory.GetTreatRepository.GetPatientAccountById(accountId);
            var result = new List<PatientAccount>();
            result.Add(pats);
            //var pats = Factory.GetTreatRepository.GetPatientsTreats(startDate, endDate);
            Log.Info($"Получено {pats}");
            return AddPostfixForDiagnose(result);
        }

        private IEnumerable<PatientAccount> AddPostfixForDiagnose(List<PatientAccount> list)
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
    }
}
