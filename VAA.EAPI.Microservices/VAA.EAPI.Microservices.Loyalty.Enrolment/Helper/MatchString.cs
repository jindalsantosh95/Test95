using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public class MatchString
    {
        private static string _matchstring;

        public static string GetMatchString(EnrolmentRequest enrolmentRequest)
        {
            string firstName = enrolmentRequest?.CustomerDetails?.FirstName.ToUpper();
            string lastName = GetLastName.PopulateLastName(enrolmentRequest).ToUpper();

            var address = enrolmentRequest?.CustomerDetails?.AddressDetails.FirstOrDefault(x => x.PreferredFlag == "Y");
            string streetName = address?.AddressLine1.ToUpper();
            string postCode = string.IsNullOrEmpty(address?.PostalCode) ? null : address?.PostalCode.ToUpper();
            string countryCode = address?.Country;

            string DOB = Utility.GetMatchStringDOB(enrolmentRequest?.CustomerDetails?.DateOfBirth, Constants.Application.MatchStringDateFormat);

            _matchstring = Common.Helpers.Utility.GenerateMatchString(enrolmentRequest.CustomerDetails.CustomerType.ToString(), streetName, postCode, firstName, lastName, countryCode, DOB);
            return _matchstring;
        }
    }
}
