using Emk.Properties;
using Emk.Repository.Interface;
using System.Threading.Tasks;

namespace Emk.Repository
{
    public class LicenseRepository(string connectionString) : DbRepository(connectionString), ILicenseRepository
    {
        public async Task<bool> IsLicenseValid()
        {
            return await Scalar<bool>(Resources.IsLicenseValid);
        }
    }
}
