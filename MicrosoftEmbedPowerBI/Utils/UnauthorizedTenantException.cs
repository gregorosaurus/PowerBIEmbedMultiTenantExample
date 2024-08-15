using System;

namespace MicrosoftEmbedPowerBI.Utils
{
    public class UnauthorizedTenantException : UnauthorizedAccessException
    {
        public UnauthorizedTenantException() : base() { }
        public UnauthorizedTenantException(string message) : base(message) { }
    }
}
