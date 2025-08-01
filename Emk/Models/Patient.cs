using System;
using Emk.EmkSvc;

namespace Emk.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int Passport { get; set; }
        public int Polis { get; set; }
        public int Address { get; set; }
        public int IdProvider { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string CartNum { get; set; }

        private DateTime _dateOfBirth;
        public DateTime DateOfBirth
        {
            get { return _dateOfBirth == DateTime.MinValue ? DateTime.Now : _dateOfBirth; }
            set { _dateOfBirth = value; }
        }
        public int SexInt { get
            {
                switch (Sex.Trim())
                {
                    case "M":
                        return 1;
                    case "М":
                        return 1;
                    case "F":
                        return 2;
                    default: return 0;
                }
            }}
        public string Sex { get; set; }
        public string Number { get; set; }
        public string Serial { get; set; }
        public string OrgName { get; set; }

        private DateTime _giveOutDate;

        public DateTime GiveOutDate
        {
            get { return _giveOutDate == DateTime.MinValue ? DateTime.Now : _giveOutDate; }
            set { _giveOutDate = value; }
        }

        public int PostId { get; set; } = -1;
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string GlobalId { get; set; }

        public string Snils { get; set; }
    }
}
