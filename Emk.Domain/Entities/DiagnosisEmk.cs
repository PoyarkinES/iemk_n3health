using System;

namespace Emk.Domain.Entities
{
	public class DiagnosisEmk
	{
		public DiagnosisEmk()
		{
		}

		public DiagnosisEmk(string diagnosisCode, string diagnosisName)
		{
			DiagnosisCode = diagnosisCode ?? throw new ArgumentNullException(nameof(diagnosisCode));
			DiagnosisName = diagnosisName;
		}

		/// <summary>
		/// Название(комментарий)
		/// </summary>
		public string DiagnosisName { get; set; }
		/// <summary>
		/// МКБ Код
		/// </summary>
		public string DiagnosisCode { get; set; }
	}
}
