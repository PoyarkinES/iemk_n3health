using Emk.Models;
using Emk.Models.Comparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Emk.Repository.Interface;

namespace Emk.Services.Files
{
    public class DoctorFileService : IDoctorFileService
    {
        private SmoSettings _smo;
        private readonly string _smoPath;
        private readonly IDoctorRepository _doctorRepository;
        private readonly System.Threading.SemaphoreSlim _initializationLock =
            new System.Threading.SemaphoreSlim(1, 1);
        private readonly System.Threading.SemaphoreSlim _saveLock =
            new System.Threading.SemaphoreSlim(1, 1);
        private bool _initialized;

        public DoctorFileService()
            : this(Factory.GetDoctorRepository)
        {
        }

        public DoctorFileService(IDoctorRepository doctorRepository)
            : this(doctorRepository,
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SmoSettings.xml"))
        {
        }

        public DoctorFileService(IDoctorRepository doctorRepository, string settingsPath)
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _smoPath = settingsPath ?? throw new ArgumentNullException(nameof(settingsPath));
        }

        public async Task InitializeAsync()
        {
            await _initializationLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_initialized)
                    return;

                Log.Info("Загружаю данные из файла SmoSettings.xml...");
                if (File.Exists(_smoPath))
                {
                    string contents;
                    using (var stream = new FileStream(
                        _smoPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                    using (var reader = new StreamReader(stream))
                    {
                        contents = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }

                    using (var reader = new StringReader(contents))
                    {
                        var serializer = new XmlSerializer(typeof(SmoSettings));
                        _smo = (SmoSettings)serializer.Deserialize(reader);
                    }
                    _smo.Doctors ??= new List<Doctor>();
                    _smo.Default ??= new DefaultData();
                }
                else
                {
                    Log.Warning("SmoSettings.xml не найден, создаю файл по умолчанию...");
                    _smo = new SmoSettings
                    {
                        Default = new DefaultData(),
                        Doctors = (await _doctorRepository.GetDoctors().ConfigureAwait(false)).ToList()
                    };
                    SaveNewData();
                    Log.Info("SmoSettings.xml сформирован.");
                }

                _initialized = true;
            }
            finally
            {
                _initializationLock.Release();
            }
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

        public Task SaveDoctorsToFileAsync(List<Doctor> doctors)
        {
            Log.Info("Сохраняю список врачей в файл");
            return SaveDataAsync(settings => settings.Doctors = doctors);
        }

        public Task SaveDefaultsAsync(DefaultData defaults)
        {
            Log.Info("Сохраняю параметры врача по умолчанию.");
            return SaveDataAsync(settings => settings.Default = defaults);
        }

        public async Task<List<Doctor>> LoadDoctorsFromDbAsync()
        {
            var dbDocs = (await _doctorRepository.GetDoctors()).ToList();
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

        private async Task SaveDataAsync(Action<SmoSettings> update)
        {
            await InitializeAsync().ConfigureAwait(false);
            await _saveLock.WaitAsync().ConfigureAwait(false);
            var temporaryPath = _smoPath + ".tmp";
            try {
                update(_smo);
                string contents;
                using (var writer = new Utf8StringWriter())
                {
                    new XmlSerializer(typeof(SmoSettings)).Serialize(writer, _smo);
                    contents = writer.ToString();
                }

                var backupPath = _smoPath + ".bak";
                using (var stream = new FileStream(
                    temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                using (var writer = new StreamWriter(stream))
                    await writer.WriteAsync(contents).ConfigureAwait(false);

                if (File.Exists(_smoPath))
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Replace(temporaryPath, _smoPath, backupPath);
                    File.Delete(backupPath);
                }
                else
                {
                    File.Move(temporaryPath, _smoPath);
                }
            }
            finally {
                try
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                finally
                {
                    _saveLock.Release();
                }
            }
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
