using System;
using System.ServiceModel;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper
{
    public class Air4FaultExceptionChecker
    {
        public static ExternalServiceIntegrationException SeatV8Exception(FaultException e, string deltaServiceErrorCode)
        {
            var errorCode = ((FaultException<BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorCode;
            var errorMessage = ((FaultException<BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorMessage;

            return new ExternalServiceIntegrationException(deltaServiceErrorCode, new Exception($"{Constants.Application.ErrorCode} : {errorCode}  {Constants.Application.ErrorMessage} : {errorMessage}"));
        }
    }
}
