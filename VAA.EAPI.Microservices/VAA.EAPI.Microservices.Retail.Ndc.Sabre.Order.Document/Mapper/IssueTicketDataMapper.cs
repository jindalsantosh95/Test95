using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Mapper
{
    public class IssueTicketDataMapper
    {
        private static AirTicketRQ AirTicketReqMapper(IssueTicketRequest issueTicketRequest, string pcc)
        {
            return new()
            {
                version = Constants.Application.SabreVersion,
                targetCity = pcc,
                Itinerary = new()
                {
                    ID = issueTicketRequest.BookingReferences.RLOC
                },
                DesignatePrinter = new()
                {
                    Profile = new()
                    {
                        Number = ConfigReader.Get(Constants.Environment.DirectTicketingPCC).Split(",").Contains(pcc) ? "2" : Constants.Application.DesignatePrinterProfileNumber
                    }
                },
                Ticketing = TicketingMapper(issueTicketRequest),
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
        private static AirTicketRQTicketing[] TicketingMapper(IssueTicketRequest issueTicketRequest)
        {
            return issueTicketRequest.CustomerDetails.GroupBy(x => x.Ptc).Select((x, index) =>
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
                        PriceQuote = [new() { Record = [new() { Number = $"{index + 1}"}] }]
                    }
                    
                };
            }).ToArray();
        }
        public static AirTicketRQRequest GetAirticketRequest(string token, IssueTicketRequest issueTicketRequest, string pcc)
        {
            return new()
            {
                AirTicketRQ = AirTicketReqMapper(issueTicketRequest, pcc),
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
        
            
       public static IEnumerable<AccountableDocumentRef> AccountableDocumentsMapper(AirTicketRSSummary airTicketRS, GetElectronicDocumentRS couponRS, IEnumerable<Customer> customerDetails, IEnumerable<Segment> segmentDetails)
        {
            var customerRefId = customerDetails.First(x => x.FirstName.Equals(airTicketRS.FirstName, StringComparison.OrdinalIgnoreCase) &&x.LastName.Equals(airTicketRS.LastName, StringComparison.OrdinalIgnoreCase)).CustId;

            var coupons = GetCouponInfo(couponRS, airTicketRS, segmentDetails);
            return coupons.GroupBy(c =>c.TicketNumber).Select(group => new AccountableDocumentRef
            {
                DocumentNumber = group.Key,
                DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
                IssueDate = airTicketRS.LocalIssueDateTime.ToString("yyyy-MM-dd"),
                CustomerRefId = customerRefId,
                Coupons = group,
            });
        }

        private static IEnumerable<Coupon> GetCouponInfo(GetElectronicDocumentRS couponRS, AirTicketRSSummary airTicketRS, IEnumerable<Segment> segmentDetails)
        {
            var ticket = (couponRS.DocumentDetailsDisplay.Item as TicketingDocumentTicketED);   
            var conjunctiveNumber = ticket?.RelatedDocument?.Conjunctive?.Select(x =>x.Number).ToList();
            var coupons = ticket.ServiceCoupon.Where(coupon => coupon.type != "A").ToList();

            return coupons.Select((x,index)=>          
            {
                const int CouponsPerTicket = 4; 
                int groupIndex = index / CouponsPerTicket;

                string ticketNumber = airTicketRS.DocumentNumber;

                if(groupIndex > 0 && conjunctiveNumber != null && conjunctiveNumber.Count > groupIndex - 1)
                {
                    ticketNumber = conjunctiveNumber[groupIndex - 1];
                }
                return new Coupon()
                {
                    CouponNumber = x.coupon,
                    TicketNumber = ticketNumber,
                    CouponStatus = x.CurrentStatus,
                    SegmentRefId = segmentDetails.First(s => s.MarketingFlightNumber == x.MarketingFlightNumber && s.OriginAirportCode == x.StartLocation.Value && s.DestinationAirportCode == x.EndLocation.Value).SegmentId,
                    FareBasis = x.FareBasis
                };
                
            });
        }

        public static void MapSSROrderItems(GetReservationRS orderRs, IssueTicketRequest request, IssueTicketResponse response)
        {
            var orderItemList = new List<OrderItems>();
            var warningList = new List<Warning>();

            var reservation = ((ReservationPNRB)orderRs.Item).PassengerReservation;

            var isSsrAvailable = reservation.Passengers.Passenger.Any(x => x.SpecialRequests?.SpecialMealRequest is not null || x.SpecialRequests?.WheelchairRequest is not null || x.SpecialRequests?.GenericSpecialRequest is not null);
            if (isSsrAvailable)
            {
                var instId = 1;

                if (reservation.Passengers.Passenger.Any(x => x.SpecialRequests?.SpecialMealRequest is not null))
                {
                    var ssrs = reservation.Passengers.Passenger.Where(p => p.SpecialRequests?.SpecialMealRequest != null)?.SelectMany(x => x.SpecialRequests.SpecialMealRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.MealType);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = request.SegmentDetails.First(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber).SegmentId;
                            var passenger = reservation.Passengers.Passenger.First(x => x.SpecialRequests.SpecialMealRequest != null && x.SpecialRequests.SpecialMealRequest.Contains(s));
                            var cust = request.CustomerDetails.First(x => string.Equals(passenger.FirstName, x.FirstName, StringComparison.OrdinalIgnoreCase) && string.Equals(passenger.LastName, x.LastName, StringComparison.OrdinalIgnoreCase));
                            if (passenger.SpecialRequests.SpecialMealRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.MealType == s.MealType && p.type == RequestTypePNRB.A && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.MealType,
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/meal/{s.MealType}"
                                });
                            }
                            else if (passenger.SpecialRequests.SpecialMealRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.MealType == s.MealType && p.type == RequestTypePNRB.A && (p.ActionCode == "NO" || p.ActionCode == "UN")))
                            {
                                warningList.Add(new()
                                {
                                    Code = "186",
                                    Details = s.MealType,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });

                                request.OrderInstructions = request.OrderInstructions?
                                    .Where(oi => oi.ItemText != $"FlightService/{cust.CustId}/{segId}/meal/{s.MealType}")
                                    .ToList();
                            }
                            else
                                warningList?.Add(new()
                                {
                                    Code = "182",
                                    Details = s.MealType,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            orderItemList.Add(new OrderItems()
                            {
                                OrderItemId = $"{request.BookingReferences.RLOC}-{orderItemList.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = request.SelectedOffers.First().OfferItems.First().OfferitemPricing.First().SalePrice.First().Amount.BaseCurrency,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                if (reservation.Passengers.Passenger.Any(x => x.SpecialRequests?.WheelchairRequest is not null))
                {
                    var ssrs = reservation.Passengers.Passenger.Where(p => p.SpecialRequests?.WheelchairRequest != null)?.SelectMany(x => x.SpecialRequests.WheelchairRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.WheelchairCode);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = request.SegmentDetails.First(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber).SegmentId;
                            var passenger = reservation.Passengers.Passenger.First(x => x.SpecialRequests?.WheelchairRequest != null && x.SpecialRequests.WheelchairRequest.Contains(s));
                            var cust = request.CustomerDetails.First(x => string.Equals(passenger.FirstName, x.FirstName, StringComparison.OrdinalIgnoreCase) && string.Equals(passenger.LastName, x.LastName, StringComparison.OrdinalIgnoreCase));

                            if (passenger.SpecialRequests.WheelchairRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.WheelchairCode == s.WheelchairCode && p.type == RequestTypePNRB.A && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.WheelchairCode.ToString(),
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/wheelChair/{s.WheelchairCode}"
                                });
                            }
                            else if (passenger.SpecialRequests.WheelchairRequest.Any(p => p.OffCity == s.OffCity && p.BoardCity == s.BoardCity && p.FlightDate == s.FlightDate && p.FlightNumber == s.FlightNumber && p.WheelchairCode == s.WheelchairCode && p.type == RequestTypePNRB.A && (p.ActionCode == "NO" || p.ActionCode == "UN")))
                            {
                                warningList.Add(new()
                                {
                                    Code = "186",
                                    Details = s.WheelchairCode.ToString(),
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });

                            }
                            else
                                warningList.Add(new()
                                {
                                    Code = "182",
                                    Details = s.WheelchairCode.ToString(),
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            orderItemList.Add(new OrderItems()
                            {
                                OrderItemId = $"{request.BookingReferences.RLOC}-{orderItemList.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = request.SelectedOffers.First().OfferItems.First().OfferitemPricing.First().SalePrice.First().Amount.BaseCurrency,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                if (reservation.Passengers.Passenger.Any(x => x.SpecialRequests?.GenericSpecialRequest is not null))
                {
                    var ssrs = reservation.Passengers.Passenger.Where(p => p.SpecialRequests?.GenericSpecialRequest != null)?.SelectMany(x => x.SpecialRequests.GenericSpecialRequest.Where(s => (s.ActionCode == "NN" || s.ActionCode == "HN") && s.Code != "OTHS" && s.Code != "INFT" && s.Code != "CHLD"))?.GroupBy(x => x.Code);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = request.SegmentDetails.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId ?? request.SegmentDetails.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{x.ClassOfService}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId;
                            var passenger = reservation.Passengers.Passenger.First(x => x.SpecialRequests.GenericSpecialRequest != null && x.SpecialRequests.GenericSpecialRequest.Contains(s));
                            var cust = request.CustomerDetails.First(x => string.Equals(passenger.FirstName, x.FirstName, StringComparison.OrdinalIgnoreCase) && string.Equals(passenger.LastName, x.LastName, StringComparison.OrdinalIgnoreCase));

                            if (passenger.SpecialRequests.GenericSpecialRequest.Any(p => p.FullText == s.FullText.Replace("NN", "KK") && p.Code == s.Code && p.type == RequestTypePNRB.A && p.ActionCode == "KK"))
                            {
                                ois.Add(new()
                                {
                                    InstructionId = $"OI{instId++}",
                                    ItemCode = s.Code,
                                    ItemType = OrderInstructions.ItemTypes.SSREnum,
                                    ItemText = $"FlightService/{cust.CustId}/{segId}/general/{s.Code}"
                                });
                            }
                            else if (passenger.SpecialRequests.GenericSpecialRequest.Any(p => p.FullText == s.FullText.Replace("NN", "KK") && p.Code == s.Code && p.type == RequestTypePNRB.A && (p.ActionCode == "NO" || p.ActionCode == "UN")))
                            {
                                warningList.Add(new()
                                {
                                    Code = "186",
                                    Details = s.Code,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });

                            }
                            else
                                warningList.Add(new()
                                {
                                    Code = "182",
                                    Details = s.Code,
                                    Message = $"Failed to add the SSR details for {cust.CustId} for seg {segId}",
                                });
                        }

                        if (ois.Count != 0)
                        {
                            orderItemList.Add(new OrderItems()
                            {
                                OrderItemId = $"{request.BookingReferences.RLOC}-{orderItemList.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = request.SelectedOffers.First().OfferItems.First().OfferitemPricing.First().SalePrice.First().Amount.BaseCurrency,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

            }

            response.OrderInstructions = request.OrderInstructions;
            response.Warnings = warningList.Count != 0 ? warningList : null;
            response.OrderItems = orderItemList;
        }

        private static IEnumerable<OrderService> GetServices(IEnumerable<OrderInstructions> orderInstructions)
        {
            return orderInstructions.Select(oi =>
            {
                var ssrInfo = oi.ItemText.Split('/');

                return new OrderService()
                {
                    ServiceId = $"SV{1}",
                    SegmentRefIds = [ssrInfo[2]],
                    ServiceBeneficiaries = [ssrInfo[1]],
                    OrderInstructionRefs = [oi.InstructionId]
                };
            });
        }

    }
}
