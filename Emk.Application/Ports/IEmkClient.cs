using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface IEmkClient
    {
        Task<int> AddCaseAsync(PatientTreatDto treat, bool isUpdate = false);
        Task<int> UpdateCaseAsync(PatientTreatDto treat);
    }
}
