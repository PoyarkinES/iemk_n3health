using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Emk.EmkSvc;
using Emk.Models;
using Emk.Repository;
using Emk.Services.Docs;
using Emk.Services.Docs.Referrals;
using Microsoft.SqlServer.Server;

namespace Emk.Services
{
    public class DocSelector
    {
        private DateTime _fileDate;
        private Patient _patient;
        private EmkSettings _settings = Factory.LoadSettings().First();
        private string _patientDir;
        private TreatRepository _treatRepository = Factory.GetTreatRepository;

        public DocSelector(Patient patient, DateTime fileDate, string patientDir)
        {
            _fileDate = fileDate;
            _patient = patient;
            _patientDir = patientDir;
        }

        public IEnumerable<MedRecord> GetDocs(int accountId)
        {
            //var dir = Path.Combine(_settings.PatientDirectory,
            //    $"{_patient.LastName} {_patient.FirstName} {_patient.MiddleName} [{_patient.Id}]",
            //    "Дневниковые записи");
            if (!Directory.Exists(_patientDir))
                return null;

            Log.Info($"Получаю файлы из директории - {_patientDir}");

            var docs = new List<MedRecord>();
            var file = getFileFromDB(accountId);
            if (!String.IsNullOrEmpty(file.Trim()))
            {
                try
                {
                    docs.Add(getMedRecord(file));
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
            else
            {
                foreach (var item in Directory.GetFiles(_patientDir))
                {
                    try
                    {
                        docs.Add(getMedRecord(item));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                }
            }

            return docs;

        }


        private IDocBase SelectDocType(FileData fd)
        {
            switch (fd.DocType)
            {
                case InternalDocType.HospitalReferral:
                    return new DocHospitalReferral(fd.FilePath, fd.CartNoteId);
                case InternalDocType.ConsultReferral:
                    return new DocConsultReferral(fd.FilePath, fd.CartNoteId);
                case InternalDocType.ExamReferral:
                    return new DocExaminationReferral(fd.FilePath, fd.CartNoteId);
                case InternalDocType.Prescription:
                    return new DocPrescription(fd.FilePath, fd.CartNoteId);
                case InternalDocType.DischargeSummary:
                    return new DocDischargeSummary(fd.FilePath, fd.CartNoteId);
                case InternalDocType.DocControlCardDispensaryObservation:
                    return new DocControlCardDispensaryObservation(fd.FilePath, fd.CartNoteId);
                case InternalDocType.DocConsultNote:
                    return new DocConsultNote(fd.FilePath, fd.CartNoteId);
                default:
                    return null;
            }

        }

        private FileData ParseFile(string filePath)
        {
            try
            {
                FileData fd = new FileData();
                fd.FilePath = _patientDir + "\\" + filePath;
                if (!fd.FileExists)
                    throw new FileNotFoundException("Файл не найден", filePath);

                string[] data = Path.GetFileName(filePath).Split('_');

                if (!data.Any())
                    throw new NotImplementedException();

                fd.FileDate = checkDate(data[0]);
                fd.CartNoteId = checkCartNoteId(data[1]);
                fd.DocType = checkDocType(data[2]);
                fd.PatientCartNum = checkCardNum(data[3]);
                fd.DoctorId = checkDoctorCode(data[4]);
                checkFIO(data.Skip(5).ToArray(), out string error, out bool check);

                if (!check) throw new Exception(error);

                return fd;
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Неверное наименование файла ({Path.GetFileName(filePath)}). " +
                    $"Ожидается формат (<yyyyddmm>_<cart_notes_id>_<тип файла>_<номер карты>_<код_врача>_<фио_врача>_<инициал имени>_<инициал отчества>.xml)." +
                    e.Message);
            }
        }

        private DateTime checkDate(string s)
        {
            return (DateTime.TryParseExact(s, "yyyyddmm", CultureInfo.CurrentCulture, DateTimeStyles.None,
                out var result))
                ? result
                : throw new Exception($"Неверный формат даты. {s} не соответствует формату <yyyyddmm>.");
        }

        private int checkCartNoteId(string s)
        {
            return int.TryParse(s, out var result)
                ? result
                : throw new Exception(
                    $"Неверный формат CartNoteId. {s} не соответсвует числовому формату.");
        }

        private InternalDocType checkDocType(string s)
        {
            return Enum.TryParse<InternalDocType>(s, out var result)
                ? result
                : throw new Exception(
                    $"Неверный формат DocType. {s} не соответсвует формату InternalDocType.");
        }

        private string checkCardNum(string s)
        {
            return int.TryParse(s, out var result)
                ? s
                : throw new Exception(
                    $"Неверный формат CardNum. {s} не соответсвует числовому формату.");
        }

        private int checkDoctorCode(string s)
        {
            return int.TryParse(s, out var result)
                ? result
                : throw new Exception(
                    $"Неверный формат DoctorCode. {s} не соответсвует числовому формату.");
        }

        private void checkFIO(string[] s, out string error, out bool check)
        {
            error = String.Empty;
            check = true;

            if (string.IsNullOrEmpty(s[0].Trim()))
            {
                check = false;
                error = "Фамилия врача не определена.";
            }

            if (string.IsNullOrEmpty(s[1].Trim()))
            {
                check = false;
                error = "Имяили инициалы врача не определены.";
            }
        }

        private string getFileFromDB(int accountId)
        {
            return _treatRepository.GetDocumentByAccountId(accountId);
        }

        private MedRecord getMedRecord(string file)
        {
            if (!file.EndsWith("sgn", StringComparison.InvariantCultureIgnoreCase) &&
                !file.EndsWith("db", StringComparison.InvariantCultureIgnoreCase) &&
                !file.EndsWith("pdf", StringComparison.InvariantCultureIgnoreCase))
            {

                var fd = ParseFile(file);

                if (fd.FileDate.Date != _fileDate.Date)
                {
                    Log.Warning($"Дата файла FileDate.Date:{fd.FileDate.Date} отличается от _fileDate.Date:{_fileDate.Date}");
                    return null;
                }

                Log.Info($"Обрабатываю файл: {file}");
                var srv = SelectDocType(fd);
                if (srv == null)
                {
                    Log.Warning("Неизвестный тип файла: " + fd.FilePath);
                    return null;
                }

                return srv.CreateDocument();
            }

            Log.Warning($"Подписанный файл {file} не найден.");
            return null;
        }
    }
}
