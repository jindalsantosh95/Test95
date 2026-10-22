using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction
{
    public class TransactionUpdateEventHandler : TransactionBaseEventHandler, IRequestHandler<TransactionUpdateEvent, Result<Unit>>
    {
        public TransactionUpdateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(TransactionUpdateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            TransactionCommandBase updateEvent = Json.DeserializeObject<TransactionCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new TransactionUpdateValidator().Validate(updateEvent.TransactionDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Transaction update request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var transactionDoc = (await TransactionCommandBase.GetListOfTransactions(_mediator, updateEvent.TransactionDocument.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.TransactionId == updateEvent.TransactionDocument.TransactionId);
            if (transactionDoc is null)
                throw new RequestValidationException(ErrorCodes.TransactionIdNotFoundMessage);

            transactionDoc.CancellationReasonCode = updateEvent.TransactionDocument.CancellationReasonCode ?? transactionDoc.CancellationReasonCode;
            transactionDoc.StatusCode = updateEvent.TransactionDocument.StatusCode ?? transactionDoc.StatusCode;
            transactionDoc.LastUpdatedBy = updateEvent.TransactionDocument.LastUpdatedBy;
            transactionDoc.LastUpdatedOn = updateEvent.TransactionDocument.LastUpdatedOn;
            transactionDoc.CancelledBy = updateEvent.TransactionDocument.CancelledBy ?? transactionDoc.CancelledBy;

            if (updateEvent.TransactionDocument.FlightDetails?.ExternalReservationNumber != null)
            {
                transactionDoc.FlightDetails ??= new();
                transactionDoc.FlightDetails.ExternalReservationNumber = updateEvent.TransactionDocument.FlightDetails.ExternalReservationNumber;
            }

            TransactionDTO dto = (TransactionDTO)transactionDoc;

            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
