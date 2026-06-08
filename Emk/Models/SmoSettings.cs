using System;
using System.Collections.Generic;

namespace Emk.Models
{
	[Serializable]
	public class SmoSettings
    {
		public SmoSettings()
		{

		}
		public List<Doctor> Doctors { get; set; }
		public DefaultData Default { get; set; }
        
    }
}
