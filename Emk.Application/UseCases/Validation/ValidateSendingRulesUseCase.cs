using System;
using System.Linq;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Domain.DomainExceptions;
using Emk.Domain.Entities;

namespace Emk.Application.UseCases.Validation
{
    public class ValidateSendingRulesUseCase : IValidateSendingRulesUseCase
    {
        private readonly ILicenseRepository _licenseRepository;
        private readonly ITreatRepository _treatRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IEmkRepository _emkRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ILoggerService _logger;
        private readonly EmkSettings _emkSettings;

        public ValidateSendingRulesUseCase(
            ILicenseRepository licenseRepository,
            ITreatRepository treatRepository,
            IPatientRepository patientRepository,
            IEmkRepository emkRepository,
            ISettingsRepository settingsRepository,
            ILoggerService logger,
            EmkSettings emkSettings = null)
        {
            _licenseRepository = licenseRepository ?? throw new ArgumentNullException(nameof(licenseRepository));
            _treatRepository = treatRepository ?? throw new ArgumentNullException(nameof(treatRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _emkRepository = emkRepository ?? throw new ArgumentNullException(nameof(emkRepository));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emkSettings = emkSettings ?? new EmkSettings();
        }

        public async Task<ValidateSendingRulesResponse> ExecuteAsync(ValidateSendingRulesRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var response = new ValidateSendingRulesResponse();

            await Check(response, CheckLicenseAsync);

            var treat = request.Treat;
            if (treat == null)
            {
                try
                {
                    var treats = request.AccountId > 0
                        ? await _treatRepository.GetByAccountIdAsync(request.AccountId)
                        : null;
                    treat = treats?.FirstOrDefault(t => request.PatientTreatId <= 0 || t.TreatId == request.PatientTreatId);
                    if (treat == null)
                        throw new NoPatientAccountsException();
                }
                catch (DomainException ex)
                {
                    response.Errors.Add(ex.Message);
                    return Complete(response, request);
                }
            }

            await Check(response, () => CheckConsentAsync(treat));
            await Check(response, () => CheckSignaturesAsync(treat));
            await Check(response, () => CheckPracticeAsync(treat));

            return Complete(response, request);
        }

        private ValidateSendingRulesResponse Complete(ValidateSendingRulesResponse response, ValidateSendingRulesRequest request)
        {
            if (!response.IsValid)
                _logger.LogWarning($"Правила отправки не выполнены (счёт {request.AccountId}): {string.Join("; ", response.Errors)}");
            return response;
        }

        private static async Task Check(ValidateSendingRulesResponse response, Func<Task> check)
        {
            try
            {
                await check();
            }
            catch (DomainException ex)
            {
                response.Errors.Add(ex.Message);
            }
        }

        private async Task CheckLicenseAsync()
        {
            if (!await _licenseRepository.IsValidAsync())
                throw new InvalidLicenseException();
        }

        private async Task CheckConsentAsync(PatientTreatDto treat)
        {
            if (!await _patientRepository.CheckConsentToShareAsync(treat.PatientId) &&
                (string.IsNullOrWhiteSpace(_emkSettings.UnknownPatientFirstName) ||
                 string.IsNullOrWhiteSpace(_emkSettings.UnknownPatientGivenName)))
                throw new PatientConsentMissingException();
        }

        private async Task CheckSignaturesAsync(PatientTreatDto treat)
        {
            if (!await _emkRepository.CheckDocumentEsignAsync(treat.AccountId))
                throw new DocumentSignatureException();
        }

        private async Task CheckPracticeAsync(PatientTreatDto treat)
        {
            var settings = await _settingsRepository.LoadSettingsAsync();
            if (settings == null || !settings.Any(s => s.Enabled && s.PracticeId == treat.PracticeId))
                throw new InvalidPracticeException();
        }
    }
}
