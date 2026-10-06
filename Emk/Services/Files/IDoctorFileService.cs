using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Models;

namespace Emk.Services.Files
{
    public interface IDoctorFileService
    {
        Task InitializeAsync();
        DefaultData LoadDefaults();
        List<Doctor> LoadDoctorsFromFile();
        Task<List<Doctor>> LoadDoctorsFromDbAsync();
        void SaveDefaults(DefaultData d);
        void SaveDoctorsToFile(List<Doctor> d);
    }
}