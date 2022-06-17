using System;

namespace Emk.Models
{
	public class EmkSettings
	{
		public string SmoSettingsPath { get; set; }
		public string DbConnectionString { get; set; }
		public string PatientDirectory { get; set; }
		public string PixUrl { get; set; }
		public string EmkUrl { get; set; }
		public Guid Guid { get; set; }
		public Guid IdLPU { get; set; }
		public TimeSpan UpdateTime { get; set; }
		public int DateInterval { get; set; }
        public SendingType SendingType { get; set; }
        public DateTime IntervalFrom { get; set; }
        public DateTime IntervalTo { get; set; }
        public int PracticeId { get; set; }
        public bool Enabled { get; set; }

        public override string ToString()
		{
			return $"PatDir: {PatientDirectory} Pix: {PixUrl} EMK: {EmkUrl} Interval: {DateInterval} Time: {UpdateTime}";
		}
	}
}
