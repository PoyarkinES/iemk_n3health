using System;
using Emk.Domain.DomainExceptions;

namespace Emk.Domain.ValueObjects
{
    /// <summary>
    /// Идентификатор практики (GUID)
    /// </summary>
    public sealed class EmkSettingId : IEquatable<EmkSettingId>
    {
        public Guid Value { get; }

        public EmkSettingId(Guid value)
        {
            if (!IsValid(value))
                throw new InvalidPracticeException("Идентификатор практики не может быть пустым.");
            Value = value;
        }

        public static bool IsValid(Guid value) => value != Guid.Empty;

        public static EmkSettingId Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            Guid guid;
            if (!Guid.TryParse(value, out guid))
                throw new InvalidPracticeException($"Некорректный формат идентификатора практики: {value}");
            return new EmkSettingId(guid);
        }

        public bool Equals(EmkSettingId other) => !ReferenceEquals(other, null) && Value == other.Value;

        public override bool Equals(object obj) => Equals(obj as EmkSettingId);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();
    }
}
