using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc
{
    public class GroupDocFromHistoryByCustomerIdQuery : TransactionBase, IRequest<Result<Response<GroupDocumentResponse>>>
    {
        public string ChildCustomerId { get; set; }

        public GroupDocFromHistoryByCustomerIdQuery(string childCustomerId) : base()
        {
            ChildCustomerId = childCustomerId;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.GroupDocType);
        }
    }
}

