using System;

namespace Emk.Domain.Entities
{
    public class DbSettings
    {
        public DbSettings()
        {
        }

        public DbSettings(string paramCode, int practiceId, string propValue)
        {
            ParamCode = paramCode ?? throw new ArgumentNullException(nameof(paramCode));
            PracticeId = practiceId;
            PropValue = propValue;
        }

        public string ParamCode { get; set; }
        public int PropId { get; set; }
        public int PracticeId { get; set; }
        public string PropValue { get; set; }
    }
}
