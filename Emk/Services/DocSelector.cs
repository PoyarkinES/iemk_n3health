using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;
using Emk.Services.Docs;
using Emk.Services.Docs.Referrals;

namespace Emk.Services
{
    public interface IDocSelectorRepository
    {
        Task<List<DocumentsDto>> GetDocumentByAccountIdAsync(int accountId);
        Task<string> GetFileDirectoryAsync(int practiceId);
    }

    public class DocSelector
    {
        private readonly DateTime _fileDate;
        private readonly PatientAccount _pa;
        private readonly IDocSelectorRepository _repository;
        private readonly IDocumentInitializationDependencies _documentDependencies;
        private Task<string> _fileDirectoryTask;

        public DocSelector(PatientAccount pa)
            : this(pa, new FactoryDocSelectorRepository(), new FactoryDocumentInitializationDependencies())
        {
        }

        public DocSelector(PatientAccount pa, IDocSelectorRepository repository)
            : this(pa, repository, new FactoryDocumentInitializationDependencies())
        {
        }

        public DocSelector(
            PatientAccount pa,
            IDocSelectorRepository repository,
            IDocumentInitializationDependencies documentDependencies)
        {
            _pa = pa ?? throw new ArgumentNullException(nameof(pa));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _documentDependencies = documentDependencies ??
                throw new ArgumentNullException(nameof(documentDependencies));
            _fileDate = pa.TreatDate;
        }

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
                var document = await GetMedRecord(file, accountId);
                if (document != null)
                    documents.Add(document);
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
                var filedir = await GetFileDirectoryAsync();
                FileData fd = new FileData();
                fd.FilePath = filedir + "\\" + filePath;
                if (!fd.FileExists)
                    throw new FileNotFoundException("Файл не найден", filePath);

                string[] data = Path.GetFileName(filePath).Split('_');

                if (data.Length < 7)
                    throw new FormatException("Недостаточно частей в имени файла.");

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
                throw new FormatException(
                    $"Неверное наименование файла ({Path.GetFileName(filePath)}). " +
                    $"Ожидается формат (<yyyyMMdd>_<cart_notes_id>_<тип файла>_<номер карты>_<код_врача>_<фио_врача>_<инициал имени>_<инициал отчества>.xml)." +
                    e.Message, e);
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

            if (s.Length < 2)
            {
                check = false;
                error = "Фамилия и имя врача не определены.";
                return;
            }

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
            return await _repository.GetDocumentByAccountIdAsync(accountId);
        }

        private Task<string> GetFileDirectoryAsync() =>
            _fileDirectoryTask ?? (_fileDirectoryTask = _repository.GetFileDirectoryAsync(_pa.PracticeId));

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

                await srv.InitializeAsync(_documentDependencies);
                return await srv.CreateDocumentAsync();
            }

            Log.Warning($"Подписанный файл {file.efiles_name} не найден.");
            return null;
        }
    }
}
