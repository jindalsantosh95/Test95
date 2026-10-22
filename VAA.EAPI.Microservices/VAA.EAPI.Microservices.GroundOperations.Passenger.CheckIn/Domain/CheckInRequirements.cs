using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using NodaTime.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class CheckInRequirements
    {
        public string? SliceId { get; set; } 
        public bool PassportRequired { get; set; }
        public bool? PassportProvided { get; set; }
        public bool? PassportVerified { get; set; }
        // Emergency contact separate from passport?
        //public string EmergencyContact { get; set; }
        public bool BaggageDeclared { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ContactTracingStatus ContactTracing { get; set; }
        public DateTime? ContactTracingAvailableFromUtc { get; set; }
        public string? ContactTracingAvailableFromDescription { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReturnDateStatus ReturnDateStatus { get; set; }
        public bool IsReturnDateDeclined { get; set; }
        public LocalDate? ReturnDate { get; set; }
        public bool CheckedIn { get; set; }
        public bool CheckinEligible { get; set; }
        public bool UnhappyPassport { get; set; }
        public IEnumerable<APIErrorResponse> PassportErrors { get; set; } = [];
        public IEnumerable<APIErrorResponse> TravelRequirementErrors { get; set; } = [];
        [JsonConverter(typeof(StringEnumConverter))]
        public SecondaryDocumentStatus SecondaryDocumentStatus { get; set; }
        public bool SecondaryDocumentStatusForETA { get; set; }
        public SecondaryDocumentRequirement[] SecondaryDocuments { get; set; } = [];
        public IEnumerable<GovernmentForm>? GovernmentForms { get; set; }

        [JsonIgnore]
        private bool IsTravelRequirementsComplete { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OverallCheckInStatus Overall
        {
            get
            {
                if (IsTravelRequirementsComplete)
                    return OverallCheckInStatus.COMPLETE;

                if (CheckedIn)
                    return OverallCheckInStatus.COMPLETE;

                if (PassportProvided == true)
                    return OverallCheckInStatus.PART_COMPLETE;

                return OverallCheckInStatus.TO_COMPLETE;
            }
        }

        public static explicit operator CheckInRequirements((IEnumerable<DeltaTrm.PassengerTripHealthRequirement>? trmPassengerTripHealthRequirements, DeltaTrm.ContactInformation? trmContactTracing) source) => (CheckInRequirements)(null, source.trmPassengerTripHealthRequirements, null, source.trmContactTracing, null, null, null, null);

        public static explicit operator CheckInRequirements((IEnumerable<DeltaTrm.PassengerTripHealthRequirement>? trmPassengerTripHealthRequirements, IEnumerable<DeltaTrm.Trip>? trmTrips, DeltaTrm.ContactInformation? trmContactTracing) source) => (CheckInRequirements)(null, source.trmPassengerTripHealthRequirements, source.trmTrips, source.trmContactTracing, null, null, null, null);

        public static explicit operator CheckInRequirements((IEnumerable<DeltaTrm.PassengerTripHealthRequirement>? trmPassengerTripHealthRequirements, IEnumerable<DeltaTrm.Trip>? trmTrips, DeltaTrm.ContactInformation? trmContactTracing, DeltaTrm.Country? nationality) source) => (CheckInRequirements)(null, source.trmPassengerTripHealthRequirements, source.trmTrips, source.trmContactTracing, null, null, null, source.nationality);

        public static explicit operator CheckInRequirements((DeltaSsci.Passenger1? ssciPassenger, IEnumerable<DeltaTrm.PassengerTripHealthRequirement>? trmPassengerTripHealthRequirements, IEnumerable<DeltaTrm.Trip>? trmTrips, DeltaTrm.ContactInformation? trmContactTracing,
            DeltaSsci.AddPassportResponse? ssciAddPassport, PassengerId? selectedPassengerId, bool? isInboundToUs, DeltaTrm.Country? nationality) source)
        {
            var trmTravelDocuments = source.trmPassengerTripHealthRequirements
                    ?.SelectMany(r => r.RequirementCategories).Where(rc => rc.Type == DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS);

            IEnumerable<DeltaTrm.Requirement> trmPassport = trmTravelDocuments?.SelectMany(t => t.Requirements).Where(r => r.RequirementType == DeltaTrm.RequirementType.PASSPORT) ?? Enumerable.Empty<DeltaTrm.Requirement>();
            var passportRequired = trmPassport.Any();
            var tripRequirements = source.trmPassengerTripHealthRequirements?.ToArray() ?? [];

            var exceptions = new List<APIErrorResponse>();

            var passportExceptions = trmTravelDocuments?.SelectMany(t => t.Requirements).Where(x => x.RequirementType == DeltaTrm.RequirementType.PASSPORT && x.Exceptions != null)?.SelectMany(x => x.Exceptions).ToList();

            if (passportExceptions?.Count > 0)
            {
                exceptions.AddRange(passportExceptions.Select(e => new APIErrorResponse
                {
                    Code = e?.Code,
                    Message = CheckInMessageHelper.GetTrmErrorMessage(e?.Code)?.Item2,
                    Title = CheckInMessageHelper.GetTrmErrorMessage(e?.Code)?.Item1,
                    Category = e.Category.ToString(),
                    DisplayCode = e?.DisplayCode
                }));
            };

            var errorExceptions = source.trmPassengerTripHealthRequirements?.Where(e => e.Exceptions != null).SelectMany(x => x.Exceptions).Where(ex => ex.Category == DeltaTrm.ErrorCategoryType.ERROR).ToList();
            if (errorExceptions?.Count > 0)
            {
                exceptions.AddRange(errorExceptions.Select(ex => new APIErrorResponse
                {
                    Code = ex?.Code,
                    Message = CheckInMessageHelper.GetTrmErrorMessage(ex?.Code)?.Item2,
                    Title = CheckInMessageHelper.GetTrmErrorMessage(ex?.Code)?.Item1,
                    Category = ex.Category.ToString(),
                    DisplayCode = ex?.DisplayCode
                }));
            }

            var travelRequirementExceptions = exceptions
                .Where(exception => Constants.Application.TravelRequirementExceptionErrors.Contains(exception.Code))
                .ToList();

            var requirements = new CheckInRequirements()
            {
                SliceId = string.Join("|", source.trmPassengerTripHealthRequirements?.Select(t => t?.TripId) ?? []),
                PassportRequired = passportRequired,
                PassportProvided = trmPassport.Any() &&
                    (trmPassport.All(p => p.Status.Code == DeltaTrm.RequirementStatusCodeType.PRESENT) ||
                     travelRequirementExceptions.Any()),
                PassportVerified = trmPassport.Any() && trmPassport.All(p => p.Verified),
                // CheckedIn means what is says. CheckedInOnASL means you are checked in but don't have a seat assigned yet.
                // In that scenario Delta issue a "Seat Request Doc" which looks just like a boarding pass. We issue a boarding pass.
                CheckedIn = source.ssciPassenger != null && (source.ssciPassenger.CheckedIn || source.ssciPassenger.CheckedInOnASL),
                BaggageDeclared = source.ssciPassenger != null && source.ssciPassenger.Bags?.Pending > 0 == true,
                CheckinEligible = source.trmPassengerTripHealthRequirements?.All(t => t.CheckinEligible) == true,
                UnhappyPassport = Constants.Application.PassportValidationErrors.Any(e => passportExceptions?.Any(ex => ex.Code == e) == true),
                PassportErrors = exceptions.Where(exception => !Constants.Application.TravelRequirementExceptionErrors.Contains(exception.Code)).ToList(),
                TravelRequirementErrors = travelRequirementExceptions
            };

            var secondaryDocumentRequirements = source.trmPassengerTripHealthRequirements?
                .SelectMany(tripRequirement => (tripRequirement.RequirementCategories ?? [])
                    .Where(category => category.Type == DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS)
                    .SelectMany(category => category.Requirements ?? [])
                    .Where(requirement => requirement.RequirementType == DeltaTrm.RequirementType.SECONDARY_DOC)
                    .Where(requirement => requirement.RequirementOptions?.Contains(DeltaTrm.RequirementOption.VISA_ON_ARRIVAL) != true)
                    .Select(requirement => new
                    {
                        Requirement = requirement,
                        Trip = source.trmTrips?.FirstOrDefault(trip => trip.Id == tripRequirement.TripId)
                    }))
                .ToArray() ?? [];

            if (secondaryDocumentRequirements.Length > 0)
            {
                var sherpaAffiliateId = ConfigReader.Get(Constants.Environment.SherpaAffiliateId);

                requirements.SecondaryDocuments = secondaryDocumentRequirements
                    .Select((item, index) => SecondaryDocumentRequirement.FromTrm(
                        item.Requirement,
                        item.Trip,
                        source.nationality,
                        sherpaAffiliateId,
                        index))
                    .ToArray();
            }

            var governmentFormsCategories = source.trmPassengerTripHealthRequirements?
                .SelectMany(tripRequirement => tripRequirement.RequirementCategories
                    .Where(category => category.Type == DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS)
                    .Select(category => new { TripRequirement = tripRequirement, Category = category }))
                .ToArray() ?? [];

            if (governmentFormsCategories.Length > 0)
            {
                var governmentForms = governmentFormsCategories
                    .SelectMany(item => item.Category.Requirements?.Select(requirement => new
                    {
                        Requirement = requirement,
                        Trip = source.trmTrips?.FirstOrDefault(trip => trip.Id == item.TripRequirement.TripId),
                        Status = item.Category.Status?.Code == DeltaTrm.DerivedStatusCodeType.COMPLETE
                            ? GovernmentFormsStatus.COMPLETED
                            : GovernmentFormsStatus.REQUIRED
                    }) ?? [])
                    .SelectMany(item =>
                    {
                        var availableFromUtc = CalculateAvailableFromUtc(item.Requirement.AcceptanceWindow, item.Trip);
                        var isAvailable = !availableFromUtc.HasValue || availableFromUtc.Value <= DateTime.UtcNow;
                        return (item.Requirement.Links ?? [])
                            .Where(link => !link.Href.IsNullOrWhiteSpace() && !link.Text.IsNullOrWhiteSpace())
                            .Select(link => new GovernmentForm
                            {
                                Status = item.Status,
                                Id = link.Id,
                                LinkId = link.LinkId,
                                Title = link.Text,
                                Url = link.Href,
                                AvailableFromUtc = isAvailable ? null : availableFromUtc,
                                AvailableFromDescription = isAvailable
                                    ? null
                                    : CalculateAvailableFromDescription(item.Requirement.AcceptanceWindow),
                                IsAvailable = isAvailable
                            });
                    })
                    .ToArray();

                if (governmentForms.Length > 0)
                    requirements.GovernmentForms = governmentForms;
            }

            // Contact Tracing from TRM
            var trmContactAddress = source.trmContactTracing?.Address;
            var contactTracingRequirements = source.trmPassengerTripHealthRequirements?
                .SelectMany(pthr => pthr.RequirementCategories
                    .Where(rc => rc.Type == DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS)
                    .SelectMany(rc => rc.Requirements)
                    .Where(requirement => requirement.RequirementType == DeltaTrm.RequirementType.CONTACT_TRACING)
                    .Select(requirement => (pthr.TripId, Requirement: requirement)))
                .ToList() ?? [];

            var contactTracingAvailability = contactTracingRequirements
                .Select(contactTracingRequirement => new
                {
                    AvailableFromUtc = CalculateAvailableFromUtc(
                        contactTracingRequirement.Requirement.AcceptanceWindow,
                        source.trmTrips?.SingleOrDefault(trip => trip.Id == contactTracingRequirement.TripId)),
                    AvailableFromDescription = CalculateAvailableFromDescription(contactTracingRequirement.Requirement.AcceptanceWindow)
                })
                .ToList();

            var futureContactTracingAvailability = contactTracingAvailability
                .Where(availability => availability.AvailableFromUtc.HasValue && availability.AvailableFromUtc.Value > DateTime.UtcNow)
                .OrderBy(availability => availability.AvailableFromUtc)
                .FirstOrDefault();

            if (!contactTracingRequirements.Any())
                requirements.ContactTracing = ContactTracingStatus.NOT_REQUIRED;
            else if (source.trmContactTracing?.Phone?.FirstOrDefault(p => p.Preference == DeltaTrm.PhoneInfoPreference.PRIMARY)?.PhoneNumber.IsNullOrEmpty() == false &&
                trmContactAddress != null && trmContactAddress.StreetAddress.IsNullOrEmpty() == false && trmContactAddress.CountryCode.IsNullOrEmpty() == false &&
                trmContactAddress.StateCode.IsNullOrEmpty() == false && trmContactAddress.CityCode.IsNullOrEmpty() == false && trmContactAddress.ZipCode.IsNullOrEmpty() == false)
                requirements.ContactTracing = ContactTracingStatus.COMPLETE;
            else if (futureContactTracingAvailability != null)
            {
                requirements.ContactTracing = ContactTracingStatus.NEEDED_LATER;
                requirements.ContactTracingAvailableFromUtc = futureContactTracingAvailability.AvailableFromUtc;
                requirements.ContactTracingAvailableFromDescription = futureContactTracingAvailability.AvailableFromDescription;
            }
            else 
                requirements.ContactTracing = ContactTracingStatus.TO_COMPLETE;

            // Backup choice: Contact Tracing from SSCI
            /* Logic to map from SSCI response. Requires an extra call to GetTravelPartiesAsync in CheckInService, but Delta may say this is the source we must use.
            if (source.ssciPassenger.ContactTracingInfo?.AddressRequirement.Required == false && source.ssciPassenger.ContactTracingInfo?.EmailRequirement.Required == false
                && source.ssciPassenger.ContactTracingInfo?.PrimaryPhoneRequirement.Required == false && source.ssciPassenger.ContactTracingInfo?.SecondaryPhoneRequirement.Required == false)
                requirements.ContactTracing = ContactTracingStatus.NOT_REQUIRED;
            else if (source.ssciPassenger.ContactTracingInfo?.ContactAddressRequiredAndEmpty == false && source.ssciPassenger.ContactTracingInfo?.EmailRequiredAndEmpty == false
                && source.ssciPassenger.ContactTracingInfo?.PrimaryPhoneRequiredAndEmpty == false && source.ssciPassenger.ContactTracingInfo?.SecondaryPhoneRequiredAndEmpty == false)
                requirements.ContactTracing = ContactTracingStatus.COMPLETE;
            else
                requirements.ContactTracing = ContactTracingStatus.TO_COMPLETE;*/

            // Return Date
            if (trmPassport.All(p => p.Status.Code == DeltaTrm.RequirementStatusCodeType.NOT_PRESENT))
                requirements.ReturnDateStatus = ReturnDateStatus.PROVIDE_PASSPORT_FIRST;
            else if (trmPassport.All(p => p.ReturnDate.IsNullOrEmpty() == false))
            {
                requirements.ReturnDateStatus = ReturnDateStatus.PROVIDED;
                requirements.ReturnDate = DateTime.TryParse(trmPassport.Last().ReturnDate, out var parsedDate) ? parsedDate.ToLocalDateTime().Date : null;
            }
            else if (trmPassport.Any(p => p.ReturnDateRequired == true) || source.ssciAddPassport?.ShowOnwardDeptDate == true)
                requirements.ReturnDateStatus = ReturnDateStatus.REQUIRED;
            else if (trmPassport.All(p => p.ReturnDateRequired == false))
                requirements.ReturnDateStatus = ReturnDateStatus.NOT_REQUIRED;
            else
                requirements.ReturnDateStatus = ReturnDateStatus.UNKNOWN;
            requirements.IsReturnDateDeclined = trmPassport.All(p => p.ReturnDateDeclined == true);

            if (requirements.PassportProvided != true)
                requirements.SecondaryDocumentStatus = SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST;
            else if (requirements.SecondaryDocuments?.Any(document => document.Status == SecondaryDocumentStatus.UNKNOWN) == true)
                requirements.SecondaryDocumentStatus = SecondaryDocumentStatus.UNKNOWN;
            else if (requirements.SecondaryDocuments?.Any() == true)
                requirements.SecondaryDocumentStatus = requirements.SecondaryDocuments.All(document => document.Status == SecondaryDocumentStatus.COMPLETE)
                    ? SecondaryDocumentStatus.COMPLETE
                    : SecondaryDocumentStatus.REQUIRED;
            else
                requirements.SecondaryDocumentStatus = SecondaryDocumentStatus.NOT_REQUIRED;

            requirements.SecondaryDocumentStatusForETA = secondaryDocumentRequirements.Any(item =>
                item.Requirement.RequirementOptions?.Count > 0 &&
                item.Requirement.RequirementOptions.First() == DeltaTrm.RequirementOption.ETA &&
                item.Requirement.Country?.IsoAlpha3Code == "USA");

            // No ssciPassenger means we're not in check in flow. We return secondary documentation status from TRM, because this is the most accurate data we have outside of the checkin window.
            if (source.ssciPassenger == null && requirements.SecondaryDocumentStatus == SecondaryDocumentStatus.REQUIRED && requirements.ReturnDateStatus != ReturnDateStatus.REQUIRED)
            {
                var errorCode = requirements.SecondaryDocumentStatusForETA ? "SecondaryDocETARequirement" : "SecondaryDocNonETARequirement";

                var errorDetails = CheckInMessageHelper.GetNoReturnDateandSecondaryDocErrorMessages(errorCode).FirstOrDefault();

                requirements.PassportErrors = new List<APIErrorResponse>
                {
                    (APIErrorResponse)errorDetails
                };
            }
            // If we are in the check in flow, then ignore secondary documentation status from TRM, because SSCI has passenger-specific details from government integrations, rather than just generic nationality-based rules from Timatic.
            else if (source.ssciAddPassport?.ShowSecondaryDocument == true && source.selectedPassengerId != null && 
                source.ssciPassenger.FirstNIN == source.selectedPassengerId.FirstNin && source.ssciPassenger.LastNIN == source.selectedPassengerId.LastNin)
            {
                var errorCode = source.isInboundToUs == true ? "ShowSecondaryDocIsInboundUs" : "ShowSecondaryDocIsNotInboundUs";
                if (source.isInboundToUs == true) requirements.SecondaryDocumentStatusForETA = true;

                var errorDetails = CheckInMessageHelper.GetStatusMessage(errorCode);

                requirements.PassportErrors = new List<APIErrorResponse>
                {
                    new APIErrorResponse {
                        Code = errorCode,
                        Title = errorDetails.Item1,
                        Message = errorDetails.Item2,
                    }
                };
            }

            requirements.IsTravelRequirementsComplete = source.ssciPassenger == null &&
                AreTravelRequirementsComplete(requirements, tripRequirements);

            return requirements;
        }

        private static bool AreTravelRequirementsComplete(
            CheckInRequirements requirements,
            IEnumerable<DeltaTrm.PassengerTripHealthRequirement> tripRequirements)
        {
            var travelDocumentRequirements = tripRequirements
                .SelectMany(requirement => requirement.RequirementCategories ?? [])
                .Where(category => category.Type == DeltaTrm.RequirementCategoryCodeType.TRAVEL_DOCUMENTS)
                .SelectMany(category => category.Requirements ?? [])
                .ToArray();
            var governmentFormsComplete = tripRequirements
                .SelectMany(requirement => requirement.RequirementCategories ?? [])
                .Where(category =>
                    category.Type == DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS &&
                    category.Requirements?.Any() == true)
                .All(category => category.Status?.Code == DeltaTrm.DerivedStatusCodeType.COMPLETE);
            var returnDateRequired = travelDocumentRequirements.Any(requirement =>
                requirement.RequirementType == DeltaTrm.RequirementType.PASSPORT &&
                requirement.ReturnDateRequired == true);
            var secondaryDocumentsRequired = travelDocumentRequirements.Any(requirement =>
                requirement.RequirementType == DeltaTrm.RequirementType.SECONDARY_DOC &&
                requirement.RequirementOptions?.Contains(DeltaTrm.RequirementOption.VISA_ON_ARRIVAL) != true);
            var passportComplete = !requirements.PassportRequired ||
                (requirements.PassportProvided == true && !requirements.UnhappyPassport);
            var returnDateComplete = !returnDateRequired ||
                requirements.ReturnDateStatus is ReturnDateStatus.PROVIDED or ReturnDateStatus.NOT_REQUIRED;
            var contactTracingComplete = requirements.ContactTracing is
                ContactTracingStatus.COMPLETE or ContactTracingStatus.NOT_REQUIRED;
            var secondaryDocumentsComplete = !secondaryDocumentsRequired ||
                requirements.SecondaryDocumentStatus is
                    SecondaryDocumentStatus.COMPLETE or SecondaryDocumentStatus.NOT_REQUIRED;

            return passportComplete &&
                returnDateComplete &&
                contactTracingComplete &&
                secondaryDocumentsComplete &&
                governmentFormsComplete;
        }

        private static DateTime? ParseUtcDate(string? value)
        {
            return DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedDate)
                    ? parsedDate.UtcDateTime
                    : null;
        }

        private static DateTime? CalculateAvailableFromUtc(DeltaTrm.TimeWindow? acceptanceWindow, DeltaTrm.Trip? trip)
        {
            if (acceptanceWindow?.From.IsNullOrEmpty() != false)
                return null;

            var segments = trip?.Segments?.OrderBy(segment => segment.Ordinal).ToArray();
            var anchorUtc = acceptanceWindow.CalculationFromType switch
            {
                DeltaTrm.CalculationFromCodeType.LEG_DEPARTURE or
                DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE => ParseUtcDate(segments?
                    .FirstOrDefault()
                    ?.Legs
                    ?.OrderBy(leg => leg.Ordinal)
                    .FirstOrDefault()
                    ?.Departure
                    ?.ScheduledDepartureUTCDateTime),
                DeltaTrm.CalculationFromCodeType.LEG_ARRIVAL or
                DeltaTrm.CalculationFromCodeType.TRIP_DESTINATION_ARRIVAL => ParseUtcDate(segments?
                    .LastOrDefault()
                    ?.Legs
                    ?.OrderBy(leg => leg.Ordinal)
                    .LastOrDefault()
                    ?.Arrival
                    ?.ScheduledArrivalUTCDateTime),
                _ => null
            };

            if (!anchorUtc.HasValue)
                return null;

            try
            {
                return anchorUtc.Value.Subtract(XmlConvert.ToTimeSpan(acceptanceWindow.From));
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        private static string? CalculateAvailableFromDescription(DeltaTrm.TimeWindow? acceptanceWindow)
        {
            var duration = FormatIsoDuration(acceptanceWindow?.From);
            if (duration == null || acceptanceWindow == null)
                return null;

            return acceptanceWindow.CalculationFromType switch
            {
                DeltaTrm.CalculationFromCodeType.LAST_DOSE => $"Available {duration} after last dose",
                DeltaTrm.CalculationFromCodeType.LEG_ARRIVAL => $"Available {duration} before leg arrival",
                DeltaTrm.CalculationFromCodeType.LEG_DEPARTURE => $"Available {duration} before leg departure",
                DeltaTrm.CalculationFromCodeType.TRIP_DESTINATION_ARRIVAL => $"Available {duration} before arrival",
                DeltaTrm.CalculationFromCodeType.TRIP_ORIGIN_DEPARTURE => $"Available {duration} before departure",
                _ => null
            };
        }

        private static string? FormatIsoDuration(string? value)
        {
            if (value.IsNullOrEmpty())
                return null;

            var match = Regex.Match(
                value!,
                @"^P(?:(?<years>\d+(?:\.\d+)?)Y)?(?:(?<months>\d+(?:\.\d+)?)M)?(?:(?<weeks>\d+(?:\.\d+)?)W)?(?:(?<days>\d+(?:\.\d+)?)D)?(?:T(?:(?<hours>\d+(?:\.\d+)?)H)?(?:(?<minutes>\d+(?:\.\d+)?)M)?(?:(?<seconds>\d+(?:\.\d+)?)S)?)?$",
                RegexOptions.CultureInvariant);

            if (!match.Success)
                return null;

            var components = new[]
            {
                FormatDurationComponent(match, "years", "year"),
                FormatDurationComponent(match, "months", "month"),
                FormatDurationComponent(match, "weeks", "week"),
                FormatDurationComponent(match, "days", "day"),
                FormatDurationComponent(match, "hours", "hour"),
                FormatDurationComponent(match, "minutes", "minute"),
                FormatDurationComponent(match, "seconds", "second")
            }.Where(component => component != null);

            var result = string.Join(" ", components);
            return result.IsNullOrEmpty() ? null : result;
        }

        private static string? FormatDurationComponent(Match match, string groupName, string unit)
        {
            var value = match.Groups[groupName].Value;
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                return null;

            return $"{number.ToString("0.##", CultureInfo.InvariantCulture)} {unit}{(number == 1 ? string.Empty : "s")}";
        }
    }
}
