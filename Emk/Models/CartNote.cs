using System;

namespace Emk.Models
{
    public class CartNote
    {
        public int Id { get; set; }
        public short GroupId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public int PatientId { get; set; }
        public short DoctorId { get; set; }
    }
}
