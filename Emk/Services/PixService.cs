using Emk.Models;
using Emk.PixSvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace Emk.Services
{
	public class PixService : IPixSendingClient
	{
		private string Url;
		private string guid;
		private string idLPU;
		private PatientDto _patient1;
        private readonly IPixServiceDependencies _dependencies;
        private readonly IPixWcfClientFactory _clientFactory;

        public PixService(EmkSettings s)
            : this(s, new FactoryPixServiceDependencies(), new FactoryPixWcfClientFactory())
        {
        }

        public PixService(EmkSettings s, IPixServiceDependencies dependencies)
            : this(s, dependencies, new FactoryPixWcfClientFactory())
        {
        }

        public PixService(
            EmkSettings s,
            IPixServiceDependencies dependencies,
            IPixWcfClientFactory clientFactory)
		{
            _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            Url = s.PixUrl;
			guid = s.Guid.ToString();
			idLPU = s.IdLPU.ToString();
		}

        public async Task<bool> AddOrUpdatePatient(PatientAccount pa)
        {
            return await GetPatientAsync(pa.PatientId) == null
                ? await AddPatient(pa)
                : await UpdatePatient(pa);
        }


		public async Task<bool> AddPatient(PatientAccount pa)
		{

			try {
                var setResult = SetPatient(pa);
                if (string.IsNullOrEmpty(await setResult))
                {
                    Log.Info(
                        $"PIX Добавляю пациента {_patient1.FamilyName} {_patient1.GivenName} {_patient1.MiddleName}.");

                    var client = _clientFactory.Create(Url);
                    try
                    {
                        await client.AddPatientAsync(guid, idLPU, _patient1);
                    }
                    finally
                    {
                        client.CloseSafely();
                    }

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
				Log.Error($"PIX Ошибка при добавлении пациента:{pa.PatientId} " + ex.Message);
            }

            return false;
        }

		public async Task<bool> UpdatePatient(PatientAccount pa)
		{
			try {
                var setResult = SetPatient(pa);
                if (string.IsNullOrEmpty(await setResult))
                {
                    var client = _clientFactory.Create(Url);
                    try
                    {
                        await client.UpdatePatientAsync(guid, idLPU, _patient1);
                    }
                    finally
                    {
                        client.CloseSafely();
                    }

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

		private async Task<string> SetPatient(PatientAccount pa)
		{
			var patient = await _dependencies.GetPatient(pa.PatientId);
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

            var documents = new List<DocumentDto>();
            var snils = await _dependencies.GetSnils(pa.PatientId);
            if (snils == null)
            {
                throw new Exception("Для пациента не указан СНИЛС");
            }
            documents.Add(snils);
            var policy = await _dependencies.GetPolicy(pa.AccountId);
            if (policy != null)
                documents.Add(policy);

            _patient1.Documents = documents.ToArray();
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

		public async Task<Patient> GetPatientAsync(int patientId)
		{
            IPixWcfClient client = null;
			try {
                client = _clientFactory.Create(Url);

				PatientDto patient = new PatientDto
				{
					IdPatientMIS = patientId.ToString()
				};

				SourceType idSource1 = SourceType.Reg;

                
                PatientDto[] patientResult = await client.GetPatientAsync(guid, idLPU, patient, idSource1);

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
            finally
            {
                if (client != null)
                    client.CloseSafely();
            }
            return null;
        }
	}
}
