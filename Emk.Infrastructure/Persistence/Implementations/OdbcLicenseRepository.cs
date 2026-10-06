using System.Threading.Tasks;
using Emk.Application.Ports;

namespace Emk.Infrastructure.Persistence.Implementations
{
    public sealed class OdbcLicenseRepository : OdbcRepository, ILicenseRepository
    {
        public OdbcLicenseRepository(string connectionString) : base(connectionString)
        {
        }

        public Task<bool> IsValidAsync()
        {
            return Scalar<bool>(SqlResources.Get("IsLicenseValid"));
        }
    }
}
