using System;
using System.Collections.Generic;
using Emk.Domain.Entities;

namespace Emk.Domain.Enums.Comparers
{
    public class DoctorComparer : IEqualityComparer<Doctor>
    {
        public bool Equals(Doctor x, Doctor y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.MemberId == y.MemberId
                && x.Surname == y.Surname
                && x.Name == y.Name
                && x.MiddleName == y.MiddleName
                && x.PersCode == y.PersCode
                && x.Snils == y.Snils;
        }

        public int GetHashCode(Doctor obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;
            return obj.MemberId;
        }
    }
}
