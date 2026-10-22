using System;
using System.ServiceModel;
using VAA.EAPI.Domain.Exceptions;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Infrastructure.Helper
{
    public class SabreFaultExceptionChecker
    {
        public static ExternalServiceIntegrationException Check(FaultException e)
        {
            return new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {e.Code.Name} {Constants.Application.ErrorMessage} : {e.Message}"));
        }
    }
}
