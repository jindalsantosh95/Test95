using CSharpFunctionalExtensions;
using FluentValidation;
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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate
{
    public class AddressUpdateNotificationCommandHandler : IRequestHandler<AddressUpdateNotificationCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressUpdateNotificationCommandHandler> _logger;
        private readonly ICustomerProvider _customerProvider;

        public AddressUpdateNotificationCommandHandler(IMediator mediator, ILogger<AddressUpdateNotificationCommandHandler> logger, ICustomerProvider customerProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        private async Task<Result<Response<string>>> PublishAddressChangeEvent(string customerId, AddressDetail address, string initiatedBy)
        {
            AddressChangeEventData updateEvent = new AddressChangeEventData()
            {
                CustomerId = customerId,
                InitiatedBy = initiatedBy,
                AddressDetail = address,
            };

            var cloudEvent = AddressUpdateNotificationEventToCloudEventMapper.CommandToEventMapper(updateEvent);

            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject });

            try
            {
                await _mediator.Publish(cloudEvent);

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "Request processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PostAddressChangeEvent Process failed due to Exception - {ex.Message}");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }

        public async Task<Result<Response<string>>> Handle(AddressUpdateNotificationCommand request, CancellationToken cancellationToken)
        {
            AddressUpdateNotificationEventData eventData = Json.Deserialize<AddressUpdateNotificationEventData>(Convert.ToString(request.Data));

            try
            {
                var commandValidator = new AddressUpdateCommandValidator().Validate(eventData);

                if (commandValidator.IsValid)
                {
                    Result<Response<FlyingClubMember>> response = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = eventData.CustomerId });
                    FlyingClubMember flyingClubMemberDoc = response.Value.Value;

                    if (flyingClubMemberDoc != null && !string.IsNullOrEmpty(flyingClubMemberDoc.GroupId) && flyingClubMemberDoc.MemberType == Constants.Application.HouseholdGroupTypeCode)
                    {
                        Customer customerDoc = await _customerProvider.GetCustomerDocument(eventData.CustomerId);
                        Address address = customerDoc.AddressDetails.FirstOrDefault(x => x.Primary == "Y");

                        if (customerDoc == null || string.IsNullOrWhiteSpace(customerDoc.CustomerId) || address == null)
                        {
                            return Result.Failure<Response<string>>("Invalid Request - CustomerId/AddressDetails Not found");
                        }

                        Result<Response<HouseHoldGroup>> groupResponse = await _mediator.Send(new GroupDocumentServiceQuery { CustomerId = customerDoc.CustomerId });
                        HouseHoldGroup groupDetails = groupResponse.Value.Value;

                        if (groupDetails == null || groupDetails.GroupHistory == null || groupDetails.GroupHistory.Count <= 0)
                        {
                            return Result.Failure<Response<string>>("Invalid Request - no group history found");
                        }

                        await Task.WhenAll(groupDetails.GroupHistory.Where(i => i.ExitDate == null || !i.ExitDate.HasValue)
                           .Select(i => PublishAddressChangeEvent(i.CustomerId, new AddressDetail()
                           {
                               AddressType = address.AddressType,
                               AddressLine1 = address.AddressLine1,
                               AddressLine2 = address.AddressLine2,
                               AddressLine3 = address.AddressLine3,
                               CompanyName = address.LandlordOrCompanyName,
                               City = address.City,
                               Country = address.Country,
                               PostalCode = address.PostCode,
                               PostalStatus = address.PostalStatus,
                               Primary = address.Primary,
                               AddressVerified = address.AddressVerified,
                               MetroArea = address.MetroArea,
                               Province = address.Province,
                               Valid = address.Valid
                           }, eventData.InitiatedBy)));

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
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.AccountIsNotHoHForAddressUpdate), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        });
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

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.AddressUpdateNotificationSuccess
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in AddressUpdateNotification command handler functionality with Customer id - {eventData.CustomerId}");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
        }
    }
}
