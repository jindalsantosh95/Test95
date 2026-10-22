using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment
{
    public class CustomerEnrolmentRequest
    {
        public Customerdetails CustomerDetails { get; set; }

        public static explicit operator CustomerEnrolmentRequest(Basket.Basket basket)
        {
            if (basket.ContactDetails.PeopleId.IsNullOrEmpty())
                return null;

            var personToEnrol = basket.People.FirstOrDefault(p => p.Id == basket.ContactDetails.PeopleId);

            if (personToEnrol.MembershipDetails?.FQTProgramme == "VS" && personToEnrol.MembershipDetails?.Validation == "VALID")
                return null;

            return new CustomerEnrolmentRequest()
            {
                CustomerDetails = new Customerdetails()
                {
                    FirstName = personToEnrol.FirstName,
                    LastName = personToEnrol.LastName,
                    DateOfBirth = DateOnly.FromDateTime(personToEnrol.DateOfBirth.Value),
                    MemberType = Constants.Application.VACEnrolmentMemberType,
                    CustomerType = Constants.Application.VACEnrolmentCustomerType,
                    Status = Constants.Application.VACEnrolmentStatus,
                    TermsAndConditionsAccepted = basket.TermsAndConditions.Accepted,
                    MediaCode = Constants.Application.VACEnrolmentMediaCode,
                    EnrollmentChannel = Constants.Application.VACEnrolmentChannel,

                    ContactDetails = new()
                    {
                        PhoneDetails = [new Phonedetail() { CountryCode = basket.ContactDetails.CountryCode, PhoneNumber = basket.ContactDetails.PhoneNumber, PreferredFlag = "Y" }],
                        EmailDetails = [new() { EmailId = basket.ContactDetails.EmailAddress, PreferredFlag = "Y" }]
                    },

                    Preferences = [new() { PreferenceType = Common.Enums.PreferenceTypes.VAAMarketingPermissions, PreferenceValue = basket.ContactDetails.AcceptEmail ? "Y" : "N" }]
                }
            };
        }
    }

}
