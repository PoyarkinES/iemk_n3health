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
			try
			{
				await Task.Factory.StartNew(() => new EmkSendingService().Run());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Log.Error(ex.Message);
			}
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
	}
}
