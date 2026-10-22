using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    internal sealed record SecondaryDocumentContentOverride(
        string CountryCode,
        [property: JsonConverter(typeof(StringEnumConverter))]
        DeltaTrm.RequirementOption RequirementOption,
        string? DisplayName = null,
        string? GovernmentTitle = null,
        string? GovernmentMessage = null,
        string? GovernmentUrl = null);

    internal static class SecondaryDocumentContentOverrides
    {
        public static SecondaryDocumentContentOverride? Find(DeltaTrm.Requirement requirement)
        {
            if (requirement.RequirementOptions?.Count is not > 0 || requirement.Country?.IsoAlpha3Code is null)
                return null;

            var overrides = JsonConvert.DeserializeObject<SecondaryDocumentContentOverride[]>(
                ConfigReader.Get(Constants.Environment.SecondaryDocumentContentOverrides)) ?? [];

            return overrides.FirstOrDefault(contentOverride =>
                string.Equals(contentOverride.CountryCode, requirement.Country.IsoAlpha3Code, StringComparison.Ordinal) &&
                contentOverride.RequirementOption == requirement.RequirementOptions.First());
        }
    }

    public class SecondaryDocumentApplication
    {
        public string SecondaryDocumentId { get; init; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public ApplicationType ApplicationType { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? Url { get; init; }
        public string? UrlText { get; init; }
    }

    internal static class SecondaryDocumentApplicationContent
    {
        private const string VendorTitle = "Apply with Sherpa";
        private const string VendorMessage = "Sherpa will guide you through the process from start to finish. Once it's approved, come back here and confirm you're ready to travel.";
        private const string GovernmentTitle = "Apply on the government website";
        private const string GovernmentUrlText = "Apply on government website";
        private const string EmbassyTitle = "Apply at the government embassy";
        private const string EmbassyUrlText = "Visit government website";

        public static SecondaryDocumentApplication Vendor(string secondaryDocumentId, string url) => new()
        {
            SecondaryDocumentId = secondaryDocumentId,
            ApplicationType = ApplicationType.Vendor,
            Title = VendorTitle,
            Message = VendorMessage,
            Url = url,
            UrlText = VendorTitle
        };

        public static SecondaryDocumentApplication Government(
            string secondaryDocumentId,
            string displayName,
            SecondaryDocumentContentOverride? contentOverride,
            bool isEmbassyVisa) => new()
        {
            SecondaryDocumentId = secondaryDocumentId,
            ApplicationType = ApplicationType.Government,
            Title = isEmbassyVisa
                ? EmbassyTitle
                : contentOverride?.GovernmentTitle ?? GovernmentTitle,
            Message = isEmbassyVisa
                ? $"Complete your {displayName} at your closest {{countryName}} embassy. Once it's approved, come back here and confirm you're ready to travel."
                : contentOverride?.GovernmentMessage
                    ?? $"Complete your {displayName} through the official government website. Once it's approved, come back here and confirm you're ready to travel.",
            Url = contentOverride?.GovernmentUrl,
            UrlText = isEmbassyVisa ? EmbassyUrlText : GovernmentUrlText
        };
    }
}
