using System;
using System.Drawing;
using System.Windows.Forms;
using Emk.Application.Ports;

namespace EmkConfig.Forms
{
    public sealed class EmkSettingsForm : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly TextBox _unknownPatientFirstName;
        private readonly TextBox _unknownPatientGivenName;

        public EmkSettingsForm(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            Text = "Настройки EMK";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(440, 190);

            var settings = _settingsService.LoadSettings();
            _unknownPatientFirstName = new TextBox
            {
                Text = settings.UnknownPatientFirstName ?? string.Empty,
                Width = 260
            };
            _unknownPatientGivenName = new TextBox
            {
                Text = settings.UnknownPatientGivenName ?? string.Empty,
                Width = 260
            };

            var editButton = new Button { Text = "Открыть настройки", AutoSize = true };
            editButton.Click += (sender, args) =>
            {
                using (var legacyForm = new Emk.Views.EmkSettingsForm())
                    legacyForm.ShowDialog(this);
            };
            var saveButton = new Button { Text = "Сохранить", AutoSize = true };
            saveButton.Click += SaveButton_Click;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true
            };
            layout.Controls.Add(new Label { Text = "Фамилия анонимного пациента", AutoSize = true }, 0, 0);
            layout.Controls.Add(_unknownPatientFirstName, 1, 0);
            layout.Controls.Add(new Label { Text = "Имя анонимного пациента", AutoSize = true }, 0, 1);
            layout.Controls.Add(_unknownPatientGivenName, 1, 1);
            layout.Controls.Add(editButton, 0, 2);
            layout.Controls.Add(saveButton, 1, 2);
            Controls.Add(layout);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                settings.UnknownPatientFirstName = _unknownPatientFirstName.Text;
                settings.UnknownPatientGivenName = _unknownPatientGivenName.Text;
                _settingsService.SaveSettings(settings);
                MessageBox.Show(
                    this,
                    "Настройки сохранены. Перезапустите приложение или службу, чтобы применить их.",
                    "Настройки EMK",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
