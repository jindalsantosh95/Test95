using System;
using System.ServiceModel;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Exceptions;

namespace VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper
{
    public class Air4FaultExceptionChecker
    {
        public static ExternalServiceIntegrationException PNRRetrieveException(FaultException e)
        {
            var faultXmlData = e.Message == Constants.Application.BusinessFault ? ((FaultException<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.schemas.delta.com.common.fault.v4.BusinessFaultType>)e).Detail.Nodes[0].OuterXml : ((FaultException<EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.schemas.delta.com.common.fault.v4.SystemFaultType>)e).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorMessage)}"));
        }
        public static ExternalServiceIntegrationException TicketingRetrieveException(FaultException e)
        {
            return new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {((FaultException<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorCode}  {Constants.Application.ErrorMessage} : {((FaultException<EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.BusinessFaultType>)e).Detail.FaultDetailedInfo.ErrorMessage}"));
        }
        public static ExternalServiceIntegrationException PerformRevenue(FaultException e)
        {
            var faultXmlData = e.Message == "Business Fault" ? ((FaultException<EAPI.Infrastructure.PSS.Proxy.schemas.delta.com.common.fault.v4.BusinessFaultType>)e).Detail.Nodes[0].OuterXml : ((FaultException<EAPI.Infrastructure.PSS.Proxy.schemas.delta.com.common.fault.v4.SystemFaultType>)e).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorMessage)}"));
        }

        public static ExternalServiceIntegrationException PassengerV4Exception(FaultException e)
        {
            var faultXmlData = e.Message == Constants.Application.BusinessFault ? ((FaultException<Proxy.PassengerV4Proxy.schemas.delta.com.common.fault.v4.BusinessFaultType>)e).Detail.Nodes[0].OuterXml : ((FaultException<Proxy.PassengerV4Proxy.schemas.delta.com.common.fault.v4.SystemFaultType>)e).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorMessage)}"));
        }
        public static ExternalServiceIntegrationException SchedulesAvailabilityException(FaultException faultException)
        {
            var faultMessage = (faultException.Message == "Business Fault") ? ((FaultException<Proxy.SchedulesAvailability.BusinessFaultType1>)faultException).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<Proxy.SchedulesAvailability.SystemFaultType1>)faultException).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(string.Empty, new Exception(faultMessage));
        }
        public static ExternalServiceIntegrationException SeatAvailabilityException(FaultException faultException)
        {
            var faultMessage = (faultException.Message == "Business Fault") ? ((FaultException<Proxy.SeatAvailability.BusinessFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<Proxy.SeatAvailability.SystemFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(string.Empty, new Exception(faultMessage));
        }
        public static ExternalServiceIntegrationException HostContextException(FaultException faultException)
        {
            var faultMessage = (faultException.Message == "Business Fault") ? ((FaultException<Proxy.HostContext.BusinessFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<Proxy.HostContext.SystemFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(string.Empty, new Exception(faultMessage));
        }
        public static ExternalServiceIntegrationException CreditCardV2Exception(FaultException e)
        {
            var faultXmlData = e.Message == "Business Fault" ? ((FaultException<Proxy.CreditCardV2.schemas.delta.com.common.fault.v4.BusinessFaultType>)e).Detail.Nodes[0].OuterXml : ((FaultException<Proxy.CreditCardV2.schemas.delta.com.common.fault.v4.SystemFaultType>)e).Detail.Nodes[0].OuterXml;
            return new ExternalServiceIntegrationException(string.Empty, new Exception($"{Constants.Application.ErrorCode} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorCode)}  {Constants.Application.ErrorMessage} : {XmlParse.GetAttributesValue(faultXmlData, Constants.Application.ErrorMessage)}"));
        }
        public static ExternalServiceIntegrationException RemarksV3Exception(FaultException faultException)
        {
            var faultMessage = (faultException.Message == "Business Fault") ? ((FaultException<Proxy.RemarksV3.BusinessFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<Proxy.RemarksV3.SystemFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(string.Empty, new Exception(faultMessage));
        }
        public static ExternalServiceIntegrationException TicketingProcessV3Exception(FaultException faultException)
        {
            var faultMessage = (faultException.Message == "Business Fault") ? ((FaultException<Proxy.TicketingProcessV3Proxy.BusinessFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage : ((FaultException<Proxy.TicketingProcessV3Proxy.SystemFaultType>)faultException).Detail.FaultDetailedInfo.ErrorMessage;
            return new ExternalServiceIntegrationException(string.Empty, new Exception(faultMessage));
        }
    }
}
