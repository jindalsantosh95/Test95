using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Constants;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command
{
    public class SFPDSyncCommandHandler : IRequestHandler<SFPDSyncCommand, Result<Response<SFPDSyncCommandResponse>>>
    {
        private readonly ILogger<SFPDSyncCommandHandler> _logger;
        private readonly IMediator _mediator;      

        public SFPDSyncCommandHandler(ILogger<SFPDSyncCommandHandler> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));         
        }

        public async Task<Result<Response<SFPDSyncCommandResponse>>> Handle(SFPDSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var cloudEvent = SFPDSyncRequestEventMapper.CommandToEventMapper(request.SfpdData);

                // Record in Event Store
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });                

                try
                {
                    // Publish to service bus
                    await _mediator.Publish(cloudEvent, cancellationToken);

                    // Send 202 ACCEPTED response
                    return Result.Success(new Response<SFPDSyncCommandResponse>
                    {
                        Status = Status.Accepted,
                        HeaderParameters = new System.Collections.Generic.List<ResponseHeader>()
                        {
                            new()
                            {
                                Key = "location",
                                Value = string.Format(ConfigReader.Get(Constants.Environment.CommandStatusLocation), Constants.Environment.SFPDSyncName,  cloudEvent.Id)
                            },
                            new()
                            {
                                Key = "CorrelationID",
                                Value =  cloudEvent.Id
                            }
                        }
                    });

                }
                catch
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SFPD update exception\n", ex.StackTrace);
                return Result.Failure<Response<SFPDSyncCommandResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }                      
        }
    }

}

