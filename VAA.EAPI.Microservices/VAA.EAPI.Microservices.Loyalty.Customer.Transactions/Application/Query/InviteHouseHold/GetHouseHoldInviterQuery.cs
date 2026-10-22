using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.InviteHouseHold
{
    public class GetHouseHoldInviterQuery : TransactionBase, IRequest<Result<Response<InviteHouseHoldResponse>>>
    {
        public string InviterMemberNumber { get; set; }

        public GetHouseHoldInviterQuery(string inviterMemberNumber) : base()
        {
            InviterMemberNumber = inviterMemberNumber ?? throw new ArgumentNullException(nameof(inviterMemberNumber));
            DocType = Constants.Application.InviteHouseHoldDocType;
        }
    }
}
