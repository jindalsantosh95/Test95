using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit
{
    public class MemberCustomAttributesPatchCommand : MemberCustomAttributesCommandBase, IRequest<Result<Response<MemberCustomAttributesCommandResponse>>>
    {
        public MemberCustomAttributesPatchCommand() : base()
        {
            EventType = TransactionCommandType.UpdateMemberCustomAttributes;
        }
    }
}
