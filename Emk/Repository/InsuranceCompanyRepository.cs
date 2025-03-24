using System.Collections.Generic;
using System.Data;
using Emk.Properties;
using Emk.Repository.Dto;

namespace Emk.Repository
{
    public class InsuranceCompanyRepository : DbRepository
    {
        public IEnumerable<InsuranceCompanyDto> GetInsuranseCompanies()
        {
            var data = Query(Resources.GetInsuranseCompanies, InsuranceCompanyMap);
            return data;
        }

        private InsuranceCompanyDto InsuranceCompanyMap(IDataReader reader)
        {
            return new InsuranceCompanyDto()
            {
                code = reader.Get<string>(nameof(InsuranceCompanyDto.code)),
                name = reader.Get<string>(nameof(InsuranceCompanyDto.name))
            };
        }

        public InsuranceCompanyRepository(string connectionString) : base(connectionString)
        {
        }
    }
}
