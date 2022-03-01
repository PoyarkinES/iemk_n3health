namespace Emk.Views
{
    partial class EmkSettingsForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtIdLPU = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtGuid = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEmkUri = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPixUri = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.txtTime = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDBConnStr = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnChoosePatintDir = new System.Windows.Forms.Button();
            this.txtPatientDir = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dtPickerFrom = new System.Windows.Forms.DateTimePicker();
            this.dtPickerTo = new System.Windows.Forms.DateTimePicker();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbDaysToNow = new System.Windows.Forms.RadioButton();
            this.rbInterval = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtIdLPU);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtGuid);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtEmkUri);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtPixUri);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(504, 172);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Настройки сервисов";
            // 
            // txtIdLPU
            // 
            this.txtIdLPU.Location = new System.Drawing.Point(113, 127);
            this.txtIdLPU.Name = "txtIdLPU";
            this.txtIdLPU.Size = new System.Drawing.Size(364, 22);
            this.txtIdLPU.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "ID LPU:";
            // 
            // txtGuid
            // 
            this.txtGuid.Location = new System.Drawing.Point(113, 98);
            this.txtGuid.Name = "txtGuid";
            this.txtGuid.Size = new System.Drawing.Size(364, 22);
            this.txtGuid.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "GUID:";
            // 
            // txtEmkUri
            // 
            this.txtEmkUri.Location = new System.Drawing.Point(113, 66);
            this.txtEmkUri.Name = "txtEmkUri";
            this.txtEmkUri.Size = new System.Drawing.Size(364, 22);
            this.txtEmkUri.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "EMK Uri:";
            // 
            // txtPixUri
            // 
            this.txtPixUri.Location = new System.Drawing.Point(113, 35);
            this.txtPixUri.Name = "txtPixUri";
            this.txtPixUri.Size = new System.Drawing.Size(364, 22);
            this.txtPixUri.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 38);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pix Uri:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtDBConnStr);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnChoosePatintDir);
            this.groupBox2.Controls.Add(this.txtPatientDir);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(16, 194);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 130);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // txtInterval
            // 
            this.txtInterval.Location = new System.Drawing.Point(346, 59);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(131, 22);
            this.txtInterval.TabIndex = 9;
            // 
            // txtTime
            // 
            this.txtTime.Location = new System.Drawing.Point(346, 31);
            this.txtTime.Name = "txtTime";
            this.txtTime.Size = new System.Drawing.Size(131, 22);
            this.txtTime.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(23, 34);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(134, 16);
            this.label8.TabIndex = 6;
            this.label8.Text = "Время обновления:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 16);
            this.label7.TabIndex = 5;
            // 
            // txtDBConnStr
            // 
            this.txtDBConnStr.Location = new System.Drawing.Point(26, 90);
            this.txtDBConnStr.Name = "txtDBConnStr";
            this.txtDBConnStr.Size = new System.Drawing.Size(451, 22);
            this.txtDBConnStr.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(197, 16);
            this.label6.TabIndex = 3;
            this.label6.Text = "Подключение к базе данных:";
            // 
            // btnChoosePatintDir
            // 
            this.btnChoosePatintDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnChoosePatintDir.Location = new System.Drawing.Point(441, 41);
            this.btnChoosePatintDir.Name = "btnChoosePatintDir";
            this.btnChoosePatintDir.Size = new System.Drawing.Size(36, 23);
            this.btnChoosePatintDir.TabIndex = 2;
            this.btnChoosePatintDir.Text = "...";
            this.btnChoosePatintDir.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnChoosePatintDir.UseVisualStyleBackColor = true;
            this.btnChoosePatintDir.Click += new System.EventHandler(this.btnChoosePatintDir_Click);
            // 
            // txtPatientDir
            // 
            this.txtPatientDir.Location = new System.Drawing.Point(26, 41);
            this.txtPatientDir.Name = "txtPatientDir";
            this.txtPatientDir.Size = new System.Drawing.Size(409, 22);
            this.txtPatientDir.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(145, 16);
            this.label5.TabIndex = 0;
            this.label5.Text = "Папка с пациентами:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(45, 481);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 39);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(393, 481);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 39);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // dtPickerFrom
            // 
            this.dtPickerFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtPickerFrom.Location = new System.Drawing.Point(197, 88);
            this.dtPickerFrom.Name = "dtPickerFrom";
            this.dtPickerFrom.Size = new System.Drawing.Size(113, 22);
            this.dtPickerFrom.TabIndex = 10;
            // 
            // dtPickerTo
            // 
            this.dtPickerTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtPickerTo.Location = new System.Drawing.Point(346, 88);
            this.dtPickerTo.Name = "dtPickerTo";
            this.dtPickerTo.Size = new System.Drawing.Size(131, 22);
            this.dtPickerTo.TabIndex = 11;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.dtPickerFrom);
            this.groupBox3.Controls.Add(this.txtTime);
            this.groupBox3.Controls.Add(this.dtPickerTo);
            this.groupBox3.Controls.Add(this.rbInterval);
            this.groupBox3.Controls.Add(this.txtInterval);
            this.groupBox3.Controls.Add(this.rbDaysToNow);
            this.groupBox3.Location = new System.Drawing.Point(16, 330);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(504, 135);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Передача данных";
            // 
            // rbDaysToNow
            // 
            this.rbDaysToNow.AutoSize = true;
            this.rbDaysToNow.Checked = true;
            this.rbDaysToNow.Location = new System.Drawing.Point(26, 59);
            this.rbDaysToNow.Name = "rbDaysToNow";
            this.rbDaysToNow.Size = new System.Drawing.Size(292, 20);
            this.rbDaysToNow.TabIndex = 0;
            this.rbDaysToNow.TabStop = true;
            this.rbDaysToNow.Text = "Минус значение в днях от текущей даты:";
            this.rbDaysToNow.UseVisualStyleBackColor = true;
            // 
            // rbInterval
            // 
            this.rbInterval.AutoSize = true;
            this.rbInterval.Location = new System.Drawing.Point(26, 90);
            this.rbInterval.Name = "rbInterval";
            this.rbInterval.Size = new System.Drawing.Size(94, 20);
            this.rbInterval.TabIndex = 1;
            this.rbInterval.Text = "Диапазон:";
            this.rbInterval.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(176, 92);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(15, 16);
            this.label9.TabIndex = 12;
            this.label9.Text = "c";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(316, 92);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 16);
            this.label10.TabIndex = 13;
            this.label10.Text = "по";
            // 
            // EmkSettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(535, 543);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "EmkSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Настройки";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtIdLPU;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtGuid;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtEmkUri;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPixUri;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.TextBox txtTime;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDBConnStr;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnChoosePatintDir;
        private System.Windows.Forms.TextBox txtPatientDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DateTimePicker dtPickerFrom;
        private System.Windows.Forms.DateTimePicker dtPickerTo;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton rbInterval;
        private System.Windows.Forms.RadioButton rbDaysToNow;
    }
}