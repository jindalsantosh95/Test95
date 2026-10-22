using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class TicketingV3RetrieveRQMapper
    {
        public static RetrieveRequestType RetrieveRequestMapper(IEnumerable<string> ticketNumbers)
        {
            return new RetrieveRequestType()
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab)
                },
                TicketRequest = TicketRequestMapper(ticketNumbers)
            };
        }

        public static TicketRequestType[] TicketRequestMapper(IEnumerable<string> ticketNumbers)
        {
            List<TicketRequestType> ticketRequestTypes = new();
            foreach (var ticket in ticketNumbers ?? Array.Empty<string>())
            {
                TicketRequestType ticketRequestType = new()
                {
                    ElectronicDocumentNumber = ticket,
                    OptionNumberForDisplayRequest = Constants.Application.NumberForDisplayRequest,
                    ProvideResidualValue = Constants.Application.ProvideResidualValue,
                    TrustedSource = Constants.Application.TrustedSource
                };

                ticketRequestTypes.Add(ticketRequestType);
            }

            return ticketRequestTypes.ToArray();
        }
    }
}
