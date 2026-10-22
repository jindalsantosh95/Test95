using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class TicketingV3EmailReceiptRQMapper
    {
        public static EmailReceiptRequestType MapToEmailReceiptRequest(string sessionId, string email, bool assignPrinter)
        {

            string printerAddress = assignPrinter ? ConfigReader.Get(Constants.Environment.PrinterData) : null;
            return new EmailReceiptRequestType()
            {
                EmailAddress = email,
                RequestInfo = new RequestInfoType()
                {
                    SessionId = sessionId,
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    Location = ConfigReader.Get(Constants.Environment.AgentCityCode),
                    HostContextParams = new HostContextParamsType { PoolType = ConfigReader.Get(Constants.Environment.PoolType) }
                },
                StampingInfo = new StampingInfoType()
                {
                    UserAuthorizationData = new UserAuthorizationDataType
                    {
                        AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                        RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                        SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                    },
                    PrinterAuthorizationData = new PrinterAuthorizationDataType
                    {
                        ATBPrinterAddress = printerAddress,
                        AdminPrinterAddress = printerAddress,
                        BagPrinterAddress = printerAddress,
                        BoardingCardPrinterAddress = printerAddress
                    }
                }
            };
        }
    }
}
