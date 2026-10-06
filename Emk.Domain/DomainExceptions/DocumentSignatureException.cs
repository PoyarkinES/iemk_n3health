using System;

namespace Emk.Domain.DomainExceptions
{
    [Serializable]
    public class DocumentSignatureException : DomainException
    {
        public DocumentSignatureException() : base("Документ не подписан или подпись недействительна.")
        {
        }

        public DocumentSignatureException(string message) : base(message)
        {
        }

        public DocumentSignatureException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DocumentSignatureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
