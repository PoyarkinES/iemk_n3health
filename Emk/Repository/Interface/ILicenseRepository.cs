using System.Threading.Tasks;

namespace Emk.Repository.Interface
{
    public interface ILicenseRepository: IDbRepository
    {
        Task<bool> IsLicenseValid();
    }
}
