using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderTicketMS
{
    public static class OrderChangeHeldRequestMapper
    {
        public static OrderTicketMSCommand MapOrderTicketMSRequest(IATA_OrderChangeRQ orderChangeRq, OrderDocument orderDocument)
        {
            return new OrderTicketMSCommand()
            {
                BookingReferences = GetBookingRef(orderChangeRq.Request.BookingRef.First()),
                CustomerDetails = orderDocument.DataList.CustomerDetails,
                PriceClass = orderDocument.DataList.PriceClass,
                SegmentDetails = MapSegmentDetails(orderDocument),
                SelectedOffers = GetSelectedOffer(orderChangeRq?.Request, orderDocument),
                Parties = orderDocument.Parties,
                RequestType = "HoldBooking",
                Pos = orderDocument.PointOfSale
            };
        }

        private static IEnumerable<SelectedOffer> GetSelectedOffer(OrderChangeRequestType request, OrderDocument document)
        {
            return [new SelectedOffer
            {
                OfferItems = document.OrderItems.Select(orderItem => new OfferItem()
                {
                    OfferitemPricing =
                    [
                        new OfferitemPricing
                        {
                            FareCalculationText = orderItem.PriceDetails.First(x => x.FareCalculation != null).FareCalculation,
                            SalePrice =  orderItem.PriceDetails.ToList()
                        }
                    ],
                    Services = GetServices(orderItem.Services)
                }).ToList()
            }];
        }

        private static IEnumerable<OfferService> GetServices(IEnumerable<OrderService> services)
        {
            return services.Select(x => new OfferService() { ServiceBeneficiaries = x.ServiceBeneficiaries, FlightSegmentsRefIds = x.SegmentRefIds, PriceClassRefIds = x.PriceClassRefIds });
        }

        private static Pos GetPOS(POS_Type pointOfSale)
        {
            return new Pos()
            {
                CityCode = pointOfSale?.City.IATA_LocationCode,
                CountryCode = pointOfSale?.Country.CountryCode
            };
        }

        private static BookingReference GetBookingRef(BookingRefType bookingRef)
        {
            return new BookingReference()
            {
                RLOC = bookingRef.BookingID
            };
        }

        private static IEnumerable<PaymentDetails> GetPaymentDetails(PaymentFunctionsChangeType[] paymentFunctions, bool isCashPayment, bool isEasyPay)
        {
            return paymentFunctions.Select(pay =>
                new PaymentDetails()
                {
                    Amount = new CurrencyAmount()
                    {
                        BaseCurrency = pay.PaymentProcessingDetails.Amount.CurCode,
                        MonetaryAmount = pay.PaymentProcessingDetails.Amount.Value.ToString(),
                    },

                    CardMethod = (isCashPayment || isEasyPay) ? null : pay.PaymentProcessingDetails?.PaymentMethod == null && pay.PaymentProcessingDetails.Payer == null ? null : GetCardDetails((Domain.OrderChangeRQ.PaymentProcessingPaymentCardType)pay.PaymentProcessingDetails.PaymentMethod.Item)

                });
        }

        private static CardDetails GetCardDetails(PaymentProcessingPaymentCardType cardType)
        {
            return new CardDetails()
            {
                CardNumber = string.IsNullOrEmpty(cardType?.CardNumber) ? null : cardType?.CardNumber,
                ExpiryDate = cardType?.ExpirationDate,
            };
        }

        private static IEnumerable<Segment> MapSegmentDetails(OrderDocument orderDocument)
        {
            var segmentDetails = orderDocument.DataList.SegmentDetails;

            return segmentDetails.Select(seg =>
            {
                var servcies = orderDocument.OrderItems.FirstOrDefault()?.Services.FirstOrDefault(service => service.SegmentRefIds.Contains(seg.SegmentId));

                if (servcies != null)
                {
                    var priceClassRefId = servcies.PriceClassRefIds.FirstOrDefault();

                    if (priceClassRefId != null)
                    {
                        var farebasisCode = orderDocument.DataList.PriceClass.FirstOrDefault(p => priceClassRefId == priceClassRefId).FareBasisCode;

                        if (farebasisCode != null)
                        {
                            seg.FareBasis = farebasisCode;
                        }
                    }
                }

                return seg;
            });
        }
    }

}


