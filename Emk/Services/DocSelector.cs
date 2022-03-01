using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Emk.EmkSvc;
using Emk.Models;
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
        
        public DocSelector(Patient patient, DateTime fileDate, string patientDir)
        {
            _fileDate = fileDate;
            _patient = patient;
            _patientDir = patientDir;
        }

        public IEnumerable<MedRecord> GetDocs()
        {
            //var dir = Path.Combine(_settings.PatientDirectory,
            //    $"{_patient.LastName} {_patient.FirstName} {_patient.MiddleName} [{_patient.Id}]",
            //    "Дневниковые записи");
            if (!Directory.Exists(_patientDir))
                return null;

            Log.Info($"Получаю файлы из директории - {_patientDir}");

            List<MedRecord> docs = null;
            foreach (var file in Directory.GetFiles(_patientDir))
            {
                try
                {
                    if (file.EndsWith("sgn", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (file.EndsWith("db", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (file.EndsWith("pdf", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    var fd = ParseFile(file);
                    if (fd.FileDate.Date != _fileDate.Date)
                        continue;
                    Log.Info($"Обрабатываю файл: {file}");
                    var srv = SelectDocType(fd);
                    if (srv == null)
                    {
                        Log.Warning("Неизвестный тип файла: " + fd.FilePath);
                        continue;
                    }

                    if(docs == null)
                        docs = new List<MedRecord>();
                    docs.Add(srv.CreateDocument());
                }
                catch (Exception e)
                {
                   Log.Error(e.ToString());
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
                default:
                    return null;
            }

        }

        private FileData ParseFile(string filePath)
        {
            FileData fd = new FileData();
            fd.FilePath = filePath;
            if(!fd.FileExists)
                throw new FileNotFoundException("Файл не найден", filePath);

            string[] data = Path.GetFileName(filePath).Split('_');
            if (data.Length != 8)
                throw new ArgumentOutOfRangeException(nameof(filePath), $"Неверное наименование файла ({Path.GetFileName(filePath)}), ожидается формат (<yyyyddmm>_<cart_notes_id>_<тип файла>_<номер карты>_<код_врача>_<фио_врача>_<инициал имени>_<инициал отчества>.xml)");
            fd.FileDate = DateTime.ParseExact(data[0], "yyyyMMdd", CultureInfo.InvariantCulture);
            fd.CartNoteId = int.Parse(data[1]);
            fd.DocType = (InternalDocType)Enum.Parse(typeof(InternalDocType), data[2]);
            fd.PatientCartNum = data[3];
            fd.DoctorId = short.Parse(data[4]);
            return fd;
        }
    }
}
