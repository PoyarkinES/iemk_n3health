using Emk.Models;
using System.Collections.Generic;

namespace Emk.Repository
{
    public class InsuranceCompanyRepository : DbRepository
    {
        public List<InsuranceCompany> GetInsuranseCompanies()
        {
            List<InsuranceCompany> comps = new List<InsuranceCompany>();
            using (var reader = Connection.Query("select third_parties.code, third_parties.name from third_parties where third_parties.thp_type = 1")) {
                InsuranceCompany doc = new InsuranceCompany();
                while(reader.Read()) {
                    doc.Code = reader[0].ToString();
                    doc.Name = reader[1].ToString();
                    comps.Add(doc);
                }
            }
            return comps;
        }
    }
}
