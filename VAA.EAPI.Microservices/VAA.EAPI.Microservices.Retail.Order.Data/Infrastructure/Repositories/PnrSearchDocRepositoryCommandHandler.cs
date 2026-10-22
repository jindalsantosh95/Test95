using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class PnrSearchDocRepositoryCommandHandler : IRequestHandler<PnrSearchDocRepositoryCommand, Unit>
    {
        private readonly IMediator _mediator;

        public PnrSearchDocRepositoryCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<Unit> Handle(PnrSearchDocRepositoryCommand request, CancellationToken cancellationToken)
        {
            var td = request?.Req?.NotificationRequest?.FulfillmentDetails.Any(x => x.IdType == OrderFulfillmentType.ETicket);
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.OrderCollection),
                Event = new()
                {
                    Document = new PnrSearch()
                    {
                        Id = Guid.NewGuid().ToString(),
                        OrderRef = request.Req.NotificationRequest.OrderReference,
                        SchemaVersion = Constants.Application.SchemaVersion,
                        DocType = EventTypes.PnrSearch.ToString(),
                        PartitionKey = $"{request.Req.NotificationRequest.FulfillmentDetails?.FirstOrDefault(x => x.IdType == OrderFulfillmentType.PNR).FulfillmentId}{"_"}{request.Req.NotificationRequest.CreationDate.Replace("-", string.Empty)}",
                        OrderCreationDate = request.Req.NotificationRequest.CreationDate,
                        OrderCreationTime = DateTime.Parse(request.Req.NotificationRequest.CreationTime).ToString(Constants.Application.ISODateTimeFormat),
                        OrderStatus = OrderCreateStatus.Basket.ToString(),
                        Channel = request.Req.Channel,
                        ETicket = (bool)td ? EticketMapper.MapEticket(request.Req).ToList() : new List<string>(),
                        Payments = new List<PnrSearchPayment>(),
                        CreatedBy = Constants.Application.CreatedBy,
                        CreatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                        UpdateBy = Constants.Application.CreatedBy,
                        UpdatedDateTime = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
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
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                }; throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }
            return Unit.Value;
        }
    }
}