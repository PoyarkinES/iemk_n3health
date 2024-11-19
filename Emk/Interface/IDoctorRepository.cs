using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emk.Models.Dto;

namespace Emk.Interface
{
    public interface IDoctorRepository: IDbRepository
    {
        IEnumerable<DoctorDto> GetDoctors();
    }
}
