using System;
using System.Collections.Generic;
using Emk.Application.Dto;
using Emk.Domain.Entities;
using Emk.Infrastructure.ExternalServices.Mappers;
using Emk.Infrastructure.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests.Infrastructure
{
    [TestClass]
    public class InfrastructureMapperTests
    {
        [TestMethod]
        public void PixServiceMapper_MapsPatientFieldsAndSnils()
        {
            var patient = PixServiceMapper.ToServiceDto(new PatientDto
            {
                PatientId = 17,
                CartNum = "card-17",
                Surname = "Surname",
                Name = "Name",
                MiddleName = "Middle",
                BirthDate = new DateTime(2000, 1, 2),
                Sex = "F",
                Snils = "123-456 789 00"
            });

            Assert.AreEqual("card-17", patient.IdPatientMIS);
            Assert.AreEqual("Surname", patient.FamilyName);
            Assert.AreEqual((byte)2, patient.Sex);
            Assert.AreEqual("12345678900", patient.Documents[0].DocN);
        }

        [TestMethod]
        public void EmkServiceMapper_MapsAvailableTreatmentFields()
        {
            var treatment = new PatientTreatDto
            {
                PatientId = 17,
                AccountId = 31,
                TreatDate = new DateTime(2025, 3, 4),
                Diagnosis = new DiagnosisDto { DiagnosisName = "Diagnosis" }
            };

            var serviceCase = (Emk.EmkSvc.CaseAmb)EmkServiceMapper.ToServiceDto(treatment, "lpu-id");

            Assert.AreEqual("17", serviceCase.HistoryNumber);
            Assert.AreEqual("31", serviceCase.IdCaseMis);
            Assert.AreEqual("lpu-id", serviceCase.IdLpu);
            Assert.AreEqual("Diagnosis", serviceCase.Comment);
        }

        [TestMethod]
        public void SmoSettingsXmlMapper_RoundTripsDomainEntities()
        {
            var settings = new SmoSettingsDocument
            {
                Doctors = new List<Doctor> { new Doctor(23, "Surname", "Name") { PersCode = "code" } },
                Default = new DefaultData { VisitPlace = 2 }
            };

            var restored = SmoSettingsXmlMapper.FromXml(SmoSettingsXmlMapper.ToXml(settings));

            Assert.AreEqual(23, restored.Doctors[0].MemberId);
            Assert.AreEqual("Surname", restored.Doctors[0].Surname);
            Assert.AreEqual(2, restored.Default.VisitPlace);
        }
    }
}
