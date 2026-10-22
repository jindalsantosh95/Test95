using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class UpdatePassengerNameRecordRqMapper
    {
        public static UpdatePassengerNameRecordRQRequest Map(string token, OrderChangeRequestData orderChangeRequest)
        {
            var timeLimits = orderChangeRequest.TimeLimits;
            return new()
            {
                MessageHeader = new()
                {
                    From = new From
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },

                    To = new To
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },

                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.UpdatePNRAction,
                },

                Security = new() { BinarySecurityToken = token },
                UpdatePassengerNameRecordRQ = new UpdatePassengerNameRecordRQ()
                {
                    version = Constants.Application.UpdatePNRVersion,
                    Itinerary = new()
                    {
                        id = orderChangeRequest.Rloc,
                    },
                    PostProcessing = new()
                    {
                        EndTransaction = new()
                        {
                            Source = new()
                            {
                                ReceivedFrom = Constants.Application.SabreSource
                            }
                        }
                    },
                    TravelItineraryAddInfo = new()
                    {
                        AgencyInfo = new()
                        {
                            Ticketing = new()
                            {
                                TicketType = "7TAW",
                                TicketTimeLimit = timeLimits != null && timeLimits.Any(x => x.Type == TimeLimit.Types.TicketingTimeLimit) ? Convert.ToDateTime(timeLimits.FirstOrDefault(x => x.Type == TimeLimit.Types.TicketingTimeLimit)?.Limit).ToString("MM-ddTHH:00") : null
                            }
                        }
                    },
                    AirBook = new()
                    {
                        OriginDestinationInformation = GetFlightSegments(orderChangeRequest.OrderAmendments.First().FlightItems, orderChangeRequest.DataList.CustomerDetails.Count(x => x.Ptc != Constants.Application.Infant))
                    }
                }
            };
        }

        private static UpdatePassengerNameRecordRQAirBookFlightSegment[] GetFlightSegments(IEnumerable<FlightItems> flightItems, int paxCount)
        {
            return flightItems.Where(x => x.ActionContext == FlightItems.ActionContexts.Add).SelectMany(x => x.Segments)
                       .Select(x => new UpdatePassengerNameRecordRQAirBookFlightSegment()
                       {
                           ArrivalDateTime = x.ScheduledArrivalTime.LocalDateTime.ToString(),
                           DepartureDateTime = x.ScheduledDepartureTime.LocalDateTime.ToString(),
                           FlightNumber = x.MarketingFlightNumber,
                           NumberInParty = Convert.ToString(paxCount),
                           ResBookDesigCode = x.ClassOfService,
                           Status = Constants.Application.Status,
                           DestinationLocation = new()
                           {
                               LocationCode = x.DestinationAirportCode
                           },
                           MarketingAirline = new()
                           {
                               FlightNumber = x.MarketingFlightNumber,
                               Code = x.MarketingCarrier
                           },
                           OriginLocation = new()
                           {
                               LocationCode = x.OriginAirportCode
                           }

                       }).ToArray();
        }
    }
}