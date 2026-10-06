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
            var clients = new StubSendingClientFactory();
            var service = CreateService(repository, clients);

            await service.Run();

            Assert.AreEqual(0, repository.PatientAccountQueries);
            Assert.AreEqual(0, clients.Pix.AddPatientCalls);
        }

        [TestMethod]
        public async Task Run_SendsCaseAndMarksDocumentsAfterSuccess()
        {
            var repository = CreateRepositoryWithCase();
            var clients = new StubSendingClientFactory();
            var service = CreateService(repository, clients);

            await service.Run(123);

            Assert.AreEqual(1, clients.Pix.AddPatientCalls);
            Assert.AreEqual(1, clients.Emk.AddCaseCalls);
            Assert.AreEqual(123, clients.Pix.LastAccountId);
            Assert.AreEqual(123, clients.Emk.LastAccountId);
            CollectionAssert.AreEqual(new[] { 123 }, repository.MarkedAccountIds.ToArray());
        }

        [TestMethod]
        public async Task Run_DoesNotCallEmkOrMarkDocumentsWhenPixAddFails()
        {
            var repository = CreateRepositoryWithCase();
            var clients = new StubSendingClientFactory();
            clients.Pix.AddPatientResult = false;
            var service = CreateService(repository, clients);

            await service.Run(123);

            Assert.AreEqual(1, clients.Pix.AddPatientCalls);
            Assert.AreEqual(0, clients.Emk.AddCaseCalls);
            Assert.AreEqual(0, repository.MarkedAccountIds.Count);
        }

        [TestMethod]
        public async Task Run_DoesNotMarkDocumentsWhenEmkAddFails()
        {
            var repository = CreateRepositoryWithCase();
            var clients = new StubSendingClientFactory();
            clients.Emk.AddCaseResult = -1;
            var service = CreateService(repository, clients);

            await service.Run(123);

            Assert.AreEqual(1, clients.Pix.AddPatientCalls);
            Assert.AreEqual(1, clients.Emk.AddCaseCalls);
            Assert.AreEqual(0, repository.MarkedAccountIds.Count);
        }

        [TestMethod]
        public async Task Run_DoesNotCreateClientsWhenNoCasesAreFound()
        {
            var repository = CreateRepositoryWithSettings();
            var clients = new StubSendingClientFactory();
            var service = CreateService(repository, clients);

            await service.Run();

            Assert.AreEqual(1, repository.PatientAccountQueries);
            Assert.AreEqual(0, clients.Pix.AddPatientCalls);
        }

        [TestMethod]
        public async Task Run_ByAccount_DoesNotCreateClientsWhenAccountIsMissing()
        {
            var repository = CreateRepositoryWithSettings();
            var clients = new StubSendingClientFactory();
            var service = CreateService(repository, clients);

            await service.Run(123);

            Assert.AreEqual(123, repository.RequestedAccountId);
            Assert.AreEqual(0, clients.Pix.AddPatientCalls);
        }

        [TestMethod]
        public async Task Run_SkipsCaseWhenPracticeSettingsAreMissingOrDisabled()
        {
            foreach (var settings in new EmkSettings[]
            {
                null,
                CreateSettings(enabled: false)
            })
            {
                var repository = settings == null
                    ? CreateRepositoryWithCase()
                    : CreateRepositoryWithCase(settings);
                if (settings == null)
                    repository.Settings.Clear();
                var clients = new StubSendingClientFactory();

                await CreateService(repository, clients).Run(123);

                Assert.AreEqual(0, clients.Pix.AddPatientCalls);
                Assert.AreEqual(0, clients.Emk.AddCaseCalls);
                Assert.AreEqual(0, repository.MarkedAccountIds.Count);
            }
        }

        [TestMethod]
        public async Task Run_SkipsCaseWhenAnyEligibilityCheckFails()
        {
            foreach (var failure in new Action<StubSendingRepository>[]
            {
                repository => repository.PracticeCheckMessages = new[] { "practice mismatch" },
                repository => repository.EsignCheckMessages = new[] { "unsigned document" },
                repository => repository.DocumentAccessMessages = new List<string> { "missing document" },
                repository => repository.PatientConsentCount = 1
            })
            {
                var repository = CreateRepositoryWithCase();
                failure(repository);
                var clients = new StubSendingClientFactory();

                await CreateService(repository, clients).Run(123);

                Assert.AreEqual(0, clients.Pix.AddPatientCalls);
                Assert.AreEqual(0, clients.Emk.AddCaseCalls);
                Assert.AreEqual(0, repository.MarkedAccountIds.Count);
            }
        }

        [TestMethod]
        public async Task Update_UpdatesPatientAndCaseThenMarksDocumentsAfterSuccess()
        {
            var repository = CreateRepositoryWithCase();
            var clients = new StubSendingClientFactory();

            await CreateService(repository, clients).Update(123);

            Assert.AreEqual(1, clients.Pix.UpdatePatientCalls);
            Assert.AreEqual(1, clients.Emk.UpdateCaseCalls);
            Assert.AreEqual(123, clients.Pix.LastAccountId);
            Assert.AreEqual(123, clients.Emk.LastAccountId);
            CollectionAssert.AreEqual(new[] { 123 }, repository.MarkedAccountIds.ToArray());
        }

        [TestMethod]
        public async Task Update_DoesNotCallClientsWhenSignatureCheckFails()
        {
            var repository = CreateRepositoryWithCase();
            repository.EsignCheckMessages = new[] { "unsigned document" };
            var clients = new StubSendingClientFactory();

            await CreateService(repository, clients).Update(123);

            Assert.AreEqual(0, clients.Pix.UpdatePatientCalls);
            Assert.AreEqual(0, clients.Emk.UpdateCaseCalls);
            Assert.AreEqual(0, repository.MarkedAccountIds.Count);
        }

        [TestMethod]
        public async Task Update_DoesNotMarkDocumentsWhenEmkUpdateFails()
        {
            var repository = CreateRepositoryWithCase();
            var clients = new StubSendingClientFactory();
            clients.Emk.UpdateCaseResult = -1;

            await CreateService(repository, clients).Update(123);

            Assert.AreEqual(1, clients.Pix.UpdatePatientCalls);
            Assert.AreEqual(1, clients.Emk.UpdateCaseCalls);
            Assert.AreEqual(0, repository.MarkedAccountIds.Count);
        }

        private static EmkSendingService CreateService(
            StubSendingRepository repository,
            StubSendingClientFactory clients) =>
            new EmkSendingService(repository, clients);

        private static StubSendingRepository CreateRepositoryWithSettings(EmkSettings settings = null) =>
            new StubSendingRepository
            {
                Settings = settings == null
                    ? new List<EmkSettings> { CreateSettings() }
                    : new List<EmkSettings> { settings }
            };

        private static StubSendingRepository CreateRepositoryWithCase(EmkSettings settings = null)
        {
            var repository = CreateRepositoryWithSettings(settings);
            repository.PatientAccounts = new List<PatientAccount> { CreatePatientAccount() };
            repository.PatientAccount = CreatePatientAccount();
            return repository;
        }

        private static EmkSettings CreateSettings(bool enabled = true) =>
            new EmkSettings
            {
                PracticeId = 7,
                Guid = Guid.NewGuid(),
                IdLPU = Guid.NewGuid(),
                Enabled = enabled,
                SendingType = SendingType.DaysBeforeNow
            };

        private static PatientAccount CreatePatientAccount() =>
            new PatientAccount
            {
                AccountId = 123,
                PatientId = 42,
                ProviderId = 5,
                PracticeId = 7,
                TreatDate = new DateTime(2026, 10, 1),
                DiagnoseCode = "A00",
                DiagnoseName = "Test diagnosis"
            };

        private sealed class StubSendingRepository : IEmkSendingRepository
        {
            public bool LicenseCheckResult { get; set; }
            public int PatientAccountQueries { get; private set; }
            public int? RequestedAccountId { get; private set; }
            public List<EmkSettings> Settings { get; set; } = new List<EmkSettings>();
            public List<PatientAccount> PatientAccounts { get; set; } = new List<PatientAccount>();
            public PatientAccount PatientAccount { get; set; }
            public IEnumerable<string> PracticeCheckMessages { get; set; } = Enumerable.Empty<string>();
            public IEnumerable<string> EsignCheckMessages { get; set; } = Enumerable.Empty<string>();
            public List<string> DocumentAccessMessages { get; set; } = new List<string>();
            public int PatientConsentCount { get; set; }
            public List<int> MarkedAccountIds { get; } = new List<int>();

            public Task<List<EmkSettings>> LoadSettings(bool reload) => Task.FromResult(Settings);
            public Task<bool> IsLicenseValid() => Task.FromResult(LicenseCheckResult);

            public Task<List<PatientAccount>> GetPatientAccountsAsync(DateTime from, DateTime to)
            {
                PatientAccountQueries++;
                return Task.FromResult(PatientAccounts);
            }

            public Task<PatientAccount> GetPatientAccountByIdAsync(int accountId)
            {
                RequestedAccountId = accountId;
                return Task.FromResult(PatientAccount);
            }

            public Task<IEnumerable<string>> GetCheckDocumentEsignAsync(int accountId) =>
                Task.FromResult(EsignCheckMessages);

            public Task<List<string>> CheckDocumentAccessAsync(int accountId) =>
                Task.FromResult(DocumentAccessMessages);

            public Task<IEnumerable<string>> GetCheckPracticIdAsync(int accountId) =>
                Task.FromResult(PracticeCheckMessages);

            public Task<int> CheckPatientConsentTransPersDataAsync(int patientId) =>
                Task.FromResult(PatientConsentCount);

            public Task UpdateEsignFiles(PatientAccount account)
            {
                MarkedAccountIds.Add(account.AccountId);
                return Task.CompletedTask;
            }
        }

        private sealed class StubSendingClientFactory : IEmkSendingClientFactory
        {
            public StubPixSendingClient Pix { get; } = new StubPixSendingClient();
            public StubEmkSendingClient Emk { get; } = new StubEmkSendingClient();

            public IPixSendingClient CreatePixClient(EmkSettings settings) => Pix;
            public IEmkCaseSendingClient CreateEmkClient(EmkSettings settings) => Emk;
        }

        private sealed class StubPixSendingClient : IPixSendingClient
        {
            public bool AddPatientResult { get; set; } = true;
            public int AddPatientCalls { get; private set; }
            public int UpdatePatientCalls { get; private set; }
            public int LastAccountId { get; private set; }

            public Task<bool> AddPatient(PatientAccount account)
            {
                AddPatientCalls++;
                LastAccountId = account.AccountId;
                return Task.FromResult(AddPatientResult);
            }

            public Task<bool> UpdatePatient(PatientAccount account)
            {
                UpdatePatientCalls++;
                LastAccountId = account.AccountId;
                return Task.FromResult(true);
            }
        }

        private sealed class StubEmkSendingClient : IEmkCaseSendingClient
        {
            public int AddCaseResult { get; set; }
            public int UpdateCaseResult { get; set; }
            public int AddCaseCalls { get; private set; }
            public int UpdateCaseCalls { get; private set; }
            public int LastAccountId { get; private set; }

            public Task<int> AddCase(PatientAccount account, bool updateOnly)
            {
                AddCaseCalls++;
                LastAccountId = account.AccountId;
                return Task.FromResult(AddCaseResult);
            }

            public Task<int> UpdateCase(PatientAccount account, string directory = null)
            {
                UpdateCaseCalls++;
                LastAccountId = account.AccountId;
                return Task.FromResult(UpdateCaseResult);
            }
        }
    }
}
