using System.Linq;
using System.Text.Json.Serialization;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Domain.CustomerDetails;

namespace VAA.Facade.EnrolmentFacade.Domain
{
    public class CustomerEnrolmentDetails
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("middleName")]
        public string MiddleName { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("contactDetails")]
        public ContactDetails ContactDetails { get; set; }

        [JsonPropertyName("loyaltyNumber")]
        public string LoyaltyNumber { get; set; }

        public static explicit operator CustomerEnrolmentDetails((EnrolmentRequestCommand enrolmentRequest, string fcNumber) input)
        {
            var request = input.enrolmentRequest;
            var fcNumber = input.fcNumber;

            var preferredEmail = request?.EnrolmentRequest?.CustomerDetails?.ContactDetails?.EmailDetails?.FirstOrDefault(x => x.PreferredFlag == "Y")?.EmailId;

            return new CustomerEnrolmentDetails()
            {

                FirstName = request?.EnrolmentRequest?.CustomerDetails?.FirstName,
                LastName = request?.EnrolmentRequest?.CustomerDetails?.LastName,
                MiddleName = request?.EnrolmentRequest?.CustomerDetails?.MiddleName,
                DateOfBirth = request?.EnrolmentRequest?.CustomerDetails?.DateOfBirth,
                ContactDetails = new ContactDetails
                {
                    EmailDetails = preferredEmail != null ?
                    [
                        new EmailDetails
                        {
                            EmailAddress = preferredEmail,
                            PreferredFlag = "Y"
                        }
                    ] : []
                },
                LoyaltyNumber = fcNumber      
            };
        }
    }
}
