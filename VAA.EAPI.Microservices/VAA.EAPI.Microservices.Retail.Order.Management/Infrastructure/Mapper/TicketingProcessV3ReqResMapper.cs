using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper
{
    public class TicketingProcessV3ReqResMapper
    {
        public static RepriceRequestType TicketingProcessV3RepriceReqMapper(string sessionId, string transactionId, string location, Traveller passengerDetails, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            RepriceRequestType repriceRequestType = new RepriceRequestType();
            repriceRequestType.RequestInfo = new RequestInfoType()
            {
                ApplicationId = channelInfo.DlApplicationId,
                DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                SessionId = sessionId,
                Location = location,
                TransactionId = transactionId,
                HostContextParams = new HostContextParamsType { PoolType = channelInfo.DlPoolType }
            };
            repriceRequestType.StampingInfo = new StampingInfoType()
            {
                UserAuthorizationData = new UserAuthorizationDataType { AgentCityCode = location, RoleIndicator = channelInfo.DlRoleIndicator, SecurityId = channelInfo.DlAgentId },
            };
            repriceRequestType.RepriceOption = new RepriceOptionType1 { RepriceStoreAsBooked = Constants.Application.RepriceStoreAsBooked };
            repriceRequestType.PassengerInfo = new PassengerInfoType2 { FirstNameNumber = passengerDetails.FirstNameNumber, LastNameNumber = passengerDetails.LastNameNumber };
            return repriceRequestType;
        }
        public static PerformRevenueRequestType PerformRevenueReqMapper(string sessionId, string transactionId, string location, string emailAddress, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            PerformRevenueRequestType performRevenueReqType = new()
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = channelInfo.DlApplicationId,
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    SessionId = sessionId,
                    Location = location,
                    TransactionId = transactionId,
                    HostContextParams = new HostContextParamsType { PoolType = channelInfo.DlPoolType }
                },
                StampingInfo = new StampingInfoType()
                {
                    UserAuthorizationData = new UserAuthorizationDataType { AgentCityCode = location, RoleIndicator = channelInfo.DlRoleIndicator, SecurityId = channelInfo.DlAgentId },
                    PrinterAuthorizationData = new PrinterAuthorizationDataType { ATBPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), AdminPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), BagPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), BoardingCardPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData) }
                },
                RequestOptionsType = new RequestOptionType() { RequestOption2 = Constants.Application.RequestOption2 },
                Email = new EmailType() { EmailAddress = emailAddress, SendEmailData = Constants.Application.SendEmailData }
            };
            return performRevenueReqType;
        }
        public static List<string> RepriceResponseTypeMapper(RepriceResponseType repriceResponseType)
        {
            // Reprice mapping need to be done once delta service starts working
            List<string> repriceResoponse = new List<string>();
            if (repriceResponseType.AdvisoryMessage != null)
                repriceResoponse = repriceResponseType.AdvisoryMessage.Select(item => item.FreeformText).ToList();
            else
            {
                repriceResoponse.Add(Constants.Application.UnEvenExchange);
            }
            return repriceResoponse;
        }
    }
}
