using Emk.Models;
using System.Collections.Generic;

namespace Emk.Repository.Interface
{
    public interface IDoctorRepository
    {
        IEnumerable<Doctor> GetDoctors();
    }
}
