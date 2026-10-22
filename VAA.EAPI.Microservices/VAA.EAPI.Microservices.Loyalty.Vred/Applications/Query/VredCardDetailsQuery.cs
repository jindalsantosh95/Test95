
using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query
{
    public class VredCardDetailsQuery : IRequest<Result<Response<VredCardDetailsResponse>>>
    {
        public string MemberNumber { get; set; }

        public VredCardDetailsQuery(string memberNumber)
        {
            MemberNumber = memberNumber;
        }
    }
}
