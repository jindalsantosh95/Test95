using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Enrolment
{
    public class LoginValidationQuery : IRequest<string>
    {
        public string MemberNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public LoginValidationQuery(MemberLoginRequest memberLoginRequest)
        {
            MemberNumber = memberLoginRequest.MemberNumber;

            EmailAddress = memberLoginRequest.EmailAddress;

            Password = memberLoginRequest.Password;

        }
    }

}
