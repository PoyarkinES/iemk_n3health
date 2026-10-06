using Emk.EmkSvc;
using Emk.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Emk.Services
{
	public class EmkService : IEmkCaseSendingClient
	{
        private readonly IEmkServiceDependencies _dependencies;
        private readonly IEmkWcfClientFactory _clientFactory;

		string Url;
		string guid;
		string IdLPU;
		CaseAmb case1;
		PersonWithIdentity patient1;
		string MName = "";
		string path = "";
		string patientsBaseDir;
        private int autoUpd = 0;

		public EmkService(EmkSettings s)
            : this(s, new FactoryEmkServiceDependencies(), new FactoryEmkWcfClientFactory())
        {
        }

        public EmkService(EmkSettings s, IEmkServiceDependencies dependencies)
            : this(s, dependencies, new FactoryEmkWcfClientFactory())
        {
        }

        public EmkService(
            EmkSettings s,
            IEmkServiceDependencies dependencies,
            IEmkWcfClientFactory clientFactory)
		{
            _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
			Url = s.EmkUrl;
			guid = s.Guid.ToString();
			IdLPU = s.IdLPU.ToString();
			patientsBaseDir = s.PatientDirectory;
            autoUpd = s.AutoUpdate;
		}

        public async Task<int> UpdateCase(PatientAccount treat, string dir = null) => await AddCase(treat, true);

        public async Task<int> AddCase(PatientAccount treat, bool updateOnly = false)
        {
            Log.Info($"EMK Добавляю случай медицинского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy}");
            try 
            {
                if (!checkTreat(treat)) return -1;

                if (treat.EsfDate != null && treat.EsfDate != DateTime.MinValue &&  treat.TreatDate != treat.EsfDate)
                {
                    Log.Warning($"AccountId: {treat.AccountId} дата случая: {treat.TreatDate} отличается от даты подписания документа: {treat.EsfDate}.");
                }

				var doctor = await getDoctor(treat);
                var patient = await _dependencies.GetPatient(treat.PatientId);
				var medDocuments = await getMedDocuments(treat, doctor, patient);

				if (medDocuments == null || !medDocuments.Any())
                {
                    Log.Info($"Документы не найдены для {treat.AccountId}.");
                    throw new Exception($"Документы не найдены для {treat.AccountId}.");
                }

                var case1 = await GetCaseAmb(treat, doctor, patient, medDocuments);              
                Log.Info($"Добавлено документов: {medDocuments.Count}, добавлено количество процедур СМО: {case1.Steps[0].MedRecords.Length}");

                if (updateOnly)
                {
                    var client = _clientFactory.Create(Url);
                    try
                    {
                        await client.UpdateCaseAsync(guid, case1);
                    }
                    finally
                    {
                        client.CloseSafely();
                    }

                    Log.Info($"EMK Cлучай медицинского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} обновлен.");
                    await SaveCase(updateOnly, treat, null, null);
                    return 0;
                }

                var addClient = _clientFactory.Create(Url);
                try
                {
                    await addClient.AddCaseAsync(guid, case1);
                }
                finally
                {
                    addClient.CloseSafely();
                }

                Log.Info($"EMK Cлучай медицинского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} добавлен.");
                await SaveCase(updateOnly, treat, null, null);

				return 0;
            }
            catch (FaultException<RequestFault[]> ex) {
                getFullError(ex.Detail);
                await SaveCase(updateOnly, treat, null, getError(ex.Detail));
				return -1;
            }
            catch (FaultException<RequestFault> ex) {
                var errDescription = ex.Detail.ErrorCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message + "\r\n";
                getFullError(ex.Detail.Errors);
				if(ex.Detail.ErrorCode == 31 && autoUpd == 1)
                    await UpdateCase(treat, path);
                await SaveCase(updateOnly, treat, null, getError(ex.Detail.Errors));
				return -1;
            }
            catch (FaultException<RequestWarning> ex) {
                Log.Warning(ex.Detail.WarningCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message + "\r\n");
                await SaveCase(updateOnly, treat, getWarning(ex.Detail.Warnings), null);
                return -1;
            }
            catch (FaultException<RequestWarning[]> ex) {

                getWarning(ex.Detail);
                await SaveCase(updateOnly, treat, getWarning(ex.Detail), null);
				return -1;
            }
            catch (Exception ex) {
                Log.Warning($"Случай медицинского обслуживания для пациента {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy} не отправлен.");
                Log.Error(ex.ToString());
                await SaveCase(updateOnly, treat, null, ex.Message);
                return -1;
            }
        }

        private Task SaveCase(bool updateOnly, PatientAccount treat, string responseText, string errorText) =>
            _dependencies.SaveCase(-1, DateTime.Now, updateOnly ? "upd" : "add", treat.PatientId,
                treat.AccountId, responseText, 'S', errorText);

        private bool IsValid(object obj)
        {
            if (!(obj is PatientAccount treat))
                return false;

            if(treat.Code == 0){
                Log.Error("Не задана специализация");
                return false;
            }

            if (treat.AccountId == 0)
            {
                Log.Error("Не задан счет");
                return false;
            }

            return true;

        }

		private string getErrorString(RequestFault[] errors, RequestFault error = null)
		{
			var result = new StringBuilder();
			foreach (var item in errors)
            {
                result.Append(Environment.NewLine);
                result.Append($"\"{item.PropertyName}\": {{ {getErrorString(item.Errors, item)} }},");
			}

            if (error != null && errors.Length == 0)
                result.Append($"\"ErrorCode\": {error.ErrorCode}, \"PropertyName\":\"{error.PropertyName}\", \"Message\":\"{error.Message}\" ");
            
            return result.ToString().Remove(result.ToString().Length -1, 1);
		}

        private void getFullError(RequestFault[] rError)
        {
            foreach (var e in rError)
            {
                Log.Error($"{e.ErrorCode} : {e.PropertyName} {e.Message} ");
                if (e.Errors.Length > 0)
                {
                    getFullError(e.Errors);
                }
            }
        }

        private string getError(RequestFault[] rError)
        {
			foreach (var e in rError)
            {
                if (e.Errors.Length == 0)
                {
                    return $"{e.ErrorCode} : {e.PropertyName} {e.Message} ";
                }
                getError(e.Errors);
            }

            return null;
        }
        
        private string getWarning(RequestWarning[] rWarning)
        {
            foreach (RequestWarning e in rWarning)
            {
                if (e.Warnings.Length == 0)
                {
                    return $"{e.WarningCode} : {e.PropertyName} {e.Message} ";
                }
                getWarning(e.Warnings);
            }

            return null;
        }

		private async Task<MedicalStaff> getDoctor(PatientAccount treat)
		{
            var doc = await _dependencies.GetDoctorByMemberId(treat.ProviderId);
            doc.Speciality = treat.Code;
            doc.AccountId = treat.AccountId;

            Log.Info($"Доктор: {doc.Surname} {doc.Name} {doc.MiddleName} Диагноз: {treat.DiagnoseCode} {treat.DiagnoseName} Процедуры: {treat.ListProcedures}");

            return doc.ToMedicalStaff(); 
		}

		private async Task<CaseAmb> GetCaseAmb(PatientAccount treat, MedicalStaff doctor, Patient patient, IEnumerable<MedRecord> medDocuments)
		{
            var def = await _dependencies.LoadDefaultsAsync();
            var diag = new DiagnosisEmk()
            {
                DiagnosisCode = treat.DiagnoseCode,
                DiagnosisName = treat.DiagnoseName
            };

            case1 = new CaseAmb();
            case1.OpenDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                DateTimeKind.Local);
            case1.CloseDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 1, 1,
                DateTimeKind.Local);
            case1.HistoryNumber = patient.CartNum;

            case1.IdCaseMis = $"{patient.CartNum}-{treat.AccountId}{treat.SmoPostfix}";

            case1.IdCaseAidType = 3;
            case1.IdCaseType = 2;
            case1.IdPaymentType = (byte)(await _dependencies.GetPayType(treat.AccountId));
            case1.IdCasePurpose = Convert.ToByte(def.VisitPurpose);

            case1.Confidentiality = Convert.ToByte(def.ConfidentialityLevel);
            case1.DoctorConfidentiality = Convert.ToByte(def.ConfidentialityDoctorLevel);
            case1.CuratorConfidentiality = Convert.ToByte(def.ConfidentialityRepresentativeLevel);
            case1.IdLpu = IdLPU;
            case1.IdCaseResult = 1;
            case1.Comment = diag.DiagnosisName;

            case1.IdPatientMis = patient.CartNum;
            case1.DoctorInCharge = doctor;
            case1.Authenticator = new Participant { Doctor = doctor, IdRole = 3 };
            case1.Author = new Participant { Doctor = doctor, IdRole = 3 };
            case1.LegalAuthenticator = new Participant { Doctor = doctor, IdRole = 3 };
            case1.CaseVisitType = 1;    // 1 - Первичный
                                        // 2 - Повторный
            Log.Info($"Создан СМО для пациентa с картой {patient.CartNum}, ИД случая: {case1.IdCaseMis}");
            case1.Steps =
            [
                     new StepAmb
                    {
                        DateStart = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                            DateTimeKind.Local),
                        DateEnd = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 1, 1,
                            DateTimeKind.Local),
                        IdStepMis = $"{treat.AccountId}-{patient.CartNum}" ,
                        Doctor = doctor,
                        IdVisitPlace = Convert.ToByte(def.VisitPlace),
                        IdVisitPurpose = Convert.ToByte(def.VisitPurpose),
                        IdPaymentType = (byte)await _dependencies.GetPayType(treat.AccountId)
            }
                ];

            case1.MedRecords = medDocuments.ToArray();
            case1.Steps[0].MedRecords = [.. (await getProcedures(treat))];

            // Новые требования, добавляем всегда 1. Удовлетворительное состояние пациента при поступлении.
            case1.AdmissionCondition = 1;

            // Новые требования, добавляем всегда 1. Удовлетворительное состояние пациента при поступлении.
            case1.IdAmbResult = 2;

            return case1;
		}

		private bool checkTreat(PatientAccount treat)
		{
            if (!IsValid(treat))
            {
                Log.Error($"ЕМК СМО для пациента ИД {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy} не прошел валидацию и будет пропущен.");
                return false;
            }

            if (treat.AccountId == 0)
            {
                Log.Warning($"Для СМО для пациента с ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} не найден счет. СМО пропущен.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(treat.DiagnoseCode) || string.IsNullOrWhiteSpace(treat.DiagnoseName))
            {
                Log.Warning($"Для СМО для пациента с ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} не задан диагноз. СМО пропущен.");
                return false;
            }

			return true;
        }

		private async Task<List<MedRecord>> getMedDocuments(PatientAccount treat, MedicalStaff doctor, Patient patient)
        {
            //var medDocuments = new List<MedRecord>
            //    {
            //        new ClinicMainDiagnosis
            //        {
            //            DiagnosisInfo = new DiagnosisInfo
            //            {
            //                IdDiseaseType = 1,
            //                DiagnosedDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
            //                    DateTimeKind.Local),
            //                IdDiagnosisType = 1,
            //                Comment = treat.DiagnoseName,
            //                DiagnosisStage = 3,
            //                MkbCode = treat.DiagnoseCode
            //            },
            //            Doctor = doctor
            //        }
            //    };

            var dir = path;
            if (!Directory.Exists(dir))
                dir = $"{patientsBaseDir.TrimEnd('\\')}\\{patient.LastName} {patient.FirstName} {patient.MiddleName} [{patient.CartNum}]\\Дневниковые записи";

            return await _dependencies.GetMedicalDocuments(treat);
        }

        private async Task<List<MedRecord>> getProcedures(PatientAccount treat)
        {
            var medRecords = new List<MedRecord>();

            foreach (var d in await _dependencies.GetProcedureDescriptions(treat.PatientId, treat.TreatDate, treat.AccountId))
            {
                if (string.IsNullOrEmpty(d.Description))
                    continue;
                medRecords.Add(new Service
                {
                    DateEnd = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                        DateTimeKind.Local),
                    DateStart = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 1, 1,
                        DateTimeKind.Local),
                    IdServiceType = d.Description,
                    ServiceName = d.FullDescription,
                    Performer = new Participant { IdRole = 3, Doctor = case1.DoctorInCharge }
                });
            }

            return medRecords;
        }
    }
}
