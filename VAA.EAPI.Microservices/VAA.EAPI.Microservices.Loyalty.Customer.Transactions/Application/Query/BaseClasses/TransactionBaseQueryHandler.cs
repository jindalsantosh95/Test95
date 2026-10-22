using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses
{
    public abstract class TransactionBaseQueryHandler<T> where T : ICustomerTransactionQueryResponse
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<TransactionBaseQueryHandler<T>> _logger;
        protected virtual bool ValidateRequest(TransactionBase request)
        {
            // Check to make sure either customer or member id is provided
            return !(string.IsNullOrEmpty(request.CustomerId) && string.IsNullOrEmpty(request.MemberId));
        }

        public TransactionBaseQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<T>> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<T>>> Handle(TransactionBase request, CancellationToken cancellationToken)
        {
            try
            {
                // Get the documents
                var transactions = await _mediator.Send(CreateRepoQuery(request), cancellationToken);

                // Transpose to domain model
                T custTransactions = (T)Activator.CreateInstance(typeof(T), request.CustomerId, transactions);

                // Set up return
                return Result.Success(new Response<T>
                {
                    Status = Status.OK,
                    Value = custTransactions
                });
            }
            catch (Exception ex)
            {
                if (ex is not Microsoft.Azure.Cosmos.CosmosException)           // Exception has already been logged
                    _logger.LogError(ex, $"Unexpected exception occured in query handler\n", ex.StackTrace);

                return new Response<T>()
                {
                    Status = Status.BusinessException,
                    Errors = EAPI.Infrastructure.Helper.ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.DBConnectionError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }

        public virtual CustomerTransactionRepoQuery CreateRepoQuery(TransactionBase request)
        {
            return new(request.CustomerId, request.DocType, GetFieldList());
        }

        protected virtual ICollection<string> GetFieldList()
        {
            // Default to ALL fields
            return new List<string>() { "*" };
        }
    }
}
