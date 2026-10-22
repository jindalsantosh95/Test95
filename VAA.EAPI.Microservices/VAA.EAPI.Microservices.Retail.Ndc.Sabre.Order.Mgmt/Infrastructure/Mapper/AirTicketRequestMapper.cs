using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class AirTicketRequestMapper
    {
        public static AirTicketRQRequest GetAirticketRequest(string token, OrderChangeRequestData orderChangeRequestData, string pcc)
        {
            return new()
            {
                AirTicketRQ = AirTicketReqMapper(orderChangeRequestData, pcc),
                MessageHeader = new()
                {
                    Action = Constants.Application.AirTicketRQAction,
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

        private static AirTicketRQ AirTicketReqMapper(OrderChangeRequestData orderChangeRequestData, string pcc)
        {
            return new()
            {
                version = Constants.Application.SabreVersion,
                targetCity = pcc,
                Itinerary = new()
                {
                    ID = orderChangeRequestData.Rloc
                },
                DesignatePrinter = new()
                {
                    Profile = new()
                    {
                        Number = ConfigReader.Get(Constants.Environment.DirectTicketingPCC).Split(",").Contains(pcc) ? "2" : Constants.Application.DesignatePrinterProfileNumber
                    }
                },
                Ticketing = TicketingMapper(orderChangeRequestData),
                PostProcessing = new()
                {
                    acceptNegotiatedFare = true,
                    acceptPriceChanges = true,
                    actionOnPQExpired = AirTicketRQPostProcessingActionOnPQExpired.Q,
                    EndTransaction = new()
                    {
                        Source = new() { ReceivedFrom = Constants.Application.SabreSource }
                    }
                }
            };
        }
        private static AirTicketRQTicketing[] TicketingMapper(OrderChangeRequestData orderChangeRequestData)
        {
            return orderChangeRequestData.DataList.CustomerDetails.GroupBy(x => x.Ptc).Select((x, index) =>
            {
                return new AirTicketRQTicketing()
                {
                    FOP_Qualifiers = new()
                    {
                        BasicFOP = new()
                        {
                            Type = Constants.Application.PaymentType,
                        }
                    },
                    FlightQualifiers = new()
                    {
                        VendorPrefs = new()
                        {
                            Airline = new() { Code = Constants.Application.OwnerAirline }
                        }
                    },
                    PricingQualifiers = new()
                    {
                        PriceQuote = [new() { Record = [new() { Number = $"{index + 1}" }] }]
                    }

                };
            }).ToArray();
        }

    }
}