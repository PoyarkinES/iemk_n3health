using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Emk.Properties;
using Emk.Repository.Dto;

namespace Emk.Repository
{
    public class InsuranceCompanyRepository(string connectionString) : DbRepository(connectionString)
    {
        public async Task<IEnumerable<InsuranceCompanyDto>> GetInsuranseCompanies()
        {
            return await Query(Resources.GetInsuranseCompanies, InsuranceCompanyMap);
        }

        private InsuranceCompanyDto InsuranceCompanyMap(IDataReader reader)
        {
            return new InsuranceCompanyDto()
            {
                code = reader.Get<string>(nameof(InsuranceCompanyDto.code)),
                name = reader.Get<string>(nameof(InsuranceCompanyDto.name))
            };
        }
    }
}
