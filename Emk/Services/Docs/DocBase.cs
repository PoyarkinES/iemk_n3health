using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emk.EmkSvc;
using Emk.Models;

namespace Emk.Services.Docs
{
    public interface IDocumentInitializationDependencies
    {
        Task<CartNote> GetCartNote(int cartNoteId);
        Task<DoctorEmk> GetDoctorOfPatientTreat(int accountId);
        Task<Patient> GetPatient(int patientId);
        Task<Patient> GetPatient(string patientCartNum);
    }

    public interface IDocBase
    {
        Task InitializeAsync(IDocumentInitializationDependencies dependencies);
        MedRecord CreateDocument();
    }

    public abstract class DocBase : IDocBase
    {
        protected IDocumentInitializationDependencies InitializationDependencies { get; private set; }
        protected DoctorEmk DocDoctor { get; set; }
        protected Patient DocPatient { get; set; }


        protected string FilePath { get; }
        protected  int CartNoteId { get; }
        protected int AccountId { get; }

        protected CartNote CartNote { get; set; }

        protected abstract int DocCode { get; }

        protected abstract string NsType { get; }


        protected DocBase(string filePath, int cartNoteId, int accountId)
        {
            FilePath = filePath;
            CartNoteId = cartNoteId;
            AccountId = accountId;
        }

        public async Task InitializeAsync(IDocumentInitializationDependencies dependencies)
        {
            InitializationDependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
            CartNote = await InitializationDependencies.GetCartNote(CartNoteId);
            await InitializeDocumentAsync();
        }

        protected virtual async Task InitializeDocumentAsync()
        {
            if (CartNote == null)
                throw new ArgumentException($"Не найдена запись в амбулаторной карте с ИД {CartNoteId} невозможно загрузить доктора.");

            DocDoctor = await InitializationDependencies.GetDoctorOfPatientTreat(AccountId);
            DocPatient = await InitializationDependencies.GetPatient(CartNote.PatientId);
        }

        public abstract MedRecord CreateDocument();

        protected abstract int DocType { get; set; }

        protected virtual FileData ParseFile(string filePath)
        {
            try
            {

                FileData fd = new FileData();
                fd.FilePath = filePath;
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

    }
}
