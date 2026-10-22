using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;


namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class EnhancedAirTicketRequestMapper
    {
        public static AirTicketRQRequest GetEnhancedAirTicketRequest(string token, string rloc, PricedItineraryPNRB[] pricedItinerary, string pcc)
        {
            return new()
            {
                AirTicketRQ = AirTicketReqMapper(rloc, pricedItinerary, pcc),
                MessageHeader = new()
                {
                    Action = Constants.Application.EnhancedAirTicketAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }
            };
        }

        private static AirTicketRQ AirTicketReqMapper(string rloc, PricedItineraryPNRB[] pricedItinerary, string pcc)
        {
            return new()
            {
                version = Constants.Application.SabreVersion,
                targetCity = pcc,
                DesignatePrinter = new()
                {
                    Profile = new()
                    {
                        Number = ConfigReader.Get(Constants.Environment.DirectTicketingPCC).Split(",").Contains(pcc) ? "2" : Constants.Application.DesignatePrinterProfileNumber
                    }
                },
                Itinerary = new()
                {
                    ID = rloc
                },
                Ticketing = TicketingMapper(pricedItinerary),
                PostProcessing = new()
                {
                    EndTransaction = new()
                    {
                        Source = new() { ReceivedFrom = Constants.Application.SabreSource }
                    }
                }
            };
        }

        private static AirTicketRQTicketing[] TicketingMapper(PricedItineraryPNRB[] pricedItinerary)
        {
            var matchingItinerary = pricedItinerary.Where(itinerary => itinerary.InputMessage.StartsWith("WFR")).Select(itinerary => itinerary.SequenceNumber);

            return matchingItinerary.Select(sequenceNumber =>
            {
                return new AirTicketRQTicketing()
                {
                    PricingQualifiers = new()
                    {
                        PriceQuote = [new() { Record = [new() { Number = $"{sequenceNumber}", Reissue = true, ReissueSpecified = true }] }]
                    }
                };
            }).ToArray();
        }
    }
}