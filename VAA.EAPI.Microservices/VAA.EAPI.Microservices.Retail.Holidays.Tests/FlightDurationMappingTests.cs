using Newtonsoft.Json.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Holidays.Tests;

public sealed class FlightDurationMappingTests
{
    [Fact]
    public void MultiSegmentFlight_MapsTbxDurationToSlice_NotLegOnAirDuration()
    {
        var source = JObject.Parse("""
        {
          "fareInfo": [
            {
              "searchSegmentIndex": 0,
              "flightOptions": [
                {
                  "reference": "F1",
                  "segments": [
                    { "reference": "S1", "departure": "LGW", "arrival": "DXB", "duration": 0 },
                    { "reference": "S2", "departure": "DXB", "arrival": "BKK", "duration": 0 },
                    { "reference": "S3", "departure": "BKK", "arrival": "USM", "duration": 1070 }
                  ]
                }
              ]
            }
          ]
        }
        """);

        var flightDetails = (FlightDetails)source;
        var slice = Assert.Single(flightDetails.Sectors);

        Assert.Equal("PT17H50M", slice.TotalSliceDuration);
        Assert.All(slice.Segments, segment => Assert.All(segment.Legs, leg => Assert.Null(leg.OnAirDuration)));
    }

    [Fact]
    public void DirectFlight_MapsTbxDurationToSlice()
    {
        var source = JObject.Parse("""
        {
          "fareInfo": [
            {
              "searchSegmentIndex": 0,
              "flightOptions": [
                {
                  "reference": "F1",
                  "segments": [
                    { "reference": "S1", "departure": "LHR", "arrival": "MLE", "duration": 620 }
                  ]
                }
              ]
            }
          ]
        }
        """);

        var flightDetails = (FlightDetails)source;
        var slice = Assert.Single(flightDetails.Sectors);

        Assert.Equal("PT10H20M", slice.TotalSliceDuration);
        Assert.Null(Assert.Single(Assert.Single(slice.Segments).Legs).OnAirDuration);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(null)]
    public void MultiSegmentFlight_WithInvalidFinalSegmentDuration_LeavesSliceDurationNull(int? finalSegmentDuration)
    {
        var finalSegment = new JObject
        {
            ["reference"] = "S2"
        };
        if (finalSegmentDuration.HasValue)
        {
            finalSegment["duration"] = finalSegmentDuration.Value;
        }

        var source = new JObject
        {
            ["fareInfo"] = new JArray
            {
                new JObject
                {
                    ["searchSegmentIndex"] = 0,
                    ["flightOptions"] = new JArray
                    {
                        new JObject
                        {
                            ["reference"] = "F1",
                            ["segments"] = new JArray
                            {
                                new JObject
                                {
                                    ["reference"] = "S1",
                                    ["duration"] = 620
                                },
                                finalSegment
                            }
                        }
                    }
                }
            }
        };

        var flightDetails = (FlightDetails)source;
        var slice = Assert.Single(flightDetails.Sectors);

        Assert.Null(slice.TotalSliceDuration);
    }

    [Fact]
    public void FareInfoGroups_WithConnection_UseFinalGroupsDurationForSlice()
    {
        var source = JObject.Parse("""
        {
          "fareInfo": [
            {
              "searchSegmentIndex": 0,
              "flightOptions": [
                {
                  "reference": "F1",
                  "segments": [
                    {
                      "reference": "S1",
                      "departureDateTime": "2026-10-06T12:45:00",
                      "departure": "HNL",
                      "arrivalDateTime": "2026-10-06T21:18:00",
                      "arrival": "LAX",
                      "duration": 0,
                      "direction": "I"
                    }
                  ]
                }
              ],
              "selectedFare": {
                "selectedFlightOptionDetail": {
                  "flightOptionRef": "F1",
                  "segmentDetails": [{ "segmentRef": "S1" }]
                }
              }
            },
            {
              "searchSegmentIndex": 1,
              "flightOptions": [
                {
                  "reference": "F1",
                  "segments": [
                    {
                      "reference": "S2",
                      "departureDateTime": "2026-10-06T22:35:00",
                      "departure": "LAX",
                      "arrivalDateTime": "2026-10-07T17:00:00",
                      "arrival": "LHR",
                      "duration": 1035,
                      "direction": "I"
                    }
                  ]
                }
              ],
              "selectedFare": {
                "selectedFlightOptionDetail": {
                  "flightOptionRef": "F1",
                  "segmentDetails": [{ "segmentRef": "S2" }]
                }
              }
            }
          ]
        }
        """);

        var flightDetails = (FlightDetails)source;
        var slice = Assert.Single(flightDetails.Sectors);

        Assert.Equal("PT17H15M", slice.TotalSliceDuration);
    }
}
