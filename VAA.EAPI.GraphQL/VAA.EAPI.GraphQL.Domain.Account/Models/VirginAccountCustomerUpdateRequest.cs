using Application = VAA.EAPI.GraphQL.Common.Constants.Application;

namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class VirginAccountCustomerUpdateRequest
    {
        public string CustomerId { get; set; }
        public string LoyaltyNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Channel { get; set; }

        public static explicit operator VirginAccountCustomerUpdateRequest((VirginAccountCustomerUpdate updateRequest, string fcNumber, string customerId) input)
        {
            var request = input.updateRequest;
            var fcNumber = input.fcNumber;
            var customerId = input.customerId;

            return new VirginAccountCustomerUpdateRequest()
            {
                PhoneNumber = request.PhoneNumber,
                CountryCode = request.Countrycode,
                LoyaltyNumber = fcNumber,
                CustomerId = customerId,
                DateOfBirth = request.DateOfBirth,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailId = request.EmailAddress,
                LastUpdatedBy = Application.WebMyaChannelID
            };
        }
    }
}
