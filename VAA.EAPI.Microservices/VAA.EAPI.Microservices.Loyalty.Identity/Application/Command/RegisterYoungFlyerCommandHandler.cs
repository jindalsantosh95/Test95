using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

public sealed class RegisterYoungFlyerCommandHandler(
    RegisterRepoCommandHandler registerRepoCommandHandler,
    EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler,
    EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler,
    ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler,
    ILogger<RegisterYoungFlyerCommandHandler> logger,
    IGetAdUserByFcNumberRepository existingUserRepository)
    : IRegisterYoungFlyerCommandHandler
{
    private readonly RegisterRepoCommandHandler _registerRepoCommandHandler = registerRepoCommandHandler ?? throw new ArgumentNullException(nameof(registerRepoCommandHandler));
    private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
    private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
    private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
    private readonly ILogger<RegisterYoungFlyerCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IGetAdUserByFcNumberRepository _existingUserRepository = existingUserRepository ?? throw new ArgumentNullException(nameof(existingUserRepository));

    public async Task<Response<B2CUserResponse>> Handle(RegisterYoungFlyerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.CustomerDetails is null)
                return EntraErrorHelper<B2CUserResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "CustomerDetails is required.");

            var emailDetails = request.CustomerDetails.ContactDetails?.EmailDetails;
            var email = emailDetails?.FirstOrDefault()?.EmailId;
            var dummyDomain = ConfigReader.Try("DummyEmailDomain", "U18.virgin.com");
            var emailParts = email?.Split('@');
            var isDummyEmail = emailParts is { Length: 2 }
                && !emailParts[0].IsNullOrWhiteSpace()
                && !emailParts[1].IsNullOrWhiteSpace()
                && emailParts[1].Is(dummyDomain);
            if (emailDetails is not { Count: 1 } || email.IsNullOrWhiteSpace() || !isDummyEmail || !emailDetails[0].Valid.Is("N"))
                return EntraErrorHelper<B2CUserResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "A managed Young Flyer registration requires a system-generated Young Flyer dummy email.");
            if (int.TryParse(request.CustomerDetails.FcNumber, out var memberNumber))
            {
                var existingObjectId = await _existingUserRepository.GetCustomerIdByFcNumber(memberNumber, cancellationToken);
                if (!existingObjectId.IsNullOrWhiteSpace())
                {
                    return new Response<B2CUserResponse>
                    {
                        Status = Status.OK,
                        Value = new B2CUserResponse { Id = existingObjectId, IsB2COperationSuccess = true }
                    };
                }
            }

            var details = request.CustomerDetails;
            var emailId = details.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.ToLowerInvariant();
            details.Gender = NormalizeGender(details.Gender);

            foreach (var phone in details.ContactDetails?.PhoneDetails ?? Enumerable.Empty<AccountPhoneDetail>())
            {
                if (!phone.CountryCode.IsNullOrWhiteSpace())
                    phone.CountryCode = phone.CountryCode.Trim().TrimStart('+');
            }

            foreach (var address in details.AddressDetails ?? Enumerable.Empty<AccountAddressDetail>())
            {
                if (address.AddressType == null && address.PreferredFlag.Is("Y"))
                    address.AddressType = AccountAddressDetail.AccountAddressTypes.Home;
            }

            var memberRegisterRequest = new MemberRegisterRequest
            {
                MemberNumber = details.FcNumber,
                EmailAddress = emailId,
                Password = details.Password,
                FirstName = details.FirstName,
                LastName = details.LastName,
                CustomerId = details.CustomerId,
                CountryCode = details.AddressDetails?.FirstOrDefault()?.Country,
                MemberType = details.LoyaltyMember?.MemberType.ToString()
            };

            var response = await _registerRepoCommandHandler.Handle(new RegisterRepoCommand(memberRegisterRequest, request.AccountEnabled), cancellationToken);
            if (response?.IsB2COperationSuccess != true)
                return EntraErrorHelper<B2CUserResponse>.CreateErrorResponse(
                    Constants.ErrorCodes.B2CBusinessError,
                    "b2c_registration_failed",
                    response?.B2CFaliureMessage ?? "Young Flyer B2C registration failed.");

            var correlationId = Guid.NewGuid().ToString();
            var customerEvent = CustomerSyncEventMapper.EventDataMapper(correlationId, details);
            var vredEvent = VredOcdbSyncEventMapper.EventDataMapper(correlationId, details);
            var tasks = new List<Task>
            {
                PublishCustomerEvent(customerEvent, correlationId, cancellationToken),
                PublishVredOcdbEvent(vredEvent, correlationId, cancellationToken)
            };

            if (!request.SuppressCommunication)
            {
                var emailEventData = EmailTriggeringEventRequestMapper.CreateEventGridSchemaDatawithWelcomeEmail(MapCustomerDetailsToCustomerEntities.MaptoCreateCustomerEntities(details));
                tasks.Add(PublishEmailEvent(EmailTriggeringCloudEventMapper.EventDataMapper(emailEventData), correlationId, cancellationToken));
            }

            await Task.WhenAll(tasks);
            return new Response<B2CUserResponse> { Status = Status.OK, Value = response };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Young Flyer registration failed.");
            return EntraErrorHelper<B2CUserResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during Young Flyer registration.");
        }
    }

    private async Task PublishCustomerEvent(CloudEvent cloudEvent, string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationId, Type = Constants.Application.EnrolCustomerEvent, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
        }
        catch
        {
            await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationId, Type = Constants.Application.EnrolCustomerEvent, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            throw;
        }
    }

    private async Task PublishVredOcdbEvent(CloudEvent cloudEvent, string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationId, Type = Constants.Application.VredOcdbSycnEvent, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
        }
        catch
        {
            await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationId, Type = Constants.Application.VredOcdbSycnEvent, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            throw;
        }
    }

    private async Task PublishEmailEvent(CloudEvent cloudEvent, string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationId, Type = Constants.Application.WelcomeEmailEvent, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.IdentityEventHandlerTopicName }, cancellationToken);
        }
        catch
        {
            await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationId, Type = Constants.Application.WelcomeEmailEvent, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
            throw;
        }
    }

    private static string NormalizeGender(string gender) => gender?.ToLowerInvariant() switch
    {
        "female" => "F",
        "male" => "M",
        "undisclosed" => "U",
        "unspecified" => "X",
        _ => gender
    };
}
