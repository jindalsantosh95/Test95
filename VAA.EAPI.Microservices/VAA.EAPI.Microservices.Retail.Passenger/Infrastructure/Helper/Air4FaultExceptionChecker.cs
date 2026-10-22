using System;
using System.ServiceModel;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy.schemas.delta.com.common.fault.v4;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper
{
    internal class Air4FaultExceptionChecker
    {
        public static ExternalServiceIntegrationException Air4Exception(FaultException faultException)
        {
            var faultXmlData = faultException.Message == Constants.Application.BusinessFault ? ((FaultException<BusinessFaultType>)faultException).Detail.Nodes[0].OuterXml : ((FaultException<SystemFaultType>)faultException).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(Constants.ErrorCodes.DeltaErrorCode, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorMessage)}"));
        }

    }
}
