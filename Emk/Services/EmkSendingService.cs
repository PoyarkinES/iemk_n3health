using Emk.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emk.Services
{
	public class EmkSendingService
	{
		private List<EmkSettings> _settings => Factory.LoadSettings();

		public void Run()
		{
			Log.Info("Начинаю отправку данных по пациентам...");
			var p = GetPatientsAndTreatDates();
			if(p.Count == 0) {
				Log.Info("Записей лечения не найдено.");
				return;
			}
			Send(p);
		}


		private void Send(List<PatientTreat> p)
		{
            
			var errId = 0;
			var errDesc = string.Empty;
			foreach (var i in p) {
				Log.Info("-----------------");
                //if(i.PatientId != 335815)
                //    continue;
                try
                {
                    var set = _settings.SingleOrDefault(x => x.PracticeId == i.PracticeId);
                    if (set == default)
                    {
                        Log.Warning($"Не найдены настройки практики для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId})");
                        continue;
                    }

                    if (!set.Enabled)
                    {
                        Log.Info($"Для практики {set.PracticeId} отключена отправка данных. СМО для пациента с ИД {i.PatientId} (TreatDate:{i.TreatDate}, Practice: {i.PracticeId}) пропущен.");
                    }

                    var pix = new PixService(set);
                    var emk = new EmkService(set);
                    pix.AddPatient(i.PatientId);
                    pix.UpdatePatient(i.PatientId);
                    emk.AddCase(i.PatientId, i.TreatDate, ref errId, ref errDesc);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
			}
			Factory.CloseDbConnection();
		}

		//private void SendEmk(List<PatientTreat> p)
		//{
		//	var emk = new EmkService(_settings);
		//	var errId = 0;
		//	var errDesc = string.Empty;
		//	foreach (var i in p) emk.AddCase(i.PatientId, i.TreatDate, ref errId, ref errDesc);
		//}


		private List<PatientTreat> GetPatientsAndTreatDates()
		{
			var startDate = DateTime.MinValue;
			var endDate = DateTime.MinValue;
			if (_settings.First().SendingType == SendingType.DaysBeforeNow) {
				startDate = DateTime.Now.AddDays(-_settings.First().DateInterval);
				Log.Info($"Получаю пациентов и лечение с {startDate.ToShortDateString()}");
			}
			else {
				startDate = _settings.First().IntervalFrom.Date;
				endDate = _settings.First().IntervalTo.Date;
				Log.Info($"Получаю пациентов и лечение с {startDate.ToShortDateString()} по {endDate.ToShortDateString()}");
			}
			var pats = Factory.GetTreatRepository.GetPatientsTreats(startDate, endDate);
			Log.Info($"Получено {pats.Count}");
			return pats;
		}
	}
}
