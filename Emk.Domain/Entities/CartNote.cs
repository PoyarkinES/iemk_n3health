using System;

namespace Emk.Domain.Entities
{
    public class CartNote
    {
        public CartNote()
        {
        }

        public CartNote(int patientId, int doctorId, DateTime dateAdded, string description)
        {
            PatientId = patientId;
            DoctorId = doctorId;
            DateAdded = dateAdded;
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public int Id { get; set; }
        public short GroupId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
    }
}
