using System;
using System.Windows.Forms;

namespace Emk.Views
{
    public partial class AccountFinderForm : Form
    {
        public int AccountId { get; private set; }

        public AccountFinderForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtAccountId.Text, out int res))
            {
                AccountId = res;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
            MessageBox.Show("Ошибка", "Номер счета введен неверно", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtAccountId.Text = string.Empty;
        }
    }
}
