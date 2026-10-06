using System;

namespace Emk.Domain.DomainExceptions
{
    [Serializable]
    public class NoPatientAccountsException : DomainException
    {
        public NoPatientAccountsException() : base("У пациента отсутствуют счета.")
        {
        }

        public NoPatientAccountsException(string message) : base(message)
        {
        }

        public NoPatientAccountsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoPatientAccountsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
