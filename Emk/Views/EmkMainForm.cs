using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emk.Services;

namespace Emk.Views
{
	public partial class EmkMainForm : Form
	{
		public EmkMainForm() => InitializeComponent();

        /// <summary>
        /// Asynchronously sends data by executing EmkSendingService.Run on a background task, updates the send button
        /// text and enabled state, and handles exceptions by showing a message box and logging errors.
        /// </summary>
        /// <remarks>Modifies btnSend.Text and btnSend.Enabled, shows error message boxes, and logs
        /// exceptions. Runs the send operation on a background thread to avoid blocking the UI.</remarks>
        /// <param name="sender">The control that raised the click event.</param>
        /// <param name="e">Event arguments for the click event.</param>
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
        /// Opens the settings dialog.
        /// </summary>
        /// <remarks>Opens EmkSettingsForm as a modal dialog.</remarks>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">Event data.</param>
		private void btnSettings_Click(object sender, EventArgs e) => new EmkSettingsForm().ShowDialog();

        /// <summary>
        /// Opens the SMO settings dialog and displays an error message if the dialog fails to open.
        /// </summary>
        /// <remarks>Shows EmkSmoSettingsForm as a modal dialog. If an exception occurs, displays an error
        /// MessageBox and logs the exception.</remarks>
        /// <param name="sender">The control that raised the click event.</param>
        /// <param name="e">Event data for the click event.</param>
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
        /// Opens an account finder dialog; if an account is selected, disables the update button, invokes
        /// EmkSendingService.Update asynchronously for the selected account, and restores the button text and enabled
        /// state. Errors are presented to the user and logged.
        /// </summary>
        /// <remarks>Performs the long-running update on a background task and updates UI state before and
        /// after the operation. Exceptions are handled by showing a MessageBox and writing to the log.</remarks>
        /// <param name="sender">The control that raised the click event.</param>
        /// <param name="e">Event data for the click event.</param>
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

        /// <summary>
        /// Open an account selector and send data for the selected account asynchronously, updating the button state
        /// and handling exceptions.
        /// </summary>
        /// <remarks>Displays AccountFinderForm modally; disables the send button and updates its text
        /// while sending; runs EmkSendingService.Run(accountId) on a background task; shows message boxes and logs any
        /// exceptions.</remarks>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">The event data.</param>
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
        /// Updates patient accounts for a selected period by sending asynchronous update requests to the EMK service
        /// and temporarily disabling the update button while the operation runs.
        /// </summary>
        /// <remarks>Prompts for a period using PeriodForm; iterates retrieved patient accounts and calls
        /// EmkSendingService.Update for each on background tasks; shows error message boxes and logs exceptions;
        /// restores button text and enabled state after completion.</remarks>
        /// <param name="sender">The event source.</param>
        /// <param name="e">The event arguments.</param>
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
        /// Отправляет данные в ФСС за выбранный период.
        /// Открывает диалог выбора периода; если пользователь подтверждает, запускает отправку в фоновой задаче.
        /// Во время выполнения кнопка блокируется и её текст изменяется на статус выполнения; в случае ошибки
        /// сообщение показывается пользователю через MessageBox, а подробности логируются.     
        /// </summary>
        /// <param name="sender">Источник события (обычно кнопка).</param>
        /// <param name="e">Аргументы события.</param>
        /// <remarks>
        /// Метод асинхронный — выполняет длительную операцию через Task.Factory.StartNew чтобы не блокировать UI.
        /// </remarks>
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
