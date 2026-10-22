using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
    public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public CreateBannerCommandHandler(IMediator mediator, ILogger<CreateBannerCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.BannerRequest == null)
                throw new ArgumentNullException(nameof(request));

            CloudEvent cloudEvent = null;
            try
            {
                var id = Guid.NewGuid().ToString();

                cloudEvent = new CloudEvent()
                {
                    Id = id,
                    Data = CreateBannerCommandToEventMapper.getBannerDocEventMapper(request.BannerRequest, id),
                    Type = Constants.Application.BannerEventSubject,
                    Source = Constants.Application.TransactionEventTopicName,
                    Subject = Constants.Application.BannerEventSubject,
                    Time = DateTime.UtcNow.ToString(Constants.Application.CloudEventTimeFormat),
                    Specversion = Constants.Application.SpecVersion
                };

                await _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });

                await _mediator.Publish(cloudEvent);

                return CreateBannerResponseMapper(id);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while creating Banner Document");

                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        public static Response<string> CreateBannerResponseMapper(string correlationID)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = "location", Value = ConfigReader.Get(Constants.EnvironmentSettings.BannerResponseURL).Replace("{CorrelationId}", correlationID) } },
            };
        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }

    }
}

