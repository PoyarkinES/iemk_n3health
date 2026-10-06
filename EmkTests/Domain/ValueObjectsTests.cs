using System;
using Emk.Domain.DomainExceptions;
using Emk.Domain.ValueObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Domain
{
    [TestClass]
    public class ValueObjectsTests
    {
        [TestMethod]
        public void EmkSettingId_EmptyGuid_Throws()
        {
            Assert.ThrowsException<InvalidPracticeException>(() => new EmkSettingId(Guid.Empty));
            Assert.IsFalse(EmkSettingId.IsValid(Guid.Empty));
        }

        [TestMethod]
        public void EmkSettingId_Parse_Works()
        {
            var guid = Guid.NewGuid();
            Assert.AreEqual(new EmkSettingId(guid), EmkSettingId.Parse(guid.ToString()));
            Assert.ThrowsException<InvalidPracticeException>(() => EmkSettingId.Parse("not a guid"));
            Assert.ThrowsException<ArgumentNullException>(() => EmkSettingId.Parse(null));
        }

        [TestMethod]
        public void LicenseKey_Validation()
        {
            Assert.IsTrue(new LicenseKey("ABCD-1234").IsValid());
            Assert.IsFalse(LicenseKey.IsValid(null));
            Assert.IsFalse(LicenseKey.IsValid(""));
            Assert.IsFalse(LicenseKey.IsValid("AB CD"));
            Assert.ThrowsException<InvalidLicenseException>(() => new LicenseKey(" "));
            Assert.ThrowsException<InvalidLicenseException>(() => new LicenseKey(null));
        }

        [TestMethod]
        public void TreatmentPeriod_StartMustBeBeforeEnd()
        {
            var start = new DateTime(2020, 1, 1);
            var period = new TreatmentPeriod(start, start.AddDays(1));
            Assert.IsTrue(period.Contains(start));
            Assert.IsFalse(period.Contains(start.AddDays(2)));
            Assert.ThrowsException<DomainException>(() => new TreatmentPeriod(start, start));
            Assert.ThrowsException<DomainException>(() => new TreatmentPeriod(start, start.AddDays(-1)));
        }

        [TestMethod]
        public void PatientConsent_EnsureGiven()
        {
            new PatientConsent(true).EnsureGiven();
            var ex = Assert.ThrowsException<PatientConsentMissingException>(() => new PatientConsent(false, "нет подписи").EnsureGiven());
            Assert.AreEqual("нет подписи", ex.Message);
            Assert.ThrowsException<PatientConsentMissingException>(() => new PatientConsent(false).EnsureGiven());
        }

        [TestMethod]
        public void DoctorInfo_RequiresNames()
        {
            var info = new DoctorInfo(1, "Иванов", "Иван");
            Assert.AreEqual(info, new DoctorInfo(1, "Иванов", "Иван"));
            Assert.ThrowsException<ArgumentException>(() => new DoctorInfo(1, "", "Иван"));
            Assert.ThrowsException<ArgumentException>(() => new DoctorInfo(1, "Иванов", null));
        }

        [TestMethod]
        public void Exceptions_DeriveFromDomainException()
        {
            Assert.IsInstanceOfType(new InvalidLicenseException(), typeof(DomainException));
            Assert.IsInstanceOfType(new InvalidPracticeException(), typeof(DomainException));
            Assert.IsInstanceOfType(new NoPatientAccountsException(), typeof(DomainException));
            Assert.IsInstanceOfType(new DocumentSignatureException(), typeof(DomainException));
            Assert.IsInstanceOfType(new PatientConsentMissingException(), typeof(DomainException));
        }
    }
}
