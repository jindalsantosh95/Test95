using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class SetupContextRequestMapper
    {
        public static SetupContextRequestType Mapper(string pnr, bool assignPrinter)
        {
            string printerAddress = assignPrinter ? ConfigReader.Get(Constants.Environment.PrinterData) : null;
            return new SetupContextRequestType()
            {
                RequestInfo = new RequestInfoType()
                {
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
                },
                RetrieveInfo = new RetrieveInfoType()
                {
                    PassengerInfo = new PassengerInfoType { RecordLocator = pnr }
                }
            };
        }
    }
}
