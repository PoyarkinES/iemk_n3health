using System;
using System.Linq;
using Emk.Domain.DomainExceptions;

namespace Emk.Domain.ValueObjects
{
    /// <summary>
    /// Строка лицензии: непустая, без пробелов, только буквы, цифры и дефис
    /// </summary>
    public sealed class LicenseKey : IEquatable<LicenseKey>
    {
        public string Value { get; }

        public LicenseKey(string value)
        {
            if (!IsValid(value))
                throw new InvalidLicenseException("Некорректный формат лицензионного ключа.");
            Value = value;
        }

        public static bool IsValid(string value) =>
            !string.IsNullOrEmpty(value) && value.All(c => char.IsLetterOrDigit(c) || c == '-');

        public bool IsValid() => IsValid(Value);

        public bool Equals(LicenseKey other) => !ReferenceEquals(other, null) && Value == other.Value;

        public override bool Equals(object obj) => Equals(obj as LicenseKey);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }
}
