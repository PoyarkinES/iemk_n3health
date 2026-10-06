using Emk.EmkSvc;
using Emk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Emk.Services
{
	public class EmkService : IEmkCaseSendingClient
	{
        private readonly IEmkServiceDependencies _dependencies;
        private readonly IEmkWcfClientFactory _clientFactory;

		private readonly string _url;
		private readonly string _guid;
		private readonly string _idLpu;
        private readonly int _autoUpdate;

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
			_url = s.EmkUrl;
			_guid = s.Guid.ToString();
			_idLpu = s.IdLPU.ToString();
            _autoUpdate = s.AutoUpdate;
		}

        public Task<int> UpdateCase(PatientAccount treat, string dir = null) => AddCase(treat, true);

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

                var doctorTask = getDoctor(treat);
                var patientTask = _dependencies.GetPatient(treat.PatientId);
                var medicalDocumentsTask = getMedDocuments(treat);
                await Task.WhenAll(doctorTask, patientTask, medicalDocumentsTask);

                var doctor = await doctorTask;
                var patient = await patientTask;
                var medDocuments = await medicalDocumentsTask;

				if (medDocuments == null || !medDocuments.Any())
                {
                    Log.Info($"Документы не найдены для {treat.AccountId}.");
                    throw new Exception($"Документы не найдены для {treat.AccountId}.");
                }

                var medicalCase = await GetCaseAmb(treat, doctor, patient, medDocuments);
                Log.Info($"Добавлено документов: {medDocuments.Count}, добавлено количество процедур СМО: {medicalCase.Steps[0].MedRecords.Length}");

                if (updateOnly)
                {
                    var client = _clientFactory.Create(_url);
                    try
                    {
                        await client.UpdateCaseAsync(_guid, medicalCase);
                    }
                    finally
                    {
                        client.CloseSafely();
                    }

                    Log.Info($"EMK Cлучай медицинского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} обновлен.");
                    await SaveCase(updateOnly, treat, null, null);
                    return 0;
                }

                var addClient = _clientFactory.Create(_url);
                try
                {
                    await addClient.AddCaseAsync(_guid, medicalCase);
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
				if(ex.Detail.ErrorCode == 31 && _autoUpdate == 1)
                    await UpdateCase(treat);
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

        private string getError(RequestFault[] errors)
        {
            foreach (var error in errors)
            {
                if (error.Errors.Length == 0)
                {
                    return $"{error.ErrorCode} : {error.PropertyName} {error.Message} ";
                }

                var nestedError = getError(error.Errors);
                if (nestedError != null)
                    return nestedError;
            }

            return null;
        }
        
        private string getWarning(RequestWarning[] warnings)
        {
            foreach (var warning in warnings)
            {
                if (warning.Warnings.Length == 0)
                {
                    return $"{warning.WarningCode} : {warning.PropertyName} {warning.Message} ";
                }

                var nestedWarning = getWarning(warning.Warnings);
                if (nestedWarning != null)
                    return nestedWarning;
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

            var medicalCase = new CaseAmb();
            medicalCase.OpenDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                DateTimeKind.Local);
            medicalCase.CloseDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 1, 1,
                DateTimeKind.Local);
            medicalCase.HistoryNumber = patient.CartNum;

            medicalCase.IdCaseMis = $"{patient.CartNum}-{treat.AccountId}{treat.SmoPostfix}";

            medicalCase.IdCaseAidType = 3;
            medicalCase.IdCaseType = 2;
            var paymentTypeTask = _dependencies.GetPayType(treat.AccountId);
            var procedureDescriptionsTask = _dependencies.GetProcedureDescriptions(
                treat.PatientId, treat.TreatDate, treat.AccountId);
            await Task.WhenAll(paymentTypeTask, procedureDescriptionsTask);
            var paymentType = await paymentTypeTask;
            var procedureDescriptions = await procedureDescriptionsTask;
            medicalCase.IdPaymentType = (byte)paymentType;
            medicalCase.IdCasePurpose = Convert.ToByte(def.VisitPurpose);

            medicalCase.Confidentiality = Convert.ToByte(def.ConfidentialityLevel);
            medicalCase.DoctorConfidentiality = Convert.ToByte(def.ConfidentialityDoctorLevel);
            medicalCase.CuratorConfidentiality = Convert.ToByte(def.ConfidentialityRepresentativeLevel);
            medicalCase.IdLpu = _idLpu;
            medicalCase.IdCaseResult = 1;
            medicalCase.Comment = diag.DiagnosisName;

            medicalCase.IdPatientMis = patient.CartNum;
            medicalCase.DoctorInCharge = doctor;
            medicalCase.Authenticator = new Participant { Doctor = doctor, IdRole = 3 };
            medicalCase.Author = new Participant { Doctor = doctor, IdRole = 3 };
            medicalCase.LegalAuthenticator = new Participant { Doctor = doctor, IdRole = 3 };
            medicalCase.CaseVisitType = 1;    // 1 - Первичный
                                        // 2 - Повторный
            Log.Info($"Создан СМО для пациентa с картой {patient.CartNum}, ИД случая: {medicalCase.IdCaseMis}");
            medicalCase.Steps =
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
                        IdPaymentType = (byte)paymentType
            }
                ];

            medicalCase.MedRecords = medDocuments.ToArray();
            medicalCase.Steps[0].MedRecords = [.. GetProcedures(treat, doctor, procedureDescriptions)];

            // Новые требования, добавляем всегда 1. Удовлетворительное состояние пациента при поступлении.
            medicalCase.AdmissionCondition = 1;

            // Новые требования, добавляем всегда 1. Удовлетворительное состояние пациента при поступлении.
            medicalCase.IdAmbResult = 2;

            return medicalCase;
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

		private Task<List<MedRecord>> getMedDocuments(PatientAccount treat)
        {
            return _dependencies.GetMedicalDocuments(treat);
        }

        private static List<MedRecord> GetProcedures(
            PatientAccount treat,
            MedicalStaff doctor,
            IEnumerable<ProcedureDescriptionEmk> descriptions)
        {
            var medRecords = new List<MedRecord>();

            foreach (var d in descriptions)
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
                    Performer = new Participant { IdRole = 3, Doctor = doctor }
                });
            }

            return medRecords;
        }
    }
}
