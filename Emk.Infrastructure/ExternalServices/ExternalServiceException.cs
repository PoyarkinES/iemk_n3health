using System;

namespace Emk.Infrastructure.ExternalServices
{
    public sealed class ExternalServiceException : Exception
    {
        public ExternalServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
