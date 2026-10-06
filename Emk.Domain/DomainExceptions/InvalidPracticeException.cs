using System;

namespace Emk.Domain.DomainExceptions
{
    [Serializable]
    public class InvalidPracticeException : DomainException
    {
        public InvalidPracticeException() : base("Идентификатор практики недействителен.")
        {
        }

        public InvalidPracticeException(string message) : base(message)
        {
        }

        public InvalidPracticeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidPracticeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
