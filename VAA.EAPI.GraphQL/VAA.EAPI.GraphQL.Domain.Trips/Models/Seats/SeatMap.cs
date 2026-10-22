using HotChocolate;
using System.Collections.Generic;
using System.Data;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatMap")]
    public class SeatMap
    {
        public string SeatOfferId { get; set; }
        public string ClassType { get; set; }
        public string Equipment { get; set; }
        public IEnumerable<string> Details { get; set; }
        public IEnumerable<string> Configuration { get; set; }
        public IEnumerable<Row> Rows { get; set; }
        public IEnumerable<FareSummary> FareSummary { get; set; }

        public static explicit operator SeatMap((SeatMap lopaTemplate, Infrastructure.Models.SeatMS.SeatResponse seatMSResponse, bool disableExitFlag, IEnumerable<CabinRefData> cabinRefs) source)
        {
            var (lopaTemplate, seatMSResponse, disableExitFlag, cabinRefs) = source;

            lopaTemplate.SeatOfferId = seatMSResponse.SeatOfferId;
            var seatMap = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.Where(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any()));

            var classCode = lopaTemplate.ClassType?.ToUpperInvariant() switch
            {
                "UPPER CLASS" => "C",
                "PREMIUM ECONOMY" => "W",
                "ECONOMY" => "Y",
                _ => null
            };

            var matchingSeatMap = seatMap?.FirstOrDefault(sm => string.Equals(sm.ClassType, classCode, StringComparison.OrdinalIgnoreCase));

            return new()
            {
                ClassType = lopaTemplate.ClassType,
                Equipment = lopaTemplate.Equipment,
                Configuration = lopaTemplate.Configuration,
                Details = lopaTemplate.Details,
                SeatOfferId = lopaTemplate.SeatOfferId,
                Rows = lopaTemplate.Rows.Select(lopaRow =>
                {
                    var matchingSeatMapRows = matchingSeatMap.SeatRows.Where(row => !row.RowNumber.IsNullOrEmpty() && row.RowNumber == lopaRow?.RowNumber.ToString());

                    return new Row
                    {
                        ExitLeft = lopaRow.ExitLeft,
                        ExitRight = lopaRow.ExitRight,
                        RowNumber = lopaRow.RowNumber,
                        Columns = lopaRow.Columns?.Select(col =>
                        {
                            var matchingColumn = matchingSeatMapRows?.FirstOrDefault()?.SeatColumns.Where(seat => seat.Seat && seat.SeatIdentifer == col?.SeatNumber);
                            var isMatch = col?.SeatNumber != null && matchingColumn.FirstOrDefault()?.SeatIdentifer != null && col.SeatNumber == matchingColumn.FirstOrDefault()?.SeatIdentifer;

                            return new Column
                            {
                                Pos = matchingColumn?.FirstOrDefault()?.Pos,
                                SeatNumber = matchingColumn?.FirstOrDefault()?.SeatIdentifer,
                                Availability = isMatch ? (matchingColumn.First().Exit && disableExitFlag ? "BLOCKED" : matchingColumn.FirstOrDefault() switch
                                {
                                    { Reserved: true } => "OCCUPIED",
                                    { Available: true, Blocked: false } => "FREE",
                                    { Blocked: true } => "BLOCKED",
                                    _ => "BLOCKED"
                                }) : null,
                                Offers = isMatch ? matchingColumn?.First()?.Eligibility?.Select(offer =>
                                {
                                    var catalogueData = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First();
                                    return new Offer()
                                    {
                                        OfferId = offer.CatalogueRefId,
                                        OfferItemId = offer.OfferItemId.First().OfferItemId,
                                        CustomerId = offer.CustomerId.First(), //check
                                        ProductId = catalogueData.ProductId,
                                        ProductName = catalogueData.ProductName,
                                        FlightIdRef = catalogueData.FlightIdRef,
                                        FareProduct = catalogueData.BrandId,
                                        FareProductName = cabinRefs.FirstOrDefault(c => c.BrandID == catalogueData.BrandId)?.BrandName,
                                        Price = new()
                                        {
                                            Amount = Convert.ToDecimal(seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First().SeatPrice.MonetaryAmount),
                                            Currency = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First().SeatPrice.BaseCurrency
                                        }
                                    };
                                }) : null,
                                OverWing = lopaRow.OverWing,
                                Type = col?.Type,
                                Attributes = matchingColumn?.FirstOrDefault()?.Attributes
                            };
                        })
                    };
                })
            };
        }

        public static explicit operator SeatMap((Infrastructure.Models.SeatMS.SeatResponse seatMSResponse, bool disableExitFlag, string classType, IEnumerable<CabinRefData> cabinRefs) source)
        {
            var (seatMSResponse, disableExitFlag, classType, cabinRefs) = source;

            var seatMap = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.First();

            return new()
            {
                ClassType = classType,
                Configuration = seatMap.SeatConfiguration.Select(c => c != '|' ? c.ToString() : null),
                Equipment = seatMap.Equipment,
                SeatOfferId = seatMSResponse.SeatOfferId,
                Rows = seatMap.SeatRows.Where(row => !row.RowNumber.IsNullOrEmpty()).Select(row => new Row()
                {
                    RowNumber = Convert.ToInt32(row.RowNumber),
                    ExitLeft = row.ExitOnLeft,
                    ExitRight = row.ExitOnRight,
                    Columns = row.SeatColumns.Where(c => c != null).Select(c => new Column()
                    {
                        Type = c switch
                        {
                            { Toilet: true } => "LAVATORY",
                            { Aisle: true } => "AISLE",
                            { Galley: true } => "GALLEY",
                            { Seat: true } => "SEAT",
                            _ => string.Empty
                        },
                        SeatNumber = c.SeatIdentifer,
                        Availability = (c.Exit && disableExitFlag) ? "BLOCKED" : c switch
                        {
                            { Reserved: true } => "OCCUPIED",
                            { Available: true, Blocked: false } => "FREE",
                            { Blocked: true } => "BLOCKED",
                            _ => "BLOCKED"
                        },
                        OverWing = false,
                        Offers = c.Available ? c.Eligibility?.Select(e =>
                        {
                            var catalogueData = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First();
                            return new Offer()
                            {
                                OfferId = e.CatalogueRefId,
                                OfferItemId = e.OfferItemId.First().OfferItemId,
                                CustomerId = e.CustomerId.First(),
                                ProductId = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().ProductId,
                                ProductName = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().ProductName,
                                FlightIdRef = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().FlightIdRef,
                                FareProduct = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().BrandId,
                                FareProductName = cabinRefs.FirstOrDefault(c => c.BrandID == catalogueData.BrandId)?.BrandName,
                                Price = new()
                                {
                                    Amount = Convert.ToDecimal(seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().SeatPrice.MonetaryAmount),
                                    Currency = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().SeatPrice.BaseCurrency
                                }
                            };
                        }) : null,
                        Pos = c.Pos,
                        Attributes = c.Attributes
                    })
                })
            };
        }
    }
}