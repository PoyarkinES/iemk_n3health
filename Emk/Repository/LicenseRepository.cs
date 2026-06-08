using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Repository
{
    public class LicenseRepository : DbRepository
    {
        public bool IsLicenseValid()
        {
            using (var r = Connection.Query("select @errorStatus, @errorMessage, @licenseNumber from dba.cd_GetLicenseData('EGISZ',1)"))
            {
                if (!r.HasRows)
                    return false;
                License lic = new License();
                while (r.Read()) {
                    lic.Status = r[0].ToString();
                    lic.Message = r[1].ToString();
                    lic.Number = r[2].ToString();
                    return lic.IsValid;
                }
            }
            return false;
        }
    }

    internal class License
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Number { get; set; }

        public bool IsValid => Status == "OK" && Message == "License Data found" && Number == "1";
    }


}
