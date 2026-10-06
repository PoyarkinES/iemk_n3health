using System.Threading.Tasks;

namespace Emk.Application.UseCases.Sending
{
    public interface ISendPatientDataUseCase
    {
        Task<SendPatientDataResponse> ExecuteAsync(SendPatientDataRequest request);
    }
}
