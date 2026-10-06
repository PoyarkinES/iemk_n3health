using System.Drawing;
using System.Windows.Forms;

namespace EmkConfig.Forms
{
    public sealed class AccountIdForm : Form
    {
        private readonly TextBox _accountIdTextBox;

        public int AccountId { get; private set; }

        public AccountIdForm()
        {
            Text = "Отправить счёт";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(320, 100);

            _accountIdTextBox = new TextBox { Width = 140 };
            var submitButton = new Button { Text = "Продолжить", AutoSize = true };
            submitButton.Click += (sender, args) =>
            {
                int accountId;
                if (!int.TryParse(_accountIdTextBox.Text, out accountId) || accountId <= 0)
                {
                    MessageBox.Show("Введите корректный номер счёта.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AccountId = accountId;
                DialogResult = DialogResult.OK;
                Close();
            };

            var layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                WrapContents = false
            };
            layout.Controls.Add(new Label { Text = "Номер счёта:", AutoSize = true });
            layout.Controls.Add(_accountIdTextBox);
            layout.Controls.Add(submitButton);
            Controls.Add(layout);
        }
    }
}
