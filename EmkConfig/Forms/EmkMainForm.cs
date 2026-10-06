using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Emk.Application.Dto;
using Emk.Application.Ports;
using Emk.Application.UseCases.Sending;
using Emk.Application.UseCases.Updating;
using Emk.Domain.Enums;

namespace EmkConfig.Forms
{
    public sealed class EmkMainForm : Form
    {
        private readonly ISendPatientDataUseCase _sendUseCase;
        private readonly IUpdatePatientDataUseCase _updateUseCase;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITreatRepository _treatRepository;
        private readonly ILoggerService _logger;
        private readonly ISettingsService _applicationSettings;
        private readonly Button _sendButton;
        private readonly Button _sendPeriodButton;
        private readonly Button _sendAccountButton;
        private readonly Button _updateButton;
        private readonly Button _updatePeriodButton;
        private readonly Button _settingsButton;
        private readonly Button _smoSettingsButton;
        private readonly Label _statusLabel;

        public EmkMainForm(
            ISendPatientDataUseCase sendUseCase,
            IUpdatePatientDataUseCase updateUseCase,
            ISettingsRepository settingsRepository,
            ITreatRepository treatRepository,
            ILoggerService logger,
            ISettingsService applicationSettings)
        {
            _sendUseCase = sendUseCase ?? throw new ArgumentNullException(nameof(sendUseCase));
            _updateUseCase = updateUseCase ?? throw new ArgumentNullException(nameof(updateUseCase));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _treatRepository = treatRepository ?? throw new ArgumentNullException(nameof(treatRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));

            Text = "EMK — управление отправкой";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(480, 320);

            _sendButton = new Button { Text = "Отправить данные", AutoSize = true };
            _sendButton.Click += SendButton_Click;
            _sendPeriodButton = new Button { Text = "Отправить за период", AutoSize = true };
            _sendPeriodButton.Click += SendPeriodButton_Click;
            _sendAccountButton = new Button { Text = "Отправить по номеру счёта", AutoSize = true };
            _sendAccountButton.Click += SendAccountButton_Click;
            _updateButton = new Button { Text = "Обновить счёт", AutoSize = true };
            _updateButton.Click += UpdateButton_Click;
            _updatePeriodButton = new Button { Text = "Обновить за период", AutoSize = true };
            _updatePeriodButton.Click += UpdatePeriodButton_Click;
            _settingsButton = new Button { Text = "Настройки", AutoSize = true };
            _settingsButton.Click += SettingsButton_Click;
            _smoSettingsButton = new Button { Text = "Настройки СМО", AutoSize = true };
            _smoSettingsButton.Click += SmoSettingsButton_Click;
            _statusLabel = new Label { AutoSize = true, MaximumSize = new Size(440, 0), Text = "Готово" };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            layout.Controls.Add(_sendButton);
            layout.Controls.Add(_sendPeriodButton);
            layout.Controls.Add(_sendAccountButton);
            layout.Controls.Add(_updateButton);
            layout.Controls.Add(_updatePeriodButton);
            layout.Controls.Add(_settingsButton);
            layout.Controls.Add(_smoSettingsButton);
            layout.Controls.Add(_statusLabel);
            Controls.Add(layout);
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            SetBusy(true, "Идёт отправка...");
            try
            {
                var settings = await _settingsRepository.LoadSettingsAsync();
                var activeSettings = settings == null ? null : settings.FirstOrDefault(setting => setting.Enabled);
                if (activeSettings == null)
                {
                    ShowResult(false, "Нет активных настроек отправки.");
                    return;
                }

                var request = CreateRequest(activeSettings);
                if (request == null)
                    return;

                var response = await _sendUseCase.ExecuteAsync(request);
                ShowResult(response.Success, response.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError("Ошибка при отправке данных пациентов", exception);
                ShowResult(false, exception.Message);
            }
            finally
            {
                SetBusy(false, _statusLabel.Text);
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            using (var form = new AccountFinderForm(_updateUseCase, _logger))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                    ShowResult(form.Response.Success, form.Response.Message);
            }
        }

        private async void SendPeriodButton_Click(object sender, EventArgs e)
        {
            using (var periodForm = new Emk.Views.PeriodForm())
            {
                periodForm.Text = "Отправить данные за период";
                if (periodForm.ShowDialog(this) != DialogResult.OK)
                    return;

                SetBusy(true, "Идёт отправка...");
                try
                {
                    var request = SendPatientDataRequest.ForPeriod(
                        periodForm.PeriodBegin.Date, periodForm.PeriodEnd.Date);
                    var response = await _sendUseCase.ExecuteAsync(request);
                    ShowResult(response.Success, response.Message);
                }
                catch (Exception exception)
                {
                    _logger.LogError("Ошибка отправки данных за период", exception);
                    ShowResult(false, exception.Message);
                }
                finally
                {
                    SetBusy(false, _statusLabel.Text);
                }
            }
        }

        private async void SendAccountButton_Click(object sender, EventArgs e)
        {
            using (var form = new AccountIdForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                SetBusy(true, "Идёт отправка...");
                try
                {
                    var response = await _sendUseCase.ExecuteAsync(
                        new SendPatientDataRequest { AccountId = form.AccountId });
                    ShowResult(response.Success, response.Message);
                }
                catch (Exception exception)
                {
                    _logger.LogError("Ошибка отправки счёта " + form.AccountId, exception);
                    ShowResult(false, exception.Message);
                }
                finally
                {
                    SetBusy(false, _statusLabel.Text);
                }
            }
        }

        private async void UpdatePeriodButton_Click(object sender, EventArgs e)
        {
            using (var periodForm = new Emk.Views.PeriodForm())
            {
                if (periodForm.ShowDialog(this) != DialogResult.OK)
                    return;

                SetBusy(true, "Идёт обновление...");
                try
                {
                    var treats = await _treatRepository.GetByPeriodAsync(
                        periodForm.PeriodBegin, periodForm.PeriodEnd);
                    var updated = 0;
                    var failed = 0;
                    foreach (var treat in treats)
                    {
                        var response = await _updateUseCase.ExecuteAsync(
                            new UpdatePatientDataRequest { AccountId = treat.AccountId });
                        if (response.Success)
                            updated++;
                        else
                            failed++;
                    }

                    ShowResult(failed == 0, $"Обновлено: {updated}, ошибок: {failed}");
                }
                catch (Exception exception)
                {
                    _logger.LogError("Ошибка обновления данных за период", exception);
                    ShowResult(false, exception.Message);
                }
                finally
                {
                    SetBusy(false, _statusLabel.Text);
                }
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (var form = new EmkSettingsForm(_applicationSettings))
                form.ShowDialog(this);
        }

        private void SmoSettingsButton_Click(object sender, EventArgs e)
        {
            using (var form = new Emk.Views.EmkSmoSettingsForm())
                form.ShowDialog(this);
        }

        private SendPatientDataRequest CreateRequest(EmkSettingsDto settings)
        {
            if (settings.SendingType == SendingType.Interval)
            {
                if (settings.IntervalFrom == DateTime.MinValue || settings.IntervalTo == DateTime.MinValue)
                {
                    ShowResult(false, "Не задан период отправки данных.");
                    return null;
                }

                return SendPatientDataRequest.ForPeriod(settings.IntervalFrom.Date, settings.IntervalTo.Date);
            }

            var endDate = DateTime.Today;
            return SendPatientDataRequest.ForPeriod(
                endDate.AddDays(-Math.Max(settings.DateInterval, 0)), endDate);
        }

        private void SetBusy(bool isBusy, string status)
        {
            _sendButton.Enabled = !isBusy;
            _sendPeriodButton.Enabled = !isBusy;
            _sendAccountButton.Enabled = !isBusy;
            _updateButton.Enabled = !isBusy;
            _updatePeriodButton.Enabled = !isBusy;
            _settingsButton.Enabled = !isBusy;
            _smoSettingsButton.Enabled = !isBusy;
            _statusLabel.Text = status;
        }

        private void ShowResult(bool success, string message)
        {
            _statusLabel.ForeColor = success ? Color.DarkGreen : Color.DarkRed;
            _statusLabel.Text = message ?? string.Empty;
        }
    }
}
