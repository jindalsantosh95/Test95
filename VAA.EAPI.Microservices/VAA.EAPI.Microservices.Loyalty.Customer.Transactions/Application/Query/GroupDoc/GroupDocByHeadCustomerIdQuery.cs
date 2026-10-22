using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc
{
    public class GroupDocByHeadCustomerIdQuery : TransactionBase, IRequest<Result<Response<GroupDocumentResponse>>>
    {
        public string HeadCustomerId { get; set; }

        public GroupDocByHeadCustomerIdQuery(string headCustomerId) : base()
        {
            HeadCustomerId = headCustomerId;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.GroupDocType);
        }
    }
}
