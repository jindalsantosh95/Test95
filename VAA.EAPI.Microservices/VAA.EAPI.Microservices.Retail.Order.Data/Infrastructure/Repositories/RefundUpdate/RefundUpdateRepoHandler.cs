using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.RefundUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate
{
    public class RefundUpdateRepoHandler : IRequestHandler<RefundUpdateRepoCommand, Unit>
    {
        private readonly IMediator _mediator;

        public RefundUpdateRepoHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<Unit> Handle(RefundUpdateRepoCommand request, CancellationToken cancellationToken)
        {
            string paymentId = PaymentHelper.GeneratePaymentId(DateTime.UtcNow);

            var orderDoc = await _mediator.Send(new CosmosQueryCommand() { DocType = Constants.Application.OrderDocType, PartitionKey = request.VoidOrderUpdateData.OrderRef, SqlQuery = Constants.Application.OrderDBSqlQuery }, cancellationToken);

            if (orderDoc.Count > 0)
            {
                var orderDetails = Json.DeserializeObject<OrderDetails>((JToken)orderDoc.FirstOrDefault());
                OrderDetails order = RefundUpdateOrderDocMapper.MapToOrderDoc(request, orderDetails, paymentId);
                CosmosModifyServiceCommand cosmosModifyServiceCommand = CosmosUpdateMapper.OrderUpdateServiceMapper(order);
                try
                {
                    await _mediator.Send(cosmosModifyServiceCommand, cancellationToken);
                }
                catch
                {
                    var error = new Response<string>()
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    }; throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
                }

                if (orderDetails.FulfilmentId.Any(x => x.IdType == OrderFulfillmentType.PNR))
                {
                    var pnrSearchDoc = await _mediator.Send(new CosmosQueryCommand() { DocType = Constants.Application.PnrSearchDocType, PartitionKey = $"{orderDetails.FulfilmentId.Find(x => x.IdType == OrderFulfillmentType.PNR).FulfillmentId}_{orderDetails.OrderCreationDate.Replace("-", string.Empty)}", SqlQuery = Constants.Application.OrderDBSqlQuery }, cancellationToken);

                    if (pnrSearchDoc.Count > 0)
                    {
                        var pnrSearchDetails = Json.DeserializeObject<PnrSearch>((JToken)pnrSearchDoc.FirstOrDefault());
                        PnrSearch pnrSearch = RefundUpdatePnrSearchDocMapper.MaptoPnrSearchDoc(request, pnrSearchDetails, paymentId);
                        CosmosModifyServiceCommand cosmosModifyService = CosmosUpdateMapper.PnrSearchUpdateServiceMapper(pnrSearch);
                        try
                        {
                            await _mediator.Send(cosmosModifyService, cancellationToken);
                        }
                        catch
                        {
                            var error = new Response<string>()
                            {
                                Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                            }; throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
                        }
                    }
                }
            }
            else
            {
                var error = new Response<string>()
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.OrderRefErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                }; throw new DomainException($"{Json.Serialize(error.Errors)}");
            }
            return Unit.Value;
        }
    }
}