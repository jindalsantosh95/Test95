using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Banner
{
    public class BannerUpdateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : TransactionBaseEventHandler(mediator, logger), IRequestHandler<BannerUpdateEvent, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(BannerUpdateEvent request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            BannerCommandBase transactionEventData = Json.DeserializeObject<BannerCommandBase>((JToken)transactionEvent.Data);

            var existingDocument = BannerCommandBase.GetBanner(_mediator, transactionEventData.CustomerId, cancellationToken).Result;

            BannerDTO dto = (BannerDTO)BannerUpdateMapper.BannerMapper(existingDocument, transactionEventData);

            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }
    }
}
