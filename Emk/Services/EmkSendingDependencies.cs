using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emk.Models;

namespace Emk.Services
{
    public interface IEmkSendingRepository
    {
        List<EmkSettings> LoadSettings(bool reload);
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

    internal sealed class FactoryEmkSendingRepository : IEmkSendingRepository
    {
        public List<EmkSettings> LoadSettings(bool reload) => Factory.LoadSettings(reload);

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
}
