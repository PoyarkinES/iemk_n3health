using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;
using Emk.PixSvc;
using Emk.Repository;
using Emk.Services.Docs;
using Emk.Services.Files;

namespace Emk.Services
{
    internal static class WcfClientLifecycle
    {
        public static void Close(ICommunicationObject client)
        {
            if (client.State == CommunicationState.Faulted)
            {
                client.Abort();
                return;
            }

            try
            {
                client.Close();
            }
            catch (CommunicationException ex)
            {
                client.Abort();
                Log.Warning($"WCF client close failed: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                client.Abort();
                Log.Warning($"WCF client close timed out: {ex.Message}");
            }
        }
    }

    internal sealed class FactoryDocumentInitializationDependencies : IDocumentInitializationDependencies
    {
        private readonly Lazy<EmkRepository> _emkRepository =
            new Lazy<EmkRepository>(() => Factory.GetEmkRepository);
        private readonly Lazy<PatientRepository> _patientRepository =
            new Lazy<PatientRepository>(() => Factory.GetPatientRepository);

        public Task<CartNote> GetCartNote(int cartNoteId) =>
            _emkRepository.Value.GetCartNote(cartNoteId);

        public Task<DoctorEmk> GetDoctorOfPatientTreat(int accountId) =>
            _emkRepository.Value.GetDoctorOfPatientTreat(accountId);

        public Task<Patient> GetPatient(int patientId) =>
            _patientRepository.Value.GetPatient(patientId);

        public Task<Patient> GetPatient(string patientCartNum) =>
            _patientRepository.Value.GetPatient(patientCartNum);
    }

    internal sealed class FactoryDocSelectorRepository : IDocSelectorRepository
    {
        private readonly TreatRepository _treatRepository = Factory.GetTreatRepository;

        public Task<List<DocumentsDto>> GetDocumentByAccountIdAsync(int accountId) =>
            _treatRepository.GetDocumentByAccountIdAsync(accountId);

        public Task<string> GetFileDirectoryAsync(int practiceId) =>
            _treatRepository.GetFileDirectoryAsync(practiceId);
    }

    public interface IEmkSendingRepository
    {
        Task<List<EmkSettings>> LoadSettings(bool reload);
        Task<bool> IsLicenseValid();
        Task<List<PatientAccount>> GetPatientAccountsAsync(DateTime from, DateTime to);
        Task<PatientAccount> GetPatientAccountByIdAsync(int accountId);
        Task<IEnumerable<string>> GetCheckDocumentEsignAsync(int accountId);
        Task<List<string>> CheckDocumentAccessAsync(int accountId);
        Task<IEnumerable<string>> GetCheckPracticIdAsync(int accountId);
        Task<int> CheckPatientConsentTransPersDataAsync(int patientId);
        Task UpdateEsignFiles(PatientAccount account);
    }

    public interface IPixSendingClient
    {
        Task<bool> AddPatient(PatientAccount account);
        Task<bool> UpdatePatient(PatientAccount account);
    }

    public interface IPixWcfClient
    {
        Task AddPatientAsync(string guid, string idLpu, PatientDto patient);
        Task UpdatePatientAsync(string guid, string idLpu, PatientDto patient);
        Task<PatientDto[]> GetPatientAsync(
            string guid, string idLpu, PatientDto patient, SourceType idSource);
        void CloseSafely();
    }

    public interface IPixWcfClientFactory
    {
        IPixWcfClient Create(string serviceUrl);
    }

    internal sealed class FactoryPixWcfClientFactory : IPixWcfClientFactory
    {
        public IPixWcfClient Create(string serviceUrl)
        {
            var binding = new BasicHttpBinding();
            var endpointAddress = new EndpointAddress(new Uri(serviceUrl));
            return new PixWcfClientAdapter(new PixServiceClient(binding, endpointAddress));
        }
    }

    internal sealed class PixWcfClientAdapter : IPixWcfClient
    {
        private readonly PixServiceClient _client;

        public PixWcfClientAdapter(PixServiceClient client)
        {
            _client = client;
        }

        public Task AddPatientAsync(string guid, string idLpu, PatientDto patient) =>
            _client.AddPatientAsync(guid, idLpu, patient);

        public Task UpdatePatientAsync(string guid, string idLpu, PatientDto patient) =>
            _client.UpdatePatientAsync(guid, idLpu, patient);

        public Task<PatientDto[]> GetPatientAsync(
            string guid, string idLpu, PatientDto patient, SourceType idSource) =>
            _client.GetPatientAsync(guid, idLpu, patient, idSource);

        public void CloseSafely() => WcfClientLifecycle.Close(_client);
    }

    public interface IEmkCaseSendingClient
    {
        Task<int> AddCase(PatientAccount account, bool updateOnly);
        Task<int> UpdateCase(PatientAccount account, string directory = null);
    }

    public interface IEmkSendingClientFactory
    {
        IPixSendingClient CreatePixClient(EmkSettings settings);
        IEmkCaseSendingClient CreateEmkClient(EmkSettings settings);
    }

