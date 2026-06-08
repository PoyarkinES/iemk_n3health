using Emk.Models;
using Emk.Services;
using System;
using System.Windows.Forms;

namespace Emk.Views
{
	public partial class EmkSettingsForm : Form
	{
		private ISettingsService _srv;
		private EmkSettings _settings;

		public EmkSettingsForm()
		{
			InitializeComponent();
			_srv = Factory.GetSettingsService;
			LoadSettings();
			rbInterval.CheckedChanged += SetRadio;
			rbDaysToNow.CheckedChanged += SetRadio;
		}


		private void LoadSettings()
		{
			_settings = _srv.LoadSettings();
			txtPatientDir.Text = _settings.PatientDirectory;
			txtDBConnStr.Text = _settings.DbConnectionString;
			txtEmkUri.Text = _settings.EmkUrl;
			txtPixUri.Text = _settings.PixUrl;
			txtGuid.Text = _settings.Guid.ToString();
			txtIdLPU.Text = _settings.IdLPU.ToString();
			txtInterval.Text = _settings.DateInterval.ToString();
			txtTime.Text = _settings.UpdateTime.ToString();
			dtPickerFrom.Value = _settings.IntervalFrom == DateTime.MinValue ? DateTime.Now : _settings.IntervalFrom;
			dtPickerTo.Value = _settings.IntervalTo == DateTime.MinValue ? DateTime.Now : _settings.IntervalTo;
			rbInterval.Checked = _settings.SendingType == SendingType.Interval;
			SetRadio();

		}

		private void SaveSettings()
		{
			_settings.PatientDirectory = txtPatientDir.Text;
			_settings.DbConnectionString = txtDBConnStr.Text;
			_settings.EmkUrl = txtEmkUri.Text;
			_settings.PixUrl = txtPixUri.Text;
			_settings.Guid = Guid.Parse(txtGuid.Text);
			_settings.IdLPU = Guid.Parse(txtIdLPU.Text);
			_settings.DateInterval = int.Parse(txtInterval.Text);
			TimeSpan.TryParse(txtTime.Text, out var r);
			_settings.UpdateTime = r;
			_settings.SendingType = rbDaysToNow.Checked ? SendingType.DaysBeforeNow : SendingType.Interval;
			_settings.IntervalFrom = dtPickerFrom.Value.Date;
			_settings.IntervalTo = dtPickerTo.Value.Date;
		}

		private void btnChoosePatintDir_Click(object sender, EventArgs e)
		{
			var fd = new FolderBrowserDialog();
			fd.ShowDialog();
			txtPatientDir.Text = fd.SelectedPath;
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			SaveSettings();
			_srv.SaveSettings(_settings);
			Factory.LoadSettings(true);
			MessageBox.Show("Настройки сохранены", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Close();
		}

		private void SetRadio(object sender = null, EventArgs e = null)
		{

			if(rbDaysToNow.Checked) {
				dtPickerFrom.Enabled = false;
				dtPickerTo.Enabled = false;
				txtInterval.Enabled = true;
			}
			else {
				dtPickerFrom.Enabled = true;
				dtPickerTo.Enabled = true;
				txtInterval.Enabled = false;
			}
		}
	}
}
