using System;

namespace Emk.Application.Dto
{
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string CartNum { get; set; }
        public string Snils { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Sex { get; set; }
    }
}
