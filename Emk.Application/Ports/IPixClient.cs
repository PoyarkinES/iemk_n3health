using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface IPixClient
    {
        Task<bool> AddPatientAsync(PatientAccountDto patient);
        Task<bool> UpdatePatientAsync(PatientAccountDto patient);
    }
}
