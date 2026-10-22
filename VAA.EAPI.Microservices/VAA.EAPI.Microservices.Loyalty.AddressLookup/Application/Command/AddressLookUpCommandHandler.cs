using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;
using EnumOperations = VAA.EAPI.Infrastructure.Helper.EnumOperations;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command
{
    public class AddressLookUpCommandHandler : IRequestHandler<AddressLookupEventProcessCommand, Result<Unit>>
    {
        private readonly ILogger<AddressLookUpCommandHandler> _logger;
        private readonly IMediator _mediator;

        public AddressLookUpCommandHandler(IMediator mediator, ILogger<AddressLookUpCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(AddressLookupEventProcessCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            bool status = false;
            var addressLookupRequest = Json.Deserialize<AddressLookUpRequest>(Convert.ToString(request.Data));
            _logger.LogInformation($"CorrelationId: {request.Id} & CustomerId: {addressLookupRequest.CustomerId}");
            _logger.LogInformation($"Address Validation - Start");
            if (await ValidateAddress(addressLookupRequest))
            {
                addressLookupRequest.TermsAndConditionsAccepted = true;
                addressLookupRequest.AddressDetails.First(x => x.PreferredFlag.Is("Y")).AddressVerified = true;
                _logger.LogInformation($"Address Validated");
            }
            else
            {
                addressLookupRequest.AddressDetails.First(x => x.PreferredFlag.Is("Y")).AddressVerified = false;
            }

            _logger.LogInformation($" Address Validation - End");


            #region Publish customer event
            var customerRequestEvent = AddressLookUpEventDataMapper.EventDataMapper(request, addressLookupRequest);

            try
            {
                _logger.LogDebug($"Store event in event store - Start");
                await _mediator.Send(new EventRequestInsertCommand { Id = request.Id, Type = customerRequestEvent.Type, Data = customerRequestEvent.Data, Subject = customerRequestEvent.Subject }, cancellationToken);
                status = true;
                _logger.LogDebug($"Store event in event store - End");

                _logger.LogInformation($"Publishing Event Start");
                await _mediator.Publish(customerRequestEvent, cancellationToken);
                _logger.LogInformation($"Publishing Event End");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from event store due to error: {ex.Message}");
                if (status)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = customerRequestEvent.Type, Data = customerRequestEvent.Data, Subject = customerRequestEvent.Subject }, cancellationToken);
            }

            #endregion

            return Unit.Value;
        }
        private async Task<bool> ValidateAddress(AddressLookUpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                if ((bool)(request.AddressDetails?.Any(x => x.PreferredFlag == "Y")))
                {
                    string countryCode = request.AddressDetails?.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.CountryCode;
                    string postalCode = request.AddressDetails?.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.PostalCode;
                    string streetDetails = request.AddressDetails?.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.AddressLine1;

                    if (countryCode == Convert.ToString(Country.GB) || countryCode == Convert.ToString(Country.US))
                    {
                        string countryCode3digit = EnumOperations.GetEnumDescription((Country)Enum.Parse(typeof(Country), countryCode));
                        return await _mediator.Send(new AddressLookUpRepoCommand(request, countryCode3digit, postalCode, streetDetails));
                    }

                    request.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y")).Valid = "Y";
                    return false;
                }

                _logger.LogInformation($"ValidateAddress-No primary address");
                request.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y")).Valid = "Y";
                return false;
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"Address Validation-Exception- {Convert.ToString(exception)}");
                request.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y")).Valid = "Y";
                return false;
            }
        }
    }
}
