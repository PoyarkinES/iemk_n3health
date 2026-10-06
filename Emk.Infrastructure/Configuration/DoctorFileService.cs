using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Domain.Entities;
using Emk.Infrastructure.Xml;

namespace Emk.Infrastructure.Configuration
{
    public sealed class DoctorFileService : IDoctorFileService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly string _settingsPath;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private SmoSettingsDocument _settings;

        public DoctorFileService(IDoctorRepository doctorRepository)
            : this(doctorRepository, Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SmoSettings.xml"))
        {
        }

        public DoctorFileService(IDoctorRepository doctorRepository, string settingsPath)
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _settingsPath = settingsPath ?? throw new ArgumentNullException(nameof(settingsPath));
        }

        public async Task InitializeAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_settings != null)
                    return;

                if (File.Exists(_settingsPath))
                {
                    using (var reader = new StreamReader(_settingsPath))
                        _settings = SmoSettingsXmlMapper.FromXml(await reader.ReadToEndAsync().ConfigureAwait(false));
                }
                else
                {
                    var doctors = await _doctorRepository.GetAllAsync().ConfigureAwait(false);
                    _settings = new SmoSettingsDocument
                    {
                        Doctors = doctors.Select(ToEntity).ToList(),
                        Default = new DefaultData()
                    };
                    await SaveAsync().ConfigureAwait(false);
                }

                _settings.Doctors = _settings.Doctors ?? new List<Doctor>();
                _settings.Default = _settings.Default ?? new DefaultData();
            }
            finally
            {
                _lock.Release();
            }
        }

        public List<Doctor> LoadDoctorsFromFile()
        {
            EnsureInitialized();
            return _settings.Doctors;
        }

        public DefaultData LoadDefaults()
        {
            EnsureInitialized();
            return _settings.Default;
        }

        public async Task<List<Doctor>> LoadDoctorsFromDbAsync()
        {
            await InitializeAsync().ConfigureAwait(false);
            var doctors = (await _doctorRepository.GetAllAsync().ConfigureAwait(false)).Select(ToEntity).ToList();
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                foreach (var doctor in doctors)
                {
                    var index = _settings.Doctors.FindIndex(existing => existing.MemberId == doctor.MemberId);
                    if (index < 0)
                        _settings.Doctors.Add(doctor);
                    else
                        _settings.Doctors[index] = doctor;
                }

                await SaveAsync().ConfigureAwait(false);
                return _settings.Doctors;
            }
            finally
            {
                _lock.Release();
            }
        }

        public Task SaveDefaultsAsync(DefaultData defaults)
        {
            if (defaults == null)
                throw new ArgumentNullException(nameof(defaults));
            return UpdateAsync(settings => settings.Default = defaults);
        }

        public Task SaveDoctorsToFileAsync(List<Doctor> doctors)
        {
            if (doctors == null)
                throw new ArgumentNullException(nameof(doctors));
            return UpdateAsync(settings => settings.Doctors = doctors);
        }

        private async Task UpdateAsync(Action<SmoSettingsDocument> update)
        {
            await InitializeAsync().ConfigureAwait(false);
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                update(_settings);
                await SaveAsync().ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var temporaryPath = _settingsPath + ".tmp";
            try
            {
                using (var writer = new StreamWriter(temporaryPath, false, Encoding.UTF8))
                    await writer.WriteAsync(SmoSettingsXmlMapper.ToXml(_settings)).ConfigureAwait(false);
                if (File.Exists(_settingsPath))
                    File.Replace(temporaryPath, _settingsPath, null);
                else
                    File.Move(temporaryPath, _settingsPath);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        private void EnsureInitialized()
        {
            if (_settings == null)
                throw new InvalidOperationException("InitializeAsync must be called before loading doctor settings.");
        }

        private static Doctor ToEntity(DoctorDto doctor)
        {
            return new Doctor
            {
                MemberId = doctor.MemberId,
                PersCode = doctor.PersCode,
                Surname = doctor.Surname,
                Name = doctor.Name,
                MiddleName = doctor.MiddleName
            };
        }
    }
}
