﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emk.Services;

namespace Emk.Views
{
	public partial class EmkMainForm : Form
	{
		public EmkMainForm() => InitializeComponent();

        /// <summary>
        /// Отправка данных в ФСС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private async void btnSend_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(exception.ToString());
            }
        }

        /// <summary>
        /// Открытие формы настроек обмена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void btnSettings_Click(object sender, EventArgs e) => new EmkSettingsForm().ShowDialog();

        /// <summary>
        /// Открытие формы настроек СМО
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
				Log.Error(ex.ToString());
			}

		}

        /// <summary>
        /// Обновление данных по номеру счета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(exception.ToString());
            }
        }

        private async void btnSendByAccount_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(exception.ToString());
            }
        }

        /// <summary>
        /// Обновление данных за период
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUpdatePeriod_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new PeriodForm();
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                btnUpdatePeriod.Enabled = false;
                btnUpdatePeriod.Text = "Идет отправка...";
                try
                {
                    var pats = Factory.GetTreatRepository.GetPatientAccounts(form.PeriodBegin, form.PeriodEnd);
                    foreach (var item in pats)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex.ToString());
            }
        }
        
        /// <summary>
        /// Отправка данных за период
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSendPeriod_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new PeriodForm();
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                btnSendPeriod.Enabled = false;
                btnSendPeriod.Text = "Идет отправка...";
                try
                {
                    await Task.Factory.StartNew(() => new EmkSendingService().Run(form.PeriodBegin, form.PeriodEnd));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.Error(ex.ToString());
                }
                btnSendPeriod.Text = "Отправить данные за период";  
                btnSendPeriod.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error(ex.ToString());
            }
        }
    }
}
