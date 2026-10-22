using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual
{
    public sealed class AutoAccrualValidationException : InvalidOperationException
    {
        public string ErrorCode { get; }

        public AutoAccrualValidationException(string message, string errorCode = "") : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
