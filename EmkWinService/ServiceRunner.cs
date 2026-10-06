using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emk.Application.Ports;
using Emk.Application.UseCases.Sending;
using Emk.Domain.Enums;

namespace EmkWinService
{
    public sealed class ServiceRunner
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);
        private readonly ISendPatientDataUseCase _sendUseCase;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ILoggerService _logger;

        public ServiceRunner(
            ISendPatientDataUseCase sendUseCase,
            ISettingsRepository settingsRepository,
            ILoggerService logger)
        {
            _sendUseCase = sendUseCase ?? throw new ArgumentNullException(nameof(sendUseCase));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await RunOnceAsync().ConfigureAwait(false);

                try
                {
                    await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public async Task RunOnceAsync()
        {
            try
            {
                var settings = await _settingsRepository.LoadSettingsAsync().ConfigureAwait(false);
                var activeSettings = settings == null ? null : settings.FirstOrDefault(setting => setting.Enabled);
                if (activeSettings == null)
                {
                    _logger.LogWarning("Нет активных настроек отправки.");
                    return;
                }

                var request = CreateRequest(activeSettings);
                if (request == null)
                    return;

                var response = await _sendUseCase.ExecuteAsync(request).ConfigureAwait(false);
                if (response.Success)
                    _logger.LogInfo($"Отправлено {response.Count} случаев: {response.Message}");
                else
                    _logger.LogWarning(response.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError("Ошибка при отправке данных пациентов", exception);
            }
        }

        private SendPatientDataRequest CreateRequest(Emk.Application.Dto.EmkSettingsDto settings)
        {
            if (settings.SendingType == SendingType.Interval)
            {
                if (settings.IntervalFrom == DateTime.MinValue || settings.IntervalTo == DateTime.MinValue)
                {
                    _logger.LogWarning("Не задан период отправки данных.");
                    return null;
                }

                return SendPatientDataRequest.ForPeriod(settings.IntervalFrom.Date, settings.IntervalTo.Date);
            }

            var endDate = DateTime.Today;
            return SendPatientDataRequest.ForPeriod(
                endDate.AddDays(-Math.Max(settings.DateInterval, 0)), endDate);
        }
    }
}
