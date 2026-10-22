using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember
{
    public class FCMDocBySSAMemberNumberQuery : TransactionBase, IRequest<Result<Response<FlyingClubMemberResponse>>>
    {
        public string SSAMemberNumber { get; set; }

        public FCMDocBySSAMemberNumberQuery(string ssaMemberNumber) : base()
        {
            SSAMemberNumber = ssaMemberNumber;
            DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType);
        }
    }
}