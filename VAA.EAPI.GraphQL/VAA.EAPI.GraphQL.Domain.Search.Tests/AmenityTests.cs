using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.Amenity;
using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests;

public class AmentiyTests
{
    public AmentiyTests()
    {
    }

    // A BrandSegment actually represents a leg
    private static Models.Brand.Segment MockBrandSegment(string originAirportCode, string destinationAirportCode, int flightNum, bool dominantLeg, string fareFamily)
    {
        switch (fareFamily)
        {
            case "BUS-FIRST":
                return new Models.Brand.Segment
                {
                    Origin = originAirportCode,
                    Destination = destinationAirportCode,
                    FlightNumber = flightNum,
                    DominantLeg = dominantLeg,
                    Amenities = new Amenities
                    {
                        Seats = new Seat { DisplayText = "Fully flat bed" },
                        FreshFood = new FreshFood { DisplayText = "Upper class dining experience" },
                        Beverage = new Beverage { DisplayText = "Alcohol & beverages provided" },
                    },
                    CheckedBaggage = new List<BaggagePolicy> { new BaggagePolicy { FreeBags = new FreeBag {
                                                Pieces = 2,
                                                Weight = new BagWeight {  Weight = new BagProperties { Metric = 23 }, Application = "Free" },
                                                Size = new BagSize { Size = new BagProperties { Metric = 100 }, Application = "Free" }
                                            } } },
                    CarryOnBaggage = new List<BaggagePolicy> { new BaggagePolicy { FreeBags = new FreeBag {
                                                Pieces = 1,
                                                Weight = new BagWeight {  Weight = new BagProperties { Metric = 10 }, Application = "Free" },
                                                Size = new BagSize { Size = new BagProperties { Metric = 50 }, Application = "Free" }
                                            } } },
                    BoardingPriorities = new List<BoardingPrority> { new BoardingPrority { Application = "Free", CommercialName = "PRIORITY BOARDING" } },
                    CheckInPriority = new List<CheckInPriority> { new CheckInPriority { Application = "Free", ComercialName = "PRIORITY CHECKIN" } },
                    SeatSelection = new List<SeatSelection> { new SeatSelection { Application = "Free", ComercialName = "FULLY FLAT BED" } },
                    LoungeAccess = new List<LoungeAccess> { new LoungeAccess { Application = "Free", ComercialName = "CLUBHOUSE ACCESS" } },
                    Carrier = "VS", // This is response from ATPCO, so carrier is always marketing carrier
                    FareFamilyType = fareFamily
                };

            default:
                throw new NotImplementedException();
        }
    }

