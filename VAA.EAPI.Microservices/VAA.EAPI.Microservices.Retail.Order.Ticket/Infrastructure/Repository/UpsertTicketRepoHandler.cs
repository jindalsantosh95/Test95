using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Mapper.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository
{
    public class UpsertTicketRepoHandler : IRequestHandler<UpsertTicketRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpsertTicketRepoHandler> _logger;

        public UpsertTicketRepoHandler(IMediator mediator, ILogger<UpsertTicketRepoHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(UpsertTicketRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.TicketDocument != null)
                {
                    return await CreateOrReplaceTicketDocument(request, cancellationToken);
                }
                return Unit.Value;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                _logger.LogError(ex, "UpsertTicketRepoCommandHandler PreconditionFailed - Document was modified by another process. current replace operation aborted - Reattempt initiated");

                return await CreateOrReplaceTicketDocument(request, cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogError(ex, "UpsertTicketRepoCommandHandler Conflict - Document was created by another process. current create operation aborted - Reattempt initiated");

                return await CreateOrReplaceTicketDocument(request, cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogError(ex, "UpsertTicketRepoCommandHandler TooManyRequests Error - The request rate is too large - Reattempt initiated");

                return await CreateOrReplaceTicketDocument(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpsertTicketRepoCommandHandler Exceptions" + ex.Message);
                throw;
            }
        }

        private async Task<List<object>> FetchIfDocumentAlreadyExists(TicketDto cosmosDocument, CancellationToken cancellationToken)
        {
            CosmosReaderRepoQuery objQuery = new()
            {
                CollectionName = ConfigReader.Get(Constants.Environment.TicketCosmosDBCollection),
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.TicketCosmosDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.TicketCosmosDBName),
                SqlQuery = Constants.Application.CheckDocExistPrimaryTicketRetrivalSelectQuery,
                QueryParams = new() { new KeyValuePair<string, object>("@TicketNumber", cosmosDocument.Document.TktDocNb) },
            };

            CosmosReaderServiceQuery cosmosReaderServiceCommand = CommandMapper.MapCosmosReaderCommand(objQuery);

            return await _mediator.Send(cosmosReaderServiceCommand, cancellationToken); ;
        }

        private async Task<Result<Unit>> CreateOrReplaceTicketDocument(UpsertTicketRepoCommand request, CancellationToken cancellationToken)
        {
            var cosmosDocument = request.TicketDocument;

            List<object> ticketDocResponse = await FetchIfDocumentAlreadyExists(cosmosDocument, cancellationToken);

            CosmosDocumentEvent docEvent;
            if (ticketDocResponse.Any())
            {
                var existDoc = Json.DeserializeObject<TicketAvailabilityModel>((JToken)ticketDocResponse.FirstOrDefault());

                if (Convert.ToInt32(existDoc.TkDcUpdtSqlNb) >= Convert.ToInt32(cosmosDocument.Document.TkDcUpdtSqlNb))
                {
                    return Unit.Value;
                }

                if (!string.IsNullOrWhiteSpace(cosmosDocument.Document.TPFTrEvtTypCd) && cosmosDocument.Document.TPFTrEvtTypCd.ToUpper().Equals(Constants.Application.TicketPurgeEventCapp))
                {
                    cosmosDocument.Ttl = Convert.ToInt32(ConfigReader.Get(Constants.Environment.TimeToLiveForTicketDocumentAfterCappEvent));
                }
                else
                {
                    cosmosDocument.Ttl = existDoc.Ttl;
                }

                docEvent = new CosmosDocumentEvent
                {
                    Document = cosmosDocument,
                    ModificationType = EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Replace,
                    RequestOptions = new ItemRequestOptions
                    {
                        IfMatchEtag = existDoc.ETag
                    }
                };
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(cosmosDocument.Document.TPFTrEvtTypCd) && cosmosDocument.Document.TPFTrEvtTypCd.ToUpper().Equals(Constants.Application.TicketPurgeEventCapp))
                {
                    return Unit.Value;
                }

                docEvent = new CosmosDocumentEvent
                {
                    Document = cosmosDocument,
                    ModificationType = EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Create
                };
            }

            cosmosDocument.PartitionKey = cosmosDocument.Id = cosmosDocument.Document.TktDocNb;

            CosmosModifyRepoCommand command = new()
            {
                Event = docEvent,
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.TicketCosmosDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.TicketCosmosDBName),
                CollectionName = ConfigReader.Get(Constants.Environment.TicketCosmosDBCollection)
            };

            await _mediator.Send(command, cancellationToken);

            return Unit.Value;
        }
    }
}
