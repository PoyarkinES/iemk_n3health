using System.Collections.Generic;
using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface IDoctorRepository
    {
        Task<DoctorDto> GetByMemberIdAsync(int memberId);
        Task<List<DoctorDto>> GetAllAsync();
    }
}
