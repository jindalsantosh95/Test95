using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public class MatchString
    {
        private static string _matchstring;
        public static string GetMatchString(EnrolmentRequestCommand enrolmentRequest)
        {
            string firstName = enrolmentRequest?.Data?.CustomerDetails?.FirstName.ToUpper();
            string lastName = GetLastName.PopulateLastName(enrolmentRequest).ToUpper();

            var address = enrolmentRequest?.Data?.CustomerDetails?.AddressDetails.FirstOrDefault(x => x.PreferredFlag == "Y");
            string streetName = address?.AddressLine1.ToUpper();
            string postCode = string.IsNullOrEmpty(address?.PostalCode) ? null : address?.PostalCode.ToUpper();
            string countryCode = address?.Country;
            string DOB = Utility.GetMatchStringDOB(enrolmentRequest?.Data?.CustomerDetails?.DateOfBirth, Constants.Application.MatchStringDateFormat);

            return Common.Helpers.Utility.GenerateMatchString(enrolmentRequest?.Data?.CustomerDetails?.CustomerType.ToString(), streetName, postCode, firstName, lastName, countryCode, DOB);
        }
    }
}
