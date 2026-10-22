using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using Segment = VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS
{
    public static class OrderChangeRequestMapper
    {
        public static SabreOrderManagementMSCommand MapOrderChangeRequest(IATA_OrderChangeRQ orderChangeRq, Model.OrderMS.OrderRS orderDoc, OfferResponse offerDoc, string addCollectTransactionId = null , bool isChangeReshopHoldBooking = false, bool isChangeReshopConfirmHoldBooking = false, bool isReshopDisruption = false)
        {
            string orderDocTransactionId = orderDoc.OrderDocument.OrderItems.FirstOrDefault(item => item.PaymentDetails != null)?.PaymentDetails?.FirstOrDefault()?.PaymentReferenceId;
            var transactionIds = new List<string>() { orderDocTransactionId, addCollectTransactionId }.Where(value => value != null);

            return new SabreOrderManagementMSCommand
            {
                OrderChangeRequest = new OrderChangeRequest()
                {
                    RLOC = orderChangeRq.Request.BookingRef.First().BookingID,
                    PaymentDetails = GetPaymentDetails(orderChangeRq),
                    OrderAmendments = isReshopDisruption ? GetOrderAmendmentsforDisruption(orderDoc.OrderDocument, offerDoc) : GetOrderAmendments(orderDoc, offerDoc),
                    DataList = GetDataList(orderChangeRq, offerDoc.DataList, orderDoc,offerDoc?.FareTypeRequest),
                    SelectedOffers = GetSelectedOffer(offerDoc, offerDoc.DataList),
                    ChannelId = Constants.Application.NDC,
                    Pos = offerDoc.Pos,
                    Parties = orderDoc.OrderDocument.Parties,
                    TransactionId = transactionIds,
                    TimeLimits = (isChangeReshopConfirmHoldBooking || isChangeReshopHoldBooking) ? GetTicketingTimeLimit(offerDoc) : null,
                    IsChangeReshopConfirmHoldBooking = isChangeReshopConfirmHoldBooking,
                    IsChangeReshopHoldBooking = isChangeReshopHoldBooking,
                    IsReshopDisruption = isReshopDisruption,
                    AccountableDocumentRefs = orderDoc.OrderDocument.OrderItems.First().AccountableDocumentRefs
                }
            };
        }

        private static IEnumerable<TimeLimit> GetTicketingTimeLimit(OfferResponse offerResponse)
        {
            var offers = offerResponse.Offers;

            return offers.Where(x => x.TicketingTimeLimit != null).Select(offer => new TimeLimit()
            {
                Limit = offer.TicketingTimeLimit,
                Type = TimeLimit.Types.TicketingTimeLimit
            });
        }
        public static SabreOrderManagementMSCommand MapOrderChangeSSRRequest(IATA_OrderChangeRQ orderChangeRq, OrderRS orderDoc, FlightServiceResponse flightOfferServiceDoc)
        {
            return new SabreOrderManagementMSCommand
            {
                OrderChangeRequest = new OrderChangeRequest()
                {
                    RLOC = orderDoc.OrderDocument.BookingReference.RLOC,
                    Pos = orderDoc.OrderDocument.PointOfSale,
                    Parties = orderDoc.OrderDocument.Parties,
                    OrderAmendments = GetOrderAmendmentsForSpecialService(orderChangeRq, orderDoc, flightOfferServiceDoc)
                }
            };
        }

        private static IEnumerable<OrderAmendment> GetOrderAmendmentsForSpecialService(IATA_OrderChangeRQ orderChangeRq, OrderRS orderRS, FlightServiceResponse flightOfferServiceDoc)
        {
            return [new OrderAmendment()
            {
                ProductType = ProductTypeEnum.SSREnum,
                SpecialServiceRequestInfo = GetSpecialServiceRequestInfo(orderChangeRq, orderRS.OrderDocument, flightOfferServiceDoc)
            }];
        }

        private static IEnumerable<SpecialServiceRequestInfo> GetSpecialServiceRequestInfo(IATA_OrderChangeRQ orderChangeRq, Model.OrderMS.OrderDocument orderDoc, FlightServiceResponse flightOfferServiceDoc)
        {
            var offerItems = ((AcceptSelectedQuotedOfferListType)orderChangeRq?.Request?.ChangeOrderChoice?.Item)?.SelectedPricedOffer?.FirstOrDefault(x => x.SelectedOfferItem.Any(x => x.SelectedALaCarteOfferItem != null && !((PaxSegmentRefListType)x.SelectedALaCarteOfferItem.OfferFlightAssociations.Item).PaxSegmentRefID.IsNullOrEmpty())).SelectedOfferItem;

            return offerItems.Select(offerItem =>
            {
                var ssrCode = flightOfferServiceDoc.Offers.SelectMany(offer => offer.OfferItems).First(x => x.OfferItemId == offerItem.OfferItemRefID).SSRCode;
                var paxDetails = orderDoc?.DataList.CustomerDetails.First(x => x.CustId == offerItem.PaxRefID?.First());

                return new SpecialServiceRequestInfo()
                {
                    ActionContext = SpecialServiceRequestInfo.ActionContexts.Add,
                    AirlineCode = Constants.Application.VS,
                    AllPassengersFlag = Constants.Application.N,
                    AllSegmentsFlag = Constants.Application.N,
                    RequestCode = ssrCode,
                    StatusCode = Constants.Application.HK,
                    SegmentNumber = ((PaxSegmentRefListType)offerItem.SelectedALaCarteOfferItem.OfferFlightAssociations.Item)?.PaxSegmentRefID.First(),
                    FirstNameNumber = paxDetails?.FirstNameNumber,
                    LastNameNumber = paxDetails?.LastNameNumber,
                    FirstName = paxDetails?.FirstName,
                    LastName = paxDetails?.LastName
                };
            });
        }

        private static IEnumerable<PaymentDetails> GetPaymentDetails(IATA_OrderChangeRQ orderChangeRQ)
        {
            return orderChangeRQ.Request?.PaymentFunctions?.Select(pi =>
            new PaymentDetails()
            {
                Amount = pi.PaymentProcessingDetails.Amount == null ? null : new EAPI.Domain.Virgin.Models.Common.CurrencyAmount()
                {
                    BaseCurrency = pi.PaymentProcessingDetails.Amount.CurCode,
                    MonetaryAmount = pi.PaymentProcessingDetails.Amount.Value.ToString(),
                },
                CardMethod = (pi.PaymentProcessingDetails?.PaymentMethod.Item) as PaymentProcessingPaymentCardType == null && pi.PaymentProcessingDetails.Payer == null ? null : GetCardDetails((PaymentProcessingPaymentCardType)pi.PaymentProcessingDetails.PaymentMethod.Item, pi.PaymentProcessingDetails.Payer),
                BspMethod = string.IsNullOrEmpty(pi.PaymentMethodCriteria?.PaymentTypeCode) ? null : pi.PaymentMethodCriteria?.PaymentTypeCode
            });

        }

        private static CardDetails GetCardDetails(PaymentProcessingPaymentCardType cardType, PayerType payer)
        {
            return new CardDetails()
            {
                CardNumber = cardType.CardNumber,
                CardholderName = cardType.CardHolderName,
                CardNetworkCode = cardType.CardIssuerName,
                ExpiryDate = cardType.ExpirationDate,
            };
        }

        private static IEnumerable<OrderAmendment> GetOrderAmendments(Model.OrderMS.OrderRS orderDoc, OfferResponse offerDoc)
        {
            var orderAmendments = new List<OrderAmendment>();
            OrderAmendment orderAmendment;
            foreach (var item in offerDoc.Offers)
            {
                orderAmendment = new OrderAmendment()
                {
                    OrderByReference =
                     [
                         new OrderByReference()
                        {
                            ItemsForAddition = new ItemsForAddition()
                            {
                                OfferRefID = item.OfferId,
                                OfferItems = GetOfferItems(offerDoc.Offers)

                            },
                            ItemsForDeletion = GetItemsforDeletion(orderDoc.OrderDocument.OrderItems),
                            CustomerReference = orderDoc.OrderDocument?.DataList?.CustomerDetails?.Select(a=>a.CustId).ToArray()
                        }
                     ],
                    CustomerPrices = new CustomerPrice
                    {
                        Ptc = orderDoc.OrderDocument?.DataList?.CustomerDetails?.FirstOrDefault()?.Ptc,
                        FareCalc = item.OfferItems.First().OfferitemPricing.First().FareCalculationText,
                        PriceDetails = new List<PriceComponent>
                        {
                            new PriceComponent
                            {
                                PriceComponentType = PriceComponent.PriceComponentTypeEnum.BaseFare,
                                PriceComponentName = Enum.GetName(typeof(Price.PriceComponentTypes), Price.PriceComponentTypes.BasePriceEnum),
                            }
                        }
                    },
                    FlightItems =
                    [
                        new FlightItems()
                        {
                            ActionContext = ActionContextEnum.Add,
                            Segments = offerDoc.DataList.FlightSegments.Where(seg => !orderDoc.OrderDocument.DataList.SegmentDetails.Any(flightSeg =>
                                                                flightSeg.OriginAirportCode == seg.OriginAirportCode &&
                                                                flightSeg.DestinationAirportCode == seg.DestinationAirportCode &&
                                                                flightSeg.MarketingFlightNumber == seg.MarketingFlightNumber &&
                                                                flightSeg.ClassOfService == seg.ClassOfService &&
                                                                Convert.ToDateTime(flightSeg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime)
                                                                )),
                        },
                        new FlightItems()
                        {
                             ActionContext = ActionContextEnum.Delete,
                             Segments = orderDoc.OrderDocument.DataList.SegmentDetails.Where(seg => !offerDoc.DataList.FlightSegments.Any(flightSeg =>
                                                                flightSeg.OriginAirportCode == seg.OriginAirportCode &&
                                                                flightSeg.DestinationAirportCode == seg.DestinationAirportCode &&
                                                                flightSeg.MarketingFlightNumber == seg.MarketingFlightNumber &&
                                                                flightSeg.ClassOfService == seg.ClassOfService &&
                                                                Convert.ToDateTime(flightSeg.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime)
                                                                )),
                        }
                    ],
                    CustomerReference = orderDoc.OrderDocument.DataList.CustomerDetails.Select(a => a.CustId).ToArray(),//PNRReferences
                    ProductType = ProductTypeEnum.Flight
                };
                orderAmendments.Add(orderAmendment);
            }

            return orderAmendments;
        }

        private static IEnumerable<ItemsForDeletion> GetItemsforDeletion(IEnumerable<EAPI.Domain.Virgin.Models.Order.OrderItems> orderItems)
        {
            return orderItems.Select(item => new ItemsForDeletion { OrderItemId = item.OrderItemId });
        }

        private static IEnumerable<Model.OrderManagementMS.OfferItem> GetOfferItems(IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferDetails> offers)
        {
            return offers.SelectMany(x => x.OfferItems).Select(x => new Model.OrderManagementMS.OfferItem()
            {
                OfferItemId = x.OfferItemId
            });
        }

        private static Model.OrderMS.DataList GetDataList(IATA_OrderChangeRQ orderChangeRQ, Model.OfferRS.DataList dataList, OrderRS orderRS,FareTypeRequest fareTypeRequest)
        {
            var segServiceDefinitionIds = new Dictionary<string, string>();
            return new Model.OrderMS.DataList()
            {
                BaggageAllowances = dataList?.BaggageAllowances,
                CustomerDetails = orderRS.OrderDocument.DataList.CustomerDetails,
                Journey = dataList?.Journey.Select(journey => { journey.FlightSegmentRefIds = journey.FlightSegmentRefIds.Select(id => id.TrimStart('F')); return journey; }),
                OriginDestinationAssociations = dataList?.OriginDestinationAssociations,
                Penalty = dataList?.Penalty,
                PriceClass = dataList?.PriceClass.Select(pc => new PriceClass
                {

                    Id = pc.Id,
                    FareBasisCode = pc.FareBasisCode,
                    FareTypeCode = pc.FareTypeCode,
                    ProductName = pc.ProductName,
                    ClassOfService = pc.ClassOfService,
                    CabinType = pc.CabinType,
                    TicketDesigCode = (fareTypeRequest?.FareTypeCode == Constants.Application.PrivateFareTypeCode || fareTypeRequest?.FareTypeCode == Constants.Application.CorporateFareTypeCode) ? pc.TicketDesigCode : null,
                }),
                ServiceDefinitions = GetServiceDefinitions(dataList?.ProductComponents, dataList?.OfferSegments, segServiceDefinitionIds),
                SegmentDetails = GetSegmentDetails(dataList?.FlightSegments, dataList?.OfferSegments, segServiceDefinitionIds),
                OrderInstructions = orderRS.OrderDocument.DataList.OrderInstructions
            };
        }

        private static IEnumerable<ServiceDefinition> GetServiceDefinitions(IEnumerable<ProductComponent> productComponents, IEnumerable<OfferSegment> offerSegments, Dictionary<string, string> segServiceDefinitionId)
        {
            var result = offerSegments.Select((seg, index) =>
            {
                var descriptions = productComponents.Where(x => seg.ProductComponentRefIds?.Contains(x.Id) == true)?.Select(y => y.Description);

                if (!descriptions.Any()) return (null, default);

                var svID = $"SV{++index}";

                return (serviceDefinitions: new ServiceDefinition()
                {
                    Description = descriptions,
                    ServiceDefinitionId = svID
                }, segServicedefinitionIds: new KeyValuePair<string, string>(seg.FlightSegmentRefId.TrimStart('F'), svID));
            });

            foreach (var kv in result.Select(x => x.segServicedefinitionIds))
            {
                if (!kv.Equals(default(KeyValuePair<string, string>)))
                    segServiceDefinitionId.Add(kv.Key, kv.Value);
            }

            return result.Where(y => y.serviceDefinitions != null).Select(x => x.serviceDefinitions);
        }

        private static IEnumerable<Segment> GetSegmentDetails(IEnumerable<Segment> flightSegments, IEnumerable<OfferSegment> offerSegments, Dictionary<string, string> segServiceDefinitionIds)
        {
            foreach (var oseg in offerSegments)
            {
                var flightSegment = flightSegments.First(fseg => fseg.SegmentId.Trim('F').Equals(oseg.FlightSegmentRefId.Trim('F')));

                flightSegment.CabinType = oseg?.CabinType;
                flightSegment.ClassOfService = oseg?.ClassOfService;
                flightSegment.ServiceDefinitionsRefID = segServiceDefinitionIds.FirstOrDefault(ss => ss.Key == oseg.FlightSegmentRefId.Replace("F", "")).Value;
                flightSegment.SegmentId = flightSegment.SegmentId.TrimStart('F');
                flightSegment.TicketNotValidAfter = oseg.TicketNotValidAfter;
                flightSegment.TicketNotValidBefore = oseg.TicketNotValidBefore;
            }

            return flightSegments;
        }

        private static IEnumerable<Offers> GetSelectedOffer(OfferResponse offers, Model.OfferRS.DataList dataList)
        {
            return offers.Offers.Select(offer =>
                new Offers()
                {
                    OfferId = offer.OfferId,
                    OfferItems = offer.OfferItems.Select(x =>
                    {
                        return new EAPI.Domain.Virgin.Models.Offer.OfferItem()
                        {
                            OfferItemId = x.OfferItemId,
                            OfferitemPricing = x.OfferitemPricing,
                            Services = GetServices(x.Services, dataList),
                            TicketDesigCode = x.TicketDesigCode,
                        };
                    })
                }
            );
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferService> GetServices(IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferService> services, Model.OfferRS.DataList dataList)
        {
            return services.Select(service => { service.FlightSegmentsRefIds = service.ProductSegmentAssociation.OfferSegmentRefIds.SelectMany(sv => dataList.OfferSegments.Where(x => x.OfferSegmentId == sv).Select(y => y.FlightSegmentRefId.TrimStart('F'))); return service; });
        }

        public static SabreOrderManagementMSCommand MapOrderMgmtReqForDisruption(OrderDocument orderDocument)
        {
            return new()
            {
                OrderChangeRequest = new OrderChangeRequest()
                {
                    DataList = orderDocument.DataList,
                    Parties = orderDocument.Parties,
                    RLOC = orderDocument.BookingReference.RLOC,
                    ChangeType = orderDocument.ChangeType
                }
            };
        }

        public static SabreOrderManagementMSCommand MapOrderMgmtReqForReshopDisruption(OrderDocument orderDocument , OfferResponse offerResponse, bool isReshopDisruption)
        {
            return new()
            {
                OrderChangeRequest = new OrderChangeRequest()
                {
                    DataList = GetDataList(null, offerResponse.DataList, new Model.OrderMS.OrderRS { OrderDocument = orderDocument }, null),
                    Parties = orderDocument.Parties,
                    RLOC = orderDocument.BookingReference.RLOC,
                    ChangeType = orderDocument.ChangeType,
                    Pos = orderDocument.PointOfSale,
                    SelectedOffers = GetSelectedOffer(offerResponse, offerResponse.DataList),
                    OrderAmendments = GetOrderAmendmentsforDisruption(orderDocument, offerResponse),
                    IsReshopDisruption = isReshopDisruption
                }
            };
        }

        private static IEnumerable<OrderAmendment> GetOrderAmendmentsforDisruption(OrderDocument orderDocument, OfferResponse offerResponse)
        {
            return
            [
                new OrderAmendment
                {
                    ProductType = ProductTypeEnum.Flight,
                    FlightItems =
                    [
                        new FlightItems
                        {
                            ActionContext = ActionContextEnum.Add,
                            Segments = GetSegmentsForAddition(orderDocument.DataList.SegmentDetails, offerResponse.DataList.FlightSegments)
                        }
                    ]
                }
            ];
        }

        private static IEnumerable<Segment> GetSegmentsForAddition(IEnumerable<Segment> orderSegments, IEnumerable<Segment> offerSegments)
        {
            return offerSegments.Where(offerSegment => !orderSegments.Any(orderSegment =>
                orderSegment.Status?.Code == Constants.Application.HK &&
                orderSegment.OriginAirportCode == offerSegment.OriginAirportCode &&
                orderSegment.DestinationAirportCode == offerSegment.DestinationAirportCode &&
                orderSegment.MarketingFlightNumber == offerSegment.MarketingFlightNumber &&
                orderSegment.ClassOfService == offerSegment.ClassOfService &&
                Convert.ToDateTime(orderSegment.ScheduledDepartureTime.LocalDateTime) == Convert.ToDateTime(offerSegment.ScheduledDepartureTime.LocalDateTime)));
        }

    }
}
