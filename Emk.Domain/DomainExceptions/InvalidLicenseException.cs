using System;

namespace Emk.Domain.DomainExceptions
{
    [Serializable]
    public class InvalidLicenseException : DomainException
    {
        public InvalidLicenseException() : base("Лицензия недействительна.")
        {
        }

        public InvalidLicenseException(string message) : base(message)
        {
        }

        public InvalidLicenseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidLicenseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
