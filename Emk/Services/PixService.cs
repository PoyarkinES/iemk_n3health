using Emk.Models;
using Emk.PixSvc;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Emk.Services
{
	public class PixService : IPixSendingClient
	{
		private readonly string _url;
		private readonly string _guid;
		private readonly string _idLpu;
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
            _url = s.PixUrl;
			_guid = s.Guid.ToString();
			_idLpu = s.IdLPU.ToString();
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
                var patient = await SetPatient(pa);
			    Log.Info(
			        $"PIX Добавляю пациента {patient.FamilyName} {patient.GivenName} {patient.MiddleName}.");

			    var client = _clientFactory.Create(_url);
			    try
			    {
			        await client.AddPatientAsync(_guid, _idLpu, patient);
			    }
			    finally
			    {
			        client.CloseSafely();
			    }

			    Log.Info($"PIX Пациент добавлен.");
			    return true;
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
                var patient = await SetPatient(pa);
			    var client = _clientFactory.Create(_url);
			    try
			    {
			        await client.UpdatePatientAsync(_guid, _idLpu, patient);
			    }
			    finally
			    {
			        client.CloseSafely();
			    }

			    Log.Info($"PIX Пациент обновлен.");
			    return true;
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

		private async Task<PatientDto> SetPatient(PatientAccount pa)
		{
            var patientTask = _dependencies.GetPatient(pa.PatientId);
            var snilsTask = _dependencies.GetSnils(pa.PatientId);
            var policyTask = _dependencies.GetPolicy(pa.AccountId);
            await Task.WhenAll(patientTask, snilsTask, policyTask);

			var patient = await patientTask;
            var patientDto = new PatientDto
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
            var snils = await snilsTask;
            if (snils == null)
            {
                throw new Exception("Для пациента не указан СНИЛС");
            }
            documents.Add(snils);
            var policy = await policyTask;
            if (policy != null)
                documents.Add(policy);

            patientDto.Documents = documents.ToArray();
            return patientDto;
        }

		public async Task<Patient> GetPatientAsync(int patientId)
		{
            IPixWcfClient client = null;
			try {
                client = _clientFactory.Create(_url);

				PatientDto patient = new PatientDto
				{
					IdPatientMIS = patientId.ToString()
				};

				SourceType idSource1 = SourceType.Reg;

                
                PatientDto[] patientResult = await client.GetPatientAsync(_guid, _idLpu, patient, idSource1);

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
