using Emk.Models;
using Emk.Models.Comparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Emk.Services.Files
{
    public class DoctorFileService : IDoctorFileService
    {
        private SmoSettings _smo;
        private string _smoPath;

        public DoctorFileService()
        {
            Log.Info("Загружаю данные из файла SmoSettings.xml...");
            _smoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SmoSettings.xml");
            if (!File.Exists(_smoPath))
                CreateDefaultSmo();       
            using(FileStream fs = new FileStream(_smoPath, FileMode.OpenOrCreate)) {
                XmlSerializer xml = new XmlSerializer(typeof(SmoSettings));
                _smo = (SmoSettings)xml.Deserialize(fs);
            }
        }

        private void CreateDefaultSmo()
        {
            Log.Warning("SmoSettings.xml не найден, создаю файл по умолчанию...");
			_smo = new SmoSettings
			{
				Default = new DefaultData(),
				Doctors = Factory.GetDoctorRepository.GetDoctors().ToList()
			};
			XmlSerializer xml = new XmlSerializer(typeof(SmoSettings));
            using (FileStream fs = new FileStream(_smoPath, FileMode.OpenOrCreate)) {
                xml.Serialize(fs, _smo);
            }
            Log.Info("SmoSettings.xml сформирован.");
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

        public List<Doctor> LoadDoctorsFromDb()
        {
            var dbDocs = Factory.GetDoctorRepository.GetDoctors().ToList();
            var diffs = dbDocs.Except(_smo.Doctors, new DoctorComparer()).ToList();
            if (diffs.Any()) {
                UpdateDoctors(diffs);
            }
                
            return _smo.Doctors;
        }

        private void UpdateDoctors(List<Doctor> diffs)
        {
            List<Doctor> newDocs = new List<Doctor>();
            foreach (var i in diffs) {
                var o = _smo.Doctors.Single(x => x.MemberId == i.MemberId);
                if (o is null) {
                    newDocs.Add(i);
                    continue;
                }
                _smo.Doctors[_smo.Doctors.IndexOf(o)] = i;
            }
            if (newDocs.Any())
                _smo.Doctors.AddRange(newDocs);
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
