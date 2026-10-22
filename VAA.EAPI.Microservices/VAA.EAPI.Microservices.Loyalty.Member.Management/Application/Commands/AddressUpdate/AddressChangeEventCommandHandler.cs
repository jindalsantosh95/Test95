using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate
{
    public class AddressChangeEventCommandHandler : IRequestHandler<AddressChangeEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressChangeEventCommandHandler> _logger;
        private readonly ICustomerProvider _customerProvider;

        public AddressChangeEventCommandHandler(IMediator mediator, ILogger<AddressChangeEventCommandHandler> logger, ICustomerProvider customerProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        private async Task<Result<Response<string>>> PublishCustomerSearchUpdateEvent(AddressChangeEventData eventData, string memberNumber)
        {
            CustomerSearchUpdateEventData updateEvent = AddressChangeEventMapper.CustomerSearchDocUpdateRequestToEventDataMapper(eventData, memberNumber);
            var cloudEvent = AddressChangeEventToCloudEventMapper.CustomerSearchDocumentUpdateCommandToEventMapper(updateEvent);

            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });

            try
            {
                await _mediator.Publish(cloudEvent);

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "PublishCustomerSearchUpdateEvent Request processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PublishCustomerSearchUpdateEvent Process failed due to Exception - {ex.Message}");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }

        private async Task<Result<Response<string>>> PublishCustomerUpdateEvent(AddressChangeEventData eventData, Address currentAddress)
        {
            CustomerUpdateEventData updateEvent = AddressChangeEventMapper.CustomerDocUpdateRequestToEventDataMapper(eventData, currentAddress);
            var cloudEvent = AddressChangeEventToCloudEventMapper.CustomerDocumentUpdateCommandToEventMapper(updateEvent);

            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });

            try
            {
                await _mediator.Publish(cloudEvent);

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "PublishCustomerUpdateEvent Request processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PublishCustomerUpdateEvent Process failed due to Exception - {ex.Message}");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }

        private async Task<Result<Response<string>>> PublishAddressHistoryCreateEvent(AddressChangeEventData eventData, Address currentAddress)
        {
            AddressHistoryCreateEventData updateEvent = AddressChangeEventMapper.AddressHistoryCreateRequestToEventDataMapper(eventData, currentAddress);
            var cloudEvent = AddressChangeEventToCloudEventMapper.AddressHistoryCreateCommandToEventMapper(updateEvent);

            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });

            try
            {
                await _mediator.Publish(cloudEvent);

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "PublishAddressHistoryCreateEvent Request processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PublishAddressHistoryCreateEvent Process failed due to Exception - {ex.Message}");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }

        public async Task<Result<Response<string>>> Handle(AddressChangeEventCommand request, CancellationToken cancellationToken)
        {
            AddressChangeEventData eventData = Json.Deserialize<AddressChangeEventData>(Convert.ToString(request.Data));

            try
            {
                var commandValidator = new AddressChangeEventValidator().Validate(eventData);

                if (commandValidator.IsValid)
                {
                    Customer customerDoc = await _customerProvider.GetCustomerDocument(eventData.CustomerId);

                    if (customerDoc == null || string.IsNullOrWhiteSpace(customerDoc.CustomerId))
                    {
                        return Result.Failure<Response<string>>($"Invalid GetCustomerDocument Request - CustomerId Not found");
                    }

                    CustomerSearch customerSearchDoc = await _customerProvider.GetCustomerSearchDocument(customerDoc.LoyaltyMemberNumber);

                    if (customerSearchDoc == null || string.IsNullOrWhiteSpace(customerSearchDoc.CustomerId))
                    {
                        return Result.Failure<Response<string>>($"Invalid GetCustomerSearchDocument Request - Member Number Not found");
                    }

                    if (eventData.AddressDetail.PostalCode != customerSearchDoc.PostCode || eventData.AddressDetail.Country != customerSearchDoc.Country)
                    {
                        await PublishCustomerSearchUpdateEvent(eventData, customerSearchDoc.LoyaltyMemberNumber);
                    }

                    Address currentAddr = customerDoc.AddressDetails?.FirstOrDefault(x => x.Primary == "Y");

                    await PublishCustomerUpdateEvent(eventData, currentAddr);

                    await PublishAddressHistoryCreateEvent(eventData, currentAddr);

                    await _mediator.Send(new HouseHoldAccountEmailTriggerCommand()
                    {
                        Id = Guid.NewGuid(),
                        FirstName = customerDoc.FirstName,
                        LastName = customerDoc.LastName,
                        CustomerId = customerDoc.CustomerId,
                        MemberNumber = customerDoc.LoyaltyMemberNumber,
                        Email = customerDoc.ContactDetails?.FirstOrDefault(x => x.ContactType == "Email")?.ContactValue.Trim(),
                        EmailTemplate = Constants.Environment.HoHAddressUpdateEmailTemplate
                    });
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

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.AddressChangeEventSuccess
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in AddressChangeEvent command handler functionality with Customer id - {eventData.CustomerId}");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
