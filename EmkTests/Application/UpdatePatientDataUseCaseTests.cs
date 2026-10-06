using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.UseCases.Updating;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Application
{
    [TestClass]
    public class UpdatePatientDataUseCaseTests
    {
        private InMemoryTreatRepository _treats;
        private InMemoryEmkRepository _emk;
        private InMemoryPixClient _pix;
        private InMemoryEmkClient _emkClient;
        private UpdatePatientDataUseCase _useCase;

        [TestInitialize]
        public void Setup()
        {
            _treats = new InMemoryTreatRepository();
            _emk = new InMemoryEmkRepository();
            _pix = new InMemoryPixClient();
            _emkClient = new InMemoryEmkClient();
            _treats.Treats.Add(new PatientTreatDto { TreatId = 5, AccountId = 10, PatientId = 7, PracticeId = 1 });
            _useCase = new UpdatePatientDataUseCase(_treats, _emk, new InMemoryLicenseRepository(), _pix, _emkClient, new InMemoryLogger());
        }

        [TestMethod]
        public async Task Update_ExistingAccount_Succeeds()
        {
            var response = await _useCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = 10 });

            Assert.IsTrue(response.Success);
            CollectionAssert.AreEqual(new[] { 10 }, _pix.Updated);
            CollectionAssert.AreEqual(new[] { 10 }, _emkClient.Updated);
        }

        [TestMethod]
        public async Task Update_MissingAccount_Fails()
        {
            var response = await _useCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = 999 });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(0, _pix.Updated.Count);
            Assert.AreEqual(0, _emkClient.Updated.Count);
        }

        [TestMethod]
        public async Task Update_NotSigned_Fails()
        {
            _emk.Signed = false;

            var response = await _useCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = 10 });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(0, _pix.Updated.Count);
        }

        [TestMethod]
        public async Task Update_NoLicense_Fails()
        {
            var license = new InMemoryLicenseRepository { Valid = false };
            var useCase = new UpdatePatientDataUseCase(_treats, _emk, license, _pix, _emkClient, new InMemoryLogger());

            var response = await useCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = 10 });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(0, _pix.Updated.Count);
        }

        [TestMethod]
        public async Task Update_PixRejects_Fails()
        {
            _pix.Result = false;

            var response = await _useCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = 10 });

            Assert.IsFalse(response.Success);
            Assert.AreEqual(0, _emkClient.Updated.Count);
        }

        [TestMethod]
        public async Task Update_EmkRejects_Fails()
        {
            _emkClient.Result = 0;

            var response = await _useCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = 10 });

            Assert.IsFalse(response.Success);
        }
    }
}
