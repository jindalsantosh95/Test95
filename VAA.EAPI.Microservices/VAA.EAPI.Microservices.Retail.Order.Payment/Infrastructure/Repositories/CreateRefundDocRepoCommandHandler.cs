using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class CreateRefundDocRepoCommandHandler : IRequestHandler<CreateRefundDocRepoCommand, Unit>
    {

        private readonly ILogger<CreateRefundDocRepoCommandHandler> _logger;
        private readonly IMediator _mediator;

        public CreateRefundDocRepoCommandHandler(IMediator mediator, ILogger<CreateRefundDocRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(CreateRefundDocRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.PaymentDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.PaymentDatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.PaymentCollection),
                Event = new()
                {
                    Document = new RefundTransactionDocument()
                    {
                        Id = request.RefundDocReq.TransactionId,
                        PartitionKey = request.RefundDocReq.TransactionId,
                        CreatedBy = request.RefundDocReq.CreatedBy,
                        CreatedOn = request.RefundDocReq.CreatedOn,
                        ModifiedBy = request.RefundDocReq.ModifiedBy,
                        ModifiedOn = request.RefundDocReq.ModifiedOn,
                        AccountableDocumentRefs = request.RefundDocReq.AccountableDocumentRefs,
                        BookingReferences = request.RefundDocReq.BookingReferences,
                        OrderId = request.RefundDocReq.OrderId,
                        Parties = request.RefundDocReq.Parties,
                        PaymentDetails = request.RefundDocReq.PaymentDetails,
                        Pos = request.RefundDocReq.Pos,
                        Status = request.RefundDocReq.Status,
                        DocType = request.RefundDocReq.DocType

                    },
                    ModificationType = CosmosModificationType.Create
                }
            };
            try
            {
                await _mediator.Send(command, CancellationToken.None);
            }
            catch
            {
                var error = new Response<string>()
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
                throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }
            return Unit.Value;
        }

    }
}
