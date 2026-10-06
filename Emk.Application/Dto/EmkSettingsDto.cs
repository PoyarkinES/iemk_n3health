using System;
using Emk.Domain.Enums;

namespace Emk.Application.Dto
{
    public class EmkSettingsDto
    {
        public int PracticeId { get; set; }
        public bool Enabled { get; set; }
        public string PixUrl { get; set; }
        public string EmkUrl { get; set; }
        public Guid IdLpu { get; set; }
        public int DateInterval { get; set; }
        public SendingType SendingType { get; set; }
        public DateTime IntervalFrom { get; set; }
        public DateTime IntervalTo { get; set; }
    }
}