    public interface IEmkServiceDependencies
    {
        Task<DoctorEmk> GetDoctorByMemberId(int memberId);
        Task<Patient> GetPatient(int patientId);
        Task<List<MedRecord>> GetMedicalDocuments(PatientAccount account);
        DefaultData LoadDefaults();
        Task<PayType> GetPayType(int accountId);
        Task<IEnumerable<ProcedureDescriptionEmk>> GetProcedureDescriptions(
            int patientId, DateTime procedureDate, int? accountId);
        Task SaveCase(int smo, DateTime uploadTime, string uploadMethod, int patientId,
            int accountId, string responseText, char isSuccess, string errorText);
    }

    public interface IPixServiceDependencies
    {
        Task<Patient> GetPatient(int patientId);
        Task<DocumentDto> GetSnils(int patientId);
        Task<DocumentDto> GetPolicy(int accountId);
    }

    internal sealed class FactoryEmkSendingRepository : IEmkSendingRepository
    {
        private readonly TreatRepository _treatRepository = Factory.GetTreatRepository;
        private readonly EmkRepository _emkRepository = Factory.GetEmkRepository;
        private readonly LicenseRepository _licenseRepository = Factory.GetLicenseRepository;

        public Task<List<EmkSettings>> LoadSettings(bool reload) => Factory.LoadSettingsAsync(reload);

        public Task<bool> IsLicenseValid() => _licenseRepository.IsLicenseValid();

        public Task<List<PatientAccount>> GetPatientAccountsAsync(DateTime from, DateTime to) =>
            _treatRepository.GetPatientAccountsAsync(from, to);

        public Task<PatientAccount> GetPatientAccountByIdAsync(int accountId) =>
            _treatRepository.GetPatientAccountByIdAsync(accountId);

        public Task<IEnumerable<string>> GetCheckDocumentEsignAsync(int accountId) =>
            _treatRepository.GetCheckDocumentEsignAsync(accountId);

        public Task<List<string>> CheckDocumentAccessAsync(int accountId) =>
            _treatRepository.CheckDocumentAccessAsync(accountId);

        public Task<IEnumerable<string>> GetCheckPracticIdAsync(int accountId) =>
            _treatRepository.GetCheckPracticIdAsync(accountId);

        public Task<int> CheckPatientConsentTransPersDataAsync(int patientId) =>
            _treatRepository.CheckPatientConsentTransPersDataAsync(patientId);

        public Task UpdateEsignFiles(PatientAccount account) =>
            _emkRepository.UpdateEsignFiles(account);
    }

    internal sealed class FactoryEmkSendingClientFactory : IEmkSendingClientFactory
    {
        public IPixSendingClient CreatePixClient(EmkSettings settings) => new PixService(settings);

        public IEmkCaseSendingClient CreateEmkClient(EmkSettings settings) => new EmkService(settings);
    }

    internal sealed class FactoryEmkServiceDependencies : IEmkServiceDependencies
    {
        private readonly EmkRepository _emkRepository = Factory.GetEmkRepository;
        private readonly PatientRepository _patientRepository = Factory.GetPatientRepository;
        private readonly IDoctorFileService _doctorFileService = Factory.GetSmoService;

        public Task<DoctorEmk> GetDoctorByMemberId(int memberId) =>
            _emkRepository.GetDoctorByMemberId(memberId);

        public Task<Patient> GetPatient(int patientId) =>
            _patientRepository.GetPatient(patientId);

        public Task<Patient> GetPatient(string patientCartNum) =>
            _patientRepository.GetPatient(patientCartNum);

        public Task<List<MedRecord>> GetMedicalDocuments(PatientAccount account) =>
            new DocSelector(account).GetDocs(account.AccountId);

        public DefaultData LoadDefaults() => _doctorFileService.LoadDefaults();

        public Task<PayType> GetPayType(int accountId) =>
            _emkRepository.GetPayType(accountId);

        public Task<IEnumerable<ProcedureDescriptionEmk>> GetProcedureDescriptions(
            int patientId, DateTime procedureDate, int? accountId) =>
            _emkRepository.GetProcedureDescriptions(patientId, procedureDate, accountId);

        public Task SaveCase(int smo, DateTime uploadTime, string uploadMethod, int patientId,
            int accountId, string responseText, char isSuccess, string errorText) =>
            _emkRepository.SaveCase(smo, uploadTime, uploadMethod, patientId, accountId,
                responseText, isSuccess, errorText);
    }

    internal sealed class FactoryPixServiceDependencies : IPixServiceDependencies
    {
        private readonly PatientRepository _patientRepository = Factory.GetPatientRepository;

        public Task<Patient> GetPatient(int patientId) =>
            _patientRepository.GetPatient(patientId);

        public Task<DocumentDto> GetSnils(int patientId) =>
            _patientRepository.GetSnils(patientId);

        public Task<DocumentDto> GetPolicy(int accountId) =>
            _patientRepository.GetPolicy(accountId);
    }
}
