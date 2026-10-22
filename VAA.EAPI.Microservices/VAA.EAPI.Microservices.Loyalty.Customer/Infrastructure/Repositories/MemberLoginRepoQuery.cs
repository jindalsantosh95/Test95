using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class MemberLoginRepoQuery : IRequest<string>
    {
        public string MemberNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public MemberLoginRepoQuery(LoginValidationQuery request)
        {
            MemberNumber = request.MemberNumber;
            EmailAddress = request.EmailAddress;
            Password = request.Password;
        }
    }
}
