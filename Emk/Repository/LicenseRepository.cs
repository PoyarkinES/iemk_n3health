using Emk.Properties;
using Emk.Repository.Interface;

namespace Emk.Repository
{
    public class LicenseRepository : DbRepository, ILicenseRepository
    {
        public bool IsLicenseValid()
        {
            var data = Scalar<bool>(Resources.IsLicenseValid);
            return data;
        }

        public LicenseRepository(string connectionString) : base(connectionString)
        {
        }
    }
}
