using System;
using System.ComponentModel;

namespace Emk.Domain.Entities
{
	/// <summary>
	/// Экземпляр врача из БД
	/// </summary>
	[Serializable]
	public class Doctor : BaseDoctor
	{
		public Doctor()
		{
		}

		public Doctor(int memberId, string surname, string name)
		{
			MemberId = memberId;
			Surname = surname ?? throw new ArgumentNullException(nameof(surname));
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

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
