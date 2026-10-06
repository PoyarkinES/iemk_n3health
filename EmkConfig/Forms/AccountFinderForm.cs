using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emk.Application.Ports;
using Emk.Application.UseCases.Updating;

namespace EmkConfig.Forms
{
    public sealed class AccountFinderForm : Form
    {
        private readonly IUpdatePatientDataUseCase _updateUseCase;
        private readonly ILoggerService _logger;
        private readonly TextBox _accountIdTextBox;
        private readonly Button _updateButton;
        private readonly Label _statusLabel;

        public int AccountId { get; private set; }
        public UpdatePatientDataResponse Response { get; private set; }

        public AccountFinderForm(IUpdatePatientDataUseCase updateUseCase, ILoggerService logger)
        {
            _updateUseCase = updateUseCase ?? throw new ArgumentNullException(nameof(updateUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Text = "Обновить счёт";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 130);

            _accountIdTextBox = new TextBox { Width = 150 };
            _updateButton = new Button { Text = "Обновить", AutoSize = true };
            _updateButton.Click += UpdateButton_Click;
            _statusLabel = new Label { AutoSize = true, MaximumSize = new Size(330, 0) };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                WrapContents = false
            };
            layout.Controls.Add(new Label { Text = "Номер счёта:", AutoSize = true });
            layout.Controls.Add(_accountIdTextBox);
            layout.Controls.Add(_updateButton);
            layout.Controls.Add(_statusLabel);
            Controls.Add(layout);
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            int accountId;
            if (!int.TryParse(_accountIdTextBox.Text, out accountId) || accountId <= 0)
            {
                _statusLabel.Text = "Введите корректный номер счёта.";
                return;
            }

            _updateButton.Enabled = false;
            _statusLabel.Text = "Идёт обновление...";
            try
            {
                AccountId = accountId;
                Response = await _updateUseCase.ExecuteAsync(new UpdatePatientDataRequest { AccountId = accountId });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                _logger.LogError("Ошибка обновления счёта " + accountId, exception);
                _statusLabel.Text = exception.Message;
                _updateButton.Enabled = true;
            }
        }
    }
}
