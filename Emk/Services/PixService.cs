using Emk.Models;
using Emk.PixSvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Emk.Services
{
	public class PixService
	{
		private string Url;
		private string guid;
		private string idLPU;
		private PatientDto _patient1;
        private List<EmkSettings> _settings;

        public PixService(EmkSettings s)
		{

            Url = s.PixUrl;
			guid = s.Guid.ToString();
			idLPU = s.IdLPU.ToString();
			var conn = Factory.GetDbConnection();
			if (conn.State != ConnectionState.Open)
				conn.Open();
		}

        public bool AddOrUpdatePatient(int patientId)
        {
            return GetPatient(patientId) == null ? AddPatient(patientId) : UpdatePatient(patientId);
        }


		public bool AddPatient(int patientId)
		{

			try {
				var binding = new BasicHttpBinding();
				var endpointAddress = new EndpointAddress(new Uri(Url), Array.Empty<AddressHeader>());
                var client = new PixServiceClient(binding, endpointAddress);

                var setResult = SetPatient(patientId);
                if (string.IsNullOrEmpty(setResult))
                {
                    Log.Info(
                        $"PIX Добавляю пациента {_patient1.FamilyName} {_patient1.GivenName} {_patient1.MiddleName}.");

                    client.AddPatient(guid, idLPU, _patient1);
                    client.Close();
                    Log.Info($"PIX Пациент добавлен.");
                    return true;
                }

                Log.Info($"PIX Пациент не добавлен. {setResult}");
                return false;
            }
			catch (FaultException<RequestFault[]> ex) {
				foreach (var er in ex.Detail) {
					Log.Error(er.ErrorCode + ": " + er.PropertyName + " " + er.Message);
				}
			}
			catch (FaultException<RequestFault> ex) {
				Log.Error(ex.Detail.ErrorCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message);
			}
			catch (Exception ex) {
				Log.Error($"PIX Ошибка при добавлении пациента:{patientId} " + ex.Message);
            }

            return false;
        }

		public bool UpdatePatient(int patientId)
		{
			try {
				BasicHttpBinding binding = new BasicHttpBinding();
				EndpointAddress endpointAddress = new EndpointAddress(new Uri(Url));
				PixServiceClient client = new PixServiceClient(binding, endpointAddress);

                var setResult = SetPatient(patientId);
                if (string.IsNullOrEmpty(setResult))
                {
                    client.UpdatePatient(guid, idLPU, _patient1);
                    client.Close();
                    Log.Info($"PIX Пациент обновлен.");
                    return true;
                }

                Log.Info($"PIX Информация о пациенте не обновлена. {setResult}");
                return false;
            }
			catch (FaultException<RequestFault[]> ex) {
				foreach (var err1 in ex.Detail) {
					Log.Error(err1.ErrorCode + ": " + err1.PropertyName + " " + err1.Message);
				}
			}
			catch (FaultException<RequestFault> ex) {
				Log.Error(ex.Detail.ErrorCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message);
			}
			catch (Exception ex) {
				Log.Error("PIX Ошибка при обновлении пациента:" + ex.Message);
			}

            return false;
        }

		private string SetPatient(int patientId)
		{
			var patient = Factory.GetPatientRepository.GetPatient(patientId);
            _patient1 = new PatientDto
            {
                FamilyName = patient.LastName,
                GivenName = patient.FirstName,
                MiddleName = patient.MiddleName,
                IdPatientMIS = patient.CartNum,
                BirthDate = patient.DateOfBirth,
                Sex = (byte) patient.SexInt,
            };

            if (patient.SexInt == 0)
            {
                throw new Exception("Для пациента не указан ПОЛ");
            }

            if (string.IsNullOrEmpty(patient.Snils?.Trim()))
            {
                throw new Exception("Для пациента не указан СНИЛС");
            }

            _patient1.Documents = new DocumentDto[1];
            _patient1.Documents[0] = new DocumentDto()
            {
                DocN = patient.Snils.Replace(" ", "").Replace("-", ""),
                DocumentName = "СНИЛС",
                IdDocumentType = 223,
                ProviderName = "ПФР"
            };

            return null;
        }

		//public int UpdatePatient(string LastName, string FirstName, string BDate, string CardNum, int Sex, ref int ErrNum, ref string ErrDescription)
		//{
		//	ErrNum = 0;
		//	ErrDescription = "";

		//	try {
		//		BasicHttpBinding binding = new BasicHttpBinding();
		//		EndpointAddress endpointAddress = new EndpointAddress(new Uri(Url));
		//		PixServiceClient client = new PixServiceClient(binding, endpointAddress);

		//		var patient1 = new PatientDto
		//		{
		//			FamilyName = LastName,
		//			GivenName = FirstName,
		//			IdPatientMIS = CardNum,
		//			BirthDate = DateTime.ParseExact(BDate, "dd-MM-yyyy", null),
		//			Sex = (byte)Sex
		//		};

		//		client.UpdatePatient(guid, idLPU, patient1);
		//		client.Close();
		//		return 0;
		//	}
		//	catch (Exception ex) {
		//		ErrNum = -1;
		//		ErrDescription = ex.Message;
		//		return -1;
		//	}
		//}

		public Patient GetPatient(int patientId)
		{
			try {
				BasicHttpBinding binding = new BasicHttpBinding();
				EndpointAddress endpointAddress = new EndpointAddress(new Uri(Url));
				PixServiceClient client = new PixServiceClient(binding, endpointAddress);

				PatientDto patient = new PatientDto
				{
					IdPatientMIS = patientId.ToString()
				};

				SourceType idSource1 = SourceType.Reg;

                
                PatientDto[] patientResult = client.GetPatient(guid, idLPU, patient, idSource1);
                 
                
				if (patientResult.Length == 1) {
                    Patient p = new Patient
                    {
                        FirstName = patientResult[0].GivenName,
                        LastName = patientResult[0].FamilyName,
                        CartNum = patientResult[0].IdPatientMIS,
                        DateOfBirth = patientResult[0].BirthDate,
                        Sex = patientResult[0].Sex.ToString(),
                        GlobalId = patientResult[0].IdGlobal
                    };
                    client.Close();
                    return p;
                }

                throw new Exception($"Возвращено более 1 пациента с ИД {patientId}");
            }
            catch (FaultException<RequestFault[]> ex) {
                foreach (var err1 in ex.Detail) {
                    Log.Error(err1.ErrorCode + ": " + err1.PropertyName + " " + err1.Message);
                }
            }
            catch (FaultException<RequestFault> ex) {
                Log.Error(ex.Detail.ErrorCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message);
            }
            catch (Exception ex) {
				Log.Error("PIX Ошибка при получении пациента: " + ex.Message);   
            }
            return null;
        }
	}
}