    private static Brand MockBrand(ICollection<Models.Brand.Segment> segments)
    {
        return new Brand()
        {
            Errors = new List<string>(),
            Itineraries = new List<Itinerary>()
            {
                new Itinerary()
                {
                    IsValid = true,
                    Amenities = null,
                    Legs = new List<Leg> {
                        new Leg {
                            Fares = new List<Fare> {
                                new Fare {
                                    Segments = segments
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static IEnumerable<OfferDetails> MockOffers(List<FareSegment> fareSegments)
    {
        return new List<OfferDetails>
        {
            new OfferDetails {
                OfferItems = new List<OfferItem>
                {
                    new OfferItem {
                        Services = new List<OfferService> {
                            new OfferService {
                                FareDetails = new FareDetails {
                                    FareSegments = fareSegments
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static Models.FlightOffer.Segment MockFlightOfferSegment(string segmentId, string operatingCarrier, Tuple<string, string>[] legOriginDestinationAirports)
    {
        return new Models.FlightOffer.Segment
        {
            SegmentId = segmentId,
            OperatingCarrier = operatingCarrier,
            OriginAirportCode = legOriginDestinationAirports.First().Item1,
            DestinationAirportCode = legOriginDestinationAirports.Last().Item2,
            Legs = legOriginDestinationAirports.Select(i => new Legs
            {
                OriginAirportCode = i.Item1,
                DestinationAirportCode = i.Item2
            })
        };
    }

    private static IEnumerable<AirlineInfo> MockAirlineInfo()
    {
        return new List<AirlineInfo>()
        {
            new AirlineInfo {
                CarrierCode = "VS",
                CarrierName = "Virgin Atlantic"
            },
            new AirlineInfo {
                CarrierCode = "AF",
                CarrierName = "AirFrance"
            },
            new AirlineInfo {
                CarrierCode = "DL",
                CarrierName = "Delta"
            },
            new AirlineInfo {
                CarrierCode = "KL",
                CarrierName = "KLM"
            }
        };
    }

    private static IEnumerable<CabinInfo> MockCabinInfo()
    {
        return new List<CabinInfo>()
        {
            new CabinInfo {
                Carrier = "VS",
                BrandName = "Upper Class",
                BrandID = "VSUP",
                FareFamily = "BUS-FIRST",
                AwardFareFamily = "AWARD-BUSINESS-FIRST"
            }
        };
    }

    [Fact]
    public void OneSegment_TwoLegs()
    {
        // Prepare
        var brandRS = MockBrand(new List<Models.Brand.Segment> {
            MockBrandSegment("LHR", "BGI", 186, true, "BUS-FIRST"),
            MockBrandSegment("BGI", "ANU", 186, true, "BUS-FIRST"),
        });
        var offers = MockOffers(new List<FareSegment> {
            new FareSegment { FlightSegmentRefID = "FSEGVS186LHRANU1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = true },
        });
        var flightOfferSegments = new List<Models.FlightOffer.Segment>
        {
            MockFlightOfferSegment("FSEGVS186LHRANU1", "VS", new [] { new Tuple<string, string>("LHR", "ANU")})
        };
        // Act
        var cabinAmenity = new CabinAmenity(brandRS, offers, flightOfferSegments, isAwardSearchResult: false, MockAirlineInfo(), MockCabinInfo());

        // Assert
        Assert.Single(cabinAmenity.CabinDetails.Segments);
        var segment = cabinAmenity.CabinDetails.Segments.First();
        Assert.Equal("LHR", segment.Origin);
        Assert.Equal("ANU", segment.Destination);
        Assert.Equal("Fully Flat Bed", segment.SeatSelection);
        Assert.Equal("Priority Boarding", segment.BoardingPriority);
        Assert.Equal("Priority Checkin", segment.CheckInPriority);
        Assert.Equal("Clubhouse Access", segment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", segment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", segment.CheckedBags);
        Assert.Equal("BUS-FIRST", segment.FareFamilyType);
        Assert.Equal("Upper Class", segment.DominantCabinName);
        Assert.Equal("T", segment.BookingClass);
    }

    [Fact]
    public void AllUpasFor_VsDom_VsNonDom()
    {
        // Prepare
        var brandRS = MockBrand(new List<Models.Brand.Segment> {
            MockBrandSegment("SFO", "LHR", 19, true, "BUS-FIRST"),
            MockBrandSegment("LHR", "LOS", 411, false, "BUS-FIRST"),
        });
        var offers = MockOffers(new List<FareSegment> {
            new FareSegment { FlightSegmentRefID = "FSEGVS19SFOLHR1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = true },
            new FareSegment { FlightSegmentRefID = "FSEGVS411LHRLOS1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = false }
        });
        var flightOfferSegments = new List<Models.FlightOffer.Segment>
        {
            MockFlightOfferSegment("FSEGVS19SFOLHR1", "VS", new [] { new Tuple<string, string>("SFO", "LHR")}),
            MockFlightOfferSegment("FSEGVS411LHRLOS1", "VS", new [] { new Tuple<string, string>("LHR", "LOS")})
        };
        // Act
        var cabinAmenity = new CabinAmenity(brandRS, offers, flightOfferSegments, isAwardSearchResult: false, MockAirlineInfo(), MockCabinInfo());

        // Assert
        Assert.Equal(2, cabinAmenity.CabinDetails.Segments.Count());
        var dominantSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == true);
        Assert.Equal("SFO", dominantSegment.Origin);
        Assert.Equal("LHR", dominantSegment.Destination);
        Assert.Equal("Fully Flat Bed", dominantSegment.SeatSelection);
        Assert.Equal("Priority Boarding", dominantSegment.BoardingPriority);
        Assert.Equal("Priority Checkin", dominantSegment.CheckInPriority);
        Assert.Equal("Clubhouse Access", dominantSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", dominantSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", dominantSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", dominantSegment.FareFamilyType);
        Assert.Equal("Upper Class", dominantSegment.DominantCabinName);
        Assert.Equal("T", dominantSegment.BookingClass);

        var minorSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == false);
        Assert.Equal("LHR", minorSegment.Origin);
        Assert.Equal("LOS", minorSegment.Destination);
        Assert.Equal("Fully Flat Bed", minorSegment.SeatSelection);
        Assert.Equal("Priority Boarding", minorSegment.BoardingPriority);
        Assert.Equal("Priority Checkin", minorSegment.CheckInPriority);
        Assert.Equal("Clubhouse Access", minorSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", minorSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", minorSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", minorSegment.FareFamilyType);
        Assert.Equal("Upper Class", minorSegment.DominantCabinName);
        Assert.Equal("T", minorSegment.BookingClass);
    }

    [Fact]
    public void AllUpasFor_NonVsDom_VsNonDom()
    {
        // Prepare
        var brandRS = MockBrand(new List<Models.Brand.Segment> {
            MockBrandSegment("LHR", "JFK", 3, false, "BUS-FIRST"),
            MockBrandSegment("JFK", "SFO", 3000, true, "BUS-FIRST")
        });
        var offers = MockOffers(new List<FareSegment> {
            new FareSegment { FlightSegmentRefID = "FSEGVS3LHRJFK1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = false },
            new FareSegment { FlightSegmentRefID = "FSEGDL3000JFKSFO1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = true }
        });
        var flightOfferSegments = new List<Models.FlightOffer.Segment>
        {
            MockFlightOfferSegment("FSEGVS3LHRJFK1", "VS", new [] { new Tuple<string, string>("LHR", "JFK") }),
            MockFlightOfferSegment("FSEGDL3000JFKSFO1", "DL", new [] { new Tuple<string, string>("JFK", "SFO") })
        };

        // Act
        var cabinAmenity = new CabinAmenity(brandRS, offers, flightOfferSegments, isAwardSearchResult: false, MockAirlineInfo(), MockCabinInfo());

        // Assert
        Assert.Equal(2, cabinAmenity.CabinDetails.Segments.Count());
        var dominantSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == true);
        Assert.Equal("JFK", dominantSegment.Origin);
        Assert.Equal("SFO", dominantSegment.Destination);
        Assert.Equal("Fully Flat Bed", dominantSegment.SeatSelection);
        Assert.Equal("Priority Boarding", dominantSegment.BoardingPriority);
        Assert.Equal("Priority Checkin", dominantSegment.CheckInPriority);
        Assert.Equal("Clubhouse Access", dominantSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", dominantSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", dominantSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", dominantSegment.FareFamilyType);
        Assert.Equal("Upper Class", dominantSegment.DominantCabinName);
        Assert.Equal("T", dominantSegment.BookingClass);

        var minorSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == false);
        Assert.Equal("LHR", minorSegment.Origin);
        Assert.Equal("JFK", minorSegment.Destination);
        Assert.Equal("Fully Flat Bed", minorSegment.SeatSelection);
        Assert.Equal("Priority Boarding", minorSegment.BoardingPriority);
        Assert.Equal("Priority Checkin", minorSegment.CheckInPriority);
        Assert.Equal("Clubhouse Access", minorSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", minorSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", minorSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", minorSegment.FareFamilyType);
        Assert.Equal("Upper Class", minorSegment.DominantCabinName);
        Assert.Equal("T", minorSegment.BookingClass);
    }

    [Fact]
    public void SingleUpasFor_VsDom_NonVsNonDom()
    {
        // Prepare
        var brandRS = MockBrand(new List<Models.Brand.Segment> {
            MockBrandSegment("LHR", "ATL", 103, true, "BUS-FIRST"),
            MockBrandSegment("ATL", "CHO", 1000, false, "BUS-FIRST"),
        });
        var offers = MockOffers(new List<FareSegment> {
            new FareSegment { FlightSegmentRefID = "FSEGVS103LHRATL1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = true },
            new FareSegment { FlightSegmentRefID = "FSEGDL1000ATLCHO1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = false }
        });
        var flightOfferSegments = new List<Models.FlightOffer.Segment>
        {
            MockFlightOfferSegment("FSEGVS103LHRATL1", "VS", new [] { new Tuple<string, string>("LHR", "ATL") }),
            MockFlightOfferSegment("FSEGDL1000ATLCHO1", "DL", new [] { new Tuple<string, string>("ATL", "CHO") })
        };

        // Act
        var cabinAmenity = new CabinAmenity(brandRS, offers, flightOfferSegments, isAwardSearchResult: false, MockAirlineInfo(), MockCabinInfo());

        // Assert
        Assert.Equal(2, cabinAmenity.CabinDetails.Segments.Count());
        var dominantSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == true);
        Assert.Equal("LHR", dominantSegment.Origin);
        Assert.Equal("ATL", dominantSegment.Destination);
        Assert.Equal("Fully Flat Bed", dominantSegment.SeatSelection);
        Assert.Equal("Priority Boarding", dominantSegment.BoardingPriority);
        Assert.Equal("Priority Checkin", dominantSegment.CheckInPriority);
        Assert.Equal("Clubhouse Access", dominantSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", dominantSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", dominantSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", dominantSegment.FareFamilyType);
        Assert.Equal("Upper Class", dominantSegment.DominantCabinName);
        Assert.Equal("T", dominantSegment.BookingClass);

        var minorSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == false);
        Assert.Equal("ATL", minorSegment.Origin);
        Assert.Equal("CHO", minorSegment.Destination);
        Assert.Null(minorSegment.SeatSelection);
        Assert.Null(minorSegment.BoardingPriority);
        Assert.Null(minorSegment.CheckInPriority);
        Assert.Null(minorSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", minorSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", minorSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", minorSegment.FareFamilyType);
        Assert.Equal("Upper Class", minorSegment.DominantCabinName);
        Assert.Equal("T", minorSegment.BookingClass);
    }

    [Fact]
    public void SingleUpasFor_NonVsDom_NonVsNonDom()
    {
        // Prepare
        var brandRS = MockBrand(new List<Models.Brand.Segment> {
            MockBrandSegment("LHR", "AMS", 2525, true, "BUS-FIRST"),
            MockBrandSegment("AMS", "JFK", 2500, false, "BUS-FIRST"),
        });
        var offers = MockOffers(new List<FareSegment> {
            new FareSegment { FlightSegmentRefID = "FSEGKL2525LHRAMS1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = true },
            new FareSegment { FlightSegmentRefID = "FSEGKL2500AMSJFK1", CabinName = "VSUP", BookingClass = "T", IsDominantLeg = false }
        });
        var flightOfferSegments = new List<Models.FlightOffer.Segment>
        {
            MockFlightOfferSegment("FSEGKL2525LHRAMS1", "KL", new [] { new Tuple<string, string>("LHR", "AMS") }),
            MockFlightOfferSegment("FSEGKL2500AMSJFK1", "KL", new [] { new Tuple<string, string>("AMS", "JFK") })
        };

        // Act
        var cabinAmenity = new CabinAmenity(brandRS, offers, flightOfferSegments, isAwardSearchResult: false, MockAirlineInfo(), MockCabinInfo());

        // Assert
        Assert.Equal(2, cabinAmenity.CabinDetails.Segments.Count());
        var dominantSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == true);
        Assert.Equal("LHR", dominantSegment.Origin);
        Assert.Equal("AMS", dominantSegment.Destination);
        Assert.Equal("Fully Flat Bed", dominantSegment.SeatSelection);
        Assert.Equal("Priority Boarding", dominantSegment.BoardingPriority);
        Assert.Equal("Priority Checkin", dominantSegment.CheckInPriority);
        Assert.Equal("Clubhouse Access", dominantSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", dominantSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", dominantSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", dominantSegment.FareFamilyType);
        Assert.Equal("Upper Class", dominantSegment.DominantCabinName);
        Assert.Equal("T", dominantSegment.BookingClass);

        var minorSegment = cabinAmenity.CabinDetails.Segments.First(l => l.DominantSegment == false);
        Assert.Equal("AMS", minorSegment.Origin);
        Assert.Equal("JFK", minorSegment.Destination);
        Assert.Null(minorSegment.SeatSelection);
        Assert.Null(minorSegment.BoardingPriority);
        Assert.Null(minorSegment.CheckInPriority);
        Assert.Null(minorSegment.LoungeAccess);
        Assert.Equal("1 × 10kg carry on bag", minorSegment.CarryOnBags);
        Assert.Equal("2 × 23kg checked bags", minorSegment.CheckedBags);
        Assert.Equal("BUS-FIRST", minorSegment.FareFamilyType);
        Assert.Equal("Upper Class", minorSegment.DominantCabinName);
        Assert.Equal("T", minorSegment.BookingClass);
    }
}