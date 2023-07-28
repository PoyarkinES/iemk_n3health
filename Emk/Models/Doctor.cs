using System;
using System.ComponentModel;

namespace Emk.Models
{
	/// <summary>
	/// Экземпляр врача из БД
	/// </summary>
	[Serializable]
	public class Doctor : BaseDoctor
	{

        [DisplayName("Код")]
		public string PersCode { get; set; }
		[DisplayName("Фамилия")]
		public string Surname { get; set; }
		[DisplayName("Имя")]
		public string Name { get; set; }
		[DisplayName("Отчество")]
		public string MiddleName { get; set; }
        [DisplayName("Пол")] public string Sex { get; set; }
    }
}
