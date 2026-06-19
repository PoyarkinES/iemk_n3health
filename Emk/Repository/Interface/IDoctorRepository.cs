using Emk.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emk.Repository.Interface
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetDoctors();
    }
}
