using System;
using System.ComponentModel;

namespace Emk.Domain.Entities
{
	[Serializable]
	public class BaseDoctor
    {
		[Browsable(false)]
		public int MemberId { get; set; }
		[DisplayName("СНИЛС")]
		public string Snils { get; set; }
		[DisplayName("Специальность")]
		public int Speciality { get; set; }
		[DisplayName("Должность")]
		public int Position { get; set; }
        
    }
}
