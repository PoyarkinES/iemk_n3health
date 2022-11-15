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


		public void Run()
		{
            if(!IsLicenseValid()) return;

			Log.Info("Начинаю отправку данных по пациентам...");
			var p = GetPatientsAndTreatDates();
			if(p.Count == 0) {
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
            var set = _settings.SingleOrDefault(x => x.PracticeId == smo.PracticeId);
            if (set == default) {
                Log.Warning($"Не найдены настройки практики для пациента с ИД {smo.PatientId} (TreatDate:{smo.TreatDate}, Practice: {smo.PracticeId})");
                return;
            }
            if (!set.Enabled) {
                Log.Info($"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {smo.PatientId} (TreatDate:{smo.TreatDate}, Practice: {smo.PracticeId}) пропущен.");
                return;
            }


            new PixService(set).UpdatePatient(smo.PatientId);
            new EmkService(set).UpdateCase(smo);

        }



        private PatientAccount FindPatientAccount(int accountId) =>
            Factory.GetTreatRepository.GetPatientAccountById(accountId);


        private void Send(List<PatientAccount> list)
        {

            var group = list.GroupBy(x => new { x.PatientId, x.ProviderId, x.DiagnoseCode });
            foreach (var smo in group)
            {
                Log.Info("-----------------");
                var i = smo.First();
                var set = _settings.SingleOrDefault(x => x.PracticeId == i.PracticeId);
                if (set == default || set.IdLPU == Guid.Empty || set.Guid == Guid.Empty) {
                    Log.Warning($"Не найдены настройки практики для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId})");
                    continue;
                }
                if (!set.Enabled) {
                    Log.Info($"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId}) пропущен.");
                    continue;
                }

                if (Factory.GetTreatRepository.GetCheckPracticId(i.PatientId).Any())
                {
                    foreach (var item in Factory.GetTreatRepository.GetCheckPracticId(i.PatientId))
                    {
                        Log.Info(item);
                    }
                    continue;
                }

                var pix = new PixService(set);
                var emk = new EmkService(set);
                pix.AddPatient(i.PatientId);
               // pix.UpdatePatient(i.PatientId);
                emk.AddCase(i);
            }



            Factory.CloseDbConnection();
        }

        private bool IsLicenseValid()
        {
            if (!new LicenseRepository().IsLicenseValid()) {
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


        private List<PatientAccount> GetPatientsAndTreatDates()
		{
#if DEBUG
            //foreach (var setting in _settings) {
            //    setting.DateInterval = 0;
            //}
#endif
            var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;
			if (_settings.First().SendingType == SendingType.DaysBeforeNow) {
				//startDate = DateTime.Now.AddDays(-1);
				startDate = DateTime.Now.AddDays(-_settings.First().DateInterval);
				Log.Info($"Получаю пациентов и лечение с {startDate.ToShortDateString()}");
			}
			else {
				startDate = _settings.First().IntervalFrom.Date;
				endDate = _settings.First().IntervalTo.Date;
				Log.Info($"Получаю пациентов и лечение с {startDate.ToShortDateString()} по {endDate.ToShortDateString()}");
			}

            var pats = Factory.GetTreatRepository.GetPatientAccounts(startDate, endDate);
            //var pats = Factory.GetTreatRepository.GetPatientsTreats(startDate, endDate);
			Log.Info($"Получено {pats.Count}");
			return pats;
		}
    }
}
