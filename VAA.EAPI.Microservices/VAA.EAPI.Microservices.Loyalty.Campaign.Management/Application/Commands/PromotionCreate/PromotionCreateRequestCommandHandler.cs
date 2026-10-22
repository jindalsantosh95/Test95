using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate
{
    public class PromotionCreateRequestCommandHandler : IRequestHandler<PromotionCreateRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionCreateRequestCommandHandler> _logger;

        public PromotionCreateRequestCommandHandler(IMediator mediator, ILogger<PromotionCreateRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(PromotionCreateRequestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Promotion Create Command Request Handler - Start");

            try
            {
                var promotionCreateRequestValidator = new PromotionCreateRequestValidator().Validate(request);

                if (promotionCreateRequestValidator.IsValid)
                {
                    var cloudEvent = PromotionCreateCommandToEventMapper.CommandToEventMapper(request);
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                    _logger.LogDebug($"Store event in event store - End");

                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent);
                        _logger.LogDebug($"Publish event - End");
                        var promotionCreateResponse = PromotionCreateEventCommandResponseMapper.PromotionCreateResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                        return promotionCreateResponse != null ? Result.Success(promotionCreateResponse) : Result.Success(new Response<string>
                        {
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        });
                    }
                    catch
                    {
                        _logger.LogDebug($"Deleting the event from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(promotionCreateRequestValidator.Errors.FirstOrDefault().ToString()), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Process failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

            }
            finally
            {
                _logger.LogInformation($"Promotion Create Command Request Handler - End");
            }
        }
    }
}
