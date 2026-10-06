using System;

namespace Emk.Domain.ValueObjects
{
    /// <summary>
    /// Базовые данные врача
    /// </summary>
    public sealed class DoctorInfo : IEquatable<DoctorInfo>
    {
        public int MemberId { get; }
        public string Surname { get; }
        public string Name { get; }
        public string MiddleName { get; }
        public string Snils { get; }

        public DoctorInfo(int memberId, string surname, string name, string middleName = null, string snils = null)
        {
            if (string.IsNullOrWhiteSpace(surname))
                throw new ArgumentException("Фамилия врача не задана.", nameof(surname));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя врача не задано.", nameof(name));
            MemberId = memberId;
            Surname = surname;
            Name = name;
            MiddleName = middleName;
            Snils = snils;
        }

        public bool Equals(DoctorInfo other) =>
            !ReferenceEquals(other, null)
            && MemberId == other.MemberId
            && Surname == other.Surname
            && Name == other.Name
            && MiddleName == other.MiddleName
            && Snils == other.Snils;

        public override bool Equals(object obj) => Equals(obj as DoctorInfo);

        public override int GetHashCode() => MemberId;
    }
}
