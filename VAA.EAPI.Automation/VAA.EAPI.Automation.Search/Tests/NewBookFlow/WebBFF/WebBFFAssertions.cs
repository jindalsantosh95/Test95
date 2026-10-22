using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.WebBFF
{
    public class WebBFFAssertions
    {
        public static bool ValidateFCMemberDetails(string fcMembershipNumber, string tierCode, dynamic offerSearchResponse, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            var details = offerSearchResponse.flyingClubMemberDetails;

            if (details.fcMembershipNumber.Value != fcMembershipNumber)
            {
                result.Message.Add($"FC number not matched -> {details.fcMembershipNumber.Value}");
            }

            if (details.tierCode.Value != tierCode)
            {
                result.Message.Add($"Tier code not exists -> {details.tierCode.Value}");
            }

            if (string.IsNullOrEmpty(details.firstName.Value))
            {
                result.Message.Add($"First name not exists -> {details.firstName.Value}");
            }

            if (string.IsNullOrEmpty(details.lastName.Value))
            {
                result.Message.Add($"Last name not exists -> {details.lastName.Value}");
            }

            if (string.IsNullOrEmpty(details.totalVirginPoints.Value))
            {
                result.Message.Add($"Total points not exists -> {details.totalVirginPoints.Value}");
            }

            return result.IsValid;
        }
    }
}