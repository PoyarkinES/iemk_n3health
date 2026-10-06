using System.Threading.Tasks;

namespace Emk.Application.Ports
{
    public interface ILicenseRepository
    {
        Task<bool> IsValidAsync();
    }
}
