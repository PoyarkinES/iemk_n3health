namespace Emk.Views
{
    partial class EmkMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSend = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnSmoSettings = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnSendByAccount = new System.Windows.Forms.Button();
            this.btnUpdatePeriod = new System.Windows.Forms.Button();
            this.brnSendPeriod = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(13, 13);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(241, 65);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Отправить данные";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(13, 374);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(241, 65);
            this.btnSettings.TabIndex = 1;
            this.btnSettings.Text = "Настройки обмена";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnSmoSettings
            // 
            this.btnSmoSettings.Location = new System.Drawing.Point(13, 466);
            this.btnSmoSettings.Margin = new System.Windows.Forms.Padding(4);
            this.btnSmoSettings.Name = "btnSmoSettings";
            this.btnSmoSettings.Size = new System.Drawing.Size(241, 65);
            this.btnSmoSettings.TabIndex = 2;
            this.btnSmoSettings.Text = "Настройки взаимодействия\r\nс D4W";
            this.btnSmoSettings.UseVisualStyleBackColor = true;
            this.btnSmoSettings.Click += new System.EventHandler(this.btnSmoSettings_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(13, 232);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(241, 65);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "Обновить по номеру счета";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnSendByAccount
            // 
            this.btnSendByAccount.Location = new System.Drawing.Point(13, 86);
            this.btnSendByAccount.Margin = new System.Windows.Forms.Padding(4);
            this.btnSendByAccount.Name = "btnSendByAccount";
            this.btnSendByAccount.Size = new System.Drawing.Size(241, 65);
            this.btnSendByAccount.TabIndex = 4;
            this.btnSendByAccount.Text = "Отправить данные по номеру счета";
            this.btnSendByAccount.UseVisualStyleBackColor = true;
            this.btnSendByAccount.Click += new System.EventHandler(this.btnSendByAccount_Click);
            // 
            // btnUpdatePeriod
            // 
            this.btnUpdatePeriod.Location = new System.Drawing.Point(13, 301);
            this.btnUpdatePeriod.Margin = new System.Windows.Forms.Padding(4);
            this.btnUpdatePeriod.Name = "btnUpdatePeriod";
            this.btnUpdatePeriod.Size = new System.Drawing.Size(241, 65);
            this.btnUpdatePeriod.TabIndex = 5;
            this.btnUpdatePeriod.Text = "Обновить за период";
            this.btnUpdatePeriod.UseVisualStyleBackColor = true;
            this.btnUpdatePeriod.Click += new System.EventHandler(this.btnUpdatePeriod_Click);
            // 
            // brnSendPeriod
            // 
            this.brnSendPeriod.Location = new System.Drawing.Point(13, 159);
            this.brnSendPeriod.Margin = new System.Windows.Forms.Padding(4);
            this.brnSendPeriod.Name = "brnSendPeriod";
            this.brnSendPeriod.Size = new System.Drawing.Size(241, 65);
            this.brnSendPeriod.TabIndex = 6;
            this.brnSendPeriod.Text = "Отправить данные за период";
            this.brnSendPeriod.UseVisualStyleBackColor = true;
            this.brnSendPeriod.Click += new System.EventHandler(this.brnSendPeriod_Click);
            // 
            // EmkMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(267, 580);
            this.Controls.Add(this.brnSendPeriod);
            this.Controls.Add(this.btnUpdatePeriod);
            this.Controls.Add(this.btnSendByAccount);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnSmoSettings);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.btnSend);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "EmkMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Сервис отправки данных           Build 59.0";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnSmoSettings;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnSendByAccount;
        private System.Windows.Forms.Button btnUpdatePeriod;
        private System.Windows.Forms.Button brnSendPeriod;
    }
}