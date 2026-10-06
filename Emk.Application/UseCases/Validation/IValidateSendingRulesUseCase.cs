using System.Threading.Tasks;

namespace Emk.Application.UseCases.Validation
{
    public interface IValidateSendingRulesUseCase
    {
        Task<ValidateSendingRulesResponse> ExecuteAsync(ValidateSendingRulesRequest request);
    }
}
