using System.Drawing;
using System.Windows.Forms;

namespace EmkConfig.Forms
{
    public sealed class EmkSettingsForm : Form
    {
        public EmkSettingsForm()
        {
            Text = "Настройки EMK";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 100);

            var editButton = new Button { Text = "Открыть настройки", AutoSize = true };
            editButton.Click += (sender, args) =>
            {
                using (var legacyForm = new Emk.Views.EmkSettingsForm())
                    legacyForm.ShowDialog(this);
            };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };
            layout.Controls.Add(editButton);
            Controls.Add(layout);
        }
    }
}
