using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.AddressHistoryCommand
{
    public class AddressHistoryCreateCommandHandler : IRequestHandler<AddressHistoryCreateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressHistoryCreateCommandHandler> _logger;

        public AddressHistoryCreateCommandHandler(IMediator mediator, ILogger<AddressHistoryCreateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(AddressHistoryCreateCommand request, CancellationToken cancellationToken)
        {
            Result<Response<AddressHistoryCreateCommand>> result = null;

            try
            {
                var docCreateData = Json.DeserializeObject<AddressHistory>((JToken)request.Data);

                docCreateData.Id = Guid.NewGuid().ToString();
                docCreateData.DocType = DocumentType.AddressHistory.ToString();

                await _mediator.Send(new AddressHistoryRepoCreateCommand(docCreateData), cancellationToken);
            }
            catch (Exception ex)
            {
                result = Result.Failure<Response<AddressHistoryCreateCommand>>(ex.Message.ToString());
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponse });
            }

            return Unit.Value;
        }
    }
}
