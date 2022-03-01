using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Emk.EmkSvc;
using Emk.Models;
using Emk.Repository;
using Emk.Services.Files;

namespace Emk.Services.Docs
{
    public interface IDocBase
    {
        MedRecord CreateDocument();
    }

    public abstract class DocBase : IDocBase
    {
        protected EmkSettings Settings = Factory.LoadSettings().First();
        protected IDoctorFileService SmoService = Factory.GetSmoService;
        protected EmkRepository EmkRep = Factory.GetEmkRepository;
        protected PatientRepository PatRep = Factory.GetPatientRepository;
        protected DoctorEmk DocDoctor { get;  }
        protected Patient DocPatient { get; }


        protected string FilePath { get; }
        protected  int CartNoteId { get; }

        protected CartNote CartNote { get; set; }

        protected abstract int DocCode { get; }

        protected abstract string NsType { get; }


        protected DocBase(string filePath, int cartNoteId)
        {
            FilePath = filePath;
            CartNoteId = cartNoteId;
            LoadCartNote();
            DocDoctor = GetDoctor();
            DocPatient = GetPatient();
        }

        public abstract MedRecord CreateDocument();



        protected virtual DoctorEmk GetDoctor()
        {
            if (CartNote == null)
                throw new ArgumentException($"Не найдена запись в амбулаторной карте с ИД {CartNoteId} невозможно загрузить доктора.");

            return EmkRep.GetDoctorOfPatientTreat(CartNote.PatientId, CartNote.DateAdded);
        }

        protected virtual Patient GetPatient()
        {
            if (CartNote == null)
                throw new ArgumentException($"Не найдена запись в амбулаторной карте с ИД {CartNoteId} невозможно загрузить пациента.");

            return PatRep.GetPatient(CartNote.PatientId);
        }

        protected virtual FileData ParseFile(string filePath)
        {
            FileData fd = new FileData();
            fd.FilePath = filePath;
            if (!fd.FileExists)
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


        private void LoadCartNote()
        {
            CartNote = EmkRep.GetCartNote(CartNoteId);
        }
    }
}
