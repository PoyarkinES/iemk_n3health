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
        public LicenseRepository(string connectionString) : base(connectionString)
        {
        }

        public bool IsLicenseValid()
        {
            var data = Scalar<bool>(Resources.IsLicenseValid);
            return data;

            //using (var r = Connection.Query("select @errorStatus, @errorMessage, @licenseNumber from dba.cd_GetLicenseData('EGISZ',1)"))
            //{
            //    if (!r.HasRows)
            //        return false;
            //    License lic = new License();
            //    while (r.Read()) {
            //        lic.Status = r[0].ToString();
            //        lic.Message = r[1].ToString();
            //        lic.Number = r[2].ToString();
            //        return lic.IsValid;
            //    }
            //}
            //return false;
        }
    }
}
