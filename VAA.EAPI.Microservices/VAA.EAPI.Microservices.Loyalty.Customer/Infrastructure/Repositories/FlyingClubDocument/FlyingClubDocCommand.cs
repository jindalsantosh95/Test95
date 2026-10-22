using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.FlyingClubDocCommand
{
    public class FlyingClubDocCommand : IRequest<Result<Unit>>
    {
        public FlyingClubMember MemberProfileDoc { get; set; }

        public FlyingClubDocCommand(FlyingClubMember updatedMemberProfileDocument)
        {
            MemberProfileDoc = updatedMemberProfileDocument;
        }
    }
}
