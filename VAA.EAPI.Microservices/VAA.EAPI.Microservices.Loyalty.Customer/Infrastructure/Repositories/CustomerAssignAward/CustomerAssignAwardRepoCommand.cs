using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerAssignAward
{
    public class CustomerAssignAwardRepoCommand : IRequest<bool>
    {
        public AssignAwardRequestData Data { get; set; }
        public string Id { get; set; }

        public CustomerAssignAwardRepoCommand(AssignAwardRequestData assignLoyaltyAwardRequestData, string id)
        {
            Data = assignLoyaltyAwardRequestData;
            Id = id;
        }
    }
}
