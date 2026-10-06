using System;
using System.ComponentModel;

namespace Emk.Domain.Enums
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
