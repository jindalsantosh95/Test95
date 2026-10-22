using System;
using System.Data;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class VacUpdateAzureB2CEventMapper
    {
        public static ManageAccountAzureB2CUpdateRequest GetUpdateAzureB2CRequest(CustomerPersonalInfoUpdateCommand request, Customers customers)
        {
            return new ManageAccountAzureB2CUpdateRequest()
            {
                MemberNumber = request.LoyaltyNumber,
                NewEmailId = request.EmailId,
                CurrentEmailId = customers.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").ContactValue,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
        }

        public static CloudEvent GetUpdateAzureB2CManageAccountEventMapperRequest(ManageAccountAzureB2CUpdateRequest request, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Subject = Constants.Application.UpdateAzureB2CManageAccountEventType,
                Data = request,
                Source = Constants.Application.EnrolmentEventHandlerTopicName,
                Type = Constants.Application.UpdateAzureB2CManageAccountEventType,
                Specversion = Constants.Application.DataVersion

            };
        }

        public static CustomerProfileAuditDocument GetUpdateCustomerDetailsAuditRequest(Customers customers, CustomerPersonalInfoUpdateCommand request)
        {
            return new CustomerProfileAuditDocument()
            {
                FirstName = !string.IsNullOrEmpty(request.FirstName) ? request.FirstName : customers.FirstName,
                LastName = !string.IsNullOrEmpty(request.LastName) ? request.LastName : customers.LastName,
                MiddleName = !string.IsNullOrEmpty(request.MiddleName) ? request.MiddleName : customers.MiddleName,
                Title = !string.IsNullOrEmpty(request.Title) ? request.Title : customers.Title,
                DoB = !string.IsNullOrEmpty(request.DateOfBirth) ? Convert.ToDateTime(request.DateOfBirth) : Convert.ToDateTime(customers.DateOfBirth),
                Status = customers.Status,
                NameChangeReason = string.Empty,
                AuthorisedUser = string.Empty,
                DowngradeProhibitFlag = Constants.Application.DowngradeProhibitFlag,
                Gender = customers.Gender,
                CustomerProfileUpdatedBy = !string.IsNullOrEmpty(request.LastUpdatedBy) ? request.LastUpdatedBy : Constants.Application.WebChannelID,
                CustomerProfileUpdatedOn = DateTime.UtcNow,
                SourceIdentifier = Constants.Application.VACAccount,
                CustomerId = !string.IsNullOrEmpty(request.CustomerId) ? request.CustomerId : customers.CustomerId,
                CreatedBy = !string.IsNullOrEmpty(request.LastUpdatedBy) ? request.LastUpdatedBy : Constants.Application.WebChannelID,
                CreatedOn = DateTime.UtcNow,
            };
        }

        public static CloudEvent GetUpdateCustomerDetailsAuditEventMapperRequest(CustomerProfileAuditDocument customerProfileAudit, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Subject = Constants.Application.UpdateCustomerDetaisAuditEventType,
                Data = new ManageAccountAuditDocument()
                {
                    CustomerProfileAuditDocument = customerProfileAudit
                },
                Source = Constants.Application.UpdateCustomerDetaisAuditEventSource,
                Type = Constants.Application.UpdateCustomerDetaisAuditEventType,
                Specversion = Constants.Application.DataVersion,
                Time = DateTime.UtcNow.ToString(),
                DataContentType = Constants.Application.ContentType
            };
        }
    }
}
