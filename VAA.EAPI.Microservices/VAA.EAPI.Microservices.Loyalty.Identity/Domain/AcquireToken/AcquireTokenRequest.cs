using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken
{
    public class AcquireTokenRequest : IRequest<Result<Response<string>>>
    {
        public string Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RLOC { get; set; }
        public string TicketRef { get; set; }
    }
}
