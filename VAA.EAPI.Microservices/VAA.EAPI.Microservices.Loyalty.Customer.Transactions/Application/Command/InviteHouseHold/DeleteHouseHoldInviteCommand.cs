using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.InviteHouseHold
{
    public class DeleteHouseHoldInviteCommand : TransactionBaseCommand, IRequest<Result<Response<string>>>
    {
        public DeleteHouseHoldInvite DeleteHouseHoldInvite { get; set; }

        public DeleteHouseHoldInviteCommand(DeleteHouseHoldInvite deleteHouseHoldInvite)
        {
            DeleteHouseHoldInvite = deleteHouseHoldInvite;
        }

    }
}
