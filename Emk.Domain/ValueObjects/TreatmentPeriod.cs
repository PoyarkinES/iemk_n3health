using System;
using Emk.Domain.DomainExceptions;

namespace Emk.Domain.ValueObjects
{
    /// <summary>
    /// Период лечения: StartDate строго меньше EndDate
    /// </summary>
    public sealed class TreatmentPeriod : IEquatable<TreatmentPeriod>
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public TreatmentPeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new DomainException("Дата начала периода лечения должна быть меньше даты окончания.");
            StartDate = startDate;
            EndDate = endDate;
        }

        public bool Contains(DateTime date) => date >= StartDate && date <= EndDate;

        public bool Equals(TreatmentPeriod other) =>
            !ReferenceEquals(other, null) && StartDate == other.StartDate && EndDate == other.EndDate;

        public override bool Equals(object obj) => Equals(obj as TreatmentPeriod);

        public override int GetHashCode() => (StartDate.GetHashCode() * 397) ^ EndDate.GetHashCode();
    }
}
