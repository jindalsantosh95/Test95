using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.InviteHouseHold
{
    public class InviteHouseHoldCommand : TransactionBaseCommand, IRequest<Result<Response<string>>>
    {
        public InviteHouseHoldCreateRequest InviteHouseHold { get; set; }

        public InviteHouseHoldCommand(InviteHouseHoldCreateRequest inviteHouseHold)
        {
            InviteHouseHold = inviteHouseHold;
        }
    }
}
