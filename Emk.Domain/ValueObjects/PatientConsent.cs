using System;
using Emk.Domain.DomainExceptions;

namespace Emk.Domain.ValueObjects
{
    /// <summary>
    /// Согласие пациента на передачу персональных данных
    /// </summary>
    public sealed class PatientConsent : IEquatable<PatientConsent>
    {
        public bool IsGiven { get; }
        public string Explanation { get; }

        public PatientConsent(bool isGiven, string explanation = null)
        {
            IsGiven = isGiven;
            Explanation = explanation ?? string.Empty;
        }

        /// <summary>
        /// Бросает <see cref="PatientConsentMissingException"/>, если согласие не получено
        /// </summary>
        public void EnsureGiven()
        {
            if (!IsGiven)
                throw new PatientConsentMissingException(
                    string.IsNullOrEmpty(Explanation)
                        ? "Отсутствует согласие пациента на передачу персональных данных."
                        : Explanation);
        }

        public bool Equals(PatientConsent other) =>
            !ReferenceEquals(other, null) && IsGiven == other.IsGiven && Explanation == other.Explanation;

        public override bool Equals(object obj) => Equals(obj as PatientConsent);

        public override int GetHashCode() => (IsGiven.GetHashCode() * 397) ^ Explanation.GetHashCode();
    }
}
