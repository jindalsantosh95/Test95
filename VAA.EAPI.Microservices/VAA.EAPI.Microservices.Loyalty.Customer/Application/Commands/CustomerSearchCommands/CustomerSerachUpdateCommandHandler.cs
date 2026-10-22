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
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerSearchCommands
{
    public class CustomerSerachUpdateCommandHandler : IRequestHandler<CustomerSearchUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSerachUpdateCommandHandler> _logger;

        public CustomerSerachUpdateCommandHandler(IMediator mediator, ILogger<CustomerSerachUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(CustomerSearchUpdateCommand request, CancellationToken cancellationToken)
        {
            Result<Response<CustomerSearchUpdateCommand>> result = null;

            try
            {
                var docUpdateData = Json.DeserializeObject<Infrastructure.Entities.CustomerSearch>((JToken)request.Data);
                var existingDoc = (await _mediator.Send(new CustomerSearchQuery(docUpdateData.LoyaltyMemberNumber), cancellationToken)).Value.Value;

                if (existingDoc is null)
                    throw new ArgumentNullException(nameof(existingDoc));

                //Update fields in document
                existingDoc.PostCode = docUpdateData.PostCode;
                existingDoc.Country = docUpdateData.Country;

                await _mediator.Send(new CustomerSearchRepoUpdateCommand(existingDoc), cancellationToken);
            }
            catch (Exception ex)
            {
                result = Result.Failure<Response<CustomerSearchUpdateCommand>>(ex.Message.ToString());
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponse });
            }

            return Unit.Value;
        }
    }
}