using MediatR;
using Microsoft.Azure.Cosmos.Linq;
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
    public class CreatePaymentDocRepoCommandHandler : IRequestHandler<CreatePaymentDocRepoCommand, Unit>
    {
        private readonly ILogger<CreatePaymentDocRepoCommandHandler> _logger;
        private readonly IMediator _mediator;

        public CreatePaymentDocRepoCommandHandler(IMediator mediator, ILogger<CreatePaymentDocRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(CreatePaymentDocRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.PaymentDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.PaymentDatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.PaymentCollection),
                Event = new()
                {
                    Document = new PaymentTransactionDocument()
                    {
                        Id =  !string.IsNullOrEmpty(request.PaymentDocReq?.PaymentDocumentId) ? request.PaymentDocReq.PaymentDocumentId : request.PaymentDocReq.TransactionId,
                        PartitionKey = !string.IsNullOrEmpty(request.PaymentDocReq?.PaymentDocumentId) ? request.PaymentDocReq.PaymentDocumentId : request.PaymentDocReq.TransactionId,
                        CreatedBy = request.PaymentDocReq.CreatedBy,
                        CreatedOn = request.PaymentDocReq.CreatedOn,
                        ModifiedBy = request.PaymentDocReq.ModifiedBy,
                        ModifiedOn = request.PaymentDocReq.ModifiedOn,
                        AccountableDocumentRefs = request.PaymentDocReq.AccountableDocumentRefs,
                        BookingReferences = request.PaymentDocReq.BookingReferences,
                        CustomerDetails = request.PaymentDocReq.CustomerDetails,
                        Errors = request.PaymentDocReq.Errors,
                        OrderId = request.PaymentDocReq.OrderId,
                        Parties = request.PaymentDocReq.Parties,
                        PaymentDetails = request.PaymentDocReq.PaymentDetails,
                        JourneyType = request.PaymentDocReq?.JourneyType,
                        Segments = request.PaymentDocReq?.Segments,
                        Pos = request.PaymentDocReq.Pos,
                        Status = request.PaymentDocReq.Status,
                        DocType = request.PaymentDocReq.DocType,
                        ProductType = request.PaymentDocReq.ProductType,
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
