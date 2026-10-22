using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction
{
    public class TransactionCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<TransactionCommandQueryResponse>>>
    {
    }

    public class TransactionCommandStatusQueryHandler : CommandStatusBaseQueryHandler<TransactionCommandQueryResponse>, IRequestHandler<TransactionCommandStatusQuery, Result<Response<TransactionCommandQueryResponse>>>
    {
        public TransactionCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<TransactionCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<TransactionCommandQueryResponse>>> Handle(TransactionCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<TransactionCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<TransactionCosmosDocumentEvent>>>(commandEventOutcome);


            // *** CURRENTLY NON FUNCTIONAL IF THE EVENT HANDLER HAD AN ERROR *** //

            //if (documentEvent.Value.Value.Document.DocType != ConfigReader.Get(Constants.EnvironmentSettings.ContactNoteDocType))
            //    return NotFoundResponse(requestId);

            return new Response<TransactionCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = documentEvent.Value.Status == Status.OK ? new TransactionCommandQueryResponse()
                {
                    CommandStatus = Status.OK,
                    RequestId = requestId,
                    Transaction = documentEvent.Value.Value is null ? null     // Document could be null if the request was a delete
                                                                        : new TransactionDocument(documentEvent.Value.Value.Document)
                } : new TransactionCommandQueryResponse()
                {
                    CommandStatus = Status.BusinessException,
                    RequestId = requestId,
                    Errors = documentEvent.Value.Errors
                }
            };
        }
    }
}

