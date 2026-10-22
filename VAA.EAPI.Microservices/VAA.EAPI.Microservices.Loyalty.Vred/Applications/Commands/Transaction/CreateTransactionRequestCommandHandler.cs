using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using CSharpFunctionalExtensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;
using VAA.EAPI.Microservices.Loyalty.Vred.Validators;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;
using VAA.EAPI.Infrastructure.Repository.EventStore;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CreateTransactionRequestCommandHandler : IRequestHandler<CreateTransactionRequestCommand, Result<Response<string>>>
    {
        protected readonly IMediator _mediator;

        public CreateTransactionRequestCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<string>>> Handle(CreateTransactionRequestCommand command, CancellationToken cancellationToken)
        {
            var validationResult = new CreateTransactionRequestCommandValidator().Validate(command);
           
            if (!validationResult.IsValid)
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception("Request contains invalid data or missing mandatory fields."), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));

            var cloudEvent = new CloudEvent()
            {
                Data = command,
                Type = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CreateTransactionEvent.ToString())),
                Source = Constants.Application.ServiceBusTopicNameForVred,
                Subject = EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CreateTransactionEvent.ToString())),
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.DataVersion
            };

            await _mediator.Send(new EventRequestInsertCommand
            {
                Id = cloudEvent.Id,
                Type = cloudEvent.Type,
                Data = command,
                Subject = cloudEvent.Subject
            });

            try
            {
                await _mediator.Publish(cloudEvent);
                return CreateTransactionResponseMapper.BuildTransactionResponse(cloudEvent.Id);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}