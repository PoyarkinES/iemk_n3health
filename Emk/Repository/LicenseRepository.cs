using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emk.Interface;
using Emk.Models;
using Emk.Properties;

namespace Emk.Repository
{
    public class LicenseRepository : DbRepository, ILicenseRepository
    {
        public bool IsLicenseValid()
        {
            var data = Scalar<bool>(Resources.IsLicenseValid);
            return data;
        }
    }
}
