using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit
{
    public class MemberCustomAttributesCreateCommand : MemberCustomAttributesCommandBase, IRequest<Result<Response<MemberCustomAttributesCommandResponse>>>
    {
        public MemberCustomAttributesCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreateMemberCustomAttributes;
        }
    }
}