using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument
{
    public class AddressHistoryRepoCreateCommand : IRequest<Result<Unit>>
    {
        public AddressHistory AddressHistory { get; set; }

        public AddressHistoryRepoCreateCommand(AddressHistory addressHistory)
        {
            AddressHistory = addressHistory;
        }
    }
}
