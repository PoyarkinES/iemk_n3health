using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Models
{
    public enum PayType
    {
        Unknown = 0,
        [Description("OMC")]
        OMS = 1,
        [Description("Бюджет")]
        Budget = 2,
        [Description("платные услуги")]
        Paid_Services = 3,
        [Description("ДМС")]
        DMS = 4,
        [Description("Собственные средства")]
        Own = 5,
        [Description("Другое")]
        Another = 6
    }
}
