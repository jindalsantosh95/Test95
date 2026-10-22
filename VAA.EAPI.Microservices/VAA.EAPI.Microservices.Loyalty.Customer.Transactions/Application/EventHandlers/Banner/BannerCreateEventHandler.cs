using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Banner
{
    public class BannerCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<BannerCreateEvent, Result<Unit>>
    {
        public BannerCreateEventHandler(IMediator mediator,ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(BannerCreateEvent request,CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            var bannerDocument = Json.DeserializeObject<BannerDetail>((JToken)transactionEvent.Data);

            return bannerDocument == null? throw new RequestValidationException("Invalid Request in Banner CreateEvent Handler"):Task.FromResult(new CosmosDocumentEvent((BannerDTO)bannerDocument, transactionEvent.EventType));       
        }
    }
}
