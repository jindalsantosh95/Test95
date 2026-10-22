using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Seat;
using VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatAvailabitlityResponseMapper
    {
        public static SeatAvailabilityResponse MapSeatResponse(RetrievePassengerMapResponseType searchResponse, SeatAvailabilityQuery searchRequest)
        {
            var result = new SeatAvailabilityResponse();

            List<FlightSegmentSeatMapCatalogue> catalogueList = new();
            List<PriceDetails> priceDetails = new();

            var cabinList = searchResponse.PassengerCabin.Select(passengerCabinType => new SeatMap
            {
                SeatRows = GetCabinRows(passengerCabinType, searchRequest, catalogueList, priceDetails),
                ClassType = passengerCabinType.ClassType,
                Equipment = passengerCabinType.Equipment,
                Location = passengerCabinType.Location,
                SeatConfiguration = passengerCabinType.SeatConfiguration,
                FareSummary = GetFareSummary(priceDetails)
            }
            ).ToList();

            result.FlightSegmentSeatMap = new List<FlightSegmentSeatMap>();

            FlightSegmentSeatMap flightSegmentSeatMap = new()
            {
                Seatmap = new List<SeatMap>()
            };

            flightSegmentSeatMap.Seatmap.AddRange(cabinList);
            flightSegmentSeatMap.Catalogue = new List<FlightSegmentSeatMapCatalogue>();
            flightSegmentSeatMap.Catalogue.AddRange(catalogueList);
            flightSegmentSeatMap.FlightSegment = searchRequest.FlightSegments.FirstOrDefault(x => x.SegmentId == searchResponse?.FlightInfo?.SegmentNumber);
            flightSegmentSeatMap.FlightSegment.EquipmentCode = searchResponse.FlightInfo.CASSEquipmentCode;
            result.FlightSegmentSeatMap.Add(flightSegmentSeatMap);

            return result;
        }

        private static IEnumerable<FareSummary> GetFareSummary(List<PriceDetails> priceDetails)
        {
            List<FareSummary> fareSummary = new();

            if (priceDetails.Any(x => x.SeatType == Constants.Application.PrefferedSeat))
            {
                fareSummary.AddRange(priceDetails.Where(pd => pd.SeatType == Constants.Application.PrefferedSeat).GroupBy(pd => new { pd.BrandId, pd.SeatType })
                           .Select(group => new FareSummary
                           {
                               SeatType = group.Key.SeatType,
                               Min = group.Min(x => x.SeatPrice),
                               Max = group.Max(x => x.SeatPrice),
                               Currency = group.First().Currency,
                               BrandId = group.Key.BrandId,
                               ProductId = group.First().ProductId
                           }));
            }
            if (priceDetails.Any(x => x.SeatType == Constants.Application.ExitSeat))
            {
                fareSummary.AddRange(priceDetails.Where(pd => pd.SeatType == Constants.Application.ExitSeat).GroupBy(pd => new { pd.BrandId, pd.SeatType })
                           .Select(group => new FareSummary
                           {
                               SeatType = group.Key.SeatType,
                               Min = group.Min(x => x.SeatPrice),
                               Max = group.Max(x => x.SeatPrice),
                               Currency = group.First().Currency,
                               BrandId = group.Key.BrandId,
                               ProductId = group.First().ProductId
                           }));
            }
            if (priceDetails.Any(x => x.SeatType == Constants.Application.StandardSeat))
            {
                fareSummary.AddRange(priceDetails.Where(pd => pd.SeatType == Constants.Application.StandardSeat).GroupBy(pd => new { pd.BrandId, pd.SeatType })
                           .Select(group => new FareSummary
                           {
                               SeatType = group.Key.SeatType,
                               Min = group.Min(x => x.SeatPrice),
                               Max = group.Max(x => x.SeatPrice),
                               Currency = group.First().Currency,
                               BrandId = group.Key.BrandId,
                               ProductId = group.First().ProductId
                           }));
            }

            return fareSummary.Any() ? fareSummary : null;
        }

        private static List<SeatRow> GetCabinRows(PassengerCabinType passengerCabinType, SeatAvailabilityQuery searchRequest, List<FlightSegmentSeatMapCatalogue> catalogueList, List<PriceDetails> priceDetails)
        {
            var rowList = new List<SeatRow>();

            priceDetails.Clear();

            passengerCabinType.PassengerRow.ToList().ForEach(passengerRowType =>
            {
                if (!string.IsNullOrEmpty(passengerRowType.Number))
                {
                    rowList.Add(new SeatRow
                    {
                        ExitOnLeft = passengerRowType.ExitOnLeft,
                        ExitOnRight = passengerRowType.ExitOnRightSpecified,
                        RowNumber = passengerRowType.Number,
                        Missing = passengerRowType.Missing,
                        OverWing = passengerRowType.OverWing,
                        UpperDeck = passengerRowType.UpperDeck,
                        SeatColumns = GetColumnWithData(passengerRowType, searchRequest, catalogueList, priceDetails)
                    });
                }
                else if (string.IsNullOrEmpty(passengerRowType.Comment))
                {
                    rowList.Add(new SeatRow
                    {
                        SeatColumns = GetColumnWithData(passengerRowType, searchRequest, catalogueList, priceDetails)
                    });
                }
            });

            return rowList;
        }

        private static List<SeatColumn> GetColumnWithData(PassengerRowType passengerRowType, SeatAvailabilityQuery searchRequest, List<FlightSegmentSeatMapCatalogue> catalogueList, List<PriceDetails> priceDetails)
        {
            var columnList = new List<SeatColumn>();

            passengerRowType.PassengerColumn?.ToList().ForEach(passengerColumn =>
            {
                if (passengerColumn.Preferred && passengerColumn.Offer != null)
                    priceDetails.Add(new() { SeatType = Constants.Application.PrefferedSeat, SeatPrice = passengerColumn.Offer?.First().PriceAmount, Currency = passengerColumn.Offer?.First().CurrencyCode, BrandId = passengerColumn.BrandId, ProductId = passengerColumn.Offer.First().ProductId });

                if (passengerColumn.Exit && passengerColumn.Offer != null)
                    priceDetails.Add(new() { SeatType = Constants.Application.ExitSeat, SeatPrice = passengerColumn.Offer?.First().PriceAmount, Currency = passengerColumn.Offer?.First().CurrencyCode, BrandId = passengerColumn.BrandId, ProductId = passengerColumn.Offer.First().ProductId });

                if (!passengerColumn.Preferred && !passengerColumn.Exit && passengerColumn.Offer != null)
                    priceDetails.Add(new() { SeatType = Constants.Application.StandardSeat, SeatPrice = passengerColumn.Offer?.First().PriceAmount, Currency = passengerColumn.Offer?.First().CurrencyCode, BrandId = passengerColumn.BrandId, ProductId = passengerColumn.Offer.First().ProductId });

                columnList.Add(new SeatColumn
                {
                    Aisle = passengerColumn.Aisle,
                    Assigned = passengerColumn.Offer != null && passengerColumn.Offer.First().ProductAlreadyAssigned,
                    Available = passengerColumn.Available,
                    Bassinet = passengerColumn.Bassinet,
                    Broken = passengerColumn.Broken,
                    Bulkhead = passengerColumn.Bulkhead,
                    CharacteristicCodes = passengerColumn.CharacteristicCodes != null ? GetCharacteristicsCode(passengerColumn, new[] { ',' }) : null,
                    Empty = passengerColumn.Empty,
                    Exit = passengerColumn.Exit,
                    ForDisabled = passengerColumn.ForHandicapped,
                    Galley = passengerColumn.Galley,
                    Index = passengerColumn.Index,
                    IndexAlpha = string.Empty,
                    Missing = passengerColumn.Missing,
                    NoSeat = passengerColumn.NoSeat,
                    NoStorage = passengerColumn.NoStowage,
                    NoWindow = passengerColumn.NoWindow,
                    OccupationCode = passengerColumn.OccupationCode,
                    Reserved = passengerColumn.Reserved,
                    Seat = passengerColumn.Seat,
                    CatalogueRefIds = passengerColumn.Offer == null ? null : GetCatalogue(passengerColumn, catalogueList),
                    Attributes = GetAttributes(passengerColumn),
                    Eligibility = passengerColumn.Offer == null ? null : GetEligibility(passengerColumn, searchRequest, catalogueList),
                    SeatIdentifer = passengerColumn.SeatId,
                    Toilet = passengerColumn.Lavatory,
                    Pos = passengerColumn.Pos,
                    Blocked = passengerColumn.Blocked,
                    BrandId = passengerColumn.BrandId,
                    Closet = passengerColumn.Closet,
                    EconomyComfort = passengerColumn.EconomyComfort,
                    ExitOverride = false // tbc as per delta response
                });
            });

            return columnList;
        }

        private static IEnumerable<string> GetAttributes(PassengerColumnType passengerColumnType)
        {
            List<string> attributes = new();

            if (passengerColumnType.Preferred)
                attributes.Add("PREFERRED_SEAT");

            if (passengerColumnType.Exit)
                attributes.Add("EXIT_SEAT");

            if (passengerColumnType.LimitedRecline)
                attributes.Add("LIMITED_RECLINE_SEAT");

            return attributes.Count > 0 ? attributes : null;
        }

        private static List<string> GetCharacteristicsCode(PassengerColumnType passengerColumnType, char[] separator)
        {
            return passengerColumnType.CharacteristicCodes.Split(separator).ToList();
        }

        private static List<SeatEligibility> GetEligibility(PassengerColumnType pC, SeatAvailabilityQuery searchRequest, List<FlightSegmentSeatMapCatalogue> catalogueList)
        {
            var seatEligibilityList = new List<SeatEligibility>();

            pC.Offer?.ToList().ForEach(offer =>
            {
                seatEligibilityList.Add(new SeatEligibility
                {
                    CustomerId = new List<string> { offer.TravelerID },
                    EligibilityReason = offer.ReasonForIssuanceCode,
                    CustomerPTC = searchRequest.CustomerDetails?.Where(cC => cC.CustId == offer.TravelerID)?.Select(c => c.Ptc),
                    OfferItemId = new List<OfferItemNumber>
                    {
                        new()
                        {
                            OfferItemId = offer.OfferItemNumber,
                            OwnerAirline = searchRequest.FlightSegments.FirstOrDefault()?.MarketingCarrier,
                            OwnerNonAirline = string.Empty
                        }
                    }
                });
            });

            //Link catalogueId to each eligibile offers
            seatEligibilityList.ForEach(eligibleOffer =>
            {
                catalogueList.ForEach(catalogue =>
                {
                    var splitCatalogue = catalogue.CatalogueId.Split("|")[1].Split("_");
                    if (splitCatalogue.Any(offerItemId => offerItemId == eligibleOffer.OfferItemId.First().OfferItemId))
                        eligibleOffer.CatalogueRefId = catalogue.CatalogueId;
                });
            });

            return seatEligibilityList;
        }

        private static IEnumerable<string> GetCatalogue(PassengerColumnType passengerColumnType, List<FlightSegmentSeatMapCatalogue> catalogueList)
        {
            List<string> catalogueIds = new();

            var diffPriceGroup = passengerColumnType.Offer.GroupBy(offer => offer.PriceAmount).OrderByDescending(x => x.First().PriceAmount);

            foreach (var group in diffPriceGroup)
            {
                var catalogueId = string.Empty;

                foreach (var offer in group)
                {
                    catalogueId = string.IsNullOrEmpty(catalogueId) ? $"{offer.OfferId}|{offer.OfferItemNumber}" : $"{catalogueId}_{offer.OfferItemNumber}";
                }
                catalogueIds.Add(catalogueId);
            }

            foreach (var catalogueId in catalogueIds)
            {
                bool catalogueIdExists = catalogueList.Any(id => id.CatalogueId == catalogueId);

                if (!catalogueIdExists)
                {
                    var offerItemId = catalogueId.Split("|").Last().Split("_").First();

                    catalogueList.Add(new FlightSegmentSeatMapCatalogue
                    {
                        CatalogueId = catalogueId,
                        BrandId = passengerColumnType.BrandId,
                        FlightIdRef = passengerColumnType.Offer.First().FlightIdRef,
                        ProductGroup = passengerColumnType.Offer.First().ProductGroup,
                        ProductId = passengerColumnType.Offer.First().ProductId,
                        ProductName = passengerColumnType.Offer.First().ProductName,
                        ProductSubGroup = passengerColumnType.Offer.First().ProductSubGroup,
                        ReasonForIssuanceCode = passengerColumnType.Offer.First().ReasonForIssuanceCode,
                        ReasonForIssuanceSubCode = passengerColumnType.Offer.First().ReasonForIssuanceSubCode,
                        SeatPrice = new CurrencyAmount
                        {
                            BaseCurrency = passengerColumnType.Offer.Where(o => o.OfferItemNumber == offerItemId).First().CurrencyCode,
                            MonetaryAmount = passengerColumnType.Offer.Where(o => o.OfferItemNumber == offerItemId).First().PriceAmount,
                            DecimalPrecisionCount = int.Parse(passengerColumnType.Offer.Where(o => o.OfferItemNumber == offerItemId).First().DecimalPrecisionCount)
                        }
                    });
                }
            }

            return catalogueIds;
        }
    }
}
