using System.Collections.Generic;
using Emk.Models;

namespace Emk.Services.Files
{
    public interface IDoctorFileService
    {
        DefaultData LoadDefaults();
        List<Doctor> LoadDoctorsFromFile();
        List<Doctor> LoadDoctorsFromDb();
        void SaveDefaults(DefaultData d);
        void SaveDoctorsToFile(List<Doctor> d);
    }
}