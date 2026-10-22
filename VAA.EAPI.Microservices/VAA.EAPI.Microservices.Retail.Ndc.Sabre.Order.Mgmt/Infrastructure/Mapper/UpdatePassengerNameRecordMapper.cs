using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class UpdatePassengerNameRecordMapper
    {
        public static UpdatePassengerNameRecordRQRequest UpdatePnrReqMapper(string token, OrderChangeRequestData orderChangeRequestData,EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB getReservationResponse,[Optional] bool isInterLineSegments, [Optional] bool corporateAccountDetails)
        {
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

                UpdatePassengerNameRecordRQ = new()
                {
                    version = Constants.Application.UpdatePNRVersion,
                    Itinerary = new()
                    {
                        id = orderChangeRequestData.Rloc
                    },
                    SpecialReqDetails = new()
                    {
                        SpecialService = new()
                        {
                            SpecialServiceInfo = new()
                            {
                                SecureFlight = isInterLineSegments ? AddSFPDInfoForInterLineSegments( getReservationResponse,orderChangeRequestData.DataList.CustomerDetails).ToArray() : null,
                                Service = corporateAccountDetails ? GetCorporateDetails(orderChangeRequestData).Concat(GetSSRinfo(orderChangeRequestData, getReservationResponse.PassengerReservation.Passengers.Passenger)).ToArray(): GetSSRinfo(orderChangeRequestData, getReservationResponse.PassengerReservation.Passengers.Passenger)
                            }
                        }
                    },
                    TravelItineraryAddInfo = corporateAccountDetails ? new()
                    {
                        CustomerInfo = new()
                        {
                            Corporate = new(){ ID = orderChangeRequestData.Parties.CorporateAccount.CorporateCode }
                        }
                    }: null,
                    PostProcessing = new()
                    {
                        EndTransaction = new()
                        {
                            Source = new()
                            {
                                ReceivedFrom = Constants.Application.SabreSource
                            }
                        }
                    }
                }
            };
        }

        private static UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService[] GetCorporateDetails(OrderChangeRequestData orderChangeRequestData)
        {
            var corporateDetails = new List<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService>();
            corporateDetails.Add(new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
            {
                SSR_Code = Constants.Application.SSRCode,
                Text = $"{Constants.Application.OSICorporateCode} {orderChangeRequestData.Parties.CorporateAccount.TicketDesignator}",
                VendorPrefs = new()
                {
                    Airline = new()
                    {
                        Code = Constants.Application.OwnerAirline
                    }
                }
            });
            return corporateDetails.ToArray();
        }

        private static UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService[] GetSSRinfo(OrderChangeRequestData orderChangeRequestData, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            var ssrsReq = new List<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService>();
            var delSegs = orderChangeRequestData.OrderAmendments.First().FlightItems.Where(x => x.ActionContext == FlightItems.ActionContexts.Delete).SelectMany(x => x.Segments);
            var addSegs = orderChangeRequestData.OrderAmendments.First().FlightItems.Where(x => x.ActionContext == FlightItems.ActionContexts.Add).SelectMany(x => x.Segments);

            foreach (var passenger in passengers.Where(p => p.SpecialRequests?.SpecialMealRequest != null))
            {
                var mealSSRs = passenger.SpecialRequests.SpecialMealRequest.Where(x => x.ActionCode == "KK");

                if (mealSSRs.Any())
                {
                    foreach (var ssr in mealSSRs)
                    {
                        if (passenger.SpecialRequests.SpecialMealRequest.Any(x => x.MealType == ssr.MealType && x.BoardCity == ssr.BoardCity && x.OffCity == ssr.OffCity && x.FlightDate == ssr.FlightDate && x.FlightNumber == ssr.FlightNumber && x.ActionCode == "NN"))
                        {
                            var oldSeg = delSegs.FirstOrDefault(x => x.OriginAirportCode.Is(ssr.BoardCity) && x.DestinationAirportCode.Is(ssr.OffCity) && x.MarketingFlightNumber.PadLeft(4, '0').Is(ssr.FlightNumber) && Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).Date == Convert.ToDateTime(ssr.FlightDate).Date);

                            if (oldSeg == null)
                            {
                                continue;
                            }

                            var newSegs = addSegs.Any(x => x.OriginAirportCode.Is(ssr.BoardCity) && x.DestinationAirportCode.Is(ssr.OffCity) && x.OdRefId == oldSeg.OdRefId) ? [addSegs.First(x => x.OriginAirportCode.Is(ssr.BoardCity) && x.DestinationAirportCode.Is(ssr.OffCity) && x.OdRefId == oldSeg.OdRefId).SegmentId] : addSegs.Where(x => x.OdRefId == oldSeg.OdRefId).Select(x => x.SegmentId);
                            ssrsReq.AddRange(newSegs.Select(s =>
                            {
                                return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
                                {
                                    SSR_Code = ssr.MealType,
                                    PersonName = new() { NameNumber = passenger.nameId },
                                    SegmentNumber = s.Replace("SEG", ""),
                                    VendorPrefs = new()
                                    {
                                        Airline = new()
                                        {
                                            Hosted = true,
                                        }
                                    }
                                };
                            }));
                        }
                    }
                }
            }

            foreach (var passenger in passengers.Where(p => p.SpecialRequests?.WheelchairRequest != null))
            {
                var wheelChairSSRs = passenger.SpecialRequests.WheelchairRequest.Where(x => x.ActionCode == "KK");

                if (wheelChairSSRs.Any())
                {
                    foreach (var ssr in wheelChairSSRs)
                    {
                        if (passenger.SpecialRequests.WheelchairRequest.Any(x => x.WheelchairCode == ssr.WheelchairCode && x.BoardCity == ssr.BoardCity && x.OffCity == ssr.OffCity && x.FlightNumber == ssr.FlightNumber && x.FlightDate == ssr.FlightDate && x.ActionCode == "NN"))
                        {
                            var oldSeg = delSegs.FirstOrDefault(x => x.OriginAirportCode.Is(ssr.BoardCity) && x.DestinationAirportCode.Is(ssr.OffCity) && x.MarketingFlightNumber.PadLeft(4, '0').Is(ssr.FlightNumber) && Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(ssr.FlightDate));

                            if (oldSeg == null)
                            {
                                continue;
                            }

                            var newSegs = addSegs.Any(x => x.OriginAirportCode.Is(ssr.BoardCity) && x.DestinationAirportCode.Is(ssr.OffCity) && x.OdRefId == oldSeg.OdRefId) ? [addSegs.First(x => x.OriginAirportCode.Is(ssr.BoardCity) && x.DestinationAirportCode.Is(ssr.OffCity) && x.OdRefId == oldSeg.OdRefId).SegmentId] : addSegs.Where(x => x.OdRefId == oldSeg.OdRefId).Select(x => x.SegmentId);

                            ssrsReq.AddRange(newSegs.Select(s =>
                            {
                                return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
                                {
                                    SSR_Code = ssr.WheelchairCode.ToString(),
                                    PersonName = new() { NameNumber = passenger.nameId },
                                    SegmentNumber = s.Replace("SEG", ""),
                                    VendorPrefs = new()
                                    {
                                        Airline = new()
                                        {
                                            Hosted = true,
                                        }
                                    }
                                };
                            }));
                        }
                    }
                }
            }

            foreach (var passenger in passengers.Where(p => p.SpecialRequests?.GenericSpecialRequest != null && p.SpecialRequests.GenericSpecialRequest.Any(x => x.Code != "OTHS")))
            {
                var generalSSRs = passenger.SpecialRequests.GenericSpecialRequest.Where(x => x.ActionCode == "KK");

                if (generalSSRs.Any())
                {
                    foreach (var ssr in generalSSRs)
                    {
                        if (ssr.Code == "INFT")
                        {
                            ssrsReq.Add(new()
                            {
                                SSR_Code = "INFT",
                                SegmentNumber = "A",
                                PersonName = new() { NameNumber = passenger.nameId },
                                Text = ssr.FreeText[1..]
                            });
                        }
                        else
                        {
                            if (passenger.SpecialRequests.GenericSpecialRequest.Any(x => x.FullText == ssr.FullText.Replace("KK1","NN1") && x.Code == ssr.Code && x.ActionCode == "NN"))
                            {
                                var oldSeg = delSegs.FirstOrDefault(x => ssr.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{x.ClassOfService}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"));

                                if (oldSeg == null)
                                {
                                    continue;
                                }

                                var newSegs = addSegs.Any(x => x.OriginAirportCode.Is(oldSeg.OriginAirportCode) && x.DestinationAirportCode.Is(oldSeg.DestinationAirportCode) && x.OdRefId == oldSeg.OdRefId) ? [addSegs.First(x => x.OriginAirportCode.Is(oldSeg.OriginAirportCode) && x.DestinationAirportCode.Is(oldSeg.DestinationAirportCode) && x.OdRefId == oldSeg.OdRefId).SegmentId] : addSegs.Where(x => x.OdRefId == oldSeg.OdRefId).Select(x => x.SegmentId);

                                ssrsReq.AddRange(newSegs.Select(s =>
                                {
                                    return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService()
                                    {
                                        SSR_Code = ssr.Code,
                                        PersonName = new() { NameNumber = passenger.nameId },
                                        SegmentNumber = s.Replace("SEG", ""),
                                        VendorPrefs = new()
                                        {
                                            Airline = new()
                                            {
                                                Hosted = true,
                                            }
                                        }
                                    };
                                }));
                            }
                        }
                    }
                }
            }

            return [.. ssrsReq];
        }

        private static IEnumerable<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight> AddSFPDInfoForInterLineSegments(EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB getReservationResponse, IEnumerable<Customer> customerDetails)
        {
            var associatedAdults = new Dictionary<string, string>();
            IEnumerable<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight> secureFlights = null;

            var adults = customerDetails.Where(x => x.Ptc == "ADT").ToList();
            if (customerDetails.Any(x => x.Ptc == Constants.Application.Infant))
            {
                foreach (var inf in customerDetails.Where(x => x.Ptc == Constants.Application.Infant))
                {
                    associatedAdults.Add($"{inf.LastNameNumber}.{inf.FirstNameNumber}", $"{adults.First().LastNameNumber}.{adults.First().FirstNameNumber}");
                    adults.RemoveAt(0);
                }
            }

            var getReservationAirSegments = getReservationResponse.PassengerReservation.Segments.Segment.Where(x => x.Item is EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.SegmentTypePNRBSegmentAir).Select(x => x.Item as EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.SegmentTypePNRBSegmentAir);
            var interLineOperatingSegments = ConfigReader.Get(Constants.Environment.InterLineOperatingSegments);
            var interlineCarriers = interLineOperatingSegments.Split(",");
            var getReservationSegments = getReservationAirSegments.Where(seg => interlineCarriers.Contains(seg.MarketingAirlineCode));

            foreach (var segments in getReservationSegments)
            {
                var sfpds = customerDetails.Select(custDetails =>
                {
                    if (custDetails.Ptc == Constants.Application.Infant)
                    {
                        var associatedAdultNameNumber = associatedAdults.First(a => a.Key == $"{custDetails.LastNameNumber}.{custDetails.FirstNameNumber}").Value;
                        var paxDetails = getReservationResponse.PassengerReservation.Passengers.Passenger.First(x => x.nameId == associatedAdultNameNumber);
                        var SfpdDetails = paxDetails.SpecialRequests.APISRequest.First(a => a.DOCSEntry != null && a.DOCSEntry.DocumentNumber == null && a.DOCSEntry.Forename == custDetails.FirstName && a.DOCSEntry.Surname == custDetails.LastName);

                        var secureFlight = new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                        {
                            SegmentNumber = Convert.ToString(segments.sequence),
                            PersonName = new()
                            {
                                DateOfBirth = SfpdDetails.DOCSEntry.DateOfBirth.ToString(Constants.Application.DateOnlyFormat),
                                Gender = Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlightPersonNameGender>(SfpdDetails.DOCSEntry.Gender),
                                GenderSpecified = true,
                                GivenName = custDetails.FirstName,
                                Surname = custDetails.LastName,
                                NameNumber = associatedAdultNameNumber
                            },
                        };
                        return secureFlight;
                    }
                    else
                    {
                        var paxDetails = getReservationResponse.PassengerReservation.Passengers.Passenger.First(x => x.nameId.Split(".").First() == custDetails.LastNameNumber);
                        var SfpdDetails = paxDetails.SpecialRequests.APISRequest.First(a => a.DOCSEntry != null && a.DOCSEntry.DocumentNumber == null && a.DOCSEntry.Forename == custDetails.FirstName && a.DOCSEntry.Surname == custDetails.LastName);

                        return new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlight()
                        {
                            SegmentNumber = Convert.ToString(segments.sequence),
                            PersonName = new()
                            {
                                DateOfBirth = SfpdDetails.DOCSEntry.DateOfBirth.ToString(Constants.Application.DateOnlyFormat),
                                Gender = Enum.Parse<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoSecureFlightPersonNameGender>(SfpdDetails.DOCSEntry.Gender),
                                GenderSpecified = true,
                                GivenName = custDetails.FirstName,
                                Surname = custDetails.LastName,
                                NameNumber = $"{custDetails.LastNameNumber}.{custDetails.FirstNameNumber}"
                            },
                        };
                    }
                });
                 secureFlights = secureFlights == null ? sfpds : secureFlights.Concat(sfpds);
            }
            return secureFlights;
        }
    }
}