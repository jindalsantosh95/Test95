using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc
{
    public class GroupDocQuery : TransactionBase, IRequest<Result<Response<GroupDocumentResponse>>>
    {
        public string GroupId { get; set; }

        public GroupDocQuery(string groupId) : base()
        {
            GroupId = groupId;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.GroupDocType);
        }
    }
}
