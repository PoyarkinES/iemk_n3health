using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emk.Services;

namespace Emk.Views
{
	public partial class EmkMainForm : Form
	{
		public EmkMainForm() => InitializeComponent();

		private async void btnSend_Click(object sender, EventArgs e)
		{
			btnSend.Enabled = false;
            btnSend.Text = "Идет отправка...";
			try
			{
				await Task.Factory.StartNew(() => new EmkSendingService().Run());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Log.Error(ex.ToString());
			}
            btnSend.Text = "Отправить данные";
            btnSend.Enabled = true;
		}

		private void btnSettings_Click(object sender, EventArgs e) => new EmkSettingsForm().ShowDialog();

		private void btnSmoSettings_Click(object sender, EventArgs e)
		{
			var form = new EmkSmoSettingsForm();
			try
			{
				form.ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Log.Error(ex.Message);
			}

		}

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            var form = new AccountFinderForm();
            if(form.ShowDialog() != DialogResult.OK)
                return;
            
            btnUpdate.Enabled = false;
            btnUpdate.Text = "Идет отправка...";
            try {
                await Task.Factory.StartNew(() => new EmkSendingService().Update(form.AccountId));
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex.ToString());
            }
            btnUpdate.Text = "Обновить по номеру счета";
            btnUpdate.Enabled = true;
        }

        private async void btnSendByAccount_Click(object sender, EventArgs e)
        {
            var form = new AccountFinderForm();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            btnSendByAccount.Enabled = false;
            btnSendByAccount.Text = "Идет отправка...";
            try
            {
                await Task.Factory.StartNew(() => new EmkSendingService().Run(form.AccountId));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex.ToString());
            }
            btnSendByAccount.Text = @"Отправить данные по номеру счета";
            btnSendByAccount.Enabled = true;
        }

        private async void btnUpdatePeriod_Click(object sender, EventArgs e)
        {
            var form = new PeriodForm();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            btnUpdatePeriod.Enabled = false;
            btnUpdatePeriod.Text = "Идет отправка...";
            try
            {
                var pats = Factory.GetTreatRepository.GetPatientAccountsAsync(form.PeriodBegin, form.PeriodEnd);
                foreach (var item in await pats)
                {
                    await Task.Factory.StartNew(() => new EmkSendingService().Update(item.AccountId));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex.ToString());
            }
            btnUpdatePeriod.Text = "Обновить за период";
            btnUpdatePeriod.Enabled = true;
        }
    }
}
