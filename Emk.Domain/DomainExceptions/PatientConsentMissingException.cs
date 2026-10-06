using System;

namespace Emk.Domain.DomainExceptions
{
    [Serializable]
    public class PatientConsentMissingException : DomainException
    {
        public PatientConsentMissingException() : base("Отсутствует согласие пациента на передачу персональных данных.")
        {
        }

        public PatientConsentMissingException(string message) : base(message)
        {
        }

        public PatientConsentMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PatientConsentMissingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
