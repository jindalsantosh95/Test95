using CSharpFunctionalExtensions;
using FluentValidation;
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
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.PromotionUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate
{
    public class PromotionUpdateRequestCommandHandler : IRequestHandler<PromotionUpdateRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionUpdateRequestCommandHandler> _logger;
        private readonly IValidator<PromotionUpdateRequestData> _validator;

        public PromotionUpdateRequestCommandHandler(IMediator mediator, ILogger<PromotionUpdateRequestCommandHandler> logger, IValidator<PromotionUpdateRequestData> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Result<Response<string>>> Handle(PromotionUpdateRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("PromotionUpdateRequestCommandHandler started");
                var promotionUpdateRequestValidator = await _validator.ValidateAsync(request);
                if (promotionUpdateRequestValidator.IsValid)
                {
                    var cloudEvent = PromotionUpdateCommandToEventMapper.CommandToEventMapper(request);
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                    _logger.LogDebug($"Store event in event store - End");
                    try
                    {
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent);
                        _logger.LogDebug($"Publish event - End");
                        var promotionUpdateResponse = PromotionUpdateEventCommandResponseMapper.PromotionUpdateResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                        return promotionUpdateResponse != null ? Result.Success<Response<string>>(promotionUpdateResponse) : Result.Success(new Response<string>
                        {
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
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
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(promotionUpdateRequestValidator.Errors.FirstOrDefault().ToString()), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
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
                _logger.LogInformation("PromotionUpdateRequestCommandHandler completed");
            }
        }
    }
}
