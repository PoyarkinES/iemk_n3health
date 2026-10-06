using System;
using Emk.Domain.Entities;
using Emk.Domain.Enums;
using Emk.Domain.Enums.Comparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Domain
{
    [TestClass]
    public class EntitiesTests
    {
        [TestMethod]
        public void Patient_NullArguments_Throw()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Patient(null, "a", "b"));
            Assert.ThrowsException<ArgumentNullException>(() => new Patient("1", null, "b"));
            Assert.ThrowsException<ArgumentNullException>(() => new Patient("1", "a", null));
            Assert.AreEqual("1", new Patient("1", "a", "b").CartNum);
        }

        [TestMethod]
        public void Patient_SexInt()
        {
            Assert.AreEqual(0, new Patient().SexInt);
            Assert.AreEqual(1, new Patient { Sex = "M " }.SexInt);
            Assert.AreEqual(2, new Patient { Sex = "F" }.SexInt);
        }

        [TestMethod]
        public void Entities_NullArguments_Throw()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PatientAccount(1, 1, 1, DateTime.Now, null));
            Assert.ThrowsException<ArgumentNullException>(() => new PatientTreat(1, 1, DateTime.Now, 1, null));
            Assert.ThrowsException<ArgumentNullException>(() => new CartNote(1, 1, DateTime.Now, null));
            Assert.ThrowsException<ArgumentNullException>(() => new Doctor(1, null, "a"));
            Assert.ThrowsException<ArgumentNullException>(() => new InsuranceCompany(1, null, "a"));
            Assert.ThrowsException<ArgumentNullException>(() => new DiagnosisEmk(null, "a"));
            Assert.ThrowsException<ArgumentNullException>(() => new FileData(null, InternalDocType.DocConsultNote));
            Assert.ThrowsException<ArgumentNullException>(() => new DbSettings(null, 1, "v"));
        }

        [TestMethod]
        public void License_IsValid()
        {
            Assert.IsTrue(new License { Status = "OK", Message = "License Data found", Number = "1" }.IsValid);
            Assert.IsFalse(new License().IsValid);
        }

        [TestMethod]
        public void FileData_FileExists_False_WhenNoPath()
        {
            Assert.IsFalse(new FileData().FileExists);
        }

        [TestMethod]
        public void DoctorComparer_Compares()
        {
            var comparer = new DoctorComparer();
            var a = new Doctor(1, "A", "B");
            var b = new Doctor(1, "A", "B");
            Assert.IsTrue(comparer.Equals(a, b));
            Assert.IsFalse(comparer.Equals(a, null));
            Assert.AreEqual(comparer.GetHashCode(a), comparer.GetHashCode(b));
            Assert.AreEqual(0, comparer.GetHashCode(null));
        }
    }
}
