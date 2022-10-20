using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using Emk.EmkSvc;
using Emk.Models;
using Emk.Repository;
using Emk.Services.Files;
using Newtonsoft.Json;

namespace Emk.Services
{
	public class EmkService
	{
		private EmkRepository _rep;


		string Url;
		string guid;
		string IdLPU;
		CaseAmb case1;
		PersonWithIdentity patient1;
		string MName = "";
		OdbcConnection _conn;
		string conString;
		string path = "";
		string patientsBaseDir;
		IDoctorFileService _smoSrv;

		public EmkService(EmkSettings s)
		{
			Url = s.EmkUrl;
			guid = s.Guid.ToString();
			IdLPU = s.IdLPU.ToString();
			conString = s.DbConnectionString;
			patientsBaseDir = s.PatientDirectory;
			_smoSrv = Factory.GetSmoService;
			_rep = Factory.GetEmkRepository;
            _conn = Factory.GetDbConnection();
        }


        public int UpdateCase(PatientAccount treat) => AddCase(treat, true);
        



        public int AddCase(PatientAccount treat, bool updateOnly = false)
        {
            Log.Info($"EMK Добавляю случай медицинского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy}");
            try {
                if (!IsValid(treat)) {
                    Log.Error($"ЕМК СМО для пациента ИД {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy} не прошел валидацию и будет пропущен.");
                    return -1;
                }


                var binding = new BasicHttpBinding();
                var endpointAddress = new EndpointAddress(new Uri(Url));
                var client = new EmkServiceClient(binding, endpointAddress);

                case1 = new CaseAmb();

                if (_conn.State != ConnectionState.Open)
                    _conn.Open();


                var doc = _rep.GetDoctorByMemberId(treat.ProviderId);
                doc.Speciality = treat.Code;
                doc.AccountId = treat.AccountId;
                if (doc.AccountId == 0) {
                    Log.Warning($"Для СМО для пациента с ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} не найден счет. СМО пропущен.");
                    return -1;
                }
                var patient = Factory.GetPatientRepository.GetPatient(treat.PatientId);
                var diag = new DiagnosisEmk()
                {
                    DiagnosisCode = treat.DiagnoseCode, DiagnosisName = treat.DiagnoseName
                }; //GetDiagnose(treat.PatientId, treat.TreatDate);
                if (string.IsNullOrWhiteSpace(diag.DiagnosisCode) || string.IsNullOrWhiteSpace(diag.DiagnosisName)) {
                    Log.Warning($"Для СМО для пациента с ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} не задан диагноз. СМО пропущен.");
                    return -1;
                }

                Log.Info($"Доктор: {doc.Surname} {doc.Name} {doc.MiddleName} Диагноз: {diag.DiagnosisCode} {diag.DiagnosisName}");


                //if (patient1 == null)
                //{
                //	int rv;
                //	patient1 = new PersonWithIdentity();

                //	rv = SetPatient(patient_id, ref ErrDescription);
                //	if (rv != 0)
                //		return rv;
                //}

                var doctor = doc.ToMedicalStaff();

                var def = _smoSrv.LoadDefaults();

                case1.OpenDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                    DateTimeKind.Local);
                case1.CloseDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                    DateTimeKind.Local);
                case1.HistoryNumber = patient.CartNum;

                case1.IdCaseMis = $"{patient.CartNum}-{doc.AccountId}";

                case1.IdCaseAidType = 3;
                case1.IdCaseType = 2;
                case1.IdPaymentType = (byte)_rep.GetPayType(doc.AccountId);
                case1.IdCasePurpose = Convert.ToByte(def.VisitPurpose);

                case1.Confidentiality = Convert.ToByte(def.ConfidentialityLevel);
                case1.DoctorConfidentiality = Convert.ToByte(def.ConfidentialityDoctorLevel);
                case1.CuratorConfidentiality = Convert.ToByte(def.ConfidentialityRepresentativeLevel);
                case1.IdLpu = IdLPU;
                case1.IdCaseResult = 1;
                case1.Comment = diag.DiagnosisName;

