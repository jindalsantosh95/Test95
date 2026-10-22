using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;

namespace VAA.EAPI.Automation.OCI.Tests.Microservices
{
    public class TravelRequirementsMSTests
    {
        public static string ocpSubKey = Vault.Default.GetSecretValue("AVATEUNAFNCHECKINMICROSERV-Subscription-Automation-Key");

        [Theory]
        [Trait("Category", Traits.TravelRequirementsMicroserviceCiCd)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public void TravelRequirementsServiceTests(JourneyType journeyType, ReservationType reservationType)
        {
            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrieveOCIPNRDetailsFromStorageTable(journeyTypeString, reservationTypeString);

            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.TryGetValue("error", out string errorMessage) ? $"Test case failed: {errorMessage}" : string.Empty);

            string pnrLocator = pnrDetails["pnr"];
            string givenNames = pnrDetails["firstName"];
            string surname = pnrDetails["lastName"];

            Assert.True(!string.IsNullOrEmpty(pnrLocator) && !string.IsNullOrEmpty(givenNames) && !string.IsNullOrEmpty(surname), "PNRLocator, GivenNames and Surname should not be null or empty");

            string travelRequirementsRequest = MicroserviceRequestBuilder.TravelRequirementsMSRequest(pnrLocator, givenNames, surname);

            HttpResponseMessage response = MicroserviceEndpoint.TravelRequirements(travelRequirementsRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            string responseContent = response.ReadResponseAsString();

            Assert.False(string.IsNullOrWhiteSpace(responseContent), "TravelRequirements response content is empty");

            JToken json = JToken.Parse(responseContent);

            bool travelRequirementsOk = MicroserviceAssertions.ValidateTravelRequirementsResponse(json, pnrLocator, out ValidationResultHelper travelRequirementsValidation);

            Assert.True(travelRequirementsOk, string.Join(" | ", travelRequirementsValidation.Message));

            bool slicesOk = MicroserviceAssertions.ValidateSlicesAndPassengerSliceRequirements(json, out ValidationResultHelper slicesValidation);

            Assert.True(slicesOk, string.Join(" | ", slicesValidation.Message));
        }
    }
}
