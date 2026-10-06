using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emk.Models;
using Emk.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests
{
    [TestClass]
    public class EmkSendingServiceTests
    {
        [TestMethod]
        public async Task Run_DoesNotLoadCasesWhenLicenseCheckReturnsTrue()
        {
            var repository = new StubSendingRepository { LicenseCheckResult = true };
            var service = new EmkSendingService(repository, new StubSendingClientFactory());

            await service.Run();

            Assert.AreEqual(0, repository.PatientAccountQueries);
        }

        [TestMethod]
        public async Task Run_DoesNotCreateClientsWhenNoCasesAreFound()
        {
            var repository = new StubSendingRepository
            {
                Settings = new List<EmkSettings>
                {
                    new EmkSettings { SendingType = SendingType.DaysBeforeNow }
                }
            };
            var service = new EmkSendingService(repository, new StubSendingClientFactory());

            await service.Run();

            Assert.AreEqual(1, repository.PatientAccountQueries);
        }

        [TestMethod]
        public async Task Run_ByAccount_DoesNotCreateClientsWhenAccountIsMissing()
        {
            var repository = new StubSendingRepository
            {
                Settings = new List<EmkSettings>
                {
                    new EmkSettings { SendingType = SendingType.DaysBeforeNow }
                }
            };
            var service = new EmkSendingService(repository, new StubSendingClientFactory());

            await service.Run(123);

            Assert.AreEqual(123, repository.RequestedAccountId);
        }

        private sealed class StubSendingRepository : IEmkSendingRepository
        {
            public bool LicenseCheckResult { get; set; }
            public int PatientAccountQueries { get; private set; }
            public int? RequestedAccountId { get; private set; }
            public List<EmkSettings> Settings { get; set; } = new List<EmkSettings>();

            public List<EmkSettings> LoadSettings(bool reload) => Settings;
            public Task<bool> IsLicenseValid() => Task.FromResult(LicenseCheckResult);

            public Task<List<PatientAccount>> GetPatientAccountsAsync(DateTime from, DateTime to)
            {
                PatientAccountQueries++;
                return Task.FromResult(new List<PatientAccount>());
            }

            public Task<PatientAccount> GetPatientAccountByIdAsync(int accountId)
            {
                RequestedAccountId = accountId;
                return Task.FromResult<PatientAccount>(null);
            }

            public Task<IEnumerable<string>> GetCheckDocumentEsignAsync(int accountId) =>
                Task.FromResult(Enumerable.Empty<string>());

            public Task<List<string>> CheckDocumentAccessAsync(int accountId) =>
                Task.FromResult(new List<string>());

            public Task<IEnumerable<string>> GetCheckPracticIdAsync(int accountId) =>
                Task.FromResult(Enumerable.Empty<string>());

            public Task<int> CheckPatientConsentTransPersDataAsync(int patientId) =>
                Task.FromResult(0);

            public Task UpdateEsignFiles(PatientAccount account) => Task.CompletedTask;
        }

        private sealed class StubSendingClientFactory : IEmkSendingClientFactory
        {
            public IPixSendingClient CreatePixClient(EmkSettings settings) =>
                throw new InvalidOperationException("No sending client should be created.");

            public IEmkCaseSendingClient CreateEmkClient(EmkSettings settings) =>
                throw new InvalidOperationException("No sending client should be created.");
        }
    }
}
