using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.PriceQuote;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public class ManageCustomerResponseMapper
    {
        public static OrderDocument SabreToManageCustomerResponseMapper(GetReservationRS response, ManageCustomerRequest request, AirTicketRS airTicketRS)
        {
            var passengers = (response.Item as ReservationPNRB).PassengerReservation.Passengers.Passenger.Where(x => x.SpecialRequests != null && x.SpecialRequests.APISRequest != null && (x.SpecialRequests.APISRequest.Any(x => x.DOCAEntry != null || x.DOCSEntry != null || x.DOCOEntry != null)) || x.SpecialRequests?.SpecialMealRequest != null || x.SpecialRequests?.WheelchairRequest != null || x.SpecialRequests?.GenericSpecialRequest != null);
            var warningList = new List<Warning>();

            return new OrderDocument()
            {
                BookingReference = new()
                {
                    RLOC = request.Rloc,
                },
                OrderItems = (request.TicketedNameCorrection || request.UnTicketedNameCorrection)
                    ? GetNameCorrectionOrderItems(request, response, airTicketRS)
                    : (request.AmendApis ? null : GetOrderItems(request, warningList, response, airTicketRS)),
                Warnings = warningList.Any() ? warningList : null,
                DataList = new()
                {
                    CustomerDetails = GetCustomerDetails(passengers.Select(x => x.SpecialRequests), request.CustomerDetails),
                    OrderInstructions = warningList.Any() ? GetOrderInstructions(request, warningList) : request.OrderInstructions
                },
                ChangeType = (request.TicketedNameCorrection || request.UnTicketedNameCorrection) ? Domain.ChangeTypeEnum.NameCorrection : null,
                ChangeDateTime = new() { UtcDateTime = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat) }

            };
        }
        private static IEnumerable<OrderItems> GetNameCorrectionOrderItems(ManageCustomerRequest request, GetReservationRS response, AirTicketRS airTicketRS)
        {
            var orderItems = new List<OrderItems>();

                orderItems.Add(new OrderItems
                {
                    Services = new[]
                    {
                        new OrderService
                        {
                            Product = new Product()
                            {
                                ProductType = Product.ProductTypes.NameCorrection
                            }
                        }
                    },
                    AccountableDocumentRefs = airTicketRS != null ? MapAccountableDocumentRefs(request.CustomerDetails, airTicketRS) : null,
                    PaymentDetails = new[]{new PaymentDetails
                    {
                        Amount = new CurrencyAmount
                        {
                        MonetaryAmount = "0.00",
                        BaseCurrency = (response.PriceQuote.Item as PriceQuoteInfoGetResponse)?.Summary?.NameAssociation?.FirstOrDefault()?.PriceQuote?.FirstOrDefault()?.Amounts?.Total?.currencyCode,
                        },
                        PaymentCommitmentDateTime = DateTime.UtcNow.ToString(Constants.Application.InputDateTimeFormat),
                        PaymentReferenceId = new Random().Next(10000000, 99999999).ToString(),
                        PaymentProcessingStatus = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.Status.SUCCESSFUL
                }}
                });

            return orderItems;

        }

        private static IEnumerable<OrderItems> GetOrderItems(ManageCustomerRequest request, List<Warning> warningList, GetReservationRS response, AirTicketRS airTicketRS)
        {
            var orderItemList = new List<OrderItems>();
            var passengers = (response.Item as ReservationPNRB)?.PassengerReservation?.Passengers.Passenger;

            var isSsrAvailable = passengers.Any(x => x.SpecialRequests?.SpecialMealRequest is not null || x.SpecialRequests?.WheelchairRequest is not null || x.SpecialRequests?.GenericSpecialRequest is not null);

            if (isSsrAvailable)
            {
                var instId = 1;

                if (passengers.Any(x => x.SpecialRequests?.SpecialMealRequest is not null))
                {
                    var ssrs = passengers.Where(p => p.SpecialRequests?.SpecialMealRequest != null)?.SelectMany(x => x.SpecialRequests.SpecialMealRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.MealType);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = request?.SegmentDetails?.FirstOrDefault(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber)?.SegmentId;
                            var passenger = passengers.First(x => x.SpecialRequests.SpecialMealRequest != null && x.SpecialRequests.SpecialMealRequest.Contains(s));
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
                                OrderItemId = $"{$"VS932{request.Rloc}{DateTime.UtcNow:yy}"}-{orderItemList.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = (response.PriceQuote.Item as PriceQuoteInfoGetResponse)?.Summary?.NameAssociation?.FirstOrDefault()?.PriceQuote?.FirstOrDefault()?.Amounts?.Total?.currencyCode,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                if (passengers.Any(x => x.SpecialRequests?.WheelchairRequest is not null))
                {
                    var ssrs = passengers.Where(p => p.SpecialRequests?.WheelchairRequest != null)?.SelectMany(x => x.SpecialRequests.WheelchairRequest.Where(s => s.ActionCode == "NN" || s.ActionCode == "HN"))?.GroupBy(x => x.WheelchairCode);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = request?.SegmentDetails?.FirstOrDefault(x => x.OriginAirportCode == s.BoardCity && x.DestinationAirportCode == s.OffCity && x.MarketingFlightNumber.PadLeft(4, '0') == s.FlightNumber)?.SegmentId;
                            var passenger = passengers.First(x => x.SpecialRequests?.WheelchairRequest != null && x.SpecialRequests.WheelchairRequest.Contains(s));
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
                                OrderItemId = $"{$"VS932{request.Rloc}{DateTime.UtcNow:yy}"}-{orderItemList.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = (response.PriceQuote.Item as PriceQuoteInfoGetResponse)?.Summary?.NameAssociation?.FirstOrDefault()?.PriceQuote?.FirstOrDefault()?.Amounts?.Total?.currencyCode,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

                if (passengers.Any(x => x.SpecialRequests?.GenericSpecialRequest is not null))
                {
                    var ssrs = passengers.Where(p => p.SpecialRequests?.GenericSpecialRequest != null)?.SelectMany(x => x.SpecialRequests.GenericSpecialRequest.Where(s => (s.ActionCode == "NN" || s.ActionCode == "HN") && s.Code != "OTHS" && s.Code != "INFT" && s.Code != "CHLD"))?.GroupBy(x => x.Code);

                    foreach (var ssr in ssrs)
                    {
                        List<OrderInstructions> ois = [];

                        foreach (var s in ssr)
                        {
                            var segId = request?.SegmentDetails?.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId ?? request?.SegmentDetails?.FirstOrDefault(x => s.FullText.Contains($"{x.OriginAirportCode}{x.DestinationAirportCode}{x.MarketingFlightNumber.PadLeft(4, '0')}{x.ClassOfService}{Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime).ToString("ddMMM").ToUpper()}"))?.SegmentId;
                            var passenger = passengers.First(x => x.SpecialRequests.GenericSpecialRequest != null && x.SpecialRequests.GenericSpecialRequest.Contains(s));
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
                                OrderItemId = $"{$"VS932{request.Rloc}{DateTime.UtcNow:yy}"}-{orderItemList.Count + 1}",
                                OwnerAirline = Constants.Application.OwnerAirline,
                                Services = GetServices(ois),
                                ItemStatus = new() { Status = Constants.Application.OrderItemStatusCurrent },
                                PriceDetails =
                                [
                                    new()
                            {
                                    Amount = new()
                                    {
                                        BaseCurrency = (response.PriceQuote.Item as PriceQuoteInfoGetResponse)?.Summary?.NameAssociation?.FirstOrDefault()?.PriceQuote?.FirstOrDefault()?.Amounts?.Total?.currencyCode,
                                        MonetaryAmount = "0.00"
                                    },
                                    PriceComponentType = Price.PriceComponentTypes.BasePriceEnum
                                }]
                            });
                        }
                    }
                }

            }

            return orderItemList;
        }

        private static IEnumerable<AccountableDocumentRef> MapAccountableDocumentRefs(IEnumerable<Customer> customers, AirTicketRS airTicketRS)
        {
            var tickets = customers.Select(customer =>
            {
                var ticket = airTicketRS.Summary.FirstOrDefault(x => x.LastName.Is(customer.LastName) && x.FirstName.Is(customer.FirstName));
                return new
                {
                    ticket.DocumentNumber,
                    CustId = customer.CustId
                };
            });

            return tickets.Select(ticket => new AccountableDocumentRef
            {
                DocumentNumber = ticket.DocumentNumber,
                CustomerRefId = ticket.CustId,
                DocumentType = AccountableDocumentRef.DocumentTypes.TEnum,
            });
        }

        private static IEnumerable<OrderInstructions> GetOrderInstructions(ManageCustomerRequest request, List<Warning> warning)
        {
            var orderInstructionList = new List<OrderInstructions>();

            if (request.OrderInstructions.Any())
            {
                foreach (var oi in request.OrderInstructions)
                {
                    if (!warning.Any(w => w.Details == oi.ItemCode && w.Message.Contains(oi.ItemText.Split("/")[1]) && w.Message.Contains(oi.ItemText.Split("/")[2])))
                    {
                        orderInstructionList.Add(oi);
                    }
                }
            }
            return orderInstructionList;
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

        private static IEnumerable<Customer> GetCustomerDetails(IEnumerable<NameAssociatedSpecialRequestsPNRB> specialReqs, IEnumerable<Customer> customers)
        {
            var unsuccessfulAPIS = new List<string>();

            foreach (var cust in customers.Where(x => x.TravelDocuments != null))
            {
                foreach (var doc in cust.TravelDocuments)
                {
                    if (((doc.DocumentType == "PT" || doc.DocumentType == "GC") && !specialReqs.Any(x => x.APISRequest.Any(a => a.DOCSEntry != null && a.DOCSEntry.DocumentNumber == doc.DocumentNumber && a.DOCSEntry.Forename == cust.FirstName && a.DOCSEntry.Surname == cust.LastName))) || (doc.DocumentType == "CR" && !specialReqs.Any(x => x.APISRequest.Any(a => a.DOCOEntry != null && a.DOCOEntry.FreeText.Contains("/K/") && a.DOCOEntry.FreeText.Contains($"/{doc.DocumentNumber}/")))) || (doc.DocumentType == "AEA" && !specialReqs.Any(x => x.APISRequest.Any(a => a.DOCOEntry != null && a.DOCOEntry.FreeText.Contains("/R/") && a.DOCOEntry.FreeText.Contains($"/{doc.DocumentNumber}/")))) || (doc.DocumentType == "VI" && !specialReqs.Any(x => x.APISRequest.Any(a => a.DOCOEntry != null && a.DOCOEntry.FreeText.Contains("/V/") && a.DOCOEntry.FreeText.Contains($"/{doc.DocumentNumber}/")))))
                    {
                        unsuccessfulAPIS.Add(doc.DocumentType);
                    }
                }
            }

            if (customers.Any(x => x.TravelDocuments != null))
            {
                customers = customers.Select(c =>
                {
                    c.TravelDocuments = c.TravelDocuments?.Select(td =>
                    {
                        if (!unsuccessfulAPIS.Contains(td.DocumentType))
                            return td;
                        else
                            return null;
                    })?.Where(x => x != null)?.ToList();

                    return c;
                });
            }

            return customers;
        }
    }
}
