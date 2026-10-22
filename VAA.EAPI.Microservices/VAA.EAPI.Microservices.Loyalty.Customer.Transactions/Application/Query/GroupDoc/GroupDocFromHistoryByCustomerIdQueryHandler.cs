using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.GroupDoc
{
    public class GroupDocFromHistoryByCustomerIdQueryHandler : TransactionBaseQueryHandler<GroupDocumentResponse>, IRequestHandler<GroupDocFromHistoryByCustomerIdQuery, Result<Response<GroupDocumentResponse>>>
    {
        public GroupDocFromHistoryByCustomerIdQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<GroupDocumentResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<GroupDocumentResponse>>> Handle(GroupDocFromHistoryByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CustomerTransactionRepoQuery CreateRepoQuery(TransactionBase request)
        {
            return new GroupDocFromHistoryByCustomerIdRepoQuery(((GroupDocFromHistoryByCustomerIdQuery)request).ChildCustomerId)
            {
                DocType = request.DocType
            };
        }
    }
}

