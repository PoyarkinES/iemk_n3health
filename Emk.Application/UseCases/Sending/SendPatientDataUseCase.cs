using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Application.UseCases.Validation;
using Emk.Domain.DomainExceptions;

namespace Emk.Application.UseCases.Sending
{
    public class SendPatientDataUseCase : ISendPatientDataUseCase
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ILicenseRepository _licenseRepository;
        private readonly ITreatRepository _treatRepository;
        private readonly IValidateSendingRulesUseCase _validateUseCase;
        private readonly IPixClient _pixClient;
        private readonly IEmkClient _emkClient;
        private readonly ILoggerService _logger;
        private readonly SendPatientDataValidator _validator = new SendPatientDataValidator();

        public SendPatientDataUseCase(
            ISettingsRepository settingsRepository,
            ILicenseRepository licenseRepository,
            ITreatRepository treatRepository,
            IValidateSendingRulesUseCase validateUseCase,
            IPixClient pixClient,
            IEmkClient emkClient,
            ILoggerService logger)
        {
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _licenseRepository = licenseRepository ?? throw new ArgumentNullException(nameof(licenseRepository));
            _treatRepository = treatRepository ?? throw new ArgumentNullException(nameof(treatRepository));
            _validateUseCase = validateUseCase ?? throw new ArgumentNullException(nameof(validateUseCase));
            _pixClient = pixClient ?? throw new ArgumentNullException(nameof(pixClient));
            _emkClient = emkClient ?? throw new ArgumentNullException(nameof(emkClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SendPatientDataResponse> ExecuteAsync(SendPatientDataRequest request)
        {
            var errors = _validator.Validate(request);
            if (errors.Count > 0)
                return Fail(string.Join(" ", errors));

            try
            {
                var settings = await _settingsRepository.LoadSettingsAsync();
                if (settings == null || !settings.Any(s => s.Enabled))
                    return Fail("Нет активных настроек отправки.");

                if (!await _licenseRepository.IsValidAsync())
                    return Fail(new InvalidLicenseException().Message);

                var treats = request.AccountId > 0
                    ? await _treatRepository.GetByAccountIdAsync(request.AccountId)
                    : await _treatRepository.GetByPeriodAsync(request.StartDate.Value, request.EndDate.Value);

                if (treats == null || treats.Count == 0)
                    return Fail(new NoPatientAccountsException().Message);

                return await SendAsync(treats);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при отправке данных пациентов", ex);
                return Fail("Внутренняя ошибка при отправке данных. Подробности в журнале.");
            }
        }

        private async Task<SendPatientDataResponse> SendAsync(List<PatientTreatDto> treats)
        {
            int sent = 0, skipped = 0, failed = 0;

            foreach (var treat in treats)
            {
                var validation = await _validateUseCase.ExecuteAsync(new ValidateSendingRulesRequest
                {
                    AccountId = treat.AccountId,
                    PatientTreatId = treat.TreatId,
                    Treat = treat
                });

                if (!validation.IsValid)
                {
                    skipped++;
                    _logger.LogWarning($"Счёт {treat.AccountId} пропущен: {string.Join("; ", validation.Errors)}");
                    continue;
                }

                try
                {
                    if (await SendTreatAsync(treat))
                        sent++;
                    else
                        failed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogError($"Ошибка отправки счёта {treat.AccountId}", ex);
                }
            }

            _logger.LogInfo($"Отправка завершена: отправлено {sent}, пропущено {skipped}, ошибок {failed}");
            return new SendPatientDataResponse
            {
                Success = failed == 0,
                Count = sent,
                Message = $"Отправлено: {sent}, пропущено: {skipped}, ошибок: {failed}"
            };
        }

        private async Task<bool> SendTreatAsync(PatientTreatDto treat)
        {
            if (!await _pixClient.AddPatientAsync(PatientAccountFactory.Create(treat)))
            {
                _logger.LogWarning($"PIX отклонил пациента по счёту {treat.AccountId}");
                return false;
            }

            if (await _emkClient.AddCaseAsync(treat) <= 0)
            {
                _logger.LogWarning($"EMK не создал случай по счёту {treat.AccountId}");
                return false;
            }

            await _treatRepository.UpdateEsignFilesAsync(treat);
            return true;
        }

        private SendPatientDataResponse Fail(string message)
        {
            _logger.LogWarning(message);
            return new SendPatientDataResponse { Success = false, Message = message, Count = 0 };
        }
    }
}
