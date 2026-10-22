using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Constants;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper
{
    public static class TicketingRetrieveMapper
    {
        public static RetrieveRequestType TicketingRetrieveRequestMapper(string ticketNumber)
        {
            return new RetrieveRequestType()
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = ConfigReader.Get(Environment.ApplicationId),
                    AppChannelName = ConfigReader.Get(Environment.AppChannelName),
                    DebugOptions = ConfigReader.Get(Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Environment.TestLab),
                    HostContextParams = new HostContextParamsType()
                    {
                        PoolType = ConfigReader.Get(Environment.PoolType)
                    }
                },
                TicketRequest = new TicketRequestType[]
                {
                    new TicketRequestType()
                    {
                        ElectronicDocumentNumber = ticketNumber,
                        OptionNumberForDisplayRequest = ConfigReader.Get(Environment.OptionNumberForDisplayRequest),
                        ProvideResidualValue = ConfigReader.Get(Environment.ProvideResidualValue),
                        TrustedSource = ConfigReader.Get(Environment.TrustedSource)
                    }

    }
            };
        }
    }
}
