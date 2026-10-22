using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;
using Xunit;
using CheckInApplication = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants.Application;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Tests
{
    public class CheckInRequirementsTests
    {
        [Fact]
        public void ConfiguresReturnOnwardTicketRequirementWithCustomerFacingMessage()
        {
            Assert.Contains("travelrequirement_030_011", CheckInApplication.TravelRequirementExceptionErrors);

            var error = CheckInMessageHelper.GetTrmErrorMessage("travelrequirement_030_011");

            Assert.Equal("Something's not quite right", error.Item1);
            Assert.Equal("Your destination needs a few more details about your travel plans. Please check in at the airport and show a ticket to leave your destination or other documents.", error.Item2);
        }

        [Fact]
        public void ConfiguresDomesticOnlyTravelRequirementsErrorWithoutTitle()
        {
            var error = CheckInMessageHelper.GetTrmErrorMessage("travelrequirement_020_003");

            Assert.Equal(string.Empty, error.Item1);
            Assert.Equal("Your domestic flight has no special travel requirements. However, government or health requirements may still apply. Please check at the airport.", error.Item2);
        }

        [Fact]
        public void ProjectsReturnOnwardTicketRequirementIntoPassportErrorsForSelectedCheckInPassenger()
        {
            const string trmPassengerResourceId = "aa587b91-98ee-4afb-abae-1cf2c4e2a5d4";
            const string tripId = "ff9a32a4-80f1-4487-8914-772b1fc7a276";

            var requirementsResponse = new DeltaTrm.RequirementsResponse
            {
                RequirementsResourceId = "940d1178-c91f-4cdc-bc93-3402410479be",
                TravelRequirements = new DeltaTrm.TravelRequirements
                {
                    BookingReference = new DeltaTrm.BookingReference
                    {
                        Id = "D9CPA3",
                        CreationDate = "2026-09-27T00:00:00Z"
                    },
                    Trips =
                    [
                        new DeltaTrm.Trip
                        {
                            Id = tripId,
                            Segments =
                            [
                                new DeltaTrm.DatedOperatingSegment
                                {
                                    Legs =
                                    [
                                        new DeltaTrm.DatedOperatingLeg
                                        {
                                            Departure = new DeltaTrm.TransportDeparture { Station = new DeltaTrm.Station { Code = "LHR" } },
                                            Arrival = new DeltaTrm.TransportArrival { Station = new DeltaTrm.Station { Code = "BGI" } }
                                        }
                                    ]
                                }
                            ]
                        }
                    ],
                    Passengers =
                    [
                        new DeltaTrm.Passenger
                        {
                            PassengerResourceId = trmPassengerResourceId,
                            NameNum = "01.01",
                            CustomerId = "9999116982373",
                            AdvancePassengerInformation =
                            [
                                new DeltaTrm.AdvancePassengerInformation
                                {
                                    TravelDocuments = [],
                                    PassengerTripHealthRequirements =
                                    [
                                        new DeltaTrm.PassengerTripHealthRequirement
                                        {
                                            TripId = tripId,
                                            RequirementCategories =
                                            [
                                                new DeltaTrm.RequirementCategory
                                                {
                                                    Type = DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS,
                                                    Requirements =
                                                    [
                                                        new DeltaTrm.Requirement
                                                        {
                                                            RequirementType = DeltaTrm.RequirementType.PASSPORT,
                                                            Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.PRESENT },
                                                            Exceptions =
                                                            [
                                                                new DeltaTrm.Error
                                                                {
                                                                    Code = "travelrequirement_030_011",
                                                                    Category = DeltaTrm.ErrorCategoryType.VALIDATE,
                                                                    DisplayCode = "ReturnOnwardTicketNeeded"
                                                                }
                                                            ]
                                                        }
                                                    ]
                                                }
                                            ]
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            };

            var travelParty = new DeltaSsci.TravelParty
            {
                Id = "travel-party-id",
                Passengers =
                [
                    new DeltaSsci.Passenger1
                    {
                        Id = "ssci-passenger-id",
                        CustomerId = "9999116982373",
                        FirstNIN = "01",
                        LastNIN = "01"
                    }
                ],
                Trip = new DeltaSsci.Trip2
                {
                    Segments =
                    [
                        new DeltaSsci.Segment { OriginCode = "LHR", DestinationCode = "BGI", OperatingAirlineCode = "VS" }
                    ]
                }
            };

            var response = (CheckInResponse)(travelParty, requirementsResponse, null,
                new PassengerId($"01.01|ssci-passenger-id|9999116982373|{trmPassengerResourceId}"), true);

            var passenger = Assert.Single(response.Passengers);
            var passportError = Assert.Single(passenger.Requirements.PassportErrors);

            Assert.Equal("travelrequirement_030_011", passportError.Code);
            Assert.Equal("Something's not quite right", passportError.Title);
            Assert.Equal("Your destination needs a few more details about your travel plans. Please check in at the airport and show a ticket to leave your destination or other documents.", passportError.Message);
            Assert.False(response.OnlineCheckInStatus.Available);
            Assert.Equal("travelrequirement_030_011", response.OnlineCheckInStatus.Code);
        }

        [Theory]
        [InlineData(DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE, "Available 72 hours before departure")]
        [InlineData(DeltaTrm.CalculationFromCodeType.LEG_DEPARTURE, "Available 72 hours before leg departure")]
        public void CalculatesFutureContactTracingAvailabilityFromAcceptanceWindow(
            DeltaTrm.CalculationFromCodeType calculationFromType,
            string expectedDescription)
        {
            var tripRequirement = CreateTripRequirement("PT72H", "2000-01-01 00:00:00Z", calculationFromType);
            var trip = CreateTrip("2099-07-24T10:30:00Z");

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, null);

            Assert.Equal(ContactTracingStatus.NEEDED_LATER, result.ContactTracing);
            Assert.Equal(
                new DateTime(2099, 7, 21, 10, 30, 0, DateTimeKind.Utc),
                result.ContactTracingAvailableFromUtc);
            Assert.Equal(expectedDescription, result.ContactTracingAvailableFromDescription);
        }

        [Fact]
        public void ClearsContactTracingAvailabilityWhenRequirementIsActionable()
        {
            // Effectivity is ignored, so we set it to an arbitrary date that would cause a failure if it was checked.
            var tripRequirement = CreateTripRequirement("PT72H", "2099-01-01 00:00:00Z");
            var trip = CreateTrip("2000-07-24T10:30:00Z");

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, null);

            Assert.Equal(ContactTracingStatus.TO_COMPLETE, result.ContactTracing);
            Assert.Null(result.ContactTracingAvailableFromUtc);
            Assert.Null(result.ContactTracingAvailableFromDescription);
        }

        [Fact]
        public void KeepsCompletedContactTracingCompleteWhenAcceptanceWindowIsInTheFuture()
        {
            var tripRequirement = CreateTripRequirement("PT72H", "2000-01-01 00:00:00Z");
            var trip = CreateTrip("2099-07-24T10:30:00Z");
            var contactTracing = CreateCompleteContactTracing();

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, contactTracing);

            Assert.Equal(ContactTracingStatus.COMPLETE, result.ContactTracing);
            Assert.Null(result.ContactTracingAvailableFromUtc);
            Assert.Null(result.ContactTracingAvailableFromDescription);
        }

        [Theory]
        [InlineData(DeltaTrm.CalculationFromCodeType.LAST_DOSE)]
        [InlineData(DeltaTrm.CalculationFromCodeType.LEG_ARRIVAL)]
        [InlineData(DeltaTrm.CalculationFromCodeType.TRIP_DESTINATION_ARRIVAL)]
        public void DoesNotReturnAvailabilityDescriptionWithoutFutureTimestamp(
            DeltaTrm.CalculationFromCodeType calculationFromType)
        {
            var tripRequirement = CreateTripRequirement("PT72H", "2000-01-01 00:00:00Z", calculationFromType);
            var trip = CreateTrip("2099-07-24T10:30:00Z");

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, null);

            Assert.Null(result.ContactTracingAvailableFromUtc);
            Assert.Null(result.ContactTracingAvailableFromDescription);
        }

        [Theory]
        [InlineData("P1DT5H6M7S", "Available 1 day 5 hours 6 minutes 7 seconds before departure")]
        [InlineData("PT1H", "Available 1 hour before departure")]
        public void FormatsAcceptanceWindowDuration(string acceptanceWindowFrom, string expectedText)
        {
            var tripRequirement = CreateTripRequirement(acceptanceWindowFrom, "2000-01-01 00:00:00Z");
            var trip = CreateTrip("2099-07-24T10:30:00Z");

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, null);

            Assert.Equal(expectedText, result.ContactTracingAvailableFromDescription);
        }

        [Fact]
        public void MarksDomesticSliceWithoutRequirementsAsCompleteAndPassportNotRequired()
        {
            var tripRequirement = new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "domestic-trip",
                RequirementCategories = []
            };
            var trip = new DeltaTrm.Trip
            {
                Id = "domestic-trip",
                Segments = [new DeltaTrm.DatedOperatingSegment { International = false }]
            };

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, null);

            Assert.False(result.PassportRequired);
            Assert.Equal(OverallCheckInStatus.COMPLETE, result.Overall);
        }

        [Fact]
        public void MarksInternationalSliceWithTravelDocumentsAsPassportRequired()
        {
            var tripRequirement = new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "international-trip",
                RequirementCategories =
                [
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS,
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.PASSPORT,
                                Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT }
                            }
                        ]
                    }
                ]
            };
            var trip = new DeltaTrm.Trip
            {
                Id = "international-trip",
                Segments = [new DeltaTrm.DatedOperatingSegment { International = true }]
            };

            var result = (CheckInRequirements)(new[] { tripRequirement }, new[] { trip }, null);

            Assert.True(result.PassportRequired);
            Assert.Equal(OverallCheckInStatus.TO_COMPLETE, result.Overall);
        }

        [Fact]
        public void MarksTravelRequirementsCompleteWhenAllRequiredItemsAreComplete()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement(true) },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);

            Assert.True(result.PassportProvided);
            Assert.Equal(ReturnDateStatus.PROVIDED, result.ReturnDateStatus);
            Assert.Equal(ContactTracingStatus.COMPLETE, result.ContactTracing);
            Assert.Equal(SecondaryDocumentStatus.COMPLETE, result.SecondaryDocumentStatus);
            Assert.All(result.GovernmentForms!, form => Assert.Equal(GovernmentFormsStatus.COMPLETED, form.Status));
            Assert.Equal(OverallCheckInStatus.COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenGovernmentFormsAreIncomplete()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement(false) },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);

            Assert.All(result.GovernmentForms!, form => Assert.Equal(GovernmentFormsStatus.REQUIRED, form.Status));
            Assert.Equal(OverallCheckInStatus.PART_COMPLETE, result.Overall);
        }

        [Fact]
        public void MarksTravelRequirementsCompleteWhenOnlyTravelRequirementErrorsRemain()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement() },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);
            result.TravelRequirementErrors =
            [
                new APIErrorResponse { Code = "travelrequirement_030_011" }
            ];

            Assert.Empty(result.PassportErrors);
            Assert.NotEmpty(result.TravelRequirementErrors);
            Assert.Equal(OverallCheckInStatus.COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenPassportIsUnhappy()
        {
            var tripRequirement = CreateCompleteTravelRequirement();
            var passportRequirement = tripRequirement.RequirementCategories
                .Single(category => category.Type == DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS)
                .Requirements
                .Single(requirement => requirement.RequirementType == DeltaTrm.RequirementType.PASSPORT);
            passportRequirement.Exceptions =
            [
                new DeltaTrm.Error
                {
                    Code = "travelrequirement_030_001",
                    Category = DeltaTrm.ErrorCategoryType.VALIDATE
                }
            ];

            var result = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);

            Assert.NotEmpty(result.PassportErrors);
            Assert.True(result.UnhappyPassport);
            Assert.Equal(OverallCheckInStatus.PART_COMPLETE, result.Overall);
        }

        [Fact]
        public void MarksTravelRequirementsCompleteWhenPassportErrorsExistButPassportIsNotUnhappy()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement() },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);
            result.PassportErrors =
            [
                new APIErrorResponse { Code = "travelrequirement_030_013" }
            ];

            Assert.NotEmpty(result.PassportErrors);
            Assert.False(result.UnhappyPassport);
            Assert.Equal(OverallCheckInStatus.COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenPassportIsIncomplete()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement(passportProvided: false) },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);

            Assert.False(result.PassportProvided);
            Assert.Equal(OverallCheckInStatus.TO_COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenReturnDateIsRequired()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement(returnDateProvided: false) },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);

            Assert.Equal(ReturnDateStatus.REQUIRED, result.ReturnDateStatus);
            Assert.Equal(OverallCheckInStatus.PART_COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenContactTracingIsIncomplete()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement() },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                null,
                null);

            Assert.Equal(ContactTracingStatus.TO_COMPLETE, result.ContactTracing);
            Assert.Equal(OverallCheckInStatus.PART_COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenSecondaryDocumentIsIncomplete()
        {
            var result = (CheckInRequirements)(
                new[] { CreateCompleteTravelRequirement(secondaryDocumentComplete: false) },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                CreateCompleteContactTracing(),
                null);

            Assert.Equal(SecondaryDocumentStatus.REQUIRED, result.SecondaryDocumentStatus);
            Assert.Equal(OverallCheckInStatus.PART_COMPLETE, result.Overall);
        }

        [Fact]
        public void DoesNotMarkTravelRequirementsCompleteWhenSecondaryDocumentIsRequiredWithoutAPassportRequirement()
        {
            var tripRequirement = new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "trip-id",
                RequirementCategories =
                [
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS,
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                                Status = new DeltaTrm.Status { Code = DeltaTrm.RequirementStatusCodeType.NOT_PRESENT },
                                RequirementOptions = [DeltaTrm.RequirementOption.ETA]
                            }
                        ]
                    }
                ]
            };

            var result = (CheckInRequirements)(
                new[] { tripRequirement },
                new[] { CreateTrip("2099-07-24T10:30:00Z") },
                null,
                null);

            Assert.False(result.PassportRequired);
            Assert.Equal(SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, result.SecondaryDocumentStatus);
            Assert.Equal(OverallCheckInStatus.TO_COMPLETE, result.Overall);
        }

        private static DeltaTrm.PassengerTripHealthRequirement CreateTripRequirement(
            string acceptanceWindowFrom,
            string effectiveUtcDate,
            DeltaTrm.CalculationFromCodeType calculationFromType = DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE)
        {
            return new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "trip-id",
                RequirementCategories =
                [
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS,
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.CONTACT_TRACING,
                                AcceptanceWindow = new DeltaTrm.TimeWindow
                                {
                                    CalculationFromType = calculationFromType,
                                    From = acceptanceWindowFrom
                                },
                                Effectivity = new DeltaTrm.TimeWindow
                                {
                                    EffectiveUTCDate = effectiveUtcDate
                                }
                            }
                        ]
                    }
                ]
            };
        }

        private static DeltaTrm.PassengerTripHealthRequirement CreateCompleteTravelRequirement(
            bool governmentFormsComplete = true,
            bool passportProvided = true,
            bool returnDateProvided = true,
            bool secondaryDocumentComplete = true)
        {
            return new DeltaTrm.PassengerTripHealthRequirement
            {
                TripId = "trip-id",
                RequirementCategories =
                [
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS,
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.PASSPORT,
                                Status = new DeltaTrm.Status
                                {
                                    Code = passportProvided
                                        ? DeltaTrm.RequirementStatusCodeType.PRESENT
                                        : DeltaTrm.RequirementStatusCodeType.NOT_PRESENT
                                },
                                ReturnDate = returnDateProvided ? "2099-07-30" : null,
                                ReturnDateRequired = !returnDateProvided
                            },
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.SECONDARY_DOC,
                                Status = new DeltaTrm.Status
                                {
                                    Code = secondaryDocumentComplete
                                        ? DeltaTrm.RequirementStatusCodeType.PRESENT
                                        : DeltaTrm.RequirementStatusCodeType.NOT_PRESENT
                                },
                                RequirementOptions = [DeltaTrm.RequirementOption.ETA]
                            }
                        ]
                    },
                    new DeltaTrm.RequirementCategory
                    {
                        Type = DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS,
                        Status = governmentFormsComplete
                            ? new DeltaTrm.DerivedStatus { Code = DeltaTrm.DerivedStatusCodeType.COMPLETE }
                            : new DeltaTrm.DerivedStatus { Code = DeltaTrm.DerivedStatusCodeType.NOT_COMPLETE },
                        Requirements =
                        [
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.GOVERNMENT_HEALTH_FORM,
                                Links =
                                [
                                    new DeltaTrm.Link
                                    {
                                        LinkId = "government-form",
                                        Text = "Government form",
                                        Href = "https://government.example/form"
                                    }
                                ]
                            },
                            new DeltaTrm.Requirement
                            {
                                RequirementType = DeltaTrm.RequirementType.CONTACT_TRACING
                            }
                        ]
                    }
                ]
            };
        }

        private static DeltaTrm.Trip CreateTrip(string scheduledDepartureUtcDateTime)
        {
            return new DeltaTrm.Trip
            {
                Id = "trip-id",
                Segments =
                [
                    new DeltaTrm.DatedOperatingSegment
                    {
                        Ordinal = 1,
                        Legs =
                        [
                            new DeltaTrm.DatedOperatingLeg
                            {
                                Ordinal = 1,
                                Departure = new DeltaTrm.TransportDeparture
                                {
                                    ScheduledDepartureUTCDateTime = scheduledDepartureUtcDateTime
                                }
                            }
                        ]
                    }
                ]
            };
        }

        private static DeltaTrm.ContactInformation CreateCompleteContactTracing()
        {
            return new DeltaTrm.ContactInformation
            {
                Address = new DeltaTrm.AddressInfo
                {
                    StreetAddress = "123 Test Street",
                    CountryCode = "US",
                    StateCode = "NY",
                    CityCode = "New York",
                    ZipCode = "10001"
                },
                Phone =
                [
                    new DeltaTrm.PhoneInfo
                    {
                        Preference = DeltaTrm.PhoneInfoPreference.PRIMARY,
                        PhoneNumber = "12025550123"
                    }
                ]
            };
        }
    }
}
