using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Application.UseCases.Sending;
using Emk.Domain.Enums;
using EmkWinService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmkTests
{
    [TestClass]
    public class EmkWinServiceTests
    {
        [TestMethod]
        public async Task RunOnceAsync_LogsSuccessfulSend()
        {
            var useCase = new CapturingSendUseCase();
            var logger = new CapturingLogger();
            var settings = new TestSettingsRepository(new EmkSettingsDto
            {
                Enabled = true,
                DateInterval = 3
            });
            var runner = new ServiceRunner(useCase, settings, logger);

            await runner.RunOnceAsync();

            Assert.AreEqual(DateTime.Today.AddDays(-3), useCase.Request.StartDate.Value);
            Assert.AreEqual(DateTime.Today, useCase.Request.EndDate.Value);
            Assert.IsTrue(logger.Infos.Exists(message => message.Contains("Отправлено 2 случаев")));
        }

        [TestMethod]
        public async Task RunOnceAsync_LogsUseCaseExceptions()
        {
            var logger = new CapturingLogger();
            var runner = new ServiceRunner(
                new ThrowingSendUseCase(),
                new TestSettingsRepository(new EmkSettingsDto { Enabled = true }),
                logger);

            await runner.RunOnceAsync();

            Assert.AreEqual(1, logger.Errors.Count);
            Assert.AreEqual("Ошибка при отправке данных пациентов", logger.Errors[0]);
        }

        [TestMethod]
        public async Task RunOnceAsync_UsesConfiguredDateInterval()
        {
            var from = new DateTime(2024, 1, 2, 10, 0, 0);
            var to = new DateTime(2024, 1, 5, 18, 0, 0);
            var useCase = new CapturingSendUseCase();
            var runner = new ServiceRunner(
                useCase,
                new TestSettingsRepository(new EmkSettingsDto
                {
                    Enabled = true,
                    SendingType = SendingType.Interval,
                    IntervalFrom = from,
                    IntervalTo = to
                }),
                new CapturingLogger());

            await runner.RunOnceAsync();

            Assert.AreEqual(from.Date, useCase.Request.StartDate.Value);
            Assert.AreEqual(to.Date, useCase.Request.EndDate.Value);
        }

        private sealed class CapturingSendUseCase : ISendPatientDataUseCase
        {
            public SendPatientDataRequest Request { get; private set; }

            public Task<SendPatientDataResponse> ExecuteAsync(SendPatientDataRequest request)
            {
                Request = request;
                return Task.FromResult(new SendPatientDataResponse
                {
                    Success = true,
                    Count = 2,
                    Message = "Отправка завершена"
                });
            }
        }

        private sealed class ThrowingSendUseCase : ISendPatientDataUseCase
        {
            public Task<SendPatientDataResponse> ExecuteAsync(SendPatientDataRequest request)
            {
                throw new InvalidOperationException("failure");
            }
        }

        private sealed class TestSettingsRepository : ISettingsRepository
        {
            private readonly List<EmkSettingsDto> _settings;

            public TestSettingsRepository(EmkSettingsDto settings)
            {
                _settings = new List<EmkSettingsDto> { settings };
            }

            public Task<List<EmkSettingsDto>> LoadSettingsAsync(bool force = false)
            {
                return Task.FromResult(_settings);
            }
        }

        private sealed class CapturingLogger : ILoggerService
        {
            public List<string> Infos { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();

            public void LogInfo(string message) => Infos.Add(message);
            public void LogWarning(string message) { }
            public void LogError(string message, Exception exception = null) => Errors.Add(message);
        }
    }
}
