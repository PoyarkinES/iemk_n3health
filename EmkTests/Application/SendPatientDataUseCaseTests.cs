using System;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.UseCases.Sending;
using Emk.Application.UseCases.Validation;
using Emk.Domain.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Application
{
    [TestClass]
    public class SendPatientDataUseCaseTests
    {
        private static readonly DateTime Day = new DateTime(2024, 1, 10);

        private InMemoryLicenseRepository _license;
        private InMemoryPatientRepository _patients;
        private InMemoryTreatRepository _treats;
        private InMemoryPixClient _pix;
        private InMemoryEmkClient _emkClient;
        private SendPatientDataUseCase _useCase;

        [TestInitialize]
        public void Setup()
        {
            var logger = new InMemoryLogger();
            var settings = new InMemorySettingsRepository();
            _license = new InMemoryLicenseRepository();
            _patients = new InMemoryPatientRepository();
            _treats = new InMemoryTreatRepository();
            _pix = new InMemoryPixClient();
            _emkClient = new InMemoryEmkClient();
            _treats.Treats.Add(new PatientTreatDto { TreatId = 5, AccountId = 10, PatientId = 7, PracticeId = 1, TreatDate = Day });

            var validate = new ValidateSendingRulesUseCase(_license, _treats, _patients, new InMemoryEmkRepository(), settings, logger);
            _useCase = new SendPatientDataUseCase(settings, _license, _treats, validate, _pix, _emkClient, logger);
        }

        private static SendPatientDataRequest Period() =>
            new SendPatientDataRequest
            {
                Period = new TreatmentPeriod(Day.AddDays(-1), Day.AddDays(1))
            };

        [TestMethod]
        public async Task Send_SingleTreat_Succeeds()
        {
            var response = await _useCase.ExecuteAsync(Period());

            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.Count);
            CollectionAssert.AreEqual(new[] { 10 }, _pix.Added);
            CollectionAssert.AreEqual(new[] { 10 }, _emkClient.Added);
            CollectionAssert.AreEqual(new[] { 10 }, _treats.UpdatedEsignAccounts);
        }

        [TestMethod]
        public async Task Send_TreatmentPeriod_UsesExactRequestBounds()
        {
            var start = Day.AddDays(-1);
            var end = Day.AddDays(1);

            var response = await _useCase.ExecuteAsync(SendPatientDataRequest.ForPeriod(start, end));

            Assert.IsTrue(response.Success);
            Assert.AreEqual(start, _treats.RequestedStart);
            Assert.AreEqual(end, _treats.RequestedEnd);
        }

        [TestMethod]
        public async Task Send_SameDayPeriod_RemainsSupported()
        {
            var response = await _useCase.ExecuteAsync(SendPatientDataRequest.ForPeriod(Day, Day));

            Assert.IsTrue(response.Success);
            Assert.AreEqual(Day, _treats.RequestedStart);
            Assert.AreEqual(Day, _treats.RequestedEnd);
        }

        [TestMethod]
        public async Task Send_ReversedPeriod_IsRejectedBeforeQuery()
        {
            var response = await _useCase.ExecuteAsync(
                SendPatientDataRequest.ForPeriod(Day.AddDays(1), Day));

            Assert.IsFalse(response.Success);
            Assert.IsNull(_treats.RequestedStart);
            Assert.IsNull(_treats.RequestedEnd);
        }

        [TestMethod]
        public async Task Send_ByAccountId_Succeeds()
        {
            var response = await _useCase.ExecuteAsync(new SendPatientDataRequest { AccountId = 10 });

            Assert.IsTrue(response.Success);
            Assert.AreEqual(1, response.Count);
        }

        [TestMethod]
        public async Task Send_NoLicense_SendsNothing()
        {
            _license.Valid = false;

            var response = await _useCase.ExecuteAsync(Period());

            Assert.IsFalse(response.Success);
            Assert.AreEqual(0, response.Count);
            Assert.AreEqual(0, _pix.Added.Count);
            Assert.AreEqual(0, _emkClient.Added.Count);
        }

        [TestMethod]
        public async Task Send_NoConsent_SkipsTreat()
        {
            _patients.Consent = false;

            var response = await _useCase.ExecuteAsync(Period());

            Assert.AreEqual(0, response.Count);
            Assert.AreEqual(0, _pix.Added.Count);
            Assert.AreEqual(0, _emkClient.Added.Count);
            Assert.AreEqual(0, _treats.UpdatedEsignAccounts.Count);
        }

        [TestMethod]
        public async Task Send_PixRejects_ReportsFailure()
        {
            _pix.Result = false;

            var response = await _useCase.ExecuteAsync(Period());

            Assert.IsFalse(response.Success);
            Assert.AreEqual(0, response.Count);
            Assert.AreEqual(0, _emkClient.Added.Count);
        }

        [TestMethod]
        public async Task Send_NoAccountAndNoPeriod_Fails()
        {
            var response = await _useCase.ExecuteAsync(new SendPatientDataRequest());

            Assert.IsFalse(response.Success);
        }
    }
}
