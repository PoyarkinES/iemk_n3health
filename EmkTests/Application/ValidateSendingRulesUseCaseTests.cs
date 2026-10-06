using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.UseCases.Validation;
using Emk.Domain.DomainExceptions;
using Emk.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Application
{
    [TestClass]
    public class ValidateSendingRulesUseCaseTests
    {
        private InMemoryLicenseRepository _license;
        private InMemoryPatientRepository _patients;
        private InMemoryTreatRepository _treats;
        private InMemoryEmkRepository _emk;
        private InMemorySettingsRepository _settings;
        private EmkSettings _emkSettings;
        private ValidateSendingRulesUseCase _useCase;

        [TestInitialize]
        public void Setup()
        {
            _license = new InMemoryLicenseRepository();
            _patients = new InMemoryPatientRepository();
            _treats = new InMemoryTreatRepository();
            _emk = new InMemoryEmkRepository();
            _settings = new InMemorySettingsRepository();
            _emkSettings = new EmkSettings();
            _treats.Treats.Add(new PatientTreatDto { TreatId = 5, AccountId = 10, PatientId = 7, PracticeId = 1 });
            _useCase = new ValidateSendingRulesUseCase(
                _license, _treats, _patients, _emk, _settings, new InMemoryLogger(), _emkSettings);
        }

        [TestMethod]
        public async Task Validate_AllRulesPass_IsValid()
        {
            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 10 });

            Assert.IsTrue(response.IsValid);
            Assert.AreEqual(0, response.Errors.Count);
        }

        [TestMethod]
        public async Task Validate_NoLicense_ReturnsLicenseError()
        {
            _license.Valid = false;

            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 10 });

            Assert.IsFalse(response.IsValid);
            CollectionAssert.Contains(response.Errors, new InvalidLicenseException().Message);
        }

        [TestMethod]
        public async Task Validate_NoConsent_ReturnsConsentError()
        {
            _patients.Consent = false;

            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 10 });

            Assert.IsFalse(response.IsValid);
            CollectionAssert.Contains(response.Errors, new PatientConsentMissingException().Message);
        }

        [TestMethod]
        public async Task Validate_NoConsent_AllowsConfiguredAnonymousPatient()
        {
            _patients.Consent = false;
            _emkSettings.UnknownPatientFirstName = "Anonymous surname";
            _emkSettings.UnknownPatientGivenName = "Anonymous name";

            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 10 });

            Assert.IsTrue(response.IsValid);
            Assert.AreEqual(0, response.Errors.Count);
        }

        [TestMethod]
        public async Task Validate_NoConsent_RequiresBothAnonymousNames()
        {
            _patients.Consent = false;
            _emkSettings.UnknownPatientFirstName = "Anonymous surname";

            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 10 });

            Assert.IsFalse(response.IsValid);
            CollectionAssert.Contains(response.Errors, new PatientConsentMissingException().Message);
        }

        [TestMethod]
        public async Task Validate_NotSigned_And_WrongPractice_ReturnBothErrors()
        {
            _emk.Signed = false;
            _settings.Settings[0].PracticeId = 2;

            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 10, PatientTreatId = 5 });

            Assert.AreEqual(2, response.Errors.Count);
            CollectionAssert.Contains(response.Errors, new DocumentSignatureException().Message);
            CollectionAssert.Contains(response.Errors, new InvalidPracticeException().Message);
        }

        [TestMethod]
        public async Task Validate_UnknownAccount_ReturnsNoAccountsError()
        {
            var response = await _useCase.ExecuteAsync(new ValidateSendingRulesRequest { AccountId = 999 });

            Assert.IsFalse(response.IsValid);
            CollectionAssert.Contains(response.Errors, new NoPatientAccountsException().Message);
        }
    }
}
