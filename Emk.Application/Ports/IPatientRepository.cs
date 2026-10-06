using System.Threading.Tasks;
using Emk.Application.Dto;

namespace Emk.Application.Ports
{
    public interface IPatientRepository
    {
        Task<PatientDto> GetByIdAsync(int patientId);
        Task<PatientDto> GetByCardNumberAsync(string cardNumber);
        Task<bool> CheckConsentToShareAsync(int patientId);
    }
}
