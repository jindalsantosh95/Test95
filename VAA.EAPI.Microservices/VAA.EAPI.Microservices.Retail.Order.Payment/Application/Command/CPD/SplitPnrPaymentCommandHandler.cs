using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class SplitPnrPaymentCommandHandler : ISplitPnrPaymentCommand
    {
        private readonly PaymentDocRepoQueryHandler _paymentDocRepoQueryHandler;
        private readonly CosmosRepoCommandHandler _cosmosRepoCommandHandler;

        public SplitPnrPaymentCommandHandler(PaymentDocRepoQueryHandler paymentDocRepoQuery, CosmosRepoCommandHandler cosmosRepoCommandHandler)
        {
            _paymentDocRepoQueryHandler = paymentDocRepoQuery ?? throw new System.ArgumentNullException(nameof(paymentDocRepoQuery));
            _cosmosRepoCommandHandler = cosmosRepoCommandHandler ?? throw new System.ArgumentNullException(nameof(cosmosRepoCommandHandler));
        }

        public async Task<Result<Response<IEnumerable<PaymentResponse>>>> Handle(SplitPnrPaymentRequest request, CancellationToken cancellationToken)
        {
            var parentPaymentDoc = await _paymentDocRepoQueryHandler.Handle(new PaymentDocRepoQuery { OrderId = request.OrderId, Query = Constants.Application.SplitPnrPaymentDocQuery }, cancellationToken);
            var paymentResponses = new List<PaymentResponse>();

            foreach (var parentPayment in parentPaymentDoc)
            {
                var parentDocResponse = Json.Deserialize<PaymentDocumentResponse>(Json.Serialize(parentPayment));
                var parentDoc = parentDocResponse.PaymentDocument;

                parentDoc.BookingReferences = request.ParentBookingReference;

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = parentDoc, ModificationType = CosmosModificationType.Upsert }), cancellationToken);

                var childDoc = PaymentDocumentMapper.CreateChildPaymentDocument(parentDoc, request);

                await _cosmosRepoCommandHandler.Handle(new CosmosRepoCommand(new CosmosDocumentEvent { Document = childDoc, ModificationType = CosmosModificationType.Upsert }), cancellationToken);

                var response = PaymentResponseMapper.SplitPnrPaymentResponse(childDoc);

                paymentResponses.AddRange(response); 
            }

            return Result.Success(new Response<IEnumerable<PaymentResponse>>(){ Status = Status.OK, Value = paymentResponses });
        }
    }
}
