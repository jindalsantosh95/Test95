using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using DimensionAllowance = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.DimensionAllowance;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper
{
    public static class ReferenceMSMapper
    {
        public static IEnumerable<FlightPassengerBaggage> UpdateBaggageDetails(Summary summaryResponse, IEnumerable<AirportInfo> airportDetails, IEnumerable<BaggageDetails> baggageDetails, FlightOfferResponse? flightOfferResponse)
        {
            if (summaryResponse.FlightPassengerBaggage == null || summaryResponse.FlightPassengerBaggage.Any(fpb => fpb.CarryOnBag.PassengerBaggageAllowance == null || fpb.CheckedBag.PassengerBaggageAllowance == null))
            {
                var ond = flightOfferResponse.DataList.OriginDestinationAssociations.FirstOrDefault();
                AirportInfo origin = airportDetails?.FirstOrDefault(x => x.AirportCode == ond.OriginDestination.OriginAirport);
                AirportInfo destination = airportDetails.FirstOrDefault(x => x.AirportCode == ond.OriginDestination.DestinationAirport);

                var routes = new List<Route>
                {
                    new() { From = origin?.CountryCode, To = destination?.CountryCode },
                    new() { From = destination?.CountryCode, To = origin?.CountryCode},
                    new() { From = origin?.CountryCode, To = "ROW" },
                    new() { From = "ROW", To = destination?.CountryCode }
                };

                var baggageInfo = baggageDetails.FirstOrDefault(x => x.Route.Any(route => routes.Any(r => r.From == route.From && r.To == route.To)));

                if (baggageInfo == null)
                {
                    return summaryResponse.FlightPassengerBaggage;
                }

                var carryOnBagsDimension = new DimensionAllowance { Length = "23", Width = "36", Height = "56", UnitOfMeasure = "CM", Linear = "0" };
                var checkedOnBagsDimension = new DimensionAllowance { Length = "90", Width = "75", Height = "43", UnitOfMeasure = "CM", Linear = "0" };

                return summaryResponse.FlightPassengerBaggage.Select(flightPassengerBaggage =>
                {
                    var fareFamily = summaryResponse?.Flights?.FirstOrDefault(x => x.FlightId == flightPassengerBaggage.FlightRefId)?.FareFamilyType;
                    var selectedBaggage = baggageInfo?.CabinAllowance?.FirstOrDefault(cabin => cabin.FareFamilies.Contains(fareFamily));

                    if (flightPassengerBaggage.CheckedBag.PassengerBaggageAllowance == null || flightPassengerBaggage.CarryOnBag.PassengerBaggageAllowance == null)
                    {
                        return new FlightPassengerBaggage
                        {
                            FlightRefId = flightPassengerBaggage.FlightRefId,
                            CarryOnBag = new CarryOnBag
                            {
                                PassengerBaggageAllowance = flightPassengerBaggage.CarryOnBag.PassengerBaggageAllowance ?? new PassengerBaggageAllowance
                                {
                                    FreeBaggageAllowance = selectedBaggage?.CarryOnBag?.FreeBaggageAllowance == null ? null : new FreeBaggageAllowance
                                    {
                                        AllowedBagCnt = selectedBaggage?.CarryOnBag?.FreeBaggageAllowance?.AllowedBagCnt ?? 0,
                                        DimensionAllowance = carryOnBagsDimension,
                                        Weight = new Search.Models.BFFSummary.Weight
                                        {
                                            UnitOfMeasureCnt = selectedBaggage?.CarryOnBag?.FreeBaggageAllowance?.Weight ?? 0,
                                            UnitOfMeasure = selectedBaggage?.CarryOnBag?.FreeBaggageAllowance?.UnitOfMeasure
                                        }
                                    },
                                    PaidBaggage = selectedBaggage?.CarryOnBag?.PaidBaggageAllowance != null ? GetPaidBaggages(selectedBaggage.CarryOnBag, carryOnBagsDimension) : null
                                }
                            },
                            CheckedBag = new CheckedBag
                            {
                                PassengerBaggageAllowance = flightPassengerBaggage.CheckedBag.PassengerBaggageAllowance ?? new PassengerBaggageAllowance
                                {
                                    FreeBaggageAllowance = selectedBaggage?.CheckedBag?.FreeBaggageAllowance == null ? null : new FreeBaggageAllowance
                                    {
                                        AllowedBagCnt = selectedBaggage?.CheckedBag?.FreeBaggageAllowance?.AllowedBagCnt ?? 0,
                                        DimensionAllowance = checkedOnBagsDimension,
                                        Weight = new Search.Models.BFFSummary.Weight
                                        {
                                            UnitOfMeasureCnt = selectedBaggage?.CheckedBag?.FreeBaggageAllowance?.Weight ?? 0,
                                            UnitOfMeasure = selectedBaggage?.CheckedBag?.FreeBaggageAllowance?.UnitOfMeasure
                                        }
                                    },
                                    PaidBaggage = selectedBaggage?.CheckedBag?.PaidBaggageAllowance != null ? GetPaidBaggages(selectedBaggage.CheckedBag, checkedOnBagsDimension) : null
                                }
                            }
                        };
                    }
                    else
                    {
                        return flightPassengerBaggage;
                    }
                });
            }
            else
                return summaryResponse.FlightPassengerBaggage;
        }

        private static IEnumerable<Search.Models.BFFSummary.PaidBaggage> GetPaidBaggages(CarryOnAndCheckedBag carryOnAndCheckedBag, DimensionAllowance dimensionAllowance)
        {
            List<Search.Models.BFFSummary.PaidBaggage> paidBaggages = new();

            if (carryOnAndCheckedBag.PaidBaggageAllowance != null)
            {
                if (carryOnAndCheckedBag?.PaidBaggageAllowance?.FirstBagCost != null)
                {
                    paidBaggages.Add(new Search.Models.BFFSummary.PaidBaggage()
                    {
                        DimensionAllowance = dimensionAllowance,
                        Price = new()
                        {
                            Amount = carryOnAndCheckedBag.PaidBaggageAllowance.FirstBagCost,
                            Currency = carryOnAndCheckedBag.PaidBaggageAllowance.Currency,
                        },
                        Weight = new Search.Models.BFFSummary.Weight
                        {
                            UnitOfMeasureCnt = carryOnAndCheckedBag.PaidBaggageAllowance.Weight ?? 0,
                            UnitOfMeasure = carryOnAndCheckedBag.PaidBaggageAllowance.UnitOfMeasure
                        }
                    });

                    if (carryOnAndCheckedBag?.PaidBaggageAllowance?.SecondBagCost > 0)
                    {
                        paidBaggages.Add(new Search.Models.BFFSummary.PaidBaggage()
                        {
                            DimensionAllowance = dimensionAllowance,
                            Price = new()
                            {
                                Amount = carryOnAndCheckedBag.PaidBaggageAllowance.SecondBagCost,
                                Currency = carryOnAndCheckedBag.PaidBaggageAllowance.Currency
                            },
                            Weight = new Search.Models.BFFSummary.Weight
                            {
                                UnitOfMeasureCnt = carryOnAndCheckedBag.PaidBaggageAllowance.Weight ?? 0,
                                UnitOfMeasure = carryOnAndCheckedBag.PaidBaggageAllowance.UnitOfMeasure
                            }
                        });
                    }
                }
                return paidBaggages;
            }
            return null;
        }
    }
}
