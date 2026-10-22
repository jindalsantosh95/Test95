using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.InviteHouseHold
{
    public class GetHouseHoldInviteeQuery : TransactionBase, IRequest<Result<Response<InviteHouseHoldResponse>>>
    {
        public string InviteeMemberNumber { get; set; }

        public GetHouseHoldInviteeQuery(string inviteeMemberNumber) : base()
        {
            InviteeMemberNumber = inviteeMemberNumber ?? throw new ArgumentNullException(nameof(inviteeMemberNumber));
            DocType = Constants.Application.InviteHouseHoldDocType;
        }
    }
}
