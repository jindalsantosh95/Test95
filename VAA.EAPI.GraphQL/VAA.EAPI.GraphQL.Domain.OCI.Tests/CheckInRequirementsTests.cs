using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using CheckInRequirementsModel = VAA.EAPI.GraphQL.Domain.PaxOps.Models.CheckInRequirements;
using CheckInRequirementsServiceModel = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS.CheckInRequirements;
using GovernmentFormServiceModel = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS.GovernmentForm;
using PassengerServiceModel = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS.Passenger;
using SecondaryDocumentApplicationServiceModel = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS.SecondaryDocumentApplication;
using SecondaryDocumentRequirementServiceModel = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS.SecondaryDocumentRequirement;
using TravelRequirementsServiceResponse = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse;
using ApplicationType = VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums.ApplicationType;
using CheckInErrorModel = VAA.EAPI.GraphQL.Domain.PaxOps.Models.CheckInError;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class CheckInRequirementsTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapsPassportRequiredFromMicroserviceResponse(bool passportRequired)
        {
            var source = new CheckInRequirementsServiceModel
            {
                PassportRequired = passportRequired
            };

            var result = (CheckInRequirementsModel)source;

            Assert.Equal(passportRequired, result.PassportRequired);
        }

        [Fact]
        public void MapsContactTracingAvailabilityFromMicroserviceResponse()
        {
            var availableFromUtc = new DateTime(2026, 7, 21, 10, 30, 0, DateTimeKind.Utc);
            var source = new CheckInRequirementsServiceModel
            {
                ContactTracing = ContactTracingStatus.NEEDED_LATER,
                ContactTracingAvailableFromUtc = availableFromUtc,
                ContactTracingAvailableFromDescription = "Available 72 hours before departure"
            };

            var result = (CheckInRequirementsModel)source;

            Assert.Equal(ContactTracingStatus.NEEDED_LATER, result.ContactTracing);
            Assert.Equal(availableFromUtc, result.ContactTracingAvailableFromUtc);
            Assert.Equal("Available 72 hours before departure", result.ContactTracingAvailableFromDescription);
        }

        [Fact]
        public void MapsActionableContactTracingWithoutAvailabilityTimestamp()
        {
            var source = new CheckInRequirementsServiceModel
            {
                ContactTracing = ContactTracingStatus.TO_COMPLETE,
                ContactTracingAvailableFromUtc = null
            };

            var result = (CheckInRequirementsModel)source;

            Assert.Equal(ContactTracingStatus.TO_COMPLETE, result.ContactTracing);
            Assert.Null(result.ContactTracingAvailableFromUtc);
        }

        [Fact]
        public void MapsTravelRequirementErrorsSeparatelyFromPassportErrors()
        {
            var source = new CheckInRequirementsServiceModel
            {
                PassportErrors = [],
                TravelRequirementErrors =
                [
                    new CheckInErrorModel
                    {
                        Code = "travelrequirement_030_011",
                        Title = "Something's not quite right",
                        Message = "Your destination needs a few more details about your travel plans. Please check in at the airport and show a ticket to leave your destination or other documents."
                    }
                ]
            };

            var result = (CheckInRequirementsModel)source;
            var error = Assert.Single(result.TravelRequirementErrors);

            Assert.Empty(result.PassportErrors);
            Assert.Equal("travelrequirement_030_011", error.Code);
            Assert.Equal("Something's not quite right", error.Title);
            Assert.Equal("Your destination needs a few more details about your travel plans. Please check in at the airport and show a ticket to leave your destination or other documents.", error.Message);
        }

        [Fact]
        public void MapsGovernmentFormAvailabilityDescriptionFromMicroserviceResponse()
        {
            var availableFromUtc = new DateTime(2099, 7, 21, 10, 30, 0, DateTimeKind.Utc);
            var source = new CheckInRequirementsServiceModel
            {
                GovernmentForms =
                [
                    new GovernmentFormServiceModel
                    {
                        Status = GovernmentFormsStatus.REQUIRED,
                        AvailableFromUtc = availableFromUtc,
                        AvailableFromDescription = "Available 72 hours before departure",
                        IsAvailable = false
                    }
                ]
            };

            var result = (CheckInRequirementsModel)source;
            var form = Assert.Single(result.GovernmentForms!);

            Assert.Equal(GovernmentFormsStatus.REQUIRED, form.Status);
            Assert.Equal(availableFromUtc, form.AvailableFromUtc);
            Assert.Equal("Available 72 hours before departure", form.AvailableFromDescription);
            Assert.False(form.IsAvailable);
        }

        [Fact]
        public void MapsSecondaryDocumentApplicationsFromMicroserviceResponse()
        {
            var source = new CheckInRequirementsServiceModel
            {
                SecondaryDocuments =
                [
                    new SecondaryDocumentRequirementServiceModel
                    {
                        Name = "Electronic visa",
                        SecondaryDocumentApplicationOptions =
                        [
                            new SecondaryDocumentApplicationServiceModel
                            {
                                SecondaryDocumentId = "visa-application",
                                ApplicationType = ApplicationType.Vendor,
                                Title = "Apply with Sherpa",
                                Message = "Sherpa guidance",
                                Url = "https://apply.joinsherpa.com/travel-restrictions/USA"
                            }
                        ]
                    }
                ]
            };

            var result = (CheckInRequirementsModel)source;
            var document = Assert.Single(result.SecondaryDocuments);
            Assert.Equal("Electronic visa", document.Name);

            var application = Assert.Single(document.SecondaryDocumentApplicationOptions);
            Assert.Equal("visa-application", application.SecondaryDocumentId);
            Assert.Equal(ApplicationType.Vendor, application.ApplicationType);
            Assert.Equal("Apply with Sherpa", application.Title);
            Assert.Equal("Sherpa guidance", application.Message);
            Assert.Equal("https://apply.joinsherpa.com/travel-restrictions/USA", application.Url);
        }

        [Fact]
        public void IncludesTimeGatedContactTracingStateInTravelRequirementsTelemetry()
        {
            var availableFromUtc = new DateTime(2099, 7, 21, 10, 30, 0, DateTimeKind.Utc);
            var response = new TravelRequirementsServiceResponse
            {
                Passengers =
                [
                    new PassengerServiceModel
                    {
                        PassengerId = "passenger-id",
                        SliceRequirements =
                        [
                            new CheckInRequirementsServiceModel
                            {
                                SliceId = "slice-id",
                                ContactTracing = ContactTracingStatus.NEEDED_LATER,
                                ContactTracingAvailableFromUtc = availableFromUtc,
                                ContactTracingAvailableFromDescription = "Available 72 hours before departure"
                            }
                        ]
                    }
                ],
                Slices = []
            };

            var telemetry = TelemetryHelper.GetTravelRequirementsSessionCreated(response);
            var passengers = JArray.Parse(Assert.IsType<string>(telemetry["PassengerSliceRequirements"]));
            var sliceRequirementsJson = passengers.Single()!["SliceRequirements"]?.Value<string>();
            var sliceRequirements = JArray.Parse(Assert.IsType<string>(sliceRequirementsJson));
            var requirement = sliceRequirements.Single();

            Assert.Equal("NEEDED_LATER", requirement!["ContactTracingStatus"]?.Value<string>());
            Assert.Equal(availableFromUtc, requirement["ContactTracingAvailableFromUtc"]?.Value<DateTime>().ToUniversalTime());
            Assert.Equal("Available 72 hours before departure", requirement["ContactTracingAvailableFromDescription"]?.Value<string>());
        }
    }
}
