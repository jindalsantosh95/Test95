using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using System.Linq;
using System.Collections.Generic;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using Microsoft.Extensions.Logging;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction
{
    public class RelatedTransactionQueryHandler : TransactionBaseQueryHandler<TransactionResponse>, IRequestHandler<RelatedTransactionQuery, Result<Response<TransactionResponse>>>
    {
        public RelatedTransactionQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<TransactionResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<TransactionResponse>>> Handle(RelatedTransactionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                List<TransactionDocument> relatedTransactions = new List<TransactionDocument>();

                var memberTransactions = await TransactionCommandBase.GetListOfTransactions(_mediator, request.CustomerId, cancellationToken);

                if (memberTransactions is null)
                    throw new RequestValidationException(ErrorCodes.CustomerIdNotFoundMessage);

                var existingTransaction = memberTransactions.FirstOrDefault(n => n.TransactionId == request.TransactionId);

                if (existingTransaction is null)
                    throw new RequestValidationException(ErrorCodes.TransactionIdNotFoundMessage);

                relatedTransactions.AddRange(memberTransactions.Where(x => x.ParentTransactionId == request.TransactionId && x.StatusCode == Constants.Application.StatusCode));

                if (!string.IsNullOrEmpty(existingTransaction.FlightDetails?.ExternalReservationNumber))
                {
                    relatedTransactions.AddRange(memberTransactions.Where(x => x.FlightDetails?.ExternalReservationNumber == existingTransaction.FlightDetails.ExternalReservationNumber && x.StatusCode == Constants.Application.StatusCode && x.TransactionId != existingTransaction.TransactionId && string.IsNullOrEmpty(x.CancelledTransactionId)));
                }

                return Result.Success(new Response<TransactionResponse>
                {
                    Status = Status.OK,
                    Value = new TransactionResponse { TransactionDocuments = relatedTransactions, CustomerId = request.CustomerId }
                });
            }
            catch (RequestValidationException rvex)
            {
                _logger.LogError(rvex, $"RelatedTransactionQueryHandler failed due to Exception");
                
                return Result.Success(new Response<TransactionResponse>()
                    {
                        Status = Status.BusinessException,
                        Errors = (await TransactionBaseEventHandler.MapValidationErrors(rvex)).ToList()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RelatedTransactionQueryHandler failed due to Exception");
                
                return Result.Failure<Response<TransactionResponse>>(ex.Message.ToString());
            }
        }
    }
}
