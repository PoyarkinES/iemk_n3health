using System;

namespace Emk.Domain.Entities
{
	public class DoctorEmk : Doctor
	{
		public DateTime BirthDay { get; set; }
		public int AccountId { get; set; }
		public string IdLpu { get; set; }
		public string SexStr { get; set; }

		public DoctorEmk DepartmentHead { get; set; }
	}
}
