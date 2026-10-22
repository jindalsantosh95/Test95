using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;
using VAA.EAPI.Microservices.Retail.Brand.Models.RouteHappy;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Brand
    {
        private static readonly string itinRegex = "^(?'o'[A-Z]{3})-(?'d'[A-Z]{3})-(?'c'[^-]{1,})-(?'n'[0-9]{1,})-(?'dt'[0-9]{8})-(?'p'.+)$";

        public ICollection<Itinerary> Itineraries { get; set; } = [];
        public IEnumerable<string> Errors { get; set; }

        public static explicit operator Brand((Resp source, BrandQueryRequest request) data)
        {
            if (data.source == null) return null;

            Models.Brand result = new()
            {
                Errors = []
            };

            foreach (var aItin in data.source.Itineraries)
            {
                Models.Itinerary bItin = new()
                {
                    IsValid = aItin.Valid ?? false
                };

                if (aItin.Errors?.Messages?.Count > 0)
                    result.Errors = aItin.Errors.Messages;

                if (aItin.Data != null)
                {
                    foreach (var aLeg in aItin.Data.Legs)
                    {
                        var totalTravelTime = (RegexCompiled.Replace(aLeg.TotalTravelTime, "[A-Za-z]", ":") + "0").Split(':');

                        int days = 0;
                        int hours = totalTravelTime[0].Parse(0);
                        int minutes = totalTravelTime[1].Parse(0);
                        int seconds = totalTravelTime[2].Parse(0);

                        if (hours == 24)
                        {
                            days = 1;
                            hours = 0;
                        }
                        else if (hours > 24)
                        {
                            var mod = hours % 24;
                            days = (hours - mod) / 24;
                            hours = mod;
                        }


                        Leg bLeg = new()
                        {
                            TravelTime = new TimeSpan(days, hours, minutes, seconds)
                        };

                        bItin.Legs.Add(bLeg);

                        foreach (var aFare in aLeg.Fares)
                        {
                            Fare bFare = new()
                            {
                                Product = data.source.Data.Brand.FirstOrDefault(i => i.Id.Equals(aFare.Brand))?.BrandName,
                                Fees = new Fees()
                                {
                                    Change = aFare.AdvanceChange.SelectMany(ac =>
                                    {
                                        var advanceChange = data.source.Data.AdvanceChange.First(s => s.Id.Equals(ac));

                                        return advanceChange.GetType()
                                                            .GetProperties()
                                                            .Where(x => x.Name != "Id" && x.GetValue(advanceChange, null) is not null)
                                                            .OrderByDescending(x => x.Name)
                                                            .Select(x =>
                                                            {
                                                                var values = (AdvanceChangeInner)x.GetValue(advanceChange, null);
                                                                var changePolicy = (ChangePolicy)values;

                                                                if (x.Name != "Anytime")
                                                                {
                                                                    var name = x.Name.SeparateUpperCamelCase();

                                                                    changePolicy.Headline = $"{name}: {changePolicy.Headline}";
                                                                    changePolicy.Description = $"{name}: {changePolicy.Description}";
                                                                }
                                                                return changePolicy;
                                                            });
                                    }),
                                    Cancellation = aFare.Cancellation.SelectMany(c =>
                                    {
                                        var cancellation = data.source.Data.Cancellation.First(s => s.Id.Equals(c));

                                        return cancellation.GetType()
                                                            .GetProperties()
                                                            .Where(x => x.Name != "Id" && x.GetValue(cancellation, null) is not null)
                                                            .OrderByDescending(x => x.Name)
                                                            .Select(x =>
                                                            {
                                                                var values = (CancellationInner)x.GetValue(cancellation, null);
                                                                var cancelPolicy = (CancelPolicy)values;

                                                                if (x.Name != "Anytime")
                                                                {
                                                                    var name = x.Name.SeparateUpperCamelCase();

                                                                    cancelPolicy.Headline = $"{name}: {cancelPolicy.Headline}";
                                                                    cancelPolicy.Description = $"{name}: {cancelPolicy.Description}";
                                                                }
                                                                return cancelPolicy;
                                                            });
                                    })
                                }
                            };

                            bLeg.Fares.Add(bFare);

                            foreach (var aSeg in aFare.Segments)
                            {
                                Match match = RegexCompiled.Match(aSeg.Id, itinRegex, RegexOptions.Singleline);
                                var marketingCarrier = match.Groups["c"].Value;
                                var marketingFlightNumber = int.Parse(match.Groups["n"].Value);
                                var origin = match.Groups["o"].Value;
                                var destination = match.Groups["d"].Value;

                                var requestedSegment = (from itinerary in data.request.Itinerary from segment in itinerary.Segments where segment.Origin == origin && segment.Destination == destination && segment.FlightNumber == marketingFlightNumber && segment.Carrier == marketingCarrier select new { segment.OperatingCarrier, segment.FareFamily })?.FirstOrDefault();

                                Segment bSeg = new()
                                {
                                    Carrier = marketingCarrier,
                                    FlightNumber = marketingFlightNumber,
                                    Origin = origin,
                                    Destination = destination,
                                    Product = match.Groups["p"].Value,
                                    flightDate = DateTime.ParseExact(match.Groups["dt"].Value, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
                                    Amenities = new Amenities()
                                    {
                                        Aircraft = (Models.Aircraft)data.source.Data.Amenity.Aircrafts.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Aircraft)),
                                        Beverage = (Models.Beverage)data.source.Data.Amenity.Beverages.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Beverage)),
                                        Entertainment = (Models.Entertainment)data.source.Data.Amenity.Entertainments.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Entertainment)),
                                        FreshFood = (Models.FreshFood)data.source.Data.Amenity.FreshFoods.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.FreshFood)),
                                        Layout = (Models.Layout)data.source.Data.Amenity.Layouts.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Layout)),
                                        Power = (Models.Power)data.source.Data.Amenity.Powers.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Power)),
                                        Seats = (Models.Seat)(data.source.Data.Amenity.Seats.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Seat)), requestedSegment?.OperatingCarrier, requestedSegment?.FareFamily),
                                        Wifi = (Models.Wifi)data.source.Data.Amenity.Wifis.FirstOrDefault(i => i.Id.Equals(aSeg.Amenity?.Wifi)),
                                    },
                                    BoardingPriorities = aSeg.BoardingPriority.SelectMany(x => x).Select(y => (BoardingPrority)data.source.Data.BoardingPriority.First(i => i.Id.Equals(y))),
                                    CarryOnBaggage = aSeg.CarryOnBag.SelectMany(x => x).Select(y => (BaggagePolicy)data.source.Data.CarryOnBag.First(i => i.Id.Equals(y))),
                                    CheckedBaggage = aSeg.CheckedBag.SelectMany(x => x).Select(y => (BaggagePolicy)data.source.Data.CheckedBag.First(i => i.Id.Equals(y))),
                                    CheckInPriority = aSeg.CheckInPriority.SelectMany(x => x).Select(y => (Models.CheckInPriority)data.source.Data.CheckInPriority.First(i => i.Id.Equals(y))),
                                    LoungeAccess = aSeg.LoungeAccess.SelectMany(x => x).Select(y => (Models.LoungeAccess)data.source.Data.LoungeAccess.First(i => i.Id.Equals(y))),
                                    SeatSelection = aSeg.SeatSelection.SelectMany(x => x).Select(y => (Models.SeatSelection)data.source.Data.SeatSelection.First(i => i.Id.Equals(y))),
                                    UpgradeEligibility = aSeg.UpgradeEligibility.SelectMany(x => x).Select(y => (Models.UpgradeEligibility)data.source.Data.UpgradeEligibility.First(i => i.Id.Equals(y))),
                                    Upas = aSeg.Upas.Select(y => (UPA)data.source.Data.Upa.First(i => i.Id.Equals(y))),
                                    OverNight = aSeg.Overnight.ToString()
                                };

                                bFare.Segments.Add(bSeg);
                            }
                        }
                    }
                }

                result.Itineraries.Add(bItin);
            }

            return result;
        }

    }
}
