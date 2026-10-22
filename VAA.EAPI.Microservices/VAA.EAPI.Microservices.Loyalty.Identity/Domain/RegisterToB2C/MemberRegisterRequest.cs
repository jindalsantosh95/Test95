using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C
{
    public class MemberRegisterRequest
    {
        public string MemberNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerId { get; set; }
        public string CountryCode { get; set; }
        public string MemberType { get; set; }

        public static explicit operator MemberRegisterRequest(CreateCustomerEntities request)
        {
            return new()
            {
                MemberNumber = request?.FcNumber,
                EmailAddress = request?.ContactDetails?.EmailDetails?.First()?.EmailId.ToLower(),
                FirstName = request?.FirstName,
                LastName = request?.LastName,
                CustomerId = request?.CustomerId,
                CountryCode = request?.AddressDetails?.First().CountryCode,
                MemberType = request?.MemberType
            };
           
        }
    }
}
