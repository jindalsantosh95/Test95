using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{

    public sealed class RewardSeatCalendarItaResponseMapperTests
    {
        private static readonly string[] Families = ["AWARD-ECONOMY", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY", "AWARD-BUSINESS-FIRST"];
        private static readonly string[] BookingCodes = ["A", "P", "G"];

        [Fact]
        public void Map_RejectsMissingSummaryRatherThanCachingAnEmptyCalendar()
        {
            var response = new result { Items = [new resultAvailSummary()] };
            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_TranslatesOnlyTheRecognizedNoOffersCodeToAnEmptyCompleteUnit()
        {
            var noOffers = new result { error = new error { code = "OfferSearchNoOffers" } };
            var lowercaseNoOffers = new result { error = new error { code = "offersearchnooffers" } };
            var unexpected = new result { error = new error { code = "DifferentBusinessError" } };

            Assert.Empty(RewardSeatCalendarItaResponseMapper.Map(noOffers, Request(), Policy(), Families, BookingCodes, "v1").Candidates);
            Assert.Empty(RewardSeatCalendarItaResponseMapper.Map(lowercaseNoOffers, Request(), Policy(), Families, BookingCodes, "v1").Candidates);
            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(unexpected, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsNoOffersWhenProviderAlsoReturnsAWarning()
        {
            var response = new result
            {
                error = new error { code = "OfferSearchNoOffers" },
                messages = new resultMessages { warning = [new resultMessagesWarning()] }
            };

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RecognizesCompleteEmptyTwoSummaryResult()
        {
            var response = new result
            {
                Items = [new resultAvailSummary(), new resultSliceBySliceSummary
                {
                    pages = new resultSliceBySliceSummaryPages { count = 1, current = 1 },
                    column = [new resultSliceBySliceSummaryColumn { cabin = Families[0] },
                        new resultSliceBySliceSummaryColumn { cabin = Families[1] },
                        new resultSliceBySliceSummaryColumn { cabin = Families[2] }]
                }]
            };
            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1");
            Assert.True(mapped.Complete);
            Assert.Empty(mapped.Candidates);
        }

        [Fact]
        public void Map_RejectsAdditionalPagesThatWereNotConsumed()
        {
            var response = new result
            {
                Items = [new resultAvailSummary(), new resultSliceBySliceSummary
                {
                    pages = new resultSliceBySliceSummaryPages { count = 2, current = 1 },
                    column = [new resultSliceBySliceSummaryColumn { cabin = Families[0] },
                        new resultSliceBySliceSummaryColumn { cabin = Families[1] },
                        new resultSliceBySliceSummaryColumn { cabin = Families[2] }]
                }]
            };
            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 2)]
        public void Map_RejectsMalformedPagingMetadata(int count, int current)
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single();
            pricing.pages = new resultSliceBySliceSummaryPages { count = count, current = current };

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsMissingPagingMetadata()
        {
            var response = OneCabinResponse();
            response.Items.OfType<resultSliceBySliceSummary>().Single().pages = null;

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_PreservesAllCabinPricesInventoryAndSaverClassification()
        {
            var availability = new resultAvailSummarySolutionSliceSegment
            {
                origin = "LHR",
                destination = "JFK",
                departure = new resultAvailSummarySolutionSliceSegmentDeparture { date = "2027-01-10", time = "10:00" },
                flight = new resultAvailSummarySolutionSliceSegmentFlight { carrier = "VS", number = 9 },
                leg = [new resultAvailSummarySolutionSliceSegmentLeg()],
                availability = [new resultAvailSummarySolutionSliceSegmentAvailability
                {
                    bookingCodeCount = [new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "A", count = 9, countSpecified = true },
                        new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "P", count = 4, countSpecified = true },
                        new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "G", count = 1, countSpecified = true }]
                }]
            };
            var pricingSegment = new resultSliceBySliceSummaryRowSliceSegment
            {
                origin = "LHR",
                destination = "JFK",
                departure = new resultSliceBySliceSummaryRowSliceSegmentDeparture { date = "2027-01-10", time = "10:00" },
                flight = new resultSliceBySliceSummaryRowSliceSegmentFlight { carrier = "VS", number = 9 },
                leg = [new resultSliceBySliceSummaryRowSliceSegmentLeg()],
                stopCount = 0,
                stopCountSpecified = true
            };
            var response = new result
            {
                Items = [new resultAvailSummary
                {
                    solution = [new resultAvailSummarySolution
                    {
                        itinerary = [new resultAvailSummarySolutionSlice { segment = [availability] }]
                    }]
                }, new resultSliceBySliceSummary
                {
                    pages = new resultSliceBySliceSummaryPages { count = 1, current = 1 },
                    column = [new resultSliceBySliceSummaryColumn { cabin = Families[0] },
                        new resultSliceBySliceSummaryColumn { cabin = Families[1] },
                        new resultSliceBySliceSummaryColumn { cabin = Families[2] }],
                    row = [new resultSliceBySliceSummaryRow
                    {
                        slice = new resultSliceBySliceSummaryRowSlice { segment = [pricingSegment] },
                        cell = [Cell(6000, 243.39m, "AFM06", "E"),
                            Cell(10500, 468.39m, "AFM06", "P", "P"),
                            Cell(350000, 720.39m, "AFM01", "G", "G")]
                    }]
                }]
            };

            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1");
            var candidates = mapped.Candidates.ToArray();

            Assert.Equal(3, candidates.Length);
            Assert.Equal(("awardEconomy", 9, 6000, 243.39m, true),
                (candidates[0].Cabin, candidates[0].InventoryCount, candidates[0].AwardPointsTotal,
                    candidates[0].CashTotal, candidates[0].IsSaverAward));
            Assert.Equal(("awardPremium", 4, 10500, 468.39m, true),
                (candidates[1].Cabin, candidates[1].InventoryCount, candidates[1].AwardPointsTotal,
                    candidates[1].CashTotal, candidates[1].IsSaverAward));
            Assert.Equal(("awardUpperClass", 1, 350000, 720.39m, false),
                (candidates[2].Cabin, candidates[2].InventoryCount, candidates[2].AwardPointsTotal,
                    candidates[2].CashTotal, candidates[2].IsSaverAward));
        }

        [Fact]
        public void Map_RejectsConflictingCountsForTheSameBookingCode()
        {
            var response = OneCabinResponse();
            var segment = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0].segment[0];
            segment.availability[0].bookingCodeCount =
            [new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "A", count = 3, countSpecified = true },
                new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "A", count = 5, countSpecified = true }];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_CollapsesIdenticalDuplicateBookingCodeCounts()
        {
            var response = OneCabinResponse();
            var segment = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0].segment[0];
            segment.availability[0].bookingCodeCount =
            [new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "A", count = 3, countSpecified = true },
                new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "A", count = 3, countSpecified = true }];

            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1");

            Assert.Equal(3, Assert.Single(mapped.Candidates).InventoryCount);
        }

        [Fact]
        public void Map_RejectsPricedCabinWhenBookingCodeInventoryIsMissing()
        {
            var response = OneCabinResponse();
            var segment = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0].segment[0];
            segment.availability[0].bookingCodeCount = [];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsPricedFareWhoseBookingCodeDiffersFromCabinInventory()
        {
            var response = OneCabinResponse();
            var fare = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].fare[0];
            fare.bookingInfo[0].bookingCode = "B";

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsPricedCabinWhenOneOfItsFaresHasADifferentBookingCode()
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0];
            pricing.fare = [pricing.fare[0], new resultSliceBySliceSummaryRowCellSolutionPricingFare
            {
                extendedFareCode = "SECOND", ptc = ["ADT"],
                bookingInfo = [new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = "B" }]
            }];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_TreatsExplicitZeroInventoryAsCompleteButUnavailable()
        {
            var response = OneCabinResponse();
            var count = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0].segment[0]
                .availability[0].bookingCodeCount[0];
            count.count = 0;

            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1");

            Assert.True(mapped.Complete);
            Assert.Empty(mapped.Candidates);
        }

        [Fact]
        public void Map_RejectsOmittedInventoryCountEvenWhenItDeserializesAsZero()
        {
            var response = OneCabinResponse();
            var count = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0].segment[0]
                .availability[0].bookingCodeCount[0];
            count.countSpecified = false;

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsOmittedDisplayAmountEvenWhenItDeserializesAsZero()
        {
            var response = OneCabinResponse();
            var total = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.displayTotal;
            total.amountSpecified = false;

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Theory]
        [InlineData("<bookingCodeCount code=\"A\" count=\"0\" />", true)]
        [InlineData("<bookingCodeCount code=\"A\" />", false)]
        public void Proxy_TracksWhetherInventoryCountWasPresentInTheProviderXml(string xml, bool expected)
        {
            var serializer = new XmlSerializer(typeof(resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount),
                new XmlRootAttribute("bookingCodeCount"));
            using var reader = new StringReader(xml);
            var count = (resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount)serializer.Deserialize(reader)!;

            Assert.Equal(expected, count.countSpecified);
        }

        [Theory]
        [InlineData("<displayTotal amount=\"0\" currency=\"GBP\" />", true)]
        [InlineData("<displayTotal currency=\"GBP\" />", false)]
        public void Proxy_TracksWhetherDisplayAmountWasPresentInTheProviderXml(string xml, bool expected)
        {
            var serializer = new XmlSerializer(typeof(resultSliceBySliceSummaryRowCellSolutionDisplayTotal),
                new XmlRootAttribute("displayTotal"));
            using var reader = new StringReader(xml);
            var total = (resultSliceBySliceSummaryRowCellSolutionDisplayTotal)serializer.Deserialize(reader)!;

            Assert.Equal(expected, total.amountSpecified);
        }

        [Theory]
        [InlineData("availability-itinerary")]
        [InlineData("availability-segment")]
        [InlineData("availability-leg")]
        [InlineData("pricing-segment")]
        [InlineData("pricing-leg")]
        [InlineData("pricing-stop-count")]
        public void Map_RejectsMissingDirectFlightMetadata(string missingComponent)
        {
            var response = OneCabinResponse();
            var availabilitySlice = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0];
            var pricingSlice = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].slice;
            switch (missingComponent)
            {
                case "availability-itinerary": response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary = null; break;
                case "availability-segment": availabilitySlice.segment = null; break;
                case "availability-leg": availabilitySlice.segment[0].leg = null; break;
                case "pricing-segment": pricingSlice.segment = null; break;
                case "pricing-leg": pricingSlice.segment[0].leg = null; break;
                case "pricing-stop-count": pricingSlice.segment[0].stopCountSpecified = false; break;
            }

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_SkipsAnExplicitlyStoppingPricingSegment()
        {
            var response = OneCabinResponse();
            var segment = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].slice.segment[0];
            segment.stopCount = 1;

            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1");

            Assert.True(mapped.Complete);
            Assert.Empty(mapped.Candidates);
        }

        [Fact]
        public void Map_SkipsAnExplicitlyMultiLegPricingSegmentWithoutStopCount()
        {
            var response = OneCabinResponse();
            var segment = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].slice.segment[0];
            segment.leg = [new resultSliceBySliceSummaryRowSliceSegmentLeg(), new resultSliceBySliceSummaryRowSliceSegmentLeg()];
            segment.stopCountSpecified = false;

            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1");

            Assert.True(mapped.Complete);
            Assert.Empty(mapped.Candidates);
        }

        [Fact]
        public void Map_AcceptsRepeatedEquivalentPassengerCodesFromSuppliedItaXml()
        {
            // Sanitized passenger shape from the supplied version-161 adult response.
            const string passengerXml = """
                <passenger id="0"><ptc>ADT</ptc><ptc>ADT</ptc></passenger>
                """;
            var serializer = new XmlSerializer(typeof(resultSliceBySliceSummaryRowCellSolutionPricingPassenger),
                new XmlRootAttribute("passenger"));
            using var reader = new StringReader(passengerXml);
            var passenger = (resultSliceBySliceSummaryRowCellSolutionPricingPassenger)serializer.Deserialize(reader)!;
            var response = OneCabinResponse();
            response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].passenger = [passenger];

            var candidate = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Assert.Equal(6000, candidate.AwardPointsTotal);
            Assert.Equal(243.39m, candidate.CashTotal);
            Assert.True(candidate.IsSaverAward);
        }

        [Theory]
        [InlineData("ADT", "CNN")]
        [InlineData("ADT", "")]
        [InlineData("ADT", " ")]
        public void Map_RejectsConflictingOrBlankPassengerCodes(string first, string second)
        {
            var response = OneCabinResponse();
            response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].passenger[0].ptc = [first, second];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_DoesNotCountRepeatedCodesAsAdditionalPassengers()
        {
            var response = OneCabinResponse();
            response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].passenger[0].ptc = ["ADT", "ADT"];
            var request = Request();
            var policy = Policy();
            policy.PassengerTypes = ["ADT", "ADT"];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, request, policy, Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsARequestedPassengerMissingFromTheProviderParty()
        {
            var response = OneCabinResponse();
            var request = Request();
            var policy = Policy();
            policy.PassengerTypes = ["ADT", "CNN"];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, request, policy, Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_AcceptsAnExplicitMixedPassengerParty()
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0];
            pricing.passenger =
            [new resultSliceBySliceSummaryRowCellSolutionPricingPassenger { id = "1", ptc = ["ADT"] },
                new resultSliceBySliceSummaryRowCellSolutionPricingPassenger { id = "2", ptc = ["CNN"] }];
            pricing.fare[0].ptc = ["ADT", "CNN"];
            var request = Request();
            var policy = Policy();
            policy.PassengerTypes = ["ADT", "CNN"];

            var mapped = RewardSeatCalendarItaResponseMapper.Map(response, request, policy, Families, BookingCodes, "v1");

            Assert.Equal(6000, Assert.Single(mapped.Candidates).AwardPointsTotal);
        }

        [Fact]
        public void Map_RejectsPartyWhenFareCoverageOmitsARequestedType()
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0];
            pricing.passenger =
            [new resultSliceBySliceSummaryRowCellSolutionPricingPassenger { id = "1", ptc = ["ADT"] },
                new resultSliceBySliceSummaryRowCellSolutionPricingPassenger { id = "2", ptc = ["CNN"] }];
            var request = Request();
            var policy = Policy();
            policy.PassengerTypes = ["ADT", "CNN"];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, request, policy, Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsFareCoverageWithAnUnrequestedPassengerType()
        {
            var response = OneCabinResponse();
            var fare = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].fare[0];
            fare.ptc = ["ADT", "CNN"];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_MixedSaverAndNonSaverFaresPreserveTheAnySaverClassification()
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0];
            pricing.fare = [pricing.fare[0], new resultSliceBySliceSummaryRowCellSolutionPricingFare
            {
                extendedFareCode = "F", ticketDesignator = "AFM01", ptc = ["ADT"],
                bookingInfo = [new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = "A" }]
            }];

            var candidate = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Assert.True(candidate.IsSaverAward);
        }

        [Fact]
        public void Map_RejectsMissingRepeatedPassengerEvenWhenItsTypeIsPresent()
        {
            var response = OneCabinResponse();
            var request = Request();
            var policy = Policy();
            policy.PassengerTypes = ["ADT", "ADT"];

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, request, policy, Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RejectsPartialOperatingFlightIdentity()
        {
            var response = OneCabinResponse();
            var segment = response.Items.OfType<resultAvailSummary>().Single().solution[0].itinerary[0].segment[0];
            segment.operationalFlight = new resultAvailSummarySolutionSliceSegmentOperationalFlight { carrier = "DL" };

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Theory]
        [InlineData("missing-ptc")]
        [InlineData("blank-ptc")]
        [InlineData("missing-booking-info")]
        [InlineData("blank-booking-code")]
        public void Map_RejectsIncompleteIdentityOnAnIndividualFare(string missingComponent)
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0];
            var fare = pricing.fare[0];
            pricing.fare = [fare, new resultSliceBySliceSummaryRowCellSolutionPricingFare
            {
                extendedFareCode = "F", ptc = ["ADT"],
                bookingInfo = [new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = "A" }]
            }];
            switch (missingComponent)
            {
                case "missing-ptc": fare.ptc = null; break;
                case "blank-ptc": fare.ptc = [" "]; break;
                case "missing-booking-info": fare.bookingInfo = null; break;
                case "blank-booking-code": fare.bookingInfo[0].bookingCode = " "; break;
            }

            Assert.Throws<InvalidOperationException>(() => RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1"));
        }

        [Fact]
        public void Map_RetainsMissingTicketDesignatorAsExplicitIdentityComponent()
        {
            var response = OneCabinResponse();
            var fare = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].fare[0];
            fare.ticketDesignator = null;

            var missing = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);
            fare.ticketDesignator = "<missing>";
            var literal = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Assert.Matches(@"\|v2:[0-9a-f]{64}$", missing.FareIdentity);
            Assert.NotEqual(missing.FareIdentity, literal.FareIdentity);
        }

        [Fact]
        public void Map_UsesUnambiguousIdentityForProviderControlledDelimiters()
        {
            var response = OneCabinResponse();
            var fare = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].fare[0];
            fare.extendedFareCode = "A:B";
            fare.ticketDesignator = "C";
            var first = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            fare.extendedFareCode = "A";
            fare.ticketDesignator = "B:C";
            var second = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Assert.NotEqual(first.FareIdentity, second.FareIdentity);
            Assert.Equal(first.FlightIdentity, second.FlightIdentity);
        }

        [Fact]
        public void Map_DistinguishesOneBookingInfoFromRepeatedEntries()
        {
            var response = OneCabinResponse();
            var fare = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0].fare[0];
            var first = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            fare.bookingInfo =
            [new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = "A" },
                new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = "A" }];
            var second = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Assert.NotEqual(first.FareIdentity, second.FareIdentity);
        }

        [Fact]
        public void Map_KeepsFareIdentityStableWhenProviderFareOrderChanges()
        {
            var response = OneCabinResponse();
            var pricing = response.Items.OfType<resultSliceBySliceSummary>().Single().row[0].cell[0].solution.pricing[0];
            pricing.fare = [pricing.fare[0], new resultSliceBySliceSummaryRowCellSolutionPricingFare
            {
                extendedFareCode = "SECOND", ticketDesignator = "SECOND", ptc = ["ADT"],
                bookingInfo = [new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = "A" }]
            }];
            var first = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Array.Reverse(pricing.fare);
            var second = Assert.Single(RewardSeatCalendarItaResponseMapper.Map(response, Request(), Policy(), Families, BookingCodes, "v1").Candidates);

            Assert.Equal(first.FareIdentity, second.FareIdentity);
        }

        private static result OneCabinResponse() => new()
        {
            Items = [new resultAvailSummary
            {
                solution = [new resultAvailSummarySolution
                {
                    itinerary = [new resultAvailSummarySolutionSlice
                    {
                        segment = [new resultAvailSummarySolutionSliceSegment
                        {
                            origin = "LHR", destination = "JFK",
                            departure = new resultAvailSummarySolutionSliceSegmentDeparture { date = "2027-01-10", time = "10:00" },
                            flight = new resultAvailSummarySolutionSliceSegmentFlight { carrier = "VS", number = 9 },
                            leg = [new resultAvailSummarySolutionSliceSegmentLeg()],
                            availability = [new resultAvailSummarySolutionSliceSegmentAvailability
                            {
                                 bookingCodeCount = [new resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount { code = "A", count = 3, countSpecified = true }]
                            }]
                        }]
                    }]
                }]
            }, new resultSliceBySliceSummary
            {
                pages = new resultSliceBySliceSummaryPages { count = 1, current = 1 },
                column = [new resultSliceBySliceSummaryColumn { cabin = Families[0] },
                    new resultSliceBySliceSummaryColumn { cabin = Families[1] },
                    new resultSliceBySliceSummaryColumn { cabin = Families[2] }],
                row = [new resultSliceBySliceSummaryRow
                {
                    slice = new resultSliceBySliceSummaryRowSlice
                    {
                        segment = [new resultSliceBySliceSummaryRowSliceSegment
                        {
                            origin = "LHR", destination = "JFK",
                            departure = new resultSliceBySliceSummaryRowSliceSegmentDeparture { date = "2027-01-10", time = "10:00" },
                            flight = new resultSliceBySliceSummaryRowSliceSegmentFlight { carrier = "VS", number = 9 },
                            leg = [new resultSliceBySliceSummaryRowSliceSegmentLeg()],
                            stopCount = 0, stopCountSpecified = true
                        }]
                    },
                    cell = [Cell(6000, 243.39m, "AFM06", "E"), new resultSliceBySliceSummaryRowCell(),
                        new resultSliceBySliceSummaryRowCell()]
                }]
            }]
        };

        private static resultSliceBySliceSummaryRowCell Cell(int points, decimal cash, string designator, string fareCode,
            string bookingCode = "A") =>
            new()
            {
                solution = new resultSliceBySliceSummaryRowCellSolution
                {
                    awardPointsTotal = points,
                    awardPointsTotalSpecified = true,
                    displayTotal = new resultSliceBySliceSummaryRowCellSolutionDisplayTotal { amount = cash, amountSpecified = true, currency = "GBP" },
                    pricing = [new resultSliceBySliceSummaryRowCellSolutionPricing
                    {
                        passenger = [new resultSliceBySliceSummaryRowCellSolutionPricingPassenger { id = "1", ptc = ["ADT"] }],
                        fare = [new resultSliceBySliceSummaryRowCellSolutionPricingFare
                        {
                            extendedFareCode = fareCode, ticketDesignator = designator, ptc = ["ADT"],
                            bookingInfo = [new resultSliceBySliceSummaryRowCellSolutionPricingFareBookingInfo { bookingCode = bookingCode }]
                        }]
                    }]
                }
            };

        private static RewardSeatCalendarRequest Request() => new()
        {
            Origin = "LHR",
            Destination = "JFK",
            AnchorDate = new DateOnly(2027, 1, 10),
            IntervalStart = new DateOnly(2027, 1, 1),
            IntervalEnd = new DateOnly(2027, 1, 31)
        };

        private static RewardSeatCalendarPolicy Policy() => new()
        {
            MarketPolicyVersion = "v1",
            CountryIssued = "GBR",
            CountrySold = "GBR",
            Currency = "GBP",
            SalesCity = "LON"
        };
    }
}
