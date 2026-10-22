using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.CRM.Mapper;
using VAA.EAPI.Microservices.CRM.Validations;

namespace VAA.EAPI.Microservices.CRM.Application.Commands
{
    public class UpdatePaymentReqeustCommandHandler : IRequestHandler<UpdatePaymentReqeustCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePaymentReqeustCommandHandler> _logger;

        public UpdatePaymentReqeustCommandHandler(IMediator mediator, ILogger<UpdatePaymentReqeustCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<string>>> Handle(UpdatePaymentReqeustCommand request, CancellationToken cancellationToken)
        {
            //Validate Request command
            var validationStatus = await new UpdatePaymentRequestValidator().ValidateAsync(request, cancellationToken);
            List<ErrorDetails> errorList = new();
            try
            {
                if (!validationStatus.IsValid)
                { //Return valdiation Errors 
                    var errorCodes = validationStatus.Errors.Select(x => x.ErrorMessage).Distinct();
                    foreach (var error in errorCodes)
                    {
                        errorList.AddRange(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(error.ToString()), AsyncContext.Current.CorrelationId, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
                    }
                    _logger.LogWarning($"UpdatePaymentReqeustCommandHandler | CorrelationId : {AsyncContext.Current.CorrelationId} | Validation Error(s): {Json.Serialize(errorList)} ");
                    return Result.Success(new Response<string>
                    {
                        Errors = errorList,
                        Status = Status.BadRequest
                    });
                }
                //Create Cloud event 

                var cloudEvent = UpdatePaymentReqeustCommandToEventMapper.CommandToEventMapper(request);

                //Store event in to event store
                _logger.LogDebug($"Store event in event store - Start");
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                _logger.LogDebug($"Store event in event store - End");

                try
                {
                    //Publis event in to Service bus
                    _logger.LogDebug($"Publish event - Start");
                    await _mediator.Publish(cloudEvent, cancellationToken);
                    _logger.LogDebug($"Publish event - End");

                    //Return response
                    return Result.Success(new Response<string>
                    {
                        Status = Status.Accepted
                    });
                }
                catch
                {
                    //Rollack from Event Store                    
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in UpdatePaymentReqeustCommandHandler for CorrelationId: {AsyncContext.Current.CorrelationId}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException
                                    (
                                        errorCode: VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError,
                                        guid: AsyncContext.Current.CorrelationId.ToString(),
                                        errorFilePath: VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath
                                    );
                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}
