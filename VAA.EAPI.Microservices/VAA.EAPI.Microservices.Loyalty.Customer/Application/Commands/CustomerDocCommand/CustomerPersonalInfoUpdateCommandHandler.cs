using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ContactHistoryCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Enum;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.FlyingClubDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand
{
    public class CustomerPersonalInfoUpdateCommandHandler : IRequestHandler<CustomerPersonalInfoUpdateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerPersonalInfoUpdateCommandHandler> _logger;
        public readonly IGetCustTransDocumentService<FlyingClubMember> _custTransFcmService;

        public CustomerPersonalInfoUpdateCommandHandler(IMediator mediator, ILogger<CustomerPersonalInfoUpdateCommandHandler> logger, IGetCustTransDocumentService<FlyingClubMember> custTransFcmService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _custTransFcmService = custTransFcmService ?? throw new ArgumentNullException(nameof(custTransFcmService));
        }

        public async Task<Result<Response<string>>> Handle(CustomerPersonalInfoUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tier = await _mediator.Send(new CustomerSearchByEmailQuery(request.EmailId), cancellationToken);

                var customerDoc = GetCustomersDocument(request, cancellationToken).Result;

                if (!string.IsNullOrEmpty(tier.Value.Value.Tier) && !customerDoc.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").ContactValue.Is(request.EmailId))
                    return GetErrorResponse(Constants.ErrorCodes.EmailExistsInCosmosDB);

                var validationResult = ValidateAndProcessRequest(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return validationResult.ErrorMessage;
                }

                var updatedAttributes = Helper.Enrolment.VacUpdatedAttrubute.FindAttribute(customerDoc, request);
              
                var customerInfo = await GetAndUpdateCustomerInfo(request, updatedAttributes, cancellationToken);

                if (!customerInfo.IsSuccess)
                {
                    return Result.Success(new Response<string>
                    {
                        Status = Status.Error,
                        Value = customerInfo.ErrorMessage,
                    });
                }

                var correlationIdForEvents = AsyncContext.Current.CorrelationId.ToString();

                if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.Email) || updatedAttributes.Contains(VacManageAccountUpdateAttributes.Name))
                {
                    var azureB2CEvent = VacUpdateAzureB2CEventMapper.GetUpdateAzureB2CRequest(request, customerDoc);

                    await PublishToVacAzureB2CEvent(VacUpdateAzureB2CEventMapper.GetUpdateAzureB2CManageAccountEventMapperRequest(azureB2CEvent, correlationIdForEvents), correlationIdForEvents, cancellationToken);
                };
               
                if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.Name) || updatedAttributes.Contains(VacManageAccountUpdateAttributes.DateOfBirth))
                {
                    await PublishToVacAuditEvent(VacUpdateAzureB2CEventMapper.GetUpdateCustomerDetailsAuditEventMapperRequest(VacUpdateAzureB2CEventMapper.GetUpdateCustomerDetailsAuditRequest(customerDoc, request), correlationIdForEvents), correlationIdForEvents, cancellationToken);
                }
                
                return Result.Success(new Response<string>
                {
                    Status = Status.OK,
                    Value = "Successfully updated"
                });
            }
            catch (Exception ex)
            {
                await LogError(request, ex, cancellationToken);
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private (bool IsValid, Response<string> ErrorMessage) ValidateAndProcessRequest(CustomerPersonalInfoUpdateCommand request, CancellationToken cancellationToken)
        {
            var validateResponse = ValidateRequest(request);

            if (!string.IsNullOrEmpty(validateResponse.Errors?.First().Message))
            {
                return (false, validateResponse);
            }
            return (true, new Response<string>());
        }

        private async Task PublishToVacAzureB2CEvent(CloudEvent cloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateAzureB2CManageAccountEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(cloudEvent, cancellationToken);
            }
            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateAzureB2CManageAccountEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }


        private async Task PublishToVacAuditEvent(CloudEvent cloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateCustomerDetaisAuditEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(cloudEvent, cancellationToken);
            }
            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateCustomerDetaisAuditEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task<Customers> GetCustomersDocument(CustomerPersonalInfoUpdateCommand request, CancellationToken cancellationToken)
        {
            var customerDocResult = await _mediator.Send(new FetchCustomerDocumentQuery(request.CustomerId), cancellationToken);

            return customerDocResult.Value.Value;
        }                 
        private async Task<(bool IsSuccess, string ErrorMessage)> GetAndUpdateCustomerInfo(CustomerPersonalInfoUpdateCommand request, List<VacManageAccountUpdateAttributes> updatedAttributes, CancellationToken cancellationToken)
        {
            return await UpdateCustomerDocument(request, updatedAttributes, cancellationToken);
        }

        private async Task<(bool IsSuccess, string ErrorMessage)> UpdateCustomerDocument(CustomerPersonalInfoUpdateCommand request, List<VacManageAccountUpdateAttributes> updatedAttributes, CancellationToken cancellationToken)
        {
            var existingDoc = GetCustomersDocument(request,cancellationToken).Result;

            var currentEmailAddress = existingDoc.ContactDetails.FirstOrDefault(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y")?.ContactValue;

            if (existingDoc is null)
            {
                return (false, "Customer details not found");
            }

            CustomerSearch customerSearchDoc = (await _mediator.Send(new CustomerSearchQuery(existingDoc.LoyaltyMemberNumber), cancellationToken)).Value.Value;
            FlyingClubMember fcmDoc = await _custTransFcmService.GetCosmosDoc(DocumentType.FlyingClubMember.ToString(), existingDoc.CustomerId);

            if (customerSearchDoc is null)
            {
                return (false, "Customer details not found");
            }

            if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.PhoneNumber))
            {
                ContactHistory phoneContactHistory = (ContactHistory)existingDoc;
                var CreatePhoneContactHistoryDoc = await _mediator.Send(new ContactHistoryCreateCommand(phoneContactHistory), cancellationToken);
            }

            if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.Email))
            {
                ContactHistory emailContactHistory = ContactHistoryMapper.EmailMapRequest(existingDoc);
                var CreateEmailContactHistoryDoc = await _mediator.Send(new ContactHistoryCreateCommand(emailContactHistory), cancellationToken);
            }

            var updatedCustomerDoc = VacCustomerCommandMapper.MapCustomerPersonalInfoUpdate(existingDoc, request, updatedAttributes);

            string matchString = MatchString.GetMatchString(updatedCustomerDoc, null, CustomerTypeEnum.VirginAccount.ToString());
            updatedCustomerDoc.MatchString = matchString;

            await _mediator.Send(new CustomerRepoUpdateCommand(updatedCustomerDoc), cancellationToken);

            if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.Email) || updatedAttributes.Contains(VacManageAccountUpdateAttributes.Name) || updatedAttributes.Contains(VacManageAccountUpdateAttributes.DateOfBirth))
            {
                var updatedCustomerSerachDoc = VacCustomerCommandMapper.MapCustomerSearchDeatilsUpdate(customerSearchDoc, request);
                customerSearchDoc.MatchString = matchString;
                await _mediator.Send(new CustomerSearchRepoUpdateCommand(updatedCustomerSerachDoc), cancellationToken);
            }                           

            if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.Name))
            {
                var updatedFlyingClubDoc = VacCustomerCommandMapper.MapFlyingClubMemberDetailsUpdate(fcmDoc, request);
                await _mediator.Send(new FlyingClubDocCommand(updatedFlyingClubDoc), cancellationToken);
            }
            
            if (fcmDoc.TierCode.Is(Constants.Application.VacCode) && updatedAttributes.Count != 0)
            {
                var eventCorrelationId = AsyncContext.Current.CorrelationId.ToString();

                foreach (var attribute in updatedAttributes)
                {
                    if (attribute == VacManageAccountUpdateAttributes.Email)
                    {                        
                        var contactDetails = new Contact[]
                        {
                            new() 
                            {
                                ContactType = Constants.Application.EmailContactType,
                                Primary = Constants.Application.TrueFlag,
                                ContactValue = currentEmailAddress
                            }
                        };

                        var emailExistingAddress = PublishEmailEvent(VacManageAccountMapper.EmailEvent(new Customers { ContactDetails = contactDetails }, eventCorrelationId, VacManageAccountMapper.EmailTemplate(attribute)), eventCorrelationId, cancellationToken);

                        var emailNewAddress = PublishEmailEvent(VacManageAccountMapper.EmailEvent(updatedCustomerDoc, eventCorrelationId, VacManageAccountMapper.EmailTemplate(attribute)), eventCorrelationId, cancellationToken);

                        await Task.WhenAll(emailExistingAddress, emailNewAddress);
                    }

                    else
                    {
                        CloudEvent eventMessage = VacManageAccountMapper.EmailEvent(updatedCustomerDoc, eventCorrelationId, VacManageAccountMapper.EmailTemplate(attribute));

                        await PublishEmailEvent(eventMessage, eventCorrelationId, cancellationToken);
                    }
                }
            }

            return (true, string.Empty);
        }

        private async Task SendServiceResponse(string id, object jsonData, ServiceResponseProcessStatus status, string type, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ServiceResponseInsertCommand
            {
                Id = id,
                JsonResponseData = Json.Serialize(jsonData),
                Status = status,
                Type = type
            }, cancellationToken);
        }

        private async Task LogError(CustomerPersonalInfoUpdateCommand request, Exception ex, CancellationToken cancellationToken)
        {
            var result = Result.Failure<Response<CustomerPersonalInfoUpdateCommand>>(ex.Message);
            await SendServiceResponse(request.CustomerId, result, result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Constants.Application.CommandResponse, cancellationToken);
        }

        private static Response<string> ValidateRequest(CustomerPersonalInfoUpdateCommand request)
        {
            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.DateOfBirth) || string.IsNullOrEmpty(request.EmailId) || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.CountryCode))
                return GetErrorResponse(Constants.ErrorCodes.MandatoryInputFieldsMissing);

            var dateValidation = ValidateDateOfBirth(request.DateOfBirth);

            if (dateValidation.Errors is not null)
                return dateValidation;

            return ValidateCustomerUpdate(request);
        }

        public static Response<string> ValidateCustomerUpdate(CustomerPersonalInfoUpdateCommand customer)
        {
            if (customer.FirstName.Length < 1 || customer.FirstName.Length > 30 || customer.PhoneNumber.Length > 20 || customer.PhoneNumber.Length < 9 || customer.LastName.Length < 1 || customer.LastName.Length > 35)
                return GetErrorResponse(Constants.ErrorCodes.InvalidCharacterLenghth);

            if (!IsValidName(customer.LastName) || !IsValidName(customer.FirstName) || !IsValidPhoneNumber(customer.PhoneNumber))
                return GetErrorResponse(Constants.ErrorCodes.InvalidCharacterInputFields);

            return new Response<string>();
        }

        private static bool IsValidName(string name)
        {
            var allowedSpecialChars = new[] { '\'', '¬', '-', '£', ' ' };
            return name.All(c => char.IsLetter(c) || allowedSpecialChars.Contains(c));
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            return phoneNumber.All(char.IsDigit);
        }

        private static Response<string> ValidateDateOfBirth(string dob)
        {
            DateTime birthDate = Convert.ToDateTime(dob);
            int age = DateTime.Now.Year - birthDate.Year;

            if (DateTime.Now < birthDate.AddYears(age))
                age--;

            if (age < Constants.Application.VacAgeLowerLimit || birthDate.Year <= 1900)
                return GetErrorResponse(Constants.ErrorCodes.AgeRestrictionError);

            if (birthDate > DateTime.Today)
                return GetErrorResponse(Constants.ErrorCodes.InvalidDateFromat);

            return new Response<string>();
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

        private async Task PublishEmailEvent(CloudEvent cloudEvent, string correlationIdForEvent, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvent, Type = Constants.Application.VacUpdatesCrmEmailEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(cloudEvent, cancellationToken);
            }

            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvent, Type = Constants.Application.VacUpdatesCrmEmailEventType, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
    }
}
