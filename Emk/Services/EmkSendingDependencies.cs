using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;
using Emk.PixSvc;
using Emk.Repository;
using Emk.Services.Files;

namespace Emk.Services
{
    internal sealed class FactoryDocSelectorRepository : IDocSelectorRepository
    {
        public Task<List<DocumentsDto>> GetDocumentByAccountIdAsync(int accountId) =>
            Factory.GetTreatRepository.GetDocumentByAccountIdAsync(accountId);

        public Task<string> GetFileDirectoryAsync(int practiceId) =>
            Factory.GetTreatRepository.GetFileDirectoryAsync(practiceId);
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
        public Task<List<EmkSettings>> LoadSettings(bool reload) => Factory.LoadSettingsAsync(reload);

        public Task<bool> IsLicenseValid() => Factory.GetLicenseRepository.IsLicenseValid();

        public Task<List<PatientAccount>> GetPatientAccountsAsync(DateTime from, DateTime to) =>
            Factory.GetTreatRepository.GetPatientAccountsAsync(from, to);

        public Task<PatientAccount> GetPatientAccountByIdAsync(int accountId) =>
            Factory.GetTreatRepository.GetPatientAccountByIdAsync(accountId);

        public Task<IEnumerable<string>> GetCheckDocumentEsignAsync(int accountId) =>
            Factory.GetTreatRepository.GetCheckDocumentEsignAsync(accountId);

        public Task<List<string>> CheckDocumentAccessAsync(int accountId) =>
            Factory.GetTreatRepository.CheckDocumentAccessAsync(accountId);

        public Task<IEnumerable<string>> GetCheckPracticIdAsync(int accountId) =>
            Factory.GetTreatRepository.GetCheckPracticIdAsync(accountId);

        public Task<int> CheckPatientConsentTransPersDataAsync(int patientId) =>
            Factory.GetTreatRepository.CheckPatientConsentTransPersDataAsync(patientId);

        public Task UpdateEsignFiles(PatientAccount account) =>
            Factory.GetEmkRepository.UpdateEsignFiles(account);
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
