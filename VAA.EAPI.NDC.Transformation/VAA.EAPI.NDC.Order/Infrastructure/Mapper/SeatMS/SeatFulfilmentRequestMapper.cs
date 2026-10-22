using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.SeatMS
{
    public static class SeatFulfilmentRequestMapper
    {
        public static SeatFulfilmentRequest MapSeatRequest(OrderDocument orderDocument, IEnumerable<SeatDetails> seatDetails, SeatAvailabilityResponse seatAvailabilityResponse)
        {
            var selectedSeatInfo = GetSeatInfo(seatDetails, seatAvailabilityResponse);

            return new SeatFulfilmentRequest()
            {
                BookingReferences = orderDocument.BookingReference,
                ChannelId = "NDC",
                pos = new()
                {
                    CityCode = orderDocument.PointOfSale.CityCode,
                    CountryCode = orderDocument.PointOfSale.CountryCode,
                    CurrencyCode = orderDocument.PointOfSale.CurrencyCode,
                    NDCCityCodeLocation = orderDocument.PointOfSale.NDCCityCodeLocation
                },
                parties = new()
                {
                    Participants = orderDocument.Parties.Participants
                },
                SelectedSeatInfo = selectedSeatInfo,
                paymentDetails = [new()
                {
                    FormOfPayments = EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum ,
                    Amount =  new()
                    {
                        MonetaryAmount = selectedSeatInfo.Sum(x => Convert.ToDecimal(x.Amount.MonetaryAmount)).ToString().Replace(".",""),
                        DecimalPrecisionCount = selectedSeatInfo.First().Amount.DecimalPrecisionCount,
                        BaseCurrency = selectedSeatInfo.First().Amount.BaseCurrency
                    }
                }],
                SeatTransactionIds = new List<SeatTransactionId>(),
            };
        }

        private static IEnumerable<SelectedSeatInfo> GetSeatInfo(IEnumerable<SeatDetails> seatDetails, SeatAvailabilityResponse seatAvailabilityResponse)
        {
            return seatDetails.Select((s, index) =>
            {
                var seatMap = seatAvailabilityResponse.FlightSegmentSeatMap.FirstOrDefault(x => x.FlightSegment.MarketingFlightNumber == s.AssociatedSegment.MarketingFlightNumber);
                var seatCols = seatMap?.Seatmap?.SelectMany(x => x.SeatRows)?.SelectMany(x => x.SeatColumns);
                var seatColumn = seatCols?.FirstOrDefault(sc => sc.SeatIdentifer == s.SeatNumber);
                var catalogueId = seatColumn.Eligibility.FirstOrDefault(x => x.CustomerId.First() == s.Beneficiary.LastNameNumber + "-" + s.Beneficiary.FirstNameNumber).CatalogueRefId;
                var catalogue = seatAvailabilityResponse.FlightSegmentSeatMap.FirstOrDefault(x => x.Catalogue.Any(x => x.CatalogueId == catalogueId)).Catalogue.FirstOrDefault(x => x.CatalogueId == catalogueId);

                return new SelectedSeatInfo()
                {
                    AssociationId = $"0{index + 1}",
                    OfferId = catalogueId.Split("|").First(),
                    FlightIdRef = catalogue.FlightIdRef,
                    SeatNumber = s.SeatNumber,
                    Amount = new() { MonetaryAmount = catalogue.SeatPrice.MonetaryAmount.Replace(".","") , BaseCurrency = catalogue.SeatPrice.BaseCurrency , DecimalPrecisionCount = catalogue.SeatPrice.DecimalPrecisionCount},
                    OfferItemNumber = seatColumn.Eligibility.FirstOrDefault(x => x.CustomerId.First() == s.Beneficiary.LastNameNumber + "-" + s.Beneficiary.FirstNameNumber).OfferItemId.FirstOrDefault().OfferItemId,
                    CustomerRefId = s.Beneficiary.CustId
                };
            });
        }
    }
}