                case1.IdPatientMis = patient.CartNum;
                case1.DoctorInCharge = doctor;
                case1.Authenticator = new Participant { Doctor = doctor, IdRole = 3};
                case1.Author = new Participant { Doctor = doctor, IdRole = 3};
                case1.LegalAuthenticator = new Participant { Doctor = doctor, IdRole = 3};
                case1.CaseVisitType = 1;    // 1 - Первичный
                                            // 2 - Повторный
                Log.Info($"Создан СМО для пациентa с картой {patient.CartNum}, ИД случая: {case1.IdCaseMis}");
                case1.Steps = new[]
                {
                     new StepAmb
                    {
                        DateStart = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                            DateTimeKind.Local),
                        DateEnd = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                            DateTimeKind.Local),
                        IdStepMis = $"{doc.AccountId}-{patient.CartNum}" ,
                        Doctor = doctor,
                        IdVisitPlace = Convert.ToByte(def.VisitPlace),
                        IdVisitPurpose = Convert.ToByte(def.VisitPurpose)
                    }
                };
                var medDocuments = new List<MedRecord>
                {
                    new ClinicMainDiagnosis
                    {
                        DiagnosisInfo = new DiagnosisInfo
                        {
                            IdDiseaseType = 1,
                            DiagnosedDate = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                                DateTimeKind.Local),
                            IdDiagnosisType = 1,
                            Comment = diag.DiagnosisName,
							//DiagnosisChangeReason = 2,
							DiagnosisStage = 3,
							//IdDispensaryState = 8,
							//IdTraumaType = 1,
							//MESImplementationFeature = 10,
							//MedicalStandard = 211010,
							MkbCode = diag.DiagnosisCode
                        },
                        Doctor = doctor
                    }
                };
                var medRecords = new List<MedRecord>();

                foreach (var d in _rep.GetProcedureDescriptions(treat.PatientId, treat.TreatDate)) {
                    if (string.IsNullOrEmpty(d.Description))
                        continue;
                    medRecords.Add(new Service
                    {
                        DateEnd = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                            DateTimeKind.Local),
                        DateStart = new DateTime(treat.TreatDate.Year, treat.TreatDate.Month, treat.TreatDate.Day, 0, 0, 0, 1,
                            DateTimeKind.Local),
                        IdServiceType = d.Description,
                        ServiceName = d.FullDescription,
                        Performer = new Participant { IdRole = 3, Doctor = doctor }
                    });
                }

                try {
                    _conn = _conn ?? new OdbcConnection(conString);
                    if (_conn.State != ConnectionState.Open)
                        _conn.Open();

                    var hasPDF = false;


                    var dir = $"{patientsBaseDir.TrimEnd('\\')}\\{patient.LastName} {patient.FirstName} {patient.MiddleName} [{patient.Id}]\\Дневниковые записи";
                    if (!Directory.Exists(dir))
                        dir = $"{patientsBaseDir.TrimEnd('\\')}\\{patient.LastName} {patient.FirstName} {patient.MiddleName} [{patient.CartNum}]\\Дневниковые записи";

                    var docs = new DocSelector(patient, treat.TreatDate, dir).GetDocs();
                    if (docs != null)
                        medDocuments.AddRange(docs);




                    if (Directory.Exists(dir)) {
                        var files = from file in Directory.EnumerateFiles(dir)
                                    orderby file ascending
                                    where file.EndsWith("pdf", StringComparison.OrdinalIgnoreCase)
                                    select file;
                        var pdf = files.LastOrDefault();
                        if (!string.IsNullOrEmpty(pdf)) {
                            hasPDF = true;

                            var data = File.ReadAllBytes(pdf);

                            // ReSharper disable UseStringInterpolation
                            var sgn1 = string.Format("{0}.sgn", string.Copy(pdf));
                            var sgn2 = string.Format("{0}2.sgn", string.Copy(pdf));
                            // ReSharper restore UseStringInterpolation

                            byte[] dsgn = null, osgn = null;
                            if (File.Exists(sgn1))
                                dsgn = File.ReadAllBytes(sgn1);
                            if (File.Exists(sgn2))
                                osgn = File.ReadAllBytes(sgn2);

                            medDocuments.Add(new ConsultNote
                            {
                                Attachments = new[]
                                {
                                    new MedDocumentDtoDocumentAttachment
                                    {
                                        Data = data, //Encoding.UTF8.GetBytes(s),
										MimeType = "application/pdf",
                                        OrganizationSign = osgn,
                                        PersonalSigns = dsgn == null ? null : new[]
                                        {
                                            new MedDocumentDtoPersonalSign
                                            {
                                                Doctor = doctor,
                                                Sign = dsgn
                                            }
                                        }
                                    }
                                },
                                Author = doctor,
                                CreationDate = DateTime.Now.Date,
                                Header = "Header",
                                IdDocumentMis = $"{patient.CartNum}-{doc.AccountId}"
                                //IdDocumentMis = $"{patient1.IdPersonMis}-{case_id}-{Guid.NewGuid().ToString()}"
                            });
                        }
                    }
                }
                catch {
                }

                case1.MedRecords = medDocuments.ToArray();
                case1.Steps[0].MedRecords = medRecords.ToArray();
                Log.Info($"Добавлено документов: {medDocuments.Count}, добавлено записей: {medRecords.Count}");

                if (updateOnly)
                {
                    client.UpdateCase(guid, case1);
                    client.Close();
                    Log.Info($"EMK Cлучай мединского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} обновлен.");
                    return 0;
                }

                client.AddCase(guid, case1);
                client.Close();
                Log.Info($"EMK Cлучай мединского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} добавлен.");
                return 0;
            }
            catch (FaultException<RequestFault[]> ex) {
				//foreach (var err1 in ex.Detail) {
				//    Log.Error(err1.ErrorCode + ": " + err1.PropertyName + " " + err1.Message);
				//}
				Log.Error("\r\n" + Newtonsoft.Json.JsonConvert.SerializeObject(JsonConvert.DeserializeObject($"{{ {getErrorString(ex.Detail)} }}"), Formatting.Indented) + "\r\n");

				return -1;
            }
            catch (FaultException<RequestFault> ex) {
                var errDescription = ex.Detail.ErrorCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message + "\r\n";
                Log.Error(errDescription);
                return -1;
            }
            catch (FaultException<RequestWarning> ex) {
                Log.Warning(ex.Detail.WarningCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message + "\r\n");
                return -1;
            }
            catch (FaultException<RequestWarning[]> ex) {

                foreach (var err1 in ex.Detail) {
                    Log.Warning(err1.WarningCode + ": " + err1.PropertyName + " " + err1.Message);
                    foreach (var e in err1.Warnings)
                        Log.Warning(e.WarningCode + ": " + e.PropertyName + " " + e.Message);
                }
                return -1;
            }
            catch (Exception ex) {
                Log.Error(ex.ToString());
                return -1;
            }
        }


		//public int AddCase(PatientTreat treat)
		//{
		//	Log.Info($"EMK Добавляю случай медицинского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy}");
		//	try
		//	{
  //              if (!IsValid(treat))
  //              {
  //                  Log.Error($"ЕМК СМО для пациента ИД {treat.PatientId} от {treat.TreatDate:dd.MM.yyyy} не прошел валидацию и будет пропущен.");
  //                  return -1;
  //              }


		//		var binding = new BasicHttpBinding();
		//		var endpointAddress = new EndpointAddress(new Uri(Url));
		//		var client = new EmkServiceClient(binding, endpointAddress);

		//		case1 = new CaseAmb();
                
		//		if (_conn.State != ConnectionState.Open)
		//			_conn.Open();
				

  //              var doc = _rep.GetDoctorOfPatientTreat(treat.PatientId, treat.TreatDate);
  //              doc.Speciality = treat.SpecialityCode;
  //              if (doc.AccountId == 0)
  //              {
  //                  Log.Warning($"Для СМО для пациента с ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} не найден счет. СМО пропущен.");
  //                  return -1;
  //              }
  //              var patient = Factory.GetPatientRepository.GetPatient(treat.PatientId);
  //              var diag = GetDiagnose(treat.PatientId, treat.TreatDate);
  //              if (string.IsNullOrWhiteSpace(diag.DiagnosisCode) || string.IsNullOrWhiteSpace(diag.DiagnosisName))
  //              {
  //                  Log.Warning($"Для СМО для пациента с ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} не задан диагноз. СМО пропущен.");
  //                  return -1;
  //              }

		//		Log.Info($"Доктор: {doc.Surname} {doc.Name} {doc.MiddleName} Диагноз: {diag.DiagnosisCode} {diag.DiagnosisName}");
				

		//		//if (patient1 == null)
		//		//{
		//		//	int rv;
		//		//	patient1 = new PersonWithIdentity();

		//		//	rv = SetPatient(patient_id, ref ErrDescription);
		//		//	if (rv != 0)
		//		//		return rv;
		//		//}

  //              var doctor = doc.ToMedicalStaff();

  //              var def = _smoSrv.LoadDefaults();

		//		case1.OpenDate = treat.TreatDate.ToString("O");
		//		case1.CloseDate = treat.TreatDate.ToString("O");
		//		case1.HistoryNumber = patient.CartNum;

		//		case1.IdCaseMis = $"{patient.CartNum}-{doc.AccountId}";
				
		//		case1.IdCaseAidType = 3; // Changed by Alex
		//		case1.IdCaseType = 2;
  //              case1.IdPaymentType = (byte)_rep.GetPayType(doc.AccountId);
		//		case1.IdCasePurpose = Convert.ToByte(def.VisitPurpose);

		//		case1.Confidentiality = Convert.ToByte(def.ConfidentialityLevel); 
		//		case1.DoctorConfidentiality = Convert.ToByte(def.ConfidentialityDoctorLevel);
		//		case1.CuratorConfidentiality = Convert.ToByte(def.ConfidentialityRepresentativeLevel);
		//		case1.IdLpu = IdLPU;
		//		case1.IdCaseResult = 1;
		//		case1.Comment = diag.DiagnosisName;

  //              case1.IdPatientMis = patient.CartNum;
		//		case1.DoctorInCharge = doctor;
		//		case1.Authenticator = new Participant { Doctor = doctor };
		//		case1.Author = new Participant { Doctor = doctor };
		//		case1.LegalAuthenticator = new Participant { Doctor = doctor };
		//		case1.CaseVisitType = 1;    // 1 - Первичный
		//									// 2 - Повторный
  //              Log.Info($"Создан СМО для пациентa с картой {patient.CartNum}, ИД случая: {case1.IdCaseMis}");
		//		case1.Steps = new[]
		//		{
		//			 new StepAmb
		//			{
		//				DateStart = treat.TreatDate.ToString("O"),
		//				DateEnd = treat.TreatDate.ToString("O"),
		//				IdStepMis = $"{doc.AccountId}-{patient.CartNum}" ,
		//				Doctor = doctor,
		//				IdVisitPlace = Convert.ToByte(def.VisitPlace),
		//				IdVisitPurpose = Convert.ToByte(def.VisitPurpose)
  //                  }
		//		};
		//		var medDocuments = new List<MedRecord>
		//		{
		//			new ClinicMainDiagnosis
		//			{
		//				DiagnosisInfo = new DiagnosisInfo
		//				{
		//					IdDiseaseType = 1,
		//					DiagnosedDate = treat.TreatDate.ToString("O"),
		//					IdDiagnosisType = 1,
		//					Comment = diag.DiagnosisName,
		//					//DiagnosisChangeReason = 2,
		//					//DiagnosisStage = 3,
		//					//IdDispensaryState = 8,
		//					//IdTraumaType = 1,
		//					//MESImplementationFeature = 10,
		//					//MedicalStandard = 211010,
		//					MkbCode = diag.DiagnosisCode,
  //                          DiagnosisStage = 3 // Added by Alex
  //                      },
		//				Doctor = doctor
		//			}
		//		};
		//		var medRecords = new List<MedRecord>();

  //              foreach (var d in _rep.GetProcedureDescriptions(treat.PatientId, treat.TreatDate))
  //              {
  //                  if (string.IsNullOrEmpty(d.Description))
  //                      continue;
  //                  medRecords.Add(new Service
  //                  {
  //                      DateEnd = d.ProcedureDate.ToString("O"),
  //                      DateStart = d.ProcedureDate.ToString("O"),
  //                      IdServiceType = d.Description,
  //                      ServiceName = d.FullDescription,
  //                      Performer = new Participant { IdRole = 3, Doctor = doctor }
  //                  });
  //              }

  //              try
		//		{
		//			_conn = _conn ?? new OdbcConnection(conString);
		//			if (_conn.State != ConnectionState.Open)
		//				_conn.Open();

		//			var hasPDF = false;
                    

  //                  var dir = $"{patientsBaseDir.TrimEnd('\\')}\\{patient.LastName} {patient.FirstName} {patient.MiddleName} [{patient.Id}]\\Дневниковые записи";
  //                  if (!Directory.Exists(dir))
  //                      dir = $"{patientsBaseDir.TrimEnd('\\')}\\{patient.LastName} {patient.FirstName} {patient.MiddleName} [{patient.CartNum}]\\Дневниковые записи";

  //                  var docs = new DocSelector(patient, treat.TreatDate, dir).GetDocs();
		//			if(docs != null)
		//				medDocuments.AddRange(docs);
                    


                    
  //                  if (Directory.Exists(dir))
  //                  {
  //                      var files = from file in Directory.EnumerateFiles(dir)
  //                                  orderby file ascending
  //                                  where file.EndsWith("pdf", StringComparison.OrdinalIgnoreCase)
  //                                  select file;
  //                      var pdf = files.LastOrDefault();
  //                      if (!string.IsNullOrEmpty(pdf))
  //                      {
  //                          hasPDF = true;

  //                          var data = File.ReadAllBytes(pdf);

  //                          // ReSharper disable UseStringInterpolation
  //                          var sgn1 = string.Format("{0}.sgn", string.Copy(pdf));
  //                          var sgn2 = string.Format("{0}2.sgn", string.Copy(pdf));
  //                          // ReSharper restore UseStringInterpolation

  //                          byte[] dsgn = null, osgn = null;
  //                          if (File.Exists(sgn1))
  //                              dsgn = File.ReadAllBytes(sgn1);
  //                          if (File.Exists(sgn2))
  //                              osgn = File.ReadAllBytes(sgn2);

  //                          medDocuments.Add(new ConsultNote
  //                          {
  //                              Attachments = new[]
  //                              {
  //                                  new MedDocumentDtoDocumentAttachment
  //                                  {
  //                                      Data = data, //Encoding.UTF8.GetBytes(s),
		//								MimeType = "application/pdf",
  //                                      OrganizationSign = osgn,
  //                                      PersonalSigns = dsgn == null ? null : new[]
  //                                      {
  //                                          new MedDocumentDtoPersonalSign
  //                                          {
  //                                              Doctor = doctor,
  //                                              Sign = dsgn
  //                                          }
  //                                      }
  //                                  }
  //                              },
  //                              Author = doctor,
  //                              CreationDate = DateTime.Now.Date,
  //                              Header = "Header",
  //                              IdDocumentMis = $"{patient.CartNum}-{doc.AccountId}"
  //                              //IdDocumentMis = $"{patient1.IdPersonMis}-{case_id}-{Guid.NewGuid().ToString()}"
  //                          });
  //                      }
  //                  }
  //              }
		//		catch
		//		{
		//		}

		//		case1.MedRecords = medDocuments.ToArray();
		//		case1.Steps[0].MedRecords = medRecords.ToArray();
  //              Log.Info($"Добавлено документов: {medDocuments.Count}, добавлено записей: {medRecords.Count}");
		//		client.AddCase(guid, case1);
		//		client.Close();
		//		Log.Info($"EMK Cлучай мединского обслуживания для пациента ИД {treat.PatientId} от {treat.TreatDate.ToString("dd.MM.yyyy")} добавлен.");
		//		return 0;
		//	}
		//	catch (FaultException<RequestFault[]> ex)
		//	{
		//		foreach (var err1 in ex.Detail)
		//		{
		//			Log.Error(err1.ErrorCode + ": " + err1.PropertyName + " " + err1.Message);
		//		}
		//		return -1;
		//	}
		//	catch (FaultException<RequestFault> ex)
		//	{
		//		var errDescription = ex.Detail.ErrorCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message + "\r\n";
		//		Log.Error(errDescription);
		//		return -1;
		//	}
		//	catch (FaultException<RequestWarning> ex)
		//	{
		//		Log.Warning(ex.Detail.WarningCode + ": " + ex.Detail.PropertyName + " " + ex.Detail.Message + "\r\n");
		//		return -1;
		//	}
		//	catch (FaultException<RequestWarning[]> ex)
		//	{
			
		//		foreach (var err1 in ex.Detail)
		//		{
		//			Log.Warning(err1.WarningCode + ": " + err1.PropertyName + " " + err1.Message);
		//			foreach (var e in err1.Warnings)
		//				Log.Warning(e.WarningCode + ": " + e.PropertyName + " " + e.Message);
		//		}
		//		return -1;
		//	}
		//	catch (Exception ex)
		//	{
		//		Log.Error(ex.ToString());
		//		return -1;
		//	}
		//}

        private bool IsValid(object obj)
        {
            if (!(obj is PatientAccount treat))
                return false;

            if(treat.Code == 0){
                Log.Error("Не задана специализация");
                return false;
            }

            if (treat.AccountId == 0)
            {
                Log.Error("Не задан счет");
                return false;
            }

            return true;

        }

		private Doctor GetDoctorByName(string LastName1, string FirstName1, string MiddleName1)
		{
			var docs = _smoSrv.LoadDoctorsFromFile();
			var doc = docs.Find(x => x.Surname == LastName1 && x.Name == FirstName1 && x.MiddleName == MiddleName1);
			if (doc == null)
			{
				Log.Warning($"Доктор {LastName1} {FirstName1} {MiddleName1} не найден в SmoSettings. Будет использован доктор по умолчанию." );
                doc = GetDefaultDoctor();
            }

            return doc;


			//SNILS1 = doc.Snils;
			//speciality1 = doc.Speciality;
			//position1 = doc.Position;
            
			//return 1;
		}
        
		private Doctor GetDefaultDoctor()
		{
			return _smoSrv.LoadDefaults().Doctor;
		}

        private DiagnosisEmk GetDiagnose(int patientId, DateTime date)
        {
            var diag = _rep.GetPatientDiagnosis(patientId, date);
            return diag;
        }


		public int CheckLogFile(string CardNum, ref int ErrNum, ref string ErrDescription)
		{
			var path = string.Empty;
			if (!Environment.Is64BitOperatingSystem)
				path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dw4import") + @"\";

			if (!Directory.Exists(path))
			{
				path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"dw4import") + @"\";
				if (!Directory.Exists(path))
				{
					ErrNum = -10;
					ErrDescription = "Каталог " + @"C:\Program Files (x86)\dw4import\" + " не существует.";
					return -1;
				}
			}

			path += @"Log\";
			if (Directory.Exists(path))
			{
				var fileName = $"{path}log_{$"{DateTime.Now:dd-MM-yyyy}"}.txt";

				if (File.Exists(fileName))
				{
					const string checkStr = "Добавление завершенного случая медицинского обслуживания на сервер выполнено успешно";
					var readText = File.ReadAllLines(fileName);
					var tmpList = readText.Where(x => x.Contains("№ карты: " + CardNum.Trim()) || x.Contains("СМО врач") || x.Contains(checkStr)).ToList();

					for (int i = 0; i < tmpList.Count; i++)
						if (i + 3 <= tmpList.Count)
						{
							var checkList1 = tmpList.GetRange(i, 3);
							if (checkList1[0].Contains($"№ карты: {CardNum.Trim()}") && checkList1[1].Contains("СМО врач") && checkList1[2].Contains(checkStr))
							{
								ErrNum = -20;
								ErrDescription = "Завершенный случай медицинского обслуживания уже добавлен на сервер.";
								return -1;
							}
						}
				}
			}

			return 0;
		}

		public int SetPatient(int patient_id, ref string ErrDescription)
		{
			int rc, passport1, polis1, address1, idProvider1 = 0;
			var LastName1 = "";
			var FirstName1 = "";
			var MiddleName1 = "";
			var cart_num1 = "";
			var dob1 = DateTime.Today;
			string patient_sex = null;
			var BDate = "";
			var number1 = "";
			var serial1 = "";
			var name_org1 = "";
			var date_give_out = DateTime.Today;
			var post_id_1 = -1;
			var address_1 = "";
			var address_2 = "";
			var code = "";
			var state = "";
			var StateName = "";
			var hf_member_code = "";
			var date_start = DateTime.Today;
			var date_end = DateTime.Today;
			var hf_plan_id = -1;
			var hf_plan_code = "";
			var hf_name = "";


			ErrDescription = "";

			try
			{
				if (_conn == null || _conn.State != ConnectionState.Open)
				{
					_conn = new OdbcConnection(conString);
					_conn.Open();
				}

				var cmd = new OdbcCommand("select patient_id, surname, firstname, middlename, dob, patient_sex, patients_cart_num, number, serial, name_org, date_give_out, post_id_1, address_1, address_2 from patients where patient_id = ?", _conn);

				var parm = new OdbcParameter
				{
					DbType = DbType.Int32,
					Value = patient_id
				};
				cmd.Parameters.Add(parm);

				var dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					LastName1 = dr["surname"].ToString();
					FirstName1 = dr["firstname"].ToString();
					MiddleName1 = dr["middlename"].ToString();
					cart_num1 = dr["patients_cart_num"].ToString();
					if (dr["dob"] != DBNull.Value)
						dob1 = (DateTime)dr["dob"];

					if (dr["patient_sex"] != DBNull.Value)
						patient_sex = dr["patient_sex"].ToString();

					if (dr["number"] != DBNull.Value)
						number1 = dr["number"].ToString();

					if (dr["serial"] != DBNull.Value)
						serial1 = dr["serial"].ToString();

					if (dr["name_org"] != DBNull.Value)
					{
						name_org1 = dr["name_org"].ToString();
					}

					if (dr["date_give_out"] != DBNull.Value)
					{
						date_give_out = (DateTime)dr["date_give_out"];
					}

					if (dr["post_id_1"] != DBNull.Value)
					{
						post_id_1 = (int)dr["post_id_1"];
					}

					if (dr["address_1"] != DBNull.Value)
					{
						address_1 = dr["address_1"].ToString();
					}

					if (dr["address_2"] != DBNull.Value)
					{
						address_2 = dr["address_2"].ToString();
					}

					byte b1 = 77;
					byte[] b;
					if (patient_sex != null)
					{
						b = Encoding.Default.GetBytes(patient_sex);
						b1 = b[0];
					}

					if (patient_sex == null || b1 == 77)
					{
					}
					else if (patient_sex.Trim() == "F")
					{
					}

					if (dob1 == null)
					{
						BDate = string.Format("{0:dd-MM-yyyy}", DateTime.Today);
					}
					else
					{
						BDate = string.Format("{0:dd-MM-yyyy}", dob1);
					}

					if (post_id_1 > 0)
					{
						var cmd1 = new OdbcCommand("select code, state from post where id = ?", _conn);

						var parm1 = new OdbcParameter
						{
							DbType = DbType.Int32,
							Value = post_id_1
						};
						cmd1.Parameters.Add(parm1);
						var dr1 = cmd1.ExecuteReader();

						while (dr1.Read())
						{
							code = dr1["code"].ToString();
							state = dr1["state"].ToString();
						}
						dr1.Close();
					}

					if (state.Length > 0)
					{
						var cmd2 = new OdbcCommand("select state, state_name from states where state = ?", _conn);

						var parm2 = new OdbcParameter
						{
							DbType = DbType.String,
							Value = state
						};
						cmd2.Parameters.Add(parm2);
						var dr2 = cmd2.ExecuteReader();

						while (dr2.Read())
						{
							StateName = dr2["state_name"].ToString();
						}
						dr2.Close();
					}

					var cmd3 = new OdbcCommand("select hf_member_code, date_start, date_end, hf_plan_series, hf_plan_id from patients_hf where patient_id = ?", _conn);

					var parm3 = new OdbcParameter
					{
						DbType = DbType.Int32,
						Value = patient_id
					};
					cmd3.Parameters.Add(parm3);
					var dr3 = cmd3.ExecuteReader();

					while (dr3.Read())
					{
						hf_member_code = dr3["hf_member_code"].ToString();
						date_start = (DateTime)dr3["date_start"];
						date_end = (DateTime)dr3["date_end"];
						hf_plan_id = (int)dr3["hf_plan_id"];

						var cmd4 = new OdbcCommand("select hf_id, code, name from hf_plans, third_parties where third_party_id = hf_id and hf_plan_id = ?", _conn);

						var parm4 = new OdbcParameter
						{
							DbType = DbType.Int32,
							Value = hf_plan_id
						};
						cmd4.Parameters.Add(parm4);
						var dr4 = cmd4.ExecuteReader();

						while (dr4.Read())
						{
							hf_plan_code = dr4["code"].ToString();
							hf_name = dr4["name"].ToString().Trim();
						}
						dr4.Close();
					}
				}
				dr.Close();

				patient1 = new PersonWithIdentity
				{
					HumanName = new HumanName
					{
						FamilyName = LastName1,
						GivenName = FirstName1
					},
					IdPersonMis = cart_num1
				};

				// patient1.BirthDate = DateTime.ParseExact(BDate, "dd-MM-yyyy", null);
				// ??? patient1.Sex = (byte)sex1;


				//rc = GetElementByName("passport", "", ref value1);
				//passport1 = int.Parse(value1);
				passport1 = 0;

				//rc = GetElementByName("polis", "", ref value1);
				//polis1 = int.Parse(value1);
				polis1 = 0;

				//rc = GetElementByName("address", "", ref value1);
				//address1 = int.Parse(value1);
				address1 = 0;

				int ind1 = 0;
				if (serial1.Length > 0 && hf_member_code.Length > 0 && passport1 > 0 && polis1 > 0)
				{
					patient1.Documents = new IdentityDocument[2];
					ind1 = 1;
				}
				else if (((serial1.Length > 0 && passport1 > 0) || (hf_member_code.Length > 0 && polis1 > 0)))
				{
					patient1.Documents = new IdentityDocument[1];
				}

				if (serial1.Length > 0 && passport1 > 0)
				{
					patient1.Documents[0] = new IdentityDocument
					{
						IdDocumentType = 14,
						DocS = serial1,
						DocN = number1,
						IssuedDate = date_give_out,
						ProviderName = name_org1
					};
				}

				if (address1 > 0)
				{
					var adr1 = "";
					if (code.Length > 0)
					{
						adr1 = code;
					}

					if (StateName.Length > 0)
					{
						adr1 += " " + StateName;
					}

					if (address_1.Length > 0)
					{
						if (adr1.Length > 0)
						{
							adr1 += ", ";
						}

						adr1 += address_1;
					}
					// if (address_2.Length > 0) adr1 += " " + address_2;

					if (adr1.Length > 0)
					{
						/*
						patient1.Addresses = new EMKService.Data.Dto.AddressDto[1];
						patient1.Addresses[0] = new EMKService.Data.Dto.AddressDto();
						patient1.Addresses[0].IdAddressType = 1;
						patient1.Addresses[0].StringAddress = adr1;
						 */
					}
				}

				if (hf_member_code.Length > 0 && polis1 > 0)
				{
					patient1.Documents[ind1] = new IdentityDocument
					{
						IdDocumentType = 228,
						DocN = hf_member_code,
						IssuedDate = date_start,
						ExpiredDate = date_end
					};
					//var hf_code1 = -1;
					int.TryParse(hf_plan_code, out var hf_code1);

					rc = GetCompanyByName(hf_name, ref idProvider1, ref ErrDescription);

					// ??? if (hf_code1 > 0) patient1.Documents[ind1].IdProvider = hf_code1;
					if (idProvider1 > 0)
					{
						patient1.Documents[ind1].IdProvider = idProvider1;
					}

					patient1.Documents[ind1].ProviderName = hf_name;
				}

				return 0;
			}
			catch (Exception ex)
			{
				ErrDescription = ex.Message;
				return -1;
			}
		}

		public int GetCompanyByName(string CName1, ref int idProvider, ref string ErrDescription)
		{
			int Count1 = 0;
			try
			{
				IEnumerable<XElement> company1 = null;
				// XDocument xdoc = XDocument.Load(@"C:\IEMK_auto\PB\smo_settings.xml");
				var xdoc = XDocument.Load(path + "smo_settings.xml");

				var insElement = xdoc.Element("settings").Element("ins_companies");

				if (insElement != null)
				{
					company1 = from xe in insElement.Elements("company")
							   where xe.Element("cname").Value == CName1
							   select xe;

					Count1 = company1.Count();
				}
				else if (insElement == null)
				{
					var companyElement1 = new XElement("ins_companies")
					{
						Value = ""
					};
					xdoc.Element("settings").Add(companyElement1);
					// xdoc.Save(@"C:\IEMK_auto\PB\smo_settings.xml");
					xdoc.Save(path + "smo_settings.xml");
				}

				if (Count1 == 1)
				{
					var companyElement1 = company1.First();

					idProvider = 0;
					if (companyElement1.Element("idprovider").Value.Length > 0)
					{
						idProvider = int.Parse(companyElement1.Element("idprovider").Value);
					}
				}
				xdoc = null;
			}
			catch (Exception ex)
			{
				ErrDescription = ex.Message;
				return -1;
			}

			return Count1;
		}

		//public int SetParameters(string Url1, string guid1, string idLPU1, ref string ErrDescription)
		//{
		//	int pos1;
		//	string conString2 = "";

		//	Url = Url1;
		//	guid = guid1;
		//	IdLPU = idLPU1;


		//	path = @"C:\Program Files\dw4import\";
		//	if (!Directory.Exists(path))
		//	{
		//		path = @"C:\Program Files (x86)\dw4import\";
		//		if (!Directory.Exists(path))
		//		{
		//			ErrDescription = "Каталог программы не найден: " + @"C:\Program Files (x86)\dw4import\";
		//			return -1;
		//		}
		//	}

		//	try
		//	{
		//		using (var sr = File.OpenText(path + "imp.ini"))
		//		{
		//			string input = null;
		//			while ((input = sr.ReadLine()) != null)
		//			{
		//				if (input.Contains("DBParm") && !input.StartsWith(";"))
		//				{
		//					pos1 = input.IndexOf("DSN=");
		//					conString = input.Substring(pos1 + 4);
		//					pos1 = conString.IndexOf(";");
		//					conString = conString.Substring(0, pos1);
		//					conString = "Data Source=" + conString;
		//				}
		//				/*
		//				if (input.Contains("ConnectionString") && !input.StartsWith(";"))
		//				{
		//					pos1 = input.IndexOf("\"");
		//					conString2 = input.Substring(pos1 + 1);
		//					pos1 = conString2.IndexOf("\"");
		//					conString2 = conString2.Substring(0, pos1);
		//				}
		//				 */
		//				if (input.Contains("OdbcConnectionString") && !input.StartsWith(";"))
		//				{
		//					pos1 = input.IndexOf("\"");
		//					conString2 = input.Substring(pos1 + 1);
		//					pos1 = conString2.IndexOf("\"");
		//					conString2 = conString2.Substring(0, pos1);
		//				}

		//				if (input.StartsWith("PatientsBaseDir", StringComparison.InvariantCultureIgnoreCase))
		//					patientsBaseDir = input.Substring(input.IndexOf('=')).TrimStart(' ', '=');
		//			}
		//		}

		//		try
		//		{
		//			_conn = new OdbcConnection(conString);
		//			_conn.Open();
		//		}
		//		catch (Exception)
		//		{
		//			if (_conn.State != ConnectionState.Open)
		//			{
		//				conString = conString2;
		//				_conn = new OdbcConnection(conString);
		//				_conn.Open();
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrDescription = ex.Message;
		//		return -1;
		//	}

		//	return 0;
		//}

		//public int WriteLog(int patient_id, ref int ErrNum, ref string ErrDescription)
		//{
		//	var tmp1 = "";

		//	try
		//	{
		//		int ind1;
		//		var log1 = "";
		//		var message = "";
		//		string LastName, FirstName, dob, CardNum, Passport = "", Polis = "";

		//		if (patient1 == null)
		//		{
		//			// patient1 = new EMKService.Data.Dto.PatientDto();
		//			patient1 = new PersonWithIdentity();
		//			SetPatient(patient_id, ref ErrDescription);
		//		}

		//		// LastName = patient1.FamilyName;
		//		LastName = patient1.HumanName.FamilyName;

		//		// FirstName = patient1.GivenName;
		//		FirstName = patient1.HumanName.GivenName;

		//		// CardNum = patient1.IdPatientMIS;
		//		CardNum = patient1.IdPersonMis;

		//		// dob = String.Format("{0:dd-MM-yyyy}", patient1.BirthDate);
		//		dob = string.Format("{0:dd-MM-yyyy}", DateTime.Today);

		//		tmp1 = "   line 1";

		//		if (patient1.Documents != null)
		//		{
		//			if (patient1.Documents.Length >= 1 && patient1.Documents[0].IdDocumentType == 14)
		//			{
		//				Passport = " паспорт: " + patient1.Documents[0].DocS + " " + patient1.Documents[0].DocN + " выдан: " + patient1.Documents[0].ProviderName;
		//				Passport += " " + string.Format("{0:dd-MM-yyyy}", patient1.Documents[0].IssuedDate);
		//			}

		//			ind1 = -1;
		//			if (patient1.Documents.Length == 1 && patient1.Documents[0].IdDocumentType == 228)
		//			{
		//				ind1 = 0;
		//			}

		//			if (patient1.Documents.Length == 2 && patient1.Documents[1].IdDocumentType == 228)
		//			{
		//				ind1 = 1;
		//			}

		//			if (ind1 >= 0)
		//			{
		//				Polis = " полис: " + patient1.Documents[ind1].DocN + " действует с " + string.Format("{0:dd-MM-yyyy}", patient1.Documents[ind1].IssuedDate) + " по " + string.Format("{0:dd-MM-yyyy}", patient1.Documents[ind1].ExpiredDate);
		//				Polis += " " + patient1.Documents[ind1].IdProvider + " " + patient1.Documents[ind1].ProviderName;
		//			}
		//		}

		//		/*
		//		if (patient1.Addresses != null)
		//		{
		//			if (patient1.Addresses.Length > 0)
		//			{
		//				Passport += " адрес: " + patient1.Addresses[0].StringAddress;
		//			}
		//		}
		//		 */

		//		tmp1 = "   line 2";

		//		log1 = LastName + " " + FirstName + " ";
		//		if (MName.Length > 0)
		//		{
		//			log1 += MName + " ";
		//		}

		//		log1 += dob + " " + "г.р." + " " + "№ карты:" + " " + CardNum.Trim() + " ";
		//		if (ErrNum != 0)
		//		{
		//			message = "oшибка:";
		//		}
		//		else
		//		{
		//			message = "сообщение:";
		//		}

		//		log1 = DateTime.Today.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " пациент: " + log1;
		//		if (Passport.Length > 0)
		//		{
		//			log1 += Passport;
		//		}

		//		if (Polis.Length > 0)
		//		{
		//			log1 += Polis;
		//		}

		//		tmp1 = "   line 3";

		//		if (case1 != null)
		//		{
		//			log1 += "\n";
		//			log1 += "СМО врач:" + " " + case1.Author.Doctor.Person.HumanName.FamilyName + " " + case1.Author.Doctor.Person.HumanName.GivenName + " " + case1.Author.Doctor.Person.HumanName.MiddleName + " ";
		//			log1 += "СНИЛС:" + " " + case1.Author.Doctor.Person.IdPersonMis + " специальность: " + case1.Author.Doctor.IdSpeciality + " должность: " + case1.Author.Doctor.IdPosition + " комментарий: " + case1.Comment;
		//			log1 += " конфиденциальность: уровень: " + case1.Confidentiality + " уровень представителя: " + case1.CuratorConfidentiality + " уровень врача: " + case1.DoctorConfidentiality;
		//			log1 += " тип случая: " + case1.IdCaseType;
		//			//    + " диагноз: статус: " + case1.MainDiagnosis.IdDiagnosisType + " код заболевания: " + case1.MainDiagnosis.MkbCode + " комментарий: " + case1.MainDiagnosis.Comment;
		//			// log1 += " этап: " + case1.MainDiagnosis.IdDiagnoseStep + " характер: " + case1.MainDiagnosis.IdDiseaseType;
		//			log1 += " визит: место: " + case1.Steps[0].IdVisitPlace + " цель: " + case1.Steps[0].IdVisitPurpose;
		//		}

		//		tmp1 = "   line 4";

		//		log1 += "\nСервер: " + Url + " " + message + " " + ErrDescription + " " + "код:" + " " + ErrNum.ToString() + "\n";

		//		/*
		//		string path2 = @"C:\Program Files\dw4import\";
		//		if (!Directory.Exists(path2))
		//		{
		//			path2 = @"C:\Program Files (x86)\dw4import\";
		//			if (!Directory.Exists(path2))
		//			{
		//				ErrDescription = "1";
		//				return -1;
		//			}
		//		}
		//		 */

		//		var path2 = path + @"Log\";
		//		if (!Directory.Exists(path2))
		//		{
		//			Directory.CreateDirectory(path2);
		//		}

		//		var fileName = path2 + "log_" + string.Format("{0:dd-MM-yyyy}", DateTime.Now) + ".txt";

		//		if (File.Exists(fileName))
		//			using (var sw1 = File.AppendText(fileName))
		//				sw1.WriteLine(log1);
		//		else
		//			using (var sw2 = File.CreateText(fileName))
		//				sw2.WriteLine(log1);

		//		var CheckDate = DateTime.Today;
		//		CheckDate = CheckDate.AddDays(-10);

		//		var DelFile = path2 + "log_" + string.Format("{0:dd-MM-yyyy}", CheckDate) + ".txt";

		//		if (File.Exists(DelFile))
		//			File.Delete(DelFile);
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrNum = -1;
		//		ErrDescription = ex.Message + tmp1;
		//		return 0;
		//	}

		//	return 0;
		//}

		private string getErrorString(RequestFault[] errors, RequestFault error = null)
		{
			var result = new StringBuilder();
			foreach (var item in errors)
            {
                result.Append(Environment.NewLine);
                result.Append($"\"{item.PropertyName}\": {{ {getErrorString(item.Errors, item)} }},");
			}

            if (error != null && errors.Length == 0)
                result.Append($"\"ErrorCode\": {error.ErrorCode}, \"PropertyName\":\"{error.PropertyName}\", \"Message\":\"{error.Message}\" ");
            
            return result.ToString().Remove(result.ToString().Length -1, 1);
		}
	}
}
