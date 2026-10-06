using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;
using Emk.Repository;
using Emk.Services.Docs;
using Emk.Services.Docs.Referrals;
using Microsoft.SqlServer.Server;

namespace Emk.Services
{
    public class DocSelector(PatientAccount pa)
    {
        private readonly DateTime _fileDate = pa.TreatDate;
        private readonly PatientAccount _pa = pa;
        private readonly TreatRepository _treatRepository = Factory.GetTreatRepository;

        public async Task<List<MedRecord>> GetDocs(int accountId)
        {
            Log.Info($"Получаю файлы");

            var files = await GetFileFromDB(accountId);
            foreach (var file in files.Where(w=> string.IsNullOrEmpty(w.uuid)))
            {
                Log.Error($"Документ {file.efiles_name} не имеет идентификатора uuid. Требуется пересоздать документ");
            }

            var documents = new List<MedRecord>();
            foreach (var file in files.Where(w =>
                         !string.IsNullOrEmpty(w.uuid) && !string.IsNullOrEmpty(w.efiles_name)))
            {
                documents.Add(await GetMedRecord(file, accountId));
            }

            return documents;
        }

        private IDocBase SelectDocType(FileData fd, int accountId, string idMis = null)
        {
            switch (fd.DocType)
            {
                case InternalDocType.HospitalReferral:
                    return new DocHospitalReferral(fd.FilePath, fd.CartNoteId, accountId);
                case InternalDocType.ConsultReferral:
                    return new DocConsultReferral(fd.FilePath, fd.CartNoteId, accountId);
                case InternalDocType.ExamReferral:
                    return new DocExaminationReferral(fd.FilePath, fd.CartNoteId, accountId);
                case InternalDocType.Prescription:
                    return new DocPrescription(fd.FilePath, fd.CartNoteId, accountId);
                case InternalDocType.DischargeSummary:
                    return new DocDischargeSummary(fd.FilePath, fd.CartNoteId, accountId);
                case InternalDocType.DocControlCardDispensaryObservation:
                    return new DocControlCardDispensaryObservation(fd.FilePath, fd.CartNoteId, accountId);
                case InternalDocType.DocConsultNote:
                    return new DocConsultNote(fd.FilePath, fd.CartNoteId, accountId, idMis);
                default:
                    return null;
            }

        }

        private async Task<FileData> ParseFile(string filePath)
        {
            try
            {
                var filedir = await _treatRepository.GetFileDirectoryAsync(_pa.PracticeId);
                FileData fd = new FileData();
                fd.FilePath = filedir + "\\" + filePath;
                if (!fd.FileExists)
                    throw new FileNotFoundException("Файл не найден", filePath);

                string[] data = Path.GetFileName(filePath).Split('_');

                if (!data.Any())
                    throw new NotImplementedException();

                fd.FileDate = checkDate(data[0]);
                fd.CartNoteId = checkCartNoteId(data[1]);
                fd.DocType = checkDocType(data[2]);
                fd.PatientCartNum = data[3];
                fd.DoctorId = checkDoctorCode(data[4]);
                checkFIO(data.Skip(5).ToArray(), out string error, out bool check);

                if (!check) throw new Exception(error);

                return fd;
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Неверное наименование файла ({Path.GetFileName(filePath)}). " +
                    $"Ожидается формат (<yyyyMMdd>_<cart_notes_id>_<тип файла>_<номер карты>_<код_врача>_<фио_врача>_<инициал имени>_<инициал отчества>.xml)." +
                    e.Message);
            }
        }

        private DateTime checkDate(string s)
        {
            return (DateTime.TryParseExact(s, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None,
                out var result))
                ? result
                : throw new Exception($"Неверный формат даты. {s} не соответствует формату <yyyyMMdd>.");
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

        private async Task<List<DocumentsDto>> GetFileFromDB(int accountId)
        {
            return await _treatRepository.GetDocumentByAccountIdAsync(accountId);
        }

        private async Task<MedRecord> GetMedRecord(DocumentsDto file, int accountId)
        {
            if (!file.efiles_name.EndsWith("sgn", StringComparison.InvariantCultureIgnoreCase) &&
                !file.efiles_name.EndsWith("db", StringComparison.InvariantCultureIgnoreCase) &&
                !file.efiles_name.EndsWith("pdf", StringComparison.InvariantCultureIgnoreCase))
            {

                var fd = await ParseFile(file.efiles_name);

                if (fd.FileDate.Date != _fileDate.Date)
                {
                    Log.Warning($"Дата файла FileDate.Date:{fd.FileDate.Date} отличается от _fileDate.Date:{_fileDate.Date}");
                    return null;
                }

                Log.Info($"Обрабатываю файл: {file.efiles_name}");
                var srv = SelectDocType(fd, accountId, file.uuid);
                if (srv == null)
                {
                    Log.Warning("Неизвестный тип файла: " + fd.FilePath);
                    return null;
                }

                return srv.CreateDocument();
            }

            Log.Warning($"Подписанный файл {file.efiles_name} не найден.");
            return null;
        }
    }
}
