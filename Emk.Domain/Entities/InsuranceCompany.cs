using System;

namespace Emk.Domain.Entities
{
    public class InsuranceCompany
    {
        public InsuranceCompany()
        {
        }

        public InsuranceCompany(int id, string code, string name)
        {
            Id = id;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int ProviderId { get; set; }
    }
}
