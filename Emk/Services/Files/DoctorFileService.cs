using Emk.Models;
using Emk.Models.Comparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Emk.Services.Files
{
    public class DoctorFileService : IDoctorFileService
    {
        private SmoSettings _smo;
        private string _smoPath;
        private bool _needsInitialLoad;

        public DoctorFileService()
        {
            Log.Info("Загружаю данные из файла SmoSettings.xml...");
            _smoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SmoSettings.xml");
            _needsInitialLoad = !File.Exists(_smoPath);
            if (_needsInitialLoad)
            {
                _smo = new SmoSettings
                {
                    Default = new DefaultData(),
                    Doctors = new List<Doctor>()
                };
                return;
            }

            using (FileStream fs = new FileStream(_smoPath, FileMode.Open, FileAccess.Read)) {
                XmlSerializer xml = new XmlSerializer(typeof(SmoSettings));
                _smo = (SmoSettings)xml.Deserialize(fs);
            }
            _smo.Doctors ??= new List<Doctor>();
            _smo.Default ??= new DefaultData();
        }

        public async Task InitializeAsync()
        {
            if (!_needsInitialLoad)
                return;

            Log.Warning("SmoSettings.xml не найден, создаю файл по умолчанию...");
            _smo.Doctors = (await Factory.GetDoctorRepository.GetDoctors()).ToList();
            SaveNewData();
            _needsInitialLoad = false;
            Log.Info("SmoSettings.xml сформирован.");
        }

        private void SaveNewData()
        {
            XmlSerializer xml = new XmlSerializer(typeof(SmoSettings));
            using (FileStream fs = new FileStream(_smoPath, FileMode.Create, FileAccess.Write)) {
                xml.Serialize(fs, _smo);
            }
        }

        public List<Doctor> LoadDoctorsFromFile()
        {
            Log.Info("Возвращаю список врачей из файла.");
            return _smo.Doctors; 
        }

        public DefaultData LoadDefaults()
        {
            Log.Info("Возвращаю параметры по умолчанию.");
            return _smo.Default;
        }

        public void SaveDoctorsToFile(List<Doctor> d)
        {
            Log.Info("Сохраняю список врачей в файл");
            _smo.Doctors = d;
            SaveData();
        }

        public void SaveDefaults(DefaultData d)
        {
            Log.Info("Сохраняю параметры врача по умолчанию.");
            _smo.Default = d;
            SaveData();
        }

        public async Task<List<Doctor>> LoadDoctorsFromDbAsync()
        {
            var dbDocs = (await Factory.GetDoctorRepository.GetDoctors()).ToList();
            foreach (var doctor in dbDocs)
            {
                var existing = _smo.Doctors.SingleOrDefault(x => x.MemberId == doctor.MemberId);
                if (existing == null)
                    _smo.Doctors.Add(doctor);
                else if (!new DoctorComparer().Equals(existing, doctor))
                    _smo.Doctors[_smo.Doctors.IndexOf(existing)] = doctor;
            }

            return _smo.Doctors;
        }

        private void SaveData()
        {
            string bak = Path.Combine(Path.GetDirectoryName(_smoPath), "SmoSettings.xml.bak");
            File.Move(_smoPath, bak);
            try {
                XmlSerializer xml = new XmlSerializer(typeof(SmoSettings));
                using (FileStream fs = new FileStream(_smoPath, FileMode.OpenOrCreate)) {
                    xml.Serialize(fs, _smo);
                }
            }
            catch(Exception e) {
                if (File.Exists(_smoPath))
                    File.Delete(_smoPath);
                File.Move(bak, _smoPath);
            }
            finally {
                if (File.Exists(bak))
                    File.Delete(bak);
            }
        }
    }
}
