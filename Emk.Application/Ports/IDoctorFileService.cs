using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Domain.Entities;

namespace Emk.Application.Ports
{
    public interface IDoctorFileService
    {
        Task InitializeAsync();
        DefaultData LoadDefaults();
        List<Doctor> LoadDoctorsFromFile();
        Task<List<Doctor>> LoadDoctorsFromDbAsync();
        Task SaveDefaultsAsync(DefaultData d);
        Task SaveDoctorsToFileAsync(List<Doctor> doctors);
    }
}
