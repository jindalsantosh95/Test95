using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Flight.Services.Application.Query;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Mapper
{
    public static class FlightServiceReponseMapper
    {
        public static FlightServiceResponse ResponseMapper(List<SsrDetails> ssrDetails, FlightServiceQuery request, string offerIdGuid)
        {
            return new FlightServiceResponse()
            {
                Parties = request.Parties,
                Pos = request.Pos,
                OrderId = request.OrderId,
                Offers = new List<OfferDetails>()
                {
                    new OfferDetails()
                    {
                        OfferId = offerIdGuid,
                        OfferItems = ssrDetails.Select((x,offerItemCounter)=>{

                             return new OfferItem()
                                {
                                    OfferItemId = $"{offerIdGuid}|{++offerItemCounter}",
                                    SSRCode = x.SSR,
                                    Description = x.Description,
                                    Classification = x.Classification,
                                    PTC = ParsePtc(x.PTC)
                                };

                        })
                    }
                }
            };

        }
        private static IEnumerable<string> ParsePtc(string ptc)
        {
            if (string.IsNullOrWhiteSpace(ptc))
                return Array.Empty<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<string[]>(ptc) ?? Array.Empty<string>();
            }
            catch (System.Text.Json.JsonException)
            {
                return ptc.Trim('[', ']')
                          .Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => x.Trim().Trim('"'))
                          .Where(x => !string.IsNullOrWhiteSpace(x))
                          .ToArray();
            }
        }
    }
}
