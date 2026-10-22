using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using VAA.EAPI.Microservices.CRM.Mapper;
using VAA.EAPI.Microservices.CRM.Validations;

namespace VAA.EAPI.Microservices.CRM.Application.Commands
{
    public class UpdatePaymentReqeustCommandHandlerV2 : IRequestHandler<UpdatePaymentReqeustCommandV2, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePaymentReqeustCommandHandlerV2> _logger;
        public UpdatePaymentReqeustCommandHandlerV2(IMediator mediator, ILogger<UpdatePaymentReqeustCommandHandlerV2> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Response<string>>> Handle(UpdatePaymentReqeustCommandV2 request, CancellationToken cancellationToken)
        {
            //Validate Request command
            var validationStatus = await new UpdatePaymentRequestValidator().ValidateAsync(request, cancellationToken);

            List<ErrorDetails> errorList = new();

            try
            {
                if (!validationStatus.IsValid)
                { //Return mandatory validation Errors 
                    var errorCodes = validationStatus.Errors.Select(x => x.ErrorMessage).Distinct().OrderBy(x => Convert.ToInt16(x));

                    foreach (var error in errorCodes)
                        errorList.AddRange(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(error.ToString()), AsyncContext.Current.CorrelationId, VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));

                    _logger.LogWarning($"UpdatePaymentReqeustCommandHandler | CorrelationId : {AsyncContext.Current.CorrelationId} | Validation Error(s): {Json.Serialize(errorList)} ");

                    return Result.Success(new Response<string>
                    {
                        Errors = errorList,
                        Status = Status.BadRequest
                    });
                }

                //Create Cloud event 
                var cloudEvent = UpdatePaymentReqeustCommandToEventMapperV2.CommandToEventMapper(request);

                //Store event in to event store
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    var paymentDetails = (await _mediator.Send(new RetrievePaymentDetailsRepoQuery(request.PaymentNumber), cancellationToken)).Value?.FirstOrDefault();

                    // check if its a duplicate as this will indicate it already has been submitted for processing and we can just silently exit successfully
                    if (paymentDetails?.VaaBankaccountdetailsentered ?? false || paymentDetails?.VaaPaymentStage == Constants.Application.SentForPaymentStage)
                    {
                        var response = new
                        {
                            Status = Domain.Enums.StatusEnum.Success,
                            CorrelationId = cloudEvent.Id
                        };

                        await _mediator.Send(new ServiceResponseInsertCommand { Id = cloudEvent.Id, JsonResponseData = Json.Serialize(response), Status = ServiceResponseProcessStatus.Completed, Type = cloudEvent.Subject }, cancellationToken);

                        return Result.Success(PaymentUpateEventCommandResponseMapper.MapToEventCommandResponse(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted)));
                    }

                    //Public event in to Service bus
                    await _mediator.Publish(cloudEvent, cancellationToken);

                    //Return response
                    var paymentUpdateResponse = PaymentUpateEventCommandResponseMapper.MapToEventCommandResponse(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));

                    return paymentUpdateResponse != null ? Result.Success<Response<string>>(paymentUpdateResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
                }
                catch
                {
                    //Rollback from Event Store                    
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                    throw;
                }
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, $"Exception in UpdatePaymentReqeustCommandHandler for CorrelationId: {AsyncContext.Current.CorrelationId}");

                var errorResponse = ErrorMapper.MapBusinessExceptionToEAPIException
                                    (
                                        ex: ex,
                                        guid: AsyncContext.Current.CorrelationId,
                                        errorFilePath: VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath
                                    );
                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
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
