using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.StampFCNumberToPNR;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.StampFCNumToPNR;
using VAA.EAPI.Microservices.Retail.Order.Management.Validations;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.StampFCNumberToPNR
{
    public class StampFCNumToPNRRequestCommandHandler : IRequestHandler<StampFCNumToPNRRequestCommand, Result<Response<string>>>
    {
        private readonly ILogger<StampFCNumToPNRRequestCommandHandler> _logger;
        private readonly IMediator _mediator;
        private string _correlationId;

        public StampFCNumToPNRRequestCommandHandler(IMediator mediator, ILogger<StampFCNumToPNRRequestCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(StampFCNumToPNRRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                AddFCNumberToPNRQueryResponse addSSRResponse = new AddFCNumberToPNRQueryResponse();

                foreach (var item in request.FCStampingDetails)
                {
                    _correlationId = AsyncContext.Current.CorrelationId.ToString();

                    if (new AddFCNumberToPNRDataValidator().Validate(item).IsValid)
                    {
                        var eventData = StampFCNumtoPNREventDataMapper.MemberDetailsToFQTVSSREventDataMapper(item);
                        var cloudEvent = FCNumberToPNRCloudEventMapper.CommandToEventMapper(eventData, _correlationId);

                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                        try
                        {
                            await _mediator.Publish(cloudEvent);
                        }
                        catch
                        {
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                            throw;
                        }
                    }
                    else
                    {
                        return Result.Success(new Response<string>
                        {
                            Status = Status.BusinessException,
                            Value = null,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        });
                    }
                }

                return Result.Success(new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.StampingProcessInitiated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in stamp fc number to pnr command handler for CorrelationId: {_correlationId}.");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
