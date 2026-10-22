using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class SecondaryDocumentRequirement
    {
        private const string CountryNamePlaceholder = "{countryName}";

        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentType DocumentType { get; set; }

        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SecondaryDocumentStatus Status { get; set; }

        public List<SecondaryDocumentApplication> SecondaryDocumentApplicationOptions { get; set; } = [];

        internal static SecondaryDocumentRequirement FromTrm(
            DeltaTrm.Requirement requirement,
            DeltaTrm.Trip? trip,
            DeltaTrm.Country? nationality,
            string sherpaAffiliateId,
            int requirementIndex)
        {
            var governmentLink = requirement.Links?.FirstOrDefault(link => link.Source.Is("government"));
            var nonGovernmentLink = requirement.Links?.FirstOrDefault(link => !ReferenceEquals(link, governmentLink));
            var documentType = MapDocumentType(requirement);
            var countryCode = GetCountryCode(requirement.Country);
            var name = GetName(requirement, governmentLink, documentType);
            var contentOverride = SecondaryDocumentContentOverrides.Find(requirement);
            var displayName = contentOverride?.DisplayName ?? $"{CountryNamePlaceholder} {name}";
            var isEmbassyVisa = requirement.RequirementOptions?.Count > 0 &&
                requirement.RequirementOptions.First() == DeltaTrm.RequirementOption.EMBASSY_VISA;
            var vendorId = GetSecondaryDocumentApplicationId(
                requirement,
                nonGovernmentLink,
                trip,
                documentType,
                countryCode,
                requirementIndex);
            var governmentId = GetSecondaryDocumentApplicationId(
                requirement,
                governmentLink,
                trip,
                documentType,
                countryCode,
                requirementIndex);

            if (nonGovernmentLink is null)
                vendorId = governmentId;
            else if (governmentLink is null)
                governmentId = vendorId;
            return new SecondaryDocumentRequirement
            {
                DocumentType = documentType,
                CountryCode = countryCode,
                CountryName = null,
                Name = name,
                DisplayName = displayName,
                Status = MapStatus(requirement.Status?.Code),
                SecondaryDocumentApplicationOptions = isEmbassyVisa
                    ?
                    [
                        SecondaryDocumentApplicationContent.Government(
                            governmentId,
                            displayName,
                            contentOverride,
                            isEmbassyVisa)
                    ]
                    :
                    [
                        SecondaryDocumentApplicationContent.Vendor(
                            vendorId,
                            BuildSherpaDeepLink(requirement, trip, nationality, sherpaAffiliateId)),
                        SecondaryDocumentApplicationContent.Government(
                            governmentId,
                            displayName,
                            contentOverride,
                            isEmbassyVisa)
                    ]
            };
        }

        private static DocumentType MapDocumentType(DeltaTrm.Requirement requirement)
        {
            if (requirement.Transit)
                return DocumentType.TRANSIT_VISA;

            if (requirement.RequirementOptions?.Contains(DeltaTrm.RequirementOption.ETA) == true)
                return DocumentType.ETA;

            return DocumentType.VISA;
        }

        private static SecondaryDocumentStatus MapStatus(DeltaTrm.RequirementStatusCodeType? status) => status switch
        {
            DeltaTrm.RequirementStatusCodeType.APPROVED or
            DeltaTrm.RequirementStatusCodeType.EXEMPTED or
            DeltaTrm.RequirementStatusCodeType.OVERRIDE or
            DeltaTrm.RequirementStatusCodeType.PRESENT or
            DeltaTrm.RequirementStatusCodeType.VERIFIED => SecondaryDocumentStatus.COMPLETE,
            DeltaTrm.RequirementStatusCodeType.ERROR => SecondaryDocumentStatus.UNKNOWN,
            null => SecondaryDocumentStatus.UNKNOWN,
            _ => SecondaryDocumentStatus.REQUIRED
        };

        private static string GetSecondaryDocumentApplicationId(
            DeltaTrm.Requirement requirement,
            DeltaTrm.Link? link,
            DeltaTrm.Trip? trip,
            DocumentType documentType,
            string? countryCode,
            int requirementIndex)
        {
            if (!link?.LinkId.IsNullOrWhiteSpace() == true)
                return link!.LinkId!;

            if (!requirement.Id.IsNullOrWhiteSpace())
                return requirement.Id;

            return string.Join(
                "|",
                new[] { trip?.Id, countryCode, documentType.ToString(), requirementIndex.ToString(CultureInfo.InvariantCulture) }
                    .Where(value => !value.IsNullOrWhiteSpace()));
        }

        private static string GetName(
            DeltaTrm.Requirement requirement,
            DeltaTrm.Link? governmentLink,
            DocumentType documentType)
        {
            var name = new[] { governmentLink?.Name }
                .Concat(requirement.Links?.Select(link => link.Name) ?? [])
                .FirstOrDefault(value => !value.IsNullOrWhiteSpace());

            return name ?? documentType switch
            {
                DocumentType.ETA => "ETA",
                DocumentType.TRANSIT_VISA => "Transit visa",
                _ => "Visa"
            };
        }

        private static string BuildSherpaDeepLink(
            DeltaTrm.Requirement requirement,
            DeltaTrm.Trip? trip,
            DeltaTrm.Country? nationality,
            string sherpaAffiliateId)
        {
            var destinationCountry = GetCountryCode(trip?.ArrivalStation?.Country) ?? GetCountryCode(requirement.Country);
            var path = destinationCountry.IsNullOrWhiteSpace()
                ? "https://apply.joinsherpa.com/travel-restrictions"
                : $"https://apply.joinsherpa.com/travel-restrictions/{Uri.EscapeDataString(destinationCountry)}";
            var parameters = new List<KeyValuePair<string, string?>>
            {
                new("affiliateId", sherpaAffiliateId),
                new("originCountry", GetCountryCode(trip?.DepartureStation?.Country)),
                new("originAirport", trip?.DepartureStation?.Code),
                new("destinationAirport", trip?.ArrivalStation?.Code),
                new("nationality", GetCountryCode(nationality)),
                new("departureDate", GetDate(trip?.FirstDepartureLocalDate)),
                new("tripType", "oneWay"),
                new("transitCountries", GetTransitCountries(trip)),
                new("transitAirports", GetTransitAirports(trip)),
                new("flightNumber", GetFlightNumbers(trip))
            };
            var query = string.Join(
                "&",
                parameters
                    .Where(parameter => !parameter.Value.IsNullOrWhiteSpace())
                    .Select(parameter => $"{parameter.Key}={Uri.EscapeDataString(parameter.Value!)}"));

            return $"{path}?{query}";
        }

        private static string? GetTransitCountries(DeltaTrm.Trip? trip) => JoinTransitValues(
            trip?.Segments?
                .OrderBy(segment => segment.Ordinal)
                .SelectMany(segment => segment.Legs?.OrderBy(leg => leg.Ordinal) ?? Enumerable.Empty<DeltaTrm.DatedOperatingLeg>())
                .SkipLast(1)
                .Select(leg => GetCountryCode(leg.Arrival?.Station?.Country)));

        private static string? GetTransitAirports(DeltaTrm.Trip? trip) => JoinTransitValues(
            trip?.Segments?
                .OrderBy(segment => segment.Ordinal)
                .SelectMany(segment => segment.Legs?.OrderBy(leg => leg.Ordinal) ?? Enumerable.Empty<DeltaTrm.DatedOperatingLeg>())
                .SkipLast(1)
                .Select(leg => leg.Arrival?.Station?.Code));

        private static string? GetFlightNumbers(DeltaTrm.Trip? trip) => JoinTransitValues(
            trip?.Segments?
                .OrderBy(segment => segment.Ordinal)
                .Select(segment =>
                {
                    var carrierCode = segment.MarketingSegment?.MarketingCarrier?.FirstOrDefault()?.Code
                        ?? segment.OperatingCarrier?.Code;
                    var flightNumber = segment.MarketingSegment?.FlightNum ?? segment.FlightNum;
                    return carrierCode.IsNullOrWhiteSpace() || flightNumber.IsNullOrWhiteSpace()
                        ? null
                        : $"{carrierCode}{flightNumber}";
                }));

        private static string? JoinTransitValues(IEnumerable<string?>? values)
        {
            var joined = string.Join(",", values?.Where(value => !value.IsNullOrWhiteSpace()) ?? []);
            return joined.IsNullOrWhiteSpace() ? null : joined;
        }

        private static string? GetDate(string? value) => DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var date)
                ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : null;

        private static string? GetCountryCode(DeltaTrm.Country? country)
        {
            if (!country?.IsoAlpha3Code.IsNullOrWhiteSpace() == true)
                return country.IsoAlpha3Code;

            return country?.Code.IsNullOrWhiteSpace() == false ? country.Code : null;
        }
    }
}
