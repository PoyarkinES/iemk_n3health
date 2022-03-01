using System.Linq;
using Emk;
using Emk.Services.Files;
using Emk.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests
{
    [TestClass]
    public class EmkDoctorFileServiceTests
    {
        [TestMethod]
        public void CheckDefaultInit()
        {
            DoctorFileService doctor = new DoctorFileService();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void LoadDoctors()
        {
            DoctorFileService doctor = new DoctorFileService();
            var res = doctor.LoadDoctorsFromFile();
            Assert.IsTrue(res != null && res.Any());
        }

        [TestMethod]
        public void LoadDefaultDoctor()
        {
            DoctorFileService doctor = new DoctorFileService();
            var res = doctor.LoadDefaults();
            Assert.IsTrue(res != null);
        }

        [TestMethod]
        public void SaveDoctors()
        {
            var id = 123;
            var memId = 4;

            var res = new DoctorFileService().LoadDoctorsFromFile();
            
            res.SingleOrDefault(x => x.MemberId == memId).Speciality = id;

            new DoctorFileService().SaveDoctorsToFile(res);

            res = new DoctorFileService().LoadDoctorsFromFile();

            Assert.IsTrue(res.SingleOrDefault(x => x.MemberId == memId && x.Speciality == id) != null);
        }

        [TestMethod]
        public void OpenForm()
        {
            EmkSmoSettingsForm form = new EmkSmoSettingsForm();
            form.ShowDialog();
            Assert.IsTrue(true);
        }
    }
}
