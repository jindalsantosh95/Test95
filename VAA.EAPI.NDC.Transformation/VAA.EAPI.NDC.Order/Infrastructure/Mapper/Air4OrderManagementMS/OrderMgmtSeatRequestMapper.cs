using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using OrderDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderDocument;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.Air4OrderManagementMS
{
    public class OrderMgmtSeatRequestMapper
    {
        public static OrderChangeRequest MapOrderSeatFulfilmentRQ(IATA_OrderChangeRQ orderModification, SeatAvailabilityResponse seatMap, IEnumerable<SeatDetails> seatDetails, OrderSearchResponse orderSearchResponse, string countryCode = null, OrderDocument orderDoc = null, IEnumerable<SeatTransactionId> transactionIds = null)
        {
            OrderChangeRequest orderChangeRequest = new OrderChangeRequest()
            {
                Pos = new EAPI.Domain.Virgin.Models.Common.Pos()
                {
                    CityCode = orderDoc?.PointOfSale?.CityCode ?? orderSearchResponse?.SegmentDetails?.FirstOrDefault().OriginCityCode,
                    NDCCityCodeLocation = orderDoc?.PointOfSale?.NDCCityCodeLocation,
                    CountryCode = orderDoc?.PointOfSale?.CountryCode ?? countryCode,
                    CurrencyCode = orderDoc?.PointOfSale?.CurrencyCode ?? seatDetails?.FirstOrDefault().Currency,
                },
                Parties = new()
                {
                    Participants =
                    [
                        new()
                        {
                            PartyType = seatMap.Parties.Participants.FirstOrDefault().PartyType,
                            AgencyID = seatMap.Parties.Participants.FirstOrDefault().AgencyID
                        }
                    ],
                },
                RLOC = orderDoc?.BookingReference?.RLOC ?? orderSearchResponse?.BookingReferences?.RLOC,
                OrderAmendments = GetOrderAmendments(seatMap, seatDetails),
                ChannelId = Constants.Application.NDC,
                PaymentDetails = [new() {
                    Amount = new ()
                    {
                        MonetaryAmount = seatDetails.Sum(x => Convert.ToDecimal(x.SeatPrice)).ToString().Replace(".",""),
                        BaseCurrency = seatDetails.First().Currency,
                        DecimalPrecisionCount = Convert.ToInt32(seatDetails.First().DecimalPrecisionCount)
                    },
                    FormOfPayments = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum
                }],
                SeatTransactionId = transactionIds != null ? transactionIds : null
            };

            return orderChangeRequest;
        }

        private static IEnumerable<OrderAmendment> GetOrderAmendments(SeatAvailabilityResponse seatMap, IEnumerable<SeatDetails> seatDetails)
        {
            return seatDetails.Where(x => x.IsInvalid == false).Select(seat =>
            {
                var custId = $"{seat.Beneficiary?.LastNameNumber}-{seat.Beneficiary?.FirstNameNumber}";

                var seatColumn = seatMap.FlightSegmentSeatMap?.FirstOrDefault(y => y.FlightSegment?.SegmentId == seat.AssociatedSegment?.SegmentId)?.Seatmap.SelectMany(y => y.SeatRows).SelectMany(q => q.SeatColumns)?.FirstOrDefault(t => t.CatalogueRefIds != null && t.CatalogueRefIds.Contains(seat.SeatOfferID) && t.SeatIdentifer == seat.SeatNumber);
                var offerItemId = seatColumn?.Eligibility?.FirstOrDefault(e => e.CustomerId.FirstOrDefault() == custId).OfferItemId?.FirstOrDefault()?.OfferItemId;

                return new OrderAmendment()
                {
                    ProductType = ProductTypeEnum.Seat,
                    OrderByReference =
                    [
                        new OrderByReference()
                        {
                            ItemsForAddition = new ItemsForAddition()
                            {
                                OfferRefID = seat.SeatOfferID?.Split('|')?.FirstOrDefault(),
                                OfferItems =
                                [
                                    new OfferItem()
                                    {
                                        OfferItemId = offerItemId
                                    }
                                ]
                            }
                        }
                    ],
                    FlightItems =
                    [
                        new FlightItems()
                        {
                            ActionContext = ActionContextEnum.Add,
                            CurrentSeatAssignment = seat.SeatNumber,
                            Segments = new() { SegmentId = seat.AssociatedSegment.SegmentId }
                        }
                    ],
                    CustomerPrices = new()
                    {
                        PriceDetails = [new() {
                         Amount = new() { MonetaryAmount = seat.SeatPrice },
                         PriceComponentType = PriceComponent.PriceComponentTypeEnum.BasePrice
                    }]
                    },
                    CustomerReference = [seat.Beneficiary.CustId]
                };
            });
        }

        public static OrderChangeRequest MapOrderSeatDeletionRQ(IEnumerable<string> segs, IEnumerable<SeatDetails> seatFulfilmentRequest, OrderSearchResponse orderSearchResponse, string countryCode = null, OrderDocument orderDoc = null)
        {
            return new OrderChangeRequest()
            {
                Pos = new EAPI.Domain.Virgin.Models.Common.Pos()
                {
                    CityCode = orderDoc?.PointOfSale?.CityCode ?? orderSearchResponse?.SegmentDetails?.FirstOrDefault().OriginCityCode,
                    CountryCode = orderDoc?.PointOfSale?.CountryCode ?? countryCode,
                    CurrencyCode = orderDoc?.PointOfSale?.CurrencyCode
                },
                RLOC = orderDoc?.BookingReference?.RLOC ?? orderSearchResponse?.BookingReferences?.RLOC,
                ExternalRloc = orderDoc?.BookingReference?.ExternalRecordLocator,
                OrderAmendments = GetOrderAmendments(segs, orderDoc, seatFulfilmentRequest),
                ChannelId = Constants.Application.NDC,
                CustomerDetails = orderDoc.DataList.CustomerDetails.Select(x => new EAPI.Domain.Virgin.Models.Customer.Customer()
                {
                    CustId = x.CustId,
                    FirstNameNumber = x.FirstNameNumber,
                    LastNameNumber = x.LastNameNumber
                }),
            };
        }

        private static IEnumerable<OrderAmendment> GetOrderAmendments(IEnumerable<string> segs, OrderDocument orderDocument, IEnumerable<SeatDetails> seatDetails)
        {
            return seatDetails.Where(s => segs.Contains(s.AssociatedSegment.SegmentId.Replace("SEG", "").PadLeft(2, '0'))).Select(s =>
            {
                var seg = s.AssociatedSegment;
                return new OrderAmendment()
                {
                    FlightItems = [new() {
                        ActionContext = ActionContextEnum.Delete,
                        CurrentSeatAssignment = s.SeatNumber,
                        Segments = new()
                        {
                            SegmentId = seg.SegmentId.Replace("SEG", "").PadLeft(2, '0'),
                            ClassOfService = seg.ClassOfService,
                            OriginAirportCode = seg.OriginAirportCode,
                            DestinationAirportCode = seg.DestinationAirportCode,
                            MarketingCarrier = seg.MarketingCarrier,
                            MarketingFlightNumber = seg.MarketingFlightNumber,
                            ScheduledDepartureTime = Convert.ToDateTime(seg.ScheduledDepartureTime.LocalDateTime),
                            OperatingCarrier = seg.OperatingCarrier,
                            OperatingFlightNumber = seg.OperatingFlightNumber
                        }
                    }],
                    ProductType = ProductTypeEnum.Seat,
                    CustomerReference = [s.Beneficiary.CustId],
                };
            });
        }
    }
}
