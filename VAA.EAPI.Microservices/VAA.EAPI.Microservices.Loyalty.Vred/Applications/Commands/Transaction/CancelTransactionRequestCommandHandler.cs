using MediatR;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using System.Threading;
using VAA.EAPI.Common.Serialisation;
using System.Net;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CancelTransactionRequestCommandHandler : IRequestHandler<CancelTransactionRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CancelTransactionRequestCommandHandler> _logger;

        public CancelTransactionRequestCommandHandler(IMediator mediator, ILogger<CancelTransactionRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(CancelTransactionRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var cloudEvent = CancelTransactionEventMapper.CancelTransactionCloudEventMapper(request, Guid.NewGuid().ToString());
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });

                try
                {
                    await _mediator.Publish(cloudEvent);
                    var cancelTransactionResponse = CancelTransactionCommandReponseMapper.CancelTransactionResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                    return cancelTransactionResponse != null ? Result.Success(cancelTransactionResponse) : Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException
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
                _logger.LogError(ex, $"Process failed due to Exception");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
