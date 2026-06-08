using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Emk.Models;
using Emk.Services.Files;

namespace Emk.Views
{
	public partial class EmkSmoSettingsForm : Form
	{
		private IDoctorFileService _srv;
		private List<Doctor> _doctors;
		private DefaultData _def;


		public EmkSmoSettingsForm()
		{
			InitializeComponent();
			_srv = Factory.GetSmoService;
			LoadDefaults();
			LoadD4w();

		}

		#region LoadDataToTabs
		private void LoadD4w(List<Doctor> doctors = null)
		{
            if (doctors == null)
                _doctors = _srv.LoadDoctorsFromFile();
            else
                _doctors = doctors;
			dgvDoctors.DataSource = _doctors;
			dgvDoctors.Columns[0].ReadOnly = true;
			dgvDoctors.Columns[1].ReadOnly = true;
			dgvDoctors.Columns[2].ReadOnly = true;
			dgvDoctors.Columns[3].ReadOnly = true;

		}

		private void LoadDefaults()
		{
			_def = _srv.LoadDefaults();
			if (_def.Doctor is null)
				_def.Doctor = new Doctor();
			txtSurName.Text = _def.Doctor.Surname;
			txtName.Text = _def.Doctor.Name;
			txtMiddleName.Text = _def.Doctor.MiddleName;
			txtSnils.Text = _def.Doctor.Snils;
			txtSpeciality.Text = _def.Doctor.Speciality.ToString();
			txtPosition.Text = _def.Doctor.Position.ToString();

			txtComment.Text = _def.Comment;

			txtConfLevel.Text = _def.ConfidentialityLevel.ToString();
			txtConfDoctorLevel.Text = _def.ConfidentialityDoctorLevel.ToString();
			txtConfPredsLevel.Text = _def.ConfidentialityRepresentativeLevel.ToString();

			txtIdentityType.Text = _def.IdentityCaseType.ToString();

			txtDiagStatusIdentity.Text = _def.DiagnosisStatus.ToString();
			txtDiagMkbCode.Text = _def.DiagnosisDiseaseCode;
			txtDiagComment.Text = _def.DiagnosisComment;
			txtDiagStageId.Text = _def.DiagnosisStage.ToString();
			txtDiagCharId.Text = _def.DiagnosisCharacter.ToString();

			txtVisitPlace.Text = _def.VisitPlace.ToString();
			txtVisitPurpose.Text = _def.VisitPurpose.ToString();
		}
		#endregion

		private void ModifyDefaults()
		{
			if (_def.Doctor is null)
				_def.Doctor = new Doctor();
			_def.Doctor.Surname = txtSurName.Text;
			_def.Doctor.Name = txtName.Text;
			_def.Doctor.MiddleName = txtMiddleName.Text;
			_def.Doctor.Snils = txtSnils.Text;
			_def.Doctor.Speciality = int.Parse(txtSpeciality.Text);
			_def.Doctor.Position = int.Parse(txtPosition.Text);
			_def.Comment = txtComment.Text;
			_def.ConfidentialityLevel = int.Parse(txtConfLevel.Text);
			_def.ConfidentialityDoctorLevel = int.Parse(txtConfDoctorLevel.Text);
			_def.ConfidentialityRepresentativeLevel = int.Parse(txtConfPredsLevel.Text);

			_def.IdentityCaseType = int.Parse(txtIdentityType.Text);

			_def.DiagnosisStatus = int.Parse(txtDiagStatusIdentity.Text);
			_def.DiagnosisDiseaseCode = txtDiagMkbCode.Text;
			_def.DiagnosisComment = txtDiagComment.Text;
			_def.DiagnosisStage = int.Parse(txtDiagStageId.Text);
			_def.DiagnosisCharacter = int.Parse(txtDiagCharId.Text);

			_def.VisitPlace = int.Parse(txtVisitPlace.Text);
			_def.VisitPurpose = int.Parse(txtVisitPurpose.Text);

		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			_srv.SaveDoctorsToFile(_doctors);
			ModifyDefaults();
			_srv.SaveDefaults(_def);
			MessageBox.Show("Данные сохранены");
		}

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            LoadD4w(_srv.LoadDoctorsFromDb());
            MessageBox.Show("Данные обновлены", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
