using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Mapper
{
    public static class OrderChangeNotifRQMapper
    {
        public static IATAOrderChangeNotifRQ MapRequest(OrderDocument orderDocument)
        {
            var latestHistory = orderDocument.OrderHistory.OrderBy(x => x.Version).Last();

            IEnumerable<DatedMarketingSegmentType> newDatedMarketingSegments = [];
            IEnumerable<DatedOperatingSegmentType> newDatedOperatingSegments = [];
            IEnumerable<DatedOperatingLegType> newDatedOperatingLegs = [];

            IEnumerable<DatedMarketingSegmentType> oldDatedMarketingSegments = [];
            IEnumerable<DatedOperatingSegmentType> oldDatedOperatingSegments = [];
            IEnumerable<DatedOperatingLegType> oldDatedOperatingLegs = [];

            return new()
            {
                DistributionChain = MapDistributionChain(orderDocument.Parties).ToArray(),
                Notification = new()
                {
                    ChangeGroup =
                    [
                        new()
                        {
                            ChangeOperation =
                            [
                                new()
                                {
                                    New = MapNewDetails(orderDocument, ref newDatedMarketingSegments , ref newDatedOperatingSegments , ref newDatedOperatingLegs),
                                    Old = latestHistory.ChangeType == ChangeTypeEnum.DocumentExchanged ? MapOldVersionDetails(latestHistory) : MapOldDetails(latestHistory ,orderDocument, ref oldDatedMarketingSegments , ref oldDatedOperatingSegments , ref oldDatedOperatingLegs),
                                    ReasonCode = latestHistory.ChangeType switch { ChangeTypeEnum.FlightCancellation => Constants.Application.ReasonCodeFlightCancellation, ChangeTypeEnum.DocumentIssuance => Constants.Application.ReasonCodeDocumentIssuance, ChangeTypeEnum.DocumentExchanged => Constants.Application.ReasonCodeDocumentExchanged, _ => Constants.Application.ReasonCodeDefault },
                                    ChangeDateTime = DateTimeOffset.Parse(latestHistory.ChangeDateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime,
                                    ChangeTypeCode = GetChangeTypeCode(latestHistory.ChangeType),
                                }
                            ],
                            OrderVersionNumber = Convert.ToString(Convert.ToInt32(latestHistory.Version) + 1)
                        }
                    ]
                }
            };
        }

        private static string GetChangeTypeCode(ChangeTypeEnum changeType)
        {
            if (changeType == ChangeTypeEnum.FlightNumberChange)
                return "1";
            else if (changeType == ChangeTypeEnum.FlightDateTimeChange || changeType == ChangeTypeEnum.FlightRouteChange)
                return "2";
            else if (changeType == ChangeTypeEnum.DocumentIssuance)
                return "10";
            else if (changeType == ChangeTypeEnum.DocumentVoided)
                return "14";
            else if (changeType == ChangeTypeEnum.DocumentRefunded)
                return "15";
            else if (changeType == ChangeTypeEnum.DocumentExchanged)
                return "16";
            else
                return "3";
        }

        private static IEnumerable<DistributionChainLinkType> MapDistributionChain(Parties parties)
        {
            var distributionChain = parties.Participants.Select(p =>
            {
                return new DistributionChainLinkType()
                {
                    Ordinal = Convert.ToString(Convert.ToInt16(p.Ordinal) + 1),
                    OrgRole = Enum.Parse<DistributionChainRoleCodeType>(Convert.ToString(p.PartyType)),
                    ParticipatingOrg = new()
                    {
                        Name = p.Name,
                        OrgID = p.AgencyID
                    }
                };
            });

            distributionChain = distributionChain.Prepend(new()
            {
                Ordinal = "1",
                OrgRole = DistributionChainRoleCodeType.Carrier,
                ParticipatingOrg = new()
                {
                    Name = "Virgin Atlantic Airways",
                    OrgID = "VS"
                }
            });

            return distributionChain;
        }

        public static IATAOrderChangeNotifRQ MapCancelledRequest(OrderDocument orderDocument)
        {
            IEnumerable<DatedMarketingSegmentType> datedMarketingSegments = [];
            IEnumerable<DatedOperatingSegmentType> datedOperatingSegments = [];
            IEnumerable<DatedOperatingLegType> datedOperatingLegs = [];

            var newStatus = orderDocument.ChangeType == ChangeTypeEnum.DocumentRefunded ? Constants.Application.DocumentRfdStatus : orderDocument.ChangeType == ChangeTypeEnum.DocumentVoided ? Constants.Application.DocumentVoidStatus : null;

            return new IATAOrderChangeNotifRQ
            {
                DistributionChain = MapDistributionChain(orderDocument.Parties).ToArray(),
                Notification = new()
                {
                    ChangeGroup =
                    [
                        new()
                            {
                                ChangeOperation =
                                [
                                    new()
                                    {

                                        New = MapNewCancelledDetails(orderDocument, ref datedMarketingSegments , ref datedOperatingSegments , ref datedOperatingLegs, newStatus),
                                        Old = MapOldCancelledDetails(orderDocument, ref datedMarketingSegments , ref datedOperatingSegments , ref datedOperatingLegs, Constants.Application.DocumentOpeStatus),
                                        ReasonCode = Constants.Application.ReasonCodeCancelled,
                                        ChangeDateTime = DateTime.Parse(orderDocument.ChangeDateTime.UtcDateTime).ToUniversalTime(),
                                        ChangeTypeCode = orderDocument.ChangeType == ChangeTypeEnum.DocumentVoided? GetChangeTypeCode(ChangeTypeEnum.DocumentVoided)
                                                       : orderDocument.ChangeType == ChangeTypeEnum.DocumentRefunded? GetChangeTypeCode(ChangeTypeEnum.DocumentRefunded): null,
                                    }
                                ],
                                 OrderVersionNumber = orderDocument.Version.ToString()
                            }
                    ]
                }
            };
        }

        private static ChangeDetailsType MapOldCancelledDetails(OrderDocument orderDocument, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs, string status)
        {
            return new()
            {
                DataLists = new()
                {
                    PaxList = GetPax(orderDocument.DataList.CustomerDetails).ToArray(),
                    PaxSegmentList = GetPaxSegment(orderDocument.DataList.SegmentDetails, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs).ToArray(),
                    OriginDestList = GetOriginDest(orderDocument.DataList.OriginDestinationAssociations).ToArray(),
                    PaxJourneyList = GetPaxJourney(orderDocument.DataList.Journey).ToArray(),
                    DatedMarketingSegmentList = datedMarketingSegments.ToArray(),
                    DatedOperatingLegList = datedOperatingLegs.ToArray(),
                    DatedOperatingSegmentList = datedOperatingSegments.ToArray(),
                },
                Order =
                 [
                     new()
                        {
                            OrderID = orderDocument.OrderNumber.OrderId,
                            OwnerCode = orderDocument.OrderNumber.OwnerAirline
                        }
                  ],
                TicketDocInfo = MapCancelTicketDocInfo(orderDocument.OrderItems, orderDocument, status).ToArray()
            };
        }
        private static ChangeDetailsType MapNewCancelledDetails(OrderDocument orderDocument, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs, string status)
        {
            return new()
            {
                DataLists = new()
                {
                    PaxList = GetPax(orderDocument.DataList.CustomerDetails).ToArray(),
                    PaxSegmentList = GetPaxSegment(orderDocument.DataList.SegmentDetails, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs).ToArray(),
                    OriginDestList = GetOriginDest(orderDocument.DataList.OriginDestinationAssociations).ToArray(),
                    PaxJourneyList = GetPaxJourney(orderDocument.DataList.Journey).ToArray(),
                    DatedMarketingSegmentList = datedMarketingSegments.ToArray(),
                    DatedOperatingLegList = datedOperatingLegs.ToArray(),
                    DatedOperatingSegmentList = datedOperatingSegments.ToArray(),
                },
                Order =
                 [
                     new()
                        {
                            OrderID = orderDocument.OrderNumber.OrderId,
                            OwnerCode = orderDocument.OrderNumber.OwnerAirline
                        }
                  ],
                TicketDocInfo = MapCancelTicketDocInfo(orderDocument.OrderItems, orderDocument, status).ToArray()
            };
        }
        private static IEnumerable<TicketDocInfoType> MapCancelTicketDocInfo(IEnumerable<OrderItems> orderItems, OrderDocument orderDocument, string status)
        {
            return orderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs)
                .Where(accDoc =>
                    accDoc.DocumentType != null &&
                    accDoc.DocumentType.Value.ToString().StartsWith(Constants.Application.TicketTypeT) &&
                    !string.IsNullOrWhiteSpace(accDoc.DocumentNumber) &&
                    accDoc.DocumentNumber.Length == 13).Select(accDoc => new TicketDocInfoType
                    {
                        PaxRefID = accDoc?.CustomerRefId,
                        BookingRef = GetBookingRef(orderDocument),
                        Ticket =
                        [
                            new TicketType
                            {
                                TicketDocTypeCode = Constants.Application.TicketTypeT,
                                TicketNumber = accDoc?.DocumentNumber,
                                Coupon = accDoc?.Coupons?.Select(coupon => new CouponType
                                {
                                    CouponNumber = Convert.ToDecimal(coupon?.CouponNumber),
                                    CouponStatusCode = status
                                }).ToArray()
                            }
                        ]
                    });
        }
        private static ChangeDetailsType MapOldDetails(OrderHistory latestHistory, OrderDocument orderDocument, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs)
        {
            return new()
            {
                DataLists = new()
                {
                    PaxList = GetPax(latestHistory.DataList.CustomerDetails).ToArray(),
                    PaxSegmentList = GetPaxSegment(latestHistory.DataList.SegmentDetails, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs).ToArray(),
                    OriginDestList = GetOriginDest(latestHistory.DataList.OriginDestinationAssociations).ToArray(),
                    PaxJourneyList = GetPaxJourney(latestHistory.DataList.Journey).ToArray(),
                    DatedMarketingSegmentList = datedMarketingSegments.ToArray(),
                    DatedOperatingLegList = datedOperatingLegs.ToArray(),
                    DatedOperatingSegmentList = datedOperatingSegments.ToArray(),
                },
                Order =
                [
                    new()
                    {
                        OrderID = orderDocument.OrderNumber.OrderId,
                        OwnerCode = orderDocument.OrderNumber.OwnerAirline,
                        OrderItem = GetOrderItems(latestHistory.OrderItems,orderDocument, true)
                    }
                 ],
                TicketDocInfo = latestHistory.ChangeType == ChangeTypeEnum.DocumentIssuance ? null : MapTicketDocInfo(latestHistory.OrderItems, orderDocument).ToArray()
            };
        }

        private static ChangeDetailsType MapOldVersionDetails(OrderHistory orderHistory)
        {
            return new()
            {
                Order = [
                    new()
                    {
                        LastModifiedDateTimeSpecified = true,
                        LastModifiedDateTime = DateTimeOffset.Parse(orderHistory.ChangeDateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).UtcDateTime,
                        OrderVersionNumber = orderHistory.Version
                    }
                ]
            };
        }

        private static ChangeDetailsType MapNewDetails(OrderDocument orderDocument, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs, [Optional] bool isProactiveTTL, [Optional] bool isReactiveTTL)
        {
            return new()
            {
                DataLists = new()
                {
                    PaxList = GetPax(orderDocument.DataList.CustomerDetails).ToArray(),
                    PaxSegmentList = GetPaxSegment(orderDocument.DataList.SegmentDetails, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs).ToArray(),
                    OriginDestList = GetOriginDest(orderDocument.DataList.OriginDestinationAssociations).ToArray(),
                    PaxJourneyList = GetPaxJourney(orderDocument.DataList.Journey).ToArray(),
                    DatedMarketingSegmentList = datedMarketingSegments.ToArray(),
                    DatedOperatingLegList = datedOperatingLegs.ToArray(),
                    DatedOperatingSegmentList = datedOperatingSegments.ToArray(),
                },
                Order =
                [
                    new()
                    {
                        OrderID = orderDocument.OrderNumber.OrderId,
                        OwnerCode = orderDocument.OrderNumber.OwnerAirline,
                        OrderItem = GetOrderItems(orderDocument.OrderItems,orderDocument, false)
                    }
                 ],
                TicketDocInfo = MapTicketDocInfo(orderDocument.OrderItems, orderDocument).ToArray(),
                Warning = isProactiveTTL ?
                [
                    new(){
                        Code = "75E",
                        DescText = "Ticketing time limit approaching expiration. Issuance required to prevent auto cancel",
                        LangCode = "EN",
                        TagText = "TTL expiry - Proactive",
                        TypeCode = "Warning"
                    }
                ] : isReactiveTTL ?
                [
                    new(){
                        Code = "75E",
                        DescText = "Ticketing time limit expired for the booking. Segments will be auto cancelled",
                        LangCode = "EN",
                        TagText = "TTL expiry - Reactive",
                        TypeCode = "Warning"
                    }
                ]
                : null
            };
        }

        private static ChangesToOrderItemType[] GetOrderItems(IEnumerable<OrderItems> orderItems, OrderDocument orderDocument, bool old)
        {
            return orderItems.Where(x => x.Services.First().Product?.ProductType == Product.ProductTypes.FlightEnum).Select(orderItem =>
            {
                var services = GetServices(orderItem, orderDocument, old);
                return new ChangesToOrderItemType()
                {
                    OrderItemID = orderItem.OrderItemId,
                    OwnerCode = orderItem.OwnerAirline,
                    PaymentTimeLimitDateTime = !string.IsNullOrEmpty(orderDocument.TimeLimits?.FirstOrDefault()?.Limit) ? Convert.ToDateTime(orderDocument.TimeLimits?.FirstOrDefault()?.Limit).ToUniversalTime() : DateTime.MinValue,
                    PaymentTimeLimitDateTimeSpecified = !string.IsNullOrEmpty(orderDocument.TimeLimits?.FirstOrDefault()?.Limit),
                    Service = services.ToArray(),
                    SellerFollowUpAction = orderDocument.DataList.SegmentDetails.Any(x => x.Status.Code == "UN" || x.Status.Code == "TK") ? new()
                    {
                        ActionCode = ["ContactAirline"],
                        ActionInd = true
                    } : null
                };
            }).ToArray();
        }

        public static string GetOrderItemStatus(IEnumerable<ChangeToServiceType> serviceList, IEnumerable<Segment> segments, IEnumerable<OrderItems> orderItems)
        {
            int removedServices = 0;

            foreach (var service in serviceList)
            {
                var deliveryStatusCode = GetDeliveryStatusCode(service, segments, orderItems);
                if (deliveryStatusCode == OrderViewStatusEnum.REQUESTED.ToString() || deliveryStatusCode == OrderViewStatusEnum.CONFIRMED.ToString() || deliveryStatusCode == OrderViewStatusEnum.READYTOPROCEED.ToString() || deliveryStatusCode == OrderViewStatusEnum.READYTODELIVER.ToString())
                    return OrderViewStatusEnum.ACTIVE.ToString();

                else if (deliveryStatusCode == OrderViewStatusEnum.REMOVED.ToString())
                    removedServices++;

                else if (deliveryStatusCode == null) return null;
            }

            if (removedServices == serviceList.Count())
                return OrderViewStatusEnum.CANCELLED.ToString();
            else
                return OrderViewStatusEnum.CLOSED.ToString();
        }

        public static string GetDeliveryStatusCode(ChangeToServiceType service, IEnumerable<Segment> segments, IEnumerable<OrderItems> orderItems)
        {
            var segment = segments.FirstOrDefault(x => (service.OrderServiceAssociation.Item as PaxSegmentRefType).PaxSegmentRefID == x.SegmentId);

            if (orderItems.Any(x => x.AccountableDocumentRefs != null))
            {
                var tktInfo = from orderItem in orderItems.Where(x => x.AccountableDocumentRefs != null)
                              from accDoc in orderItem.AccountableDocumentRefs
                              from coupon in accDoc.Coupons
                              from seg in segments.Where(x => x.SegmentId == coupon.SegmentRefId)
                              where service.PaxRefID == accDoc.CustomerRefId && (service.OrderServiceAssociation.Item as PaxSegmentRefType).PaxSegmentRefID == seg.SegmentId
                              select new { seg, coupon, DocumentType = accDoc.DocumentType.Value.ToString().Replace(Constants.Application.TicketTypeEnumChar, string.Empty) };


                if (!tktInfo.IsNullOrEmpty())
                {
                    foreach (var tkt in tktInfo)
                    {
                        if (tkt.seg.Status != null)
                        {
                            var ticketExpirationDate = tkt.coupon.TicketExpirationDate is null ? DateTime.Now : DateTime.ParseExact(tkt.coupon.TicketExpirationDate, "ddMMMyy", CultureInfo.InvariantCulture);

                            if (Constants.Application.FlightRequestedStatusCode.Contains(tkt.seg.Status.Code))
                            {
                                return OrderViewStatusEnum.REQUESTED.ToString();
                            }
                            else if (tkt.seg.Status.Code == Constants.Application.FlightStatusCodeRemoved)
                            {
                                return OrderViewStatusEnum.REMOVED.ToString();
                            }
                            else if ((Constants.Application.FlightStatusCode.Contains(tkt.seg.Status.Code) || tkt.seg.Status.Code == Constants.Application.FlightStatusCodeRemoved)
                                 && tkt.coupon.CouponStatus == Constants.Application.CouponStatusOpen && (tkt.DocumentType == Constants.Application.TicketTypeT || tkt.DocumentType == Constants.Application.DocTypePaidSeats))
                            {
                                return OrderViewStatusEnum.READYTOPROCEED.ToString();
                            }
                            else if (Constants.Application.FlightStatusCode.Contains(tkt.seg.Status.Code) && Constants.Application.CouponStatusReadyToDeliver.Contains(tkt.coupon.CouponStatus) && (tkt.DocumentType == Constants.Application.TicketTypeT || tkt.DocumentType == Constants.Application.DocTypePaidSeats))
                            {
                                return OrderViewStatusEnum.READYTODELIVER.ToString();
                            }
                            else if (Constants.Application.FlightStatusCode.Contains(tkt.seg.Status.Code) && tkt.coupon.CouponStatus == Constants.Application.CouponStatusFlwn && (tkt.DocumentType == Constants.Application.TicketTypeT || tkt.DocumentType == Constants.Application.DocTypePaidSeats))
                            {
                                return OrderViewStatusEnum.DELIVERED.ToString();
                            }
                            else if (tkt.coupon.CouponStatus == Constants.Application.CouponStatusVoid || tkt.coupon.CouponStatus == Constants.Application.CouponStatusRefunded || DateTime.Now > ticketExpirationDate)
                            {
                                return OrderViewStatusEnum.REMOVED.ToString();
                            }

                        }
                        else if (tkt.seg.Status.Code == Constants.Application.FlightStatusCodeRemoved)
                        {
                            return OrderViewStatusEnum.REMOVED.ToString();
                        }
                    }
                }

                else if (segment != null && Constants.Application.FlightStatusCode.Contains(segment.Status.Code))
                    return OrderViewStatusEnum.DELIVERED.ToString();

            }

            else if (segment != null && Constants.Application.FlightRequestedStatusCode.Contains(segment.Status.Code))
                return OrderViewStatusEnum.REQUESTED.ToString();

            else if (segment != null && Constants.Application.FlightStatusCode.Contains(segment.Status.Code))
                return OrderViewStatusEnum.CONFIRMED.ToString();

            else if (segment != null && Constants.Application.FlightStatusCodeRemoved.Contains(segment.Status.Code))
                return OrderViewStatusEnum.REMOVED.ToString();

            return null;
        }

        public static IEnumerable<ChangeToServiceType> GetServices(OrderItems orderItems, OrderDocument orderDocument, bool old)
        {
            int serviceId = 0;
            var services = new List<ChangeToServiceType>();

            foreach (var service in orderItems.Services)
            {
                foreach (var pax in service.ServiceBeneficiaries)
                {
                    services.Add(new()
                    {
                        ServiceID = $"SV{++serviceId}",
                        BookingRef = GetBookingRef(orderDocument),
                        OrderServiceAssociation = new OrderServiceAssociationType()
                        {
                            Item = new PaxSegmentRefType()
                            {
                                PaxSegmentRefID = service.SegmentRefIds?.FirstOrDefault()
                            }
                        },
                        PaxRefID = pax,
                        StatusCode = GetStatusCode(old ? orderDocument.OrderHistory?.OrderBy(x => x.Version).Last().DataList.SegmentDetails.First(x => x.SegmentId == service.SegmentRefIds.FirstOrDefault()) : orderDocument.DataList.SegmentDetails.First(x => x.SegmentId == service.SegmentRefIds.FirstOrDefault()), orderItems)
                    });
                }
            }

            return services;
        }

        public static string GetStatusCode(Segment segment, OrderItems orderItems)
        {
            string statusCode = null;

            if (segment.Status != null)
            {
                if (segment.Status.Code == Constants.Application.FlightStatusCodeRemoved)
                {
                    return OrderViewStatusEnum.CANCELLED.ToString();
                }
                else if (Constants.Application.FlightStatusCodeCancelled == segment.Status.Code)
                {
                    statusCode = OrderViewStatusEnum.CANCELLED.ToString();
                }
                else if (Constants.Application.FlightServiceWLStatusCode.Contains(segment.Status.Code))
                {
                    statusCode = OrderViewStatusEnum.WAITLISTED.ToString();
                }
                else if (Constants.Application.FlightStatusCode.Contains(segment.Status.Code))
                {
                    statusCode = OrderViewStatusEnum.CONFIRMED.ToString();
                }
                else if (Constants.Application.FlightRequestedStatusCode.Contains(segment.Status.Code))
                {
                    statusCode = OrderViewStatusEnum.REQUESTED.ToString();
                }
                else
                {
                    statusCode = orderItems.Services.First().Status?.Status;
                }
            }

            return statusCode;
        }

        private static BookingRefType[] GetBookingRef(OrderDocument orderDocument)
        {
            return new BookingRefType[]
            {
                new BookingRefType()
                {
                    BookingID = orderDocument.BookingReference?.RLOC,
                    BookingEntity = new BookingEntityType()
                    {
                        Item = new CarrierType()
                        {
                            AirlineDesigCode = "VS"
                        }
                    }
                }
            };
        }

        private static IEnumerable<TicketDocInfoType> MapTicketDocInfo(IEnumerable<OrderItems> orderItems, OrderDocument orderDocument)
        {
            return orderItems.Where(x => x.AccountableDocumentRefs != null).SelectMany(x => x.AccountableDocumentRefs)
                .Where(accDoc =>
                    accDoc.DocumentType != null &&
                    accDoc.DocumentType.Value.ToString().StartsWith(Constants.Application.TicketTypeT) &&
                    !string.IsNullOrWhiteSpace(accDoc.DocumentNumber) &&
                    accDoc.DocumentNumber.Length == 13).Select(accDoc => new TicketDocInfoType
                    {
                        PaxRefID = accDoc?.CustomerRefId,
                        BookingRef = GetBookingRef(orderDocument),
                        Ticket =
                        [
                            new TicketType
                            {
                                TicketDocTypeCode = Constants.Application.TicketTypeT,
                                TicketNumber = accDoc?.DocumentNumber,
                                Coupon = accDoc?.Coupons?.Select(coupon => new CouponType
                                {
                                    CouponNumber = Convert.ToDecimal(coupon?.CouponNumber),
                                    CouponStatusCode = coupon?.CouponStatus
                                }).ToArray()
                            }
                        ]
                    });
        }

        private static IEnumerable<OptionalOriginDestType> GetOriginDest(IEnumerable<OriginDestinationAssociation> originDestinationAssociations)
        {
            return originDestinationAssociations.Select((od, index) =>
            new OptionalOriginDestType()
            {
                DestCode = od.OriginDestination?.DestinationAirport,
                OriginCode = od?.OriginDestination?.OriginAirport,
                OriginDestID = od?.OdLabel,
                PaxJourneyRefID = od?.JourneyRefIds?.ToArray(),
            });
        }

        private static IEnumerable<PaxJourneyType> GetPaxJourney(IEnumerable<Journey> journeys)
        {
            return journeys.Select(journey => new PaxJourneyType()
            {
                Duration = GetDuration(TimeSpan.Parse(journey.Duration)),
                PaxSegmentRefID = journey?.FlightSegmentRefIds.ToArray(),
                PaxJourneyID = journey?.JourneyId
            });
        }

        private static IEnumerable<PaxType> GetPax(IEnumerable<Customer> customerDetails)
        {
            return customerDetails.Select((cust, index) =>
                new PaxType()
                {
                    Individual = new IndividualType()
                    {
                        Birthdate = Convert.ToDateTime(cust.DateOfBirth),
                        BirthdateSpecified = !string.IsNullOrEmpty(cust.DateOfBirth),
                        GenderCode = GetGenderCode(cust.Gender?.Split("I").FirstOrDefault()),
                        GenderCodeSpecified = cust.Gender != null,
                        GivenName = [cust.FirstName],
                        Surname = cust.LastName
                    },
                    PaxID = cust.CustId,
                    PTC = cust.Ptc,
                });
        }

        private static GenderCodeType GetGenderCode(string gender) =>
           gender switch
           {
               "M" => GenderCodeType.M,
               "F" => GenderCodeType.F,
               "U" => GenderCodeType.U,
               _ => GenderCodeType.X,
           };

        private static IEnumerable<PaxSegmentType> GetPaxSegment(IEnumerable<Segment> segmentDetails, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs)
        {
            var paxSegments = new List<PaxSegmentType>();
            int index = 0;

            foreach (var seg in segmentDetails)
            {
                paxSegments.Add(new PaxSegmentType
                {
                    PaxSegmentID = $"SEG{++index}",
                    CabinTypeAssociationChoice = new CabinTypeAssociationChoiceType()
                    {
                        Item = new CabinTypeType()
                        {
                            CabinTypeCode = seg.CabinType?.CabinCode,
                            CabinTypeName = seg.CabinType?.CabinName
                        }
                    },
                    MarketingCarrierRBD_Code = seg.ClassOfService,
                    DatedMarketingSegmentRefId = GetDatedMarketingSegmentRefId(seg, index, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs)
                });
            }

            return paxSegments;
        }

        private static string GetDatedMarketingSegmentRefId(Segment seg, int index, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs)
        {
            var dmsId = $"DMS{index}";

            datedMarketingSegments = datedMarketingSegments.Append(new DatedMarketingSegmentType
            {
                DatedMarketingSegmentId = dmsId,
                DatedOperatingSegmentRefId = GetDatedOperatingSegmentRefId(seg, index, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs),
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = seg.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = seg.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(seg.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierDesigCode = seg.MarketingCarrier,
                CarrierName = GetCarrierName(seg.MarketingCarrier),
                MarketingCarrierFlightNumberText = seg.MarketingFlightNumber
            });

            return dmsId;
        }

        private static string GetCarrierName(string code)
        {
            return code switch
            {
                "VS" => "Virgin Atlantic Airways",
                "DL" => "Delta Air Lines",
                "CA" => "Air China",
                "SQ" => "Singapore Airlines",
                "NZ" => "Air New Zealand",
                "VA" => "Virgin Australia",
                "AM" => "Aeromexico",
                "AF" => "Air France",
                "KL" => "KLM",
                "WS" => "WestJet",
                _ => default
            };
        }

        private static string GetDatedOperatingSegmentRefId(Segment seg, int index, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs)
        {
            var dosId = $"DOS{index}";

            datedOperatingSegments = datedOperatingSegments.Append(new DatedOperatingSegmentType()
            {
                DatedOperatingSegmentId = dosId,
                CarrierDesigCode = seg.OperatingCarrier ?? seg.MarketingCarrier,
                CarrierName = seg.OperatingCarrier == null ? GetCarrierName(seg.MarketingCarrier) : GetCarrierName(seg.OperatingCarrier),
                OperatingCarrierFlightNumberText = seg.OperatingFlightNumber ?? seg.MarketingFlightNumber,
                Duration = GetDuration(TimeSpan.Parse(seg.FlightDuration)),
                DatedOperatingLegRefID = GetDatedOperatingLegs(seg.Legs, seg.EquipmentCode, index, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs)
            });

            return dosId;
        }

        private static string[] GetDatedOperatingLegs(IEnumerable<Legs> legs, string aircraft, int index, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs)
        {
            var legIds = new List<string>();

            foreach (var leg in legs)
                legIds.Add(GetDatedOperatingLegRefID(leg, aircraft, index, ref datedMarketingSegments, ref datedOperatingSegments, ref datedOperatingLegs));

            return [.. legIds];
        }

        private static string GetDatedOperatingLegRefID(Legs legs, string equipmentCode, int index, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegments, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegments, ref IEnumerable<DatedOperatingLegType> datedOperatingLegs)
        {
            var dolId = $"DOL{index}";

            datedOperatingLegs = datedOperatingLegs.Append(new DatedOperatingLegType
            {
                DatedOperatingLegID = dolId,
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = legs.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(legs.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = legs.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(legs.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierAircraftType = new CarrierAircraftTypeType
                {
                    CarrierAircraftTypeCode = legs.CarrierAircraftTypeCode,
                    CarrierAircraftTypeName = legs.CarrierAircraftTypeName
                },
                OnGroundDuration = String.IsNullOrEmpty(legs.StopOverDuration) ? null : GetDuration(legs.StopOverDuration.Contains(':') ? TimeSpan.Parse(legs.StopOverDuration) : TimeSpan.FromMinutes(int.Parse(legs.StopOverDuration))),
                IATA_AircraftType = string.IsNullOrEmpty(equipmentCode) ? null : new IATA_AircraftTypeType() { IATA_AircraftTypeCode = equipmentCode },
            });

            return dolId;
        }

        public static string GetDuration(TimeSpan timeSpan)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, timeSpan.Days * 24 + timeSpan.Hours, Constants.Application.HourText, timeSpan.Minutes, Constants.Application.MinuteText, timeSpan.Seconds, Constants.Application.SecondsText);
        }

        public static IATAOrderChangeNotifRQ MapProactiveTTLRequest(OrderDocument orderDocument)
        {
            IEnumerable<DatedMarketingSegmentType> newDatedMarketingSegments = [];
            IEnumerable<DatedOperatingSegmentType> newDatedOperatingSegments = [];
            IEnumerable<DatedOperatingLegType> newDatedOperatingLegs = [];

            return new()
            {
                DistributionChain = MapDistributionChain(orderDocument.Parties).ToArray(),
                Notification = new()
                {
                    ChangeGroup =
                    [
                        new()
                        {
                            ChangeOperation =
                            [
                                new()
                                {
                                    New = MapNewDetails(orderDocument, ref newDatedMarketingSegments , ref newDatedOperatingSegments , ref newDatedOperatingLegs, true),
                                    ReasonCode = "116",
                                    ChangeDateTime = DateTime.UtcNow,
                                    ChangeTypeCode = "7",
                                    }
                            ],
                            OrderVersionNumber = Convert.ToString(1) // always 1 for proactive TTL as there is no change in order
                        }
                    ]
                }
            };
        }

        public static IATAOrderChangeNotifRQ MapReactiveTTLRequest(OrderDocument orderDocument)
        {
            IEnumerable<DatedMarketingSegmentType> newDatedMarketingSegments = [];
            IEnumerable<DatedOperatingSegmentType> newDatedOperatingSegments = [];
            IEnumerable<DatedOperatingLegType> newDatedOperatingLegs = [];
            return new()
            {
                DistributionChain = MapDistributionChain(orderDocument.Parties).ToArray(),
                Notification = new()
                {
                    ChangeGroup =
                    [
                        new()
                        {
                            ChangeOperation =
                            [
                                new()
                                {
                                    New = MapNewDetails(orderDocument, ref newDatedMarketingSegments , ref newDatedOperatingSegments , ref newDatedOperatingLegs, false, true),
                                    Old = new(){
                                        Order = [new()
                                        {
                                            LastModifiedDateTimeSpecified = true,
                                            LastModifiedDateTime = DateTime.UtcNow.AddHours(-24),
                                            OrderVersionNumber = Convert.ToString(1)
                                        }]
                                    },
                                    ReasonCode = "104",
                                    ChangeDateTime = DateTime.UtcNow,
                                    ChangeTypeCode = "7",
                                    }
                            ],
                            OrderVersionNumber = Convert.ToString(2) // always 2 for reactive TTL as there is no change in order
                        }
                    ]
                }
            };
        }
        public static IATAOrderChangeNotifRQ MapDuplicatePnrRequest(OrderDocument orderDocument)
        {
            var changeDateTime = !string.IsNullOrWhiteSpace(orderDocument?.ChangeDateTime?.UtcDateTime) ? DateTime.Parse(orderDocument.ChangeDateTime.UtcDateTime).ToUniversalTime() : DateTime.UtcNow;
            return new()
            {
                DistributionChain = MapDistributionChain(orderDocument.Parties).ToArray(),
                Notification = new()
                {
                    ChangeGroup =
                    [
                        new()
                        {
                            ChangeOperation =
                            [
                                new()
                                {
                                    ChangeDateTime = changeDateTime,
                                    ChangeTypeCode = Constants.Application.ChangeTypeCodeDuplicatePnr,
                                    New = new ChangeDetailsType
                                    {
                                        Order =
                                        [
                                            new()
                                            {
                                                OrderID = orderDocument.OrderNumber.OrderId
                                            }
                                        ],
                                        Warning =
                                        [
                                            new()
                                            {
                                                Code = "195",
                                                DescText = Constants.Application.DuplicatePnrWarningText,
                                                LangCode = "EN",
                                                TagText = "Duplicate PNR",
                                                TypeCode = "Warning"
                                            }
                                        ]
                                    },
                                    ReasonCode = Constants.Application.ReasonCodeDuplicatePnr
                                }
                            ],
                            OrderVersionNumber = Convert.ToString(Convert.ToInt32(orderDocument.Version) + 1)
                        }
                    ]
                }
            };
        }
    }
}
