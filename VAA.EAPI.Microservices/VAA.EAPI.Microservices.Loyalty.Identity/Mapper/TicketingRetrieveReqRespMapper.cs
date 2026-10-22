using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class TicketingRetrieveReqRespMapper
    {
        public static RetrieveRequestType TicketingRetrieveRequestMapper(AcquireTokenRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new RetrieveRequestType
            {
                RequestInfo = new RequestInfoType
                {
                    ApplicationId = ConfigReader.Get(Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Environment.TestLab),
                    HostContextParams = new HostContextParamsType
                    {
                        PoolType = ConfigReader.Get(Environment.PoolType)
                    }
                },
                TicketRequest = new[]
                {
                    new TicketRequestType
                    {
                        ElectronicDocumentNumber =  request.TicketRef,
                        OptionNumberForDisplayRequest = ConfigReader.Get(Environment.OptionNumberForDisplayRequest),
                        ProvideResidualValue = ConfigReader.Get(Environment.ProvideResidualValue),
                        TrustedSource = ConfigReader.Get(Environment.TrustedSource)
                    }
                }
            };
        }
        public static List<Claim> ResponseMapper(RetrieveResponseType ticketingResponse, AcquireTokenRequest request)
        {
            var claims = new List<Claim>();

            if (request == null) throw new ArgumentNullException(nameof(request));

            if (ticketingResponse != null)
            {
                foreach (var tktResp in ticketingResponse.TicketRequestResponse.First().TicketResponse)
                {
                    if (tktResp.PassengerInformation.PassengerName.Equals($"{request.LastName.ToUpper()}{request.FirstName.ToUpper()}"))
                    {
                        var claim = new Claim
                        {
                            Type = request.Type,
                            TicketRef = tktResp.TicketInformation.FirstTicketNumber,
                            HashKey = Hash.SHA256Managed($"{request.FirstName.ToUpper()}{request.LastName.ToUpper()}"),
                            CreationDate = DateTimeHelper.Air4PNRDateTimeConversion(tktResp.TicketInformation.TicketCreateDate, "0000A")?.ToString(Constants.Application.ISODateTimeFormat),
                        };
                        claims.Add(claim);
                    }
                }
            }

            return claims;
        }
    }
}
