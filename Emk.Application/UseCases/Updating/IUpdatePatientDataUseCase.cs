using System.Threading.Tasks;

namespace Emk.Application.UseCases.Updating
{
    public interface IUpdatePatientDataUseCase
    {
        Task<UpdatePatientDataResponse> ExecuteAsync(UpdatePatientDataRequest request);
    }
}
