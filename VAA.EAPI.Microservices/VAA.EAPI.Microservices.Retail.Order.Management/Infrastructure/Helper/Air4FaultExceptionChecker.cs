using System;
using System.ServiceModel;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper
{
    public class Air4FaultExceptionChecker
    {
        public static ExternalServiceIntegrationException PNRCommonV6Exception(FaultException e, string deltaServiceErrorCode)
        {
            var faultXmlData = e.Message == Constants.Application.BusinessFault ? ((FaultException<Proxy.PNRCommonV6Proxy.schemas.delta.com.common.fault.v4.BusinessFaultType>)e).Detail.Nodes[0].OuterXml : ((FaultException<Proxy.PNRCommonV6Proxy.schemas.delta.com.common.fault.v4.SystemFaultType>)e).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(deltaServiceErrorCode, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.Parse(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.Parse(faultXmlData, Constants.Application.ErrorMessage)}"));
        }
        public static ExternalServiceIntegrationException SegmentV4Exception(FaultException e, string deltaServiceErrorCode)
        {
            var errorCode = e.Message == Constants.Application.BusinessFault ? ((FaultException<Proxy.SegmentV4Proxy.BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorCode : ((FaultException<Proxy.SegmentV4Proxy.SystemFaultType>)e).Detail.FaultDetailedInfo.ErrorCode;
            var errorMessage = e.Message == Constants.Application.BusinessFault ? ((FaultException<Proxy.SegmentV4Proxy.BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<Proxy.SegmentV4Proxy.SystemFaultType>)e).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(deltaServiceErrorCode, new Exception($"{Constants.Application.ErrorCode} : {errorCode}  {Constants.Application.ErrorMessage} : {errorMessage}"));
        }
        public static ExternalServiceIntegrationException TicketingProcessV3Exception(FaultException e, string deltaServiceErrorCode)
        {
            var errorCode = RegExHelper.RemoveWhiteSpace(e.Message) == Constants.Application.BusinessFault ? ((FaultException<BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorCode : ((FaultException<SystemFaultType>)e).Detail.FaultDetailedInfo.ErrorCode;
            var errorMessage = RegExHelper.RemoveWhiteSpace(e.Message) == Constants.Application.BusinessFault ? ((FaultException<BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<SystemFaultType>)e).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(deltaServiceErrorCode, new Exception($"{Constants.Application.ErrorCode} : {errorCode}  {Constants.Application.ErrorMessage} : {errorMessage}"));
        }
        public static ExternalServiceIntegrationException SpecialServiceRequestV4Exception(FaultException e, string deltaServiceErrorCode)
        {
            var faultXmlData = e.Message == Constants.Application.BusinessFault ? ((FaultException<Proxy.SpecialServiceRequestV4Proxy.schemas.delta.com.common.fault.v4.BusinessFaultType>)e).Detail.Nodes[0].OuterXml : ((FaultException<Proxy.PNRCommonV6Proxy.schemas.delta.com.common.fault.v4.SystemFaultType>)e).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(deltaServiceErrorCode, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.Parse(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.Parse(faultXmlData, Constants.Application.ErrorMessage)}"));
        }
    }
}
