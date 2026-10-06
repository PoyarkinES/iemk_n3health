using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Emk.Models;
using Emk.EmkSvc;
using Emk.PixSvc;
using Emk.Repository.Interface;
using Emk.Services;
using Emk.Services.Docs;
using Emk.Services.Files;
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
        public async Task Run_AssignsSuffixesToCasesWithDifferentDiagnoses()
        {
            var repository = CreateRepositoryWithSettings();
            var firstCase = CreatePatientAccount();
            var secondCase = CreatePatientAccount();
            secondCase.DiagnoseCode = "B00";
            secondCase.DiagnoseName = "Another diagnosis";
            repository.PatientAccounts = new List<PatientAccount> { firstCase, secondCase };
            var clients = new StubSendingClientFactory();

            await CreateService(repository, clients).Run();

            CollectionAssert.AreEqual(new[] { "a", "b" }, clients.Emk.AddedCaseSuffixes.ToArray());
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
        public async Task Run_DoesNotQueryCasesWhenSettingsAreEmpty()
        {
            var repository = new StubSendingRepository();
            var clients = new StubSendingClientFactory();

            await CreateService(repository, clients).Run();

            Assert.AreEqual(0, repository.PatientAccountQueries);
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
        public async Task Update_DoesNotQueryAccountWhenSettingsAreEmpty()
        {
            var repository = new StubSendingRepository();
            var clients = new StubSendingClientFactory();

            await CreateService(repository, clients).Update(123);

            Assert.IsNull(repository.RequestedAccountId);
            Assert.AreEqual(0, clients.Pix.UpdatePatientCalls);
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
        public async Task Update_DoesNotCallEmkOrMarkDocumentsWhenPixUpdateFails()
        {
            var repository = CreateRepositoryWithCase();
            var clients = new StubSendingClientFactory();
            clients.Pix.UpdatePatientResult = false;

            await CreateService(repository, clients).Update(123);

            Assert.AreEqual(1, clients.Pix.UpdatePatientCalls);
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

        [TestMethod]
        public async Task DocSelector_LoadsDocumentsFromInjectedRepository()
        {
            var repository = new StubDocSelectorRepository();
            var selector = new DocSelector(CreatePatientAccount(), repository);

            var documents = await selector.GetDocs(123);

            Assert.AreEqual(123, repository.RequestedAccountId);
            Assert.AreEqual(0, documents.Count);
        }

        [TestMethod]
        public async Task DocSelector_OmitsFilesWithDifferentTreatmentDate()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var fileName = "20261002_17_86_123_5_Test_Name.xml";
            File.WriteAllText(Path.Combine(directory, fileName), string.Empty);

            try
            {
                var repository = new StubDocSelectorRepository
                {
                    Directory = directory,
                    Documents = new List<DocumentsDto>
                    {
                        new DocumentsDto { uuid = "doc-1", efiles_name = fileName }
                    }
                };
                var selector = new DocSelector(
                    CreatePatientAccount(), repository, new StubDocumentInitializationDependencies());

                var documents = await selector.GetDocs(123);

                Assert.AreEqual(0, documents.Count);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DocSelector_LoadsPracticeDirectoryOnlyOnceForMultipleDocuments()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var firstFile = "20261002_17_86_123_5_Test_Name.xml";
            var secondFile = "20261003_17_86_123_5_Test_Name.xml";
            File.WriteAllText(Path.Combine(directory, firstFile), string.Empty);
            File.WriteAllText(Path.Combine(directory, secondFile), string.Empty);

            try
            {
                var repository = new StubDocSelectorRepository
                {
                    Directory = directory,
                    Documents = new List<DocumentsDto>
                    {
                        new DocumentsDto { uuid = "doc-1", efiles_name = firstFile },
                        new DocumentsDto { uuid = "doc-2", efiles_name = secondFile }
                    }
                };
                var selector = new DocSelector(
                    CreatePatientAccount(), repository, new StubDocumentInitializationDependencies());

                var documents = await selector.GetDocs(123);

                Assert.AreEqual(0, documents.Count);
                Assert.AreEqual(1, repository.FileDirectoryCalls);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DocSelector_ReportsMalformedDocumentFilename()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var fileName = "invalid.xml";
            File.WriteAllText(Path.Combine(directory, fileName), string.Empty);

            try
            {
                var repository = new StubDocSelectorRepository
                {
                    Directory = directory,
                    Documents = new List<DocumentsDto>
                    {
                        new DocumentsDto { uuid = "doc-1", efiles_name = fileName }
                    }
                };
                var selector = new DocSelector(
                    CreatePatientAccount(), repository, new StubDocumentInitializationDependencies());

                var exception = await Assert.ThrowsExceptionAsync<FormatException>(
                    () => selector.GetDocs(123));

                StringAssert.Contains(exception.Message, fileName);
                Assert.IsInstanceOfType(exception.InnerException, typeof(FormatException));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DocSelector_PreservesMissingDocumentFileError()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);

            try
            {
                var repository = new StubDocSelectorRepository
                {
                    Directory = directory,
                    Documents = new List<DocumentsDto>
                    {
                        new DocumentsDto
                        {
                            uuid = "doc-1",
                            efiles_name = "20261002_17_86_123_5_Test_Name.xml"
                        }
                    }
                };
                var selector = new DocSelector(
                    CreatePatientAccount(), repository, new StubDocumentInitializationDependencies());

                await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => selector.GetDocs(123));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DocumentInitialization_UsesInjectedDependencies()
        {
            var dependencies = new StubDocumentInitializationDependencies();
            var document = new DocPrescription("unused.xml", 17, 123);

            await document.InitializeAsync(dependencies);

            Assert.AreEqual(17, dependencies.RequestedCartNoteId);
            Assert.AreEqual(123, dependencies.RequestedAccountId);
            Assert.AreEqual(42, dependencies.RequestedPatientId);
        }

        [TestMethod]
        public async Task DocumentInitialization_LoadsDoctorAndPatientConcurrently()
        {
            var dependencies = new ConcurrentDocumentInitializationDependencies();
            var document = new DocPrescription("unused.xml", 17, 123);

            var initialization = document.InitializeAsync(dependencies);
            var completed = await Task.WhenAny(initialization, Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.AreSame(initialization, completed);
            await initialization;
        }

        [TestMethod]
        public async Task DocumentCreation_LoadsPdfAndSignaturesAsAttachments()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "consult.pdf");
            var pdf = new byte[] { 1, 2, 3 };
            var personalSign = new byte[] { 4, 5 };
            var organizationSign = new byte[] { 6, 7 };
            File.WriteAllBytes(path, pdf);
            File.WriteAllBytes(path + ".sgn", personalSign);
            File.WriteAllBytes(path + "2.sgn", organizationSign);

            try
            {
                var document = new DocConsultNotePdf(path, 17, 123);
                await document.InitializeAsync(new StubDocumentInitializationDependencies());

                var result = (ConsultNote)await document.CreateDocumentAsync();
                var attachment = result.Attachments.Single();

                CollectionAssert.AreEqual(pdf, attachment.Data);
                CollectionAssert.AreEqual(organizationSign, attachment.OrganizationSign);
                CollectionAssert.AreEqual(personalSign, attachment.PersonalSigns.Single().Sign);
                Assert.AreEqual("application/pdf", attachment.MimeType);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DoctorFileService_InitializesExistingSettingsAsynchronously()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "SmoSettings.xml");
            var settings = new SmoSettings
            {
                Doctors = new List<Doctor>(),
                Default = new DefaultData { VisitPurpose = 9 }
            };
            using (var stream = File.Create(path))
                new System.Xml.Serialization.XmlSerializer(typeof(SmoSettings)).Serialize(stream, settings);

            try
            {
                var repository = new StubDoctorRepository();
                var service = new DoctorFileService(repository, path);

                await service.InitializeAsync();

                Assert.AreEqual(0, repository.GetDoctorsCalls);
                Assert.AreEqual(9, service.LoadDefaults().VisitPurpose);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DoctorFileService_CreatesMissingSettingsFromRepositoryOnce()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "SmoSettings.xml");

            try
            {
                var doctor = new Doctor { MemberId = 23 };
                var repository = new StubDoctorRepository
                {
                    Doctors = new[] { doctor }
                };
                var service = new DoctorFileService(repository, path);

                await service.InitializeAsync();
                await service.InitializeAsync();

                Assert.AreEqual(1, repository.GetDoctorsCalls);
                Assert.IsTrue(File.Exists(path));
                Assert.AreEqual(23, service.LoadDoctorsFromFile().Single().MemberId);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task DoctorFileService_SavesDoctorsAndDefaultsAsynchronously()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "SmoSettings.xml");

            try
            {
                var service = new DoctorFileService(new StubDoctorRepository(), path);
                await service.InitializeAsync();
                await service.SaveDoctorsToFileAsync(new List<Doctor>
                {
                    new Doctor { MemberId = 31 }
                });
                await service.SaveDefaultsAsync(new DefaultData { VisitPurpose = 11 });

                var reloadedService = new DoctorFileService(new StubDoctorRepository(), path);
                await reloadedService.InitializeAsync();

                Assert.AreEqual(31, reloadedService.LoadDoctorsFromFile().Single().MemberId);
                Assert.AreEqual(11, reloadedService.LoadDefaults().VisitPurpose);
                Assert.IsFalse(File.Exists(path + ".tmp"));
                Assert.IsFalse(File.Exists(path + ".bak"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public async Task PixService_GetPatientAsyncUsesInjectedWcfClient()
        {
            var clientFactory = new StubPixWcfClientFactory();
            var service = new PixService(
                new EmkSettings
                {
                    PixUrl = "http://pix.test",
                    Guid = Guid.NewGuid(),
                    IdLPU = Guid.NewGuid()
                },
                new StubPixServiceDependencies(),
                clientFactory);

            var patient = await service.GetPatientAsync(42);

            Assert.IsNotNull(patient);
            Assert.AreEqual("card-42", patient.CartNum);
            Assert.AreEqual(42, clientFactory.Client.RequestedPatientId);
            Assert.IsTrue(clientFactory.Client.ClosedSafely);
        }

        [TestMethod]
        public async Task PixService_AddPatientLoadsIndependentProfileDataConcurrently()
        {
            var dependencies = new ConcurrentPixServiceDependencies();
            var clientFactory = new StubPixWcfClientFactory();
            var service = new PixService(
                new EmkSettings
                {
                    PixUrl = "http://pix.test",
                    Guid = Guid.NewGuid(),
                    IdLPU = Guid.NewGuid()
                },
                dependencies,
                clientFactory);
            var account = CreatePatientAccount();

            var addPatient = service.AddPatient(account);
            var completed = await Task.WhenAny(addPatient, Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.AreSame(addPatient, completed);
            Assert.IsTrue(await addPatient);
            Assert.AreEqual(3, dependencies.StartedLookupCount);
        }

        [TestMethod]
        public async Task EmkService_AddCaseUsesInjectedWcfClient()
        {
            var clientFactory = new StubEmkWcfClientFactory();
            var dependencies = new StubEmkServiceDependencies();
            var service = new EmkService(
                new EmkSettings
                {
                    EmkUrl = "http://emk.test",
                    Guid = Guid.NewGuid(),
                    IdLPU = Guid.NewGuid(),
                    PatientDirectory = @"C:\patients"
                },
                dependencies,
                clientFactory);
            var account = CreatePatientAccount();
            account.Code = 1;

            var result = await service.AddCase(account);

            Assert.AreEqual(1, dependencies.GetDoctorCalls, dependencies.LastError);
            Assert.AreEqual(1, dependencies.DefaultsLoadCalls);
            Assert.AreEqual(0, result, dependencies.LastError);
            Assert.AreEqual(1, clientFactory.Client.AddCaseCalls);
            Assert.IsTrue(clientFactory.Client.ClosedSafely);
        }

        [TestMethod]
        public void EmkService_AddCaseReturnsNestedFaultDetails()
        {
            var service = CreateEmkService();
            var errors = new[]
            {
                new Emk.EmkSvc.RequestFault
                {
                    ErrorCode = 10,
                    PropertyName = "Root",
                    Message = "root",
                    Errors = new[]
                    {
                        new Emk.EmkSvc.RequestFault
                        {
                            ErrorCode = 42,
                            PropertyName = "Nested",
                            Message = "nested",
                            Errors = new[]
                            {
                                new Emk.EmkSvc.RequestFault
                                {
                                    ErrorCode = 99,
                                    PropertyName = "Leaf",
                                    Message = "leaf",
                                    Errors = new Emk.EmkSvc.RequestFault[0]
                                }
                            }
                        }
                    }
                }
            };

            var error = typeof(EmkService)
                .GetMethod("getError", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(service, new object[] { errors });

            Assert.AreEqual("99 : Leaf leaf ", error);
        }

        [TestMethod]
        public void EmkService_AddCaseReturnsNestedWarningDetails()
        {
            var service = CreateEmkService();
            var warnings = new[]
            {
                new RequestWarning
                {
                    WarningCode = 10,
                    PropertyName = "Root",
                    Message = "root",
                    Warnings = new[]
                    {
                        new RequestWarning
                        {
                            WarningCode = 42,
                            PropertyName = "Leaf",
                            Message = "leaf",
                            Warnings = new RequestWarning[0]
                        }
                    }
                }
            };

            var warning = typeof(EmkService)
                .GetMethod("getWarning", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(service, new object[] { warnings });

            Assert.AreEqual("42 : Leaf leaf ", warning);
        }

        private static EmkService CreateEmkService() =>
            new EmkService(
                new EmkSettings
                {
                    EmkUrl = "http://emk.test",
                    Guid = Guid.NewGuid(),
                    IdLPU = Guid.NewGuid(),
                    PatientDirectory = @"C:\patients"
                },
                new StubEmkServiceDependencies(),
                new StubEmkWcfClientFactory());

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

        private sealed class StubDocSelectorRepository : IDocSelectorRepository
        {
            public int RequestedAccountId { get; private set; }
            public int FileDirectoryCalls { get; private set; }
            public string Directory { get; set; } = string.Empty;
            public List<DocumentsDto> Documents { get; set; } = new List<DocumentsDto>();

            public Task<List<Emk.Models.DocumentsDto>> GetDocumentByAccountIdAsync(int accountId)
            {
                RequestedAccountId = accountId;
                return Task.FromResult(Documents);
            }

            public Task<string> GetFileDirectoryAsync(int practiceId)
            {
                FileDirectoryCalls++;
                return Task.FromResult(Directory);
            }
        }

        private sealed class StubDocumentInitializationDependencies :
            IDocumentInitializationDependencies
        {
            public int RequestedCartNoteId { get; private set; }
            public int RequestedAccountId { get; private set; }
            public int RequestedPatientId { get; private set; }

            public Task<CartNote> GetCartNote(int cartNoteId)
            {
                RequestedCartNoteId = cartNoteId;
                return Task.FromResult(new CartNote { PatientId = 42 });
            }

            public Task<DoctorEmk> GetDoctorOfPatientTreat(int accountId)
            {
                RequestedAccountId = accountId;
                return Task.FromResult(new DoctorEmk
                {
                    BirthDay = new DateTime(1980, 1, 1),
                    Surname = "Test",
                    Name = "Doctor"
                });
            }

            public Task<Patient> GetPatient(int patientId)
            {
                RequestedPatientId = patientId;
                return Task.FromResult(new Patient());
            }

            public Task<Patient> GetPatient(string patientCartNum) =>
                Task.FromResult(new Patient { CartNum = patientCartNum });
        }

        private sealed class ConcurrentDocumentInitializationDependencies :
            IDocumentInitializationDependencies
        {
            private readonly TaskCompletionSource<DoctorEmk> _doctor =
                new TaskCompletionSource<DoctorEmk>();
            private readonly TaskCompletionSource<Patient> _patient =
                new TaskCompletionSource<Patient>();
            private int _lookupCount;

            public Task<CartNote> GetCartNote(int cartNoteId) =>
                Task.FromResult(new CartNote { PatientId = 42 });

            public Task<DoctorEmk> GetDoctorOfPatientTreat(int accountId)
            {
                CompleteLookupsWhenBothStarted();
                return _doctor.Task;
            }

            public Task<Patient> GetPatient(int patientId)
            {
                CompleteLookupsWhenBothStarted();
                return _patient.Task;
            }

            public Task<Patient> GetPatient(string patientCartNum) =>
                Task.FromResult(new Patient { CartNum = patientCartNum });

            private void CompleteLookupsWhenBothStarted()
            {
                if (System.Threading.Interlocked.Increment(ref _lookupCount) == 2)
                {
                    _doctor.SetResult(new DoctorEmk());
                    _patient.SetResult(new Patient());
                }
            }
        }

        private sealed class StubDoctorRepository : IDoctorRepository
        {
            public IEnumerable<Doctor> Doctors { get; set; } = Enumerable.Empty<Doctor>();
            public int GetDoctorsCalls { get; private set; }

            public Task<IEnumerable<Doctor>> GetDoctors()
            {
                GetDoctorsCalls++;
                return Task.FromResult(Doctors);
            }
        }

        private sealed class StubPixServiceDependencies : IPixServiceDependencies
        {
            public Task<Patient> GetPatient(int patientId) =>
                Task.FromResult(new Patient { Id = patientId });

            public Task<DocumentDto> GetSnils(int patientId) =>
                Task.FromResult<DocumentDto>(null);

            public Task<DocumentDto> GetPolicy(int accountId) =>
                Task.FromResult<DocumentDto>(null);
        }

        private sealed class ConcurrentPixServiceDependencies : IPixServiceDependencies
        {
            private readonly TaskCompletionSource<Patient> _patient =
                new TaskCompletionSource<Patient>();
            private readonly TaskCompletionSource<DocumentDto> _snils =
                new TaskCompletionSource<DocumentDto>();
            private readonly TaskCompletionSource<DocumentDto> _policy =
                new TaskCompletionSource<DocumentDto>();
            private int _startedLookupCount;

            public int StartedLookupCount => _startedLookupCount;

            public Task<Patient> GetPatient(int patientId)
            {
                CompleteLookupsWhenAllStarted();
                return _patient.Task;
            }

            public Task<DocumentDto> GetSnils(int patientId)
            {
                CompleteLookupsWhenAllStarted();
                return _snils.Task;
            }

            public Task<DocumentDto> GetPolicy(int accountId)
            {
                CompleteLookupsWhenAllStarted();
                return _policy.Task;
            }

            private void CompleteLookupsWhenAllStarted()
            {
                if (System.Threading.Interlocked.Increment(ref _startedLookupCount) == 3)
                {
                    _patient.SetResult(new Patient
                    {
                        Id = 42,
                        LastName = "Test",
                        FirstName = "Patient",
                        CartNum = "card-42",
                        Sex = "M"
                    });
                    _snils.SetResult(new DocumentDto { DocN = "123" });
                    _policy.SetResult(null);
                }
            }
        }

        private sealed class StubPixWcfClientFactory : IPixWcfClientFactory
        {
            public StubPixWcfClient Client { get; } = new StubPixWcfClient();

            public IPixWcfClient Create(string serviceUrl) => Client;
        }

        private sealed class StubPixWcfClient : IPixWcfClient
        {
            public int RequestedPatientId { get; private set; }
            public bool ClosedSafely { get; private set; }

            public Task AddPatientAsync(string guid, string idLpu, PatientDto patient) =>
                Task.CompletedTask;

            public Task UpdatePatientAsync(string guid, string idLpu, PatientDto patient) =>
                Task.CompletedTask;

            public Task<PatientDto[]> GetPatientAsync(
                string guid, string idLpu, PatientDto patient, SourceType idSource)
            {
                RequestedPatientId = int.Parse(patient.IdPatientMIS);
                return Task.FromResult(new[]
                {
                    new PatientDto { IdPatientMIS = "card-42" }
                });
            }

            public void CloseSafely() => ClosedSafely = true;
        }

        private sealed class StubEmkServiceDependencies : IEmkServiceDependencies
        {
            public string LastError { get; private set; }
            public int GetDoctorCalls { get; private set; }
            public int DefaultsLoadCalls { get; private set; }

            public Task<DoctorEmk> GetDoctorByMemberId(int memberId)
            {
                GetDoctorCalls++;
                return Task.FromResult(new DoctorEmk
                {
                    MemberId = memberId,
                    BirthDay = new DateTime(1980, 1, 1),
                    Surname = "Test",
                    Name = "Doctor",
                    SexStr = "M"
                });
            }

            public Task<Patient> GetPatient(int patientId) =>
                Task.FromResult(new Patient
                {
                    Id = patientId,
                    CartNum = "card-42",
                    LastName = "Test",
                    FirstName = "Patient"
                });

            public Task<List<MedRecord>> GetMedicalDocuments(PatientAccount account) =>
                Task.FromResult(new List<MedRecord> { new MedDocument() });

            public Task<DefaultData> LoadDefaultsAsync()
            {
                DefaultsLoadCalls++;
                return Task.FromResult(new DefaultData());
            }

            public Task<PayType> GetPayType(int accountId) =>
                Task.FromResult(default(PayType));

            public Task<IEnumerable<ProcedureDescriptionEmk>> GetProcedureDescriptions(
                int patientId, DateTime procedureDate, int? accountId) =>
                Task.FromResult(Enumerable.Empty<ProcedureDescriptionEmk>());

            public Task SaveCase(int smo, DateTime uploadTime, string uploadMethod, int patientId,
                int accountId, string responseText, char isSuccess, string errorText)
            {
                LastError = errorText;
                return Task.CompletedTask;
            }
        }

        private sealed class StubEmkWcfClientFactory : IEmkWcfClientFactory
        {
            public StubEmkWcfClient Client { get; } = new StubEmkWcfClient();

            public IEmkWcfClient Create(string serviceUrl) => Client;
        }

        private sealed class StubEmkWcfClient : IEmkWcfClient
        {
            public int AddCaseCalls { get; private set; }
            public bool ClosedSafely { get; private set; }

            public Task AddCaseAsync(string guid, CaseBase caseData)
            {
                AddCaseCalls++;
                return Task.CompletedTask;
            }

            public Task UpdateCaseAsync(string guid, CaseBase caseData) =>
                Task.CompletedTask;

            public void CloseSafely() => ClosedSafely = true;
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
            public bool UpdatePatientResult { get; set; } = true;
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
                return Task.FromResult(UpdatePatientResult);
            }
        }

        private sealed class StubEmkSendingClient : IEmkCaseSendingClient
        {
            public int AddCaseResult { get; set; }
            public int UpdateCaseResult { get; set; }
            public int AddCaseCalls { get; private set; }
            public int UpdateCaseCalls { get; private set; }
            public int LastAccountId { get; private set; }
            public List<string> AddedCaseSuffixes { get; } = new List<string>();

            public Task<int> AddCase(PatientAccount account, bool updateOnly)
            {
                AddCaseCalls++;
                LastAccountId = account.AccountId;
                AddedCaseSuffixes.Add(account.SmoPostfix);
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
