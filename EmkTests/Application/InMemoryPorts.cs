using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;

namespace EmkTests.Application
{
    internal class InMemoryLicenseRepository : ILicenseRepository
    {
        public bool Valid { get; set; } = true;
        public Task<bool> IsValidAsync() => Task.FromResult(Valid);
    }

    internal class InMemoryPatientRepository : IPatientRepository
    {
        public bool Consent { get; set; } = true;
        public Task<PatientDto> GetByIdAsync(int patientId) => Task.FromResult(new PatientDto { PatientId = patientId });
        public Task<PatientDto> GetByCardNumberAsync(string cardNumber) => Task.FromResult(new PatientDto { CartNum = cardNumber });
        public Task<bool> CheckConsentToShareAsync(int patientId) => Task.FromResult(Consent);
    }

    internal class InMemoryTreatRepository : ITreatRepository
    {
        public List<PatientTreatDto> Treats { get; } = new List<PatientTreatDto>();
        public List<int> UpdatedEsignAccounts { get; } = new List<int>();

        public Task<List<PatientTreatDto>> GetByPeriodAsync(DateTime start, DateTime end) =>
            Task.FromResult(Treats.Where(t => t.TreatDate >= start && t.TreatDate <= end).ToList());

        public Task<List<PatientTreatDto>> GetByAccountIdAsync(int accountId) =>
            Task.FromResult(Treats.Where(t => t.AccountId == accountId).ToList());

        public Task UpdateEsignFilesAsync(PatientTreatDto treat)
        {
            UpdatedEsignAccounts.Add(treat.AccountId);
            return Task.FromResult(0);
        }
    }

    internal class InMemoryEmkRepository : IEmkRepository
    {
        public bool Signed { get; set; } = true;
        public Task<List<DocumentDto>> GetDocumentsByAccountIdAsync(int accountId) => Task.FromResult(new List<DocumentDto>());
        public Task<bool> CheckDocumentEsignAsync(int accountId) => Task.FromResult(Signed);
    }

    internal class InMemorySettingsRepository : ISettingsRepository
    {
        public List<EmkSettingsDto> Settings { get; } = new List<EmkSettingsDto>
        {
            new EmkSettingsDto { PracticeId = 1, Enabled = true }
        };

        public Task<List<EmkSettingsDto>> LoadSettingsAsync(bool force = false) => Task.FromResult(Settings);
    }

    internal class InMemoryPixClient : IPixClient
    {
        public bool Result { get; set; } = true;
        public List<int> Added { get; } = new List<int>();
        public List<int> Updated { get; } = new List<int>();

        public Task<bool> AddPatientAsync(PatientAccountDto patient)
        {
            Added.Add(patient.AccountId);
            return Task.FromResult(Result);
        }

        public Task<bool> UpdatePatientAsync(PatientAccountDto patient)
        {
            Updated.Add(patient.AccountId);
            return Task.FromResult(Result);
        }
    }

    internal class InMemoryEmkClient : IEmkClient
    {
        public int Result { get; set; } = 100;
        public List<int> Added { get; } = new List<int>();
        public List<int> Updated { get; } = new List<int>();

        public Task<int> AddCaseAsync(PatientTreatDto treat, bool isUpdate = false)
        {
            Added.Add(treat.AccountId);
            return Task.FromResult(Result);
        }

        public Task<int> UpdateCaseAsync(PatientTreatDto treat)
        {
            Updated.Add(treat.AccountId);
            return Task.FromResult(Result);
        }
    }

    internal class InMemoryLogger : ILoggerService
    {
        public List<string> Messages { get; } = new List<string>();
        public void LogInfo(string message) => Messages.Add("I: " + message);
        public void LogWarning(string message) => Messages.Add("W: " + message);
        public void LogError(string message, Exception ex = null) => Messages.Add("E: " + message);
    }
}
