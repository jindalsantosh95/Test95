using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Enum;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class VacCustomerCommandMapper
    {
        public static Customers MapCustomerPersonalInfoUpdate(Customers customers, CustomerPersonalInfoUpdateCommand request, List<VacManageAccountUpdateAttributes> updatedAttributes)
        {
            if (customers == null || request == null)
                throw new ArgumentNullException(customers == null ? nameof(customers) : nameof(request));
            try
            {
                UpdateBasicInfo(customers, request);
                UpdateContactInfo(customers, request, updatedAttributes);

                return customers;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating customer information: {ex.Message}", ex);
            }
        }

        public static CustomerSearch MapCustomerSearchDeatilsUpdate(CustomerSearch customersearch, CustomerPersonalInfoUpdateCommand request)
        {
            if (customersearch == null || request == null)
                throw new ArgumentNullException(customersearch == null ? nameof(CustomerSearch) : nameof(request));
            try
            {
                UpdateBasicSearchInfo(customersearch, request);
                return customersearch;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating customer information: {ex.Message}", ex);
            }
        }

        public static FlyingClubMember MapFlyingClubMemberDetailsUpdate(FlyingClubMember flyingClubMemberDoc, CustomerPersonalInfoUpdateCommand request)
        {
            if (flyingClubMemberDoc == null || request == null)
                throw new ArgumentNullException(flyingClubMemberDoc == null ? nameof(CustomerSearch) : nameof(request));
            try
            {
                flyingClubMemberDoc.NameOnCard = $"{request.FirstName} {request.LastName}";
                flyingClubMemberDoc.MemberShipCardDetails.NameOnCard = $"{request.FirstName}";
                flyingClubMemberDoc.LastUpdatedOn =  DateTime.UtcNow;
                flyingClubMemberDoc.LastUpdatedBy = request.LastUpdatedBy;

                return flyingClubMemberDoc;
            }

            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating customer information: {ex.Message}", ex);
            }
        }
        
        private static void UpdateBasicInfo(Customers customers, CustomerPersonalInfoUpdateCommand request)
        {
            customers.LastUpdatedBy = request.LastUpdatedBy;
            customers.FirstName = request.FirstName;
            customers.LastName = request.LastName;
            customers.DateOfBirth = request.DateOfBirth;
            customers.PNRName = $"{request.LastName.ToUpper()}/{request.FirstName.ToUpper()}";
            customers.EnvelopeSalutation = string.IsNullOrEmpty(customers.Title) ? $"{request.FirstName} {request.LastName}" : $"{customers.Title} {request.FirstName} {request.LastName}";
            customers.FormalSalutation = string.IsNullOrEmpty(customers.Title) ? $"Dear {request.LastName}" : $"Dear {customers.Title} {request.LastName}";
            customers.InformalSalutation = $"Dear {request.FirstName}";
            customers.LastUpdatedOn = DateTime.UtcNow.ToString();
        }

        private static void UpdateBasicSearchInfo(CustomerSearch customers, CustomerPersonalInfoUpdateCommand request)
        {
            customers.FirstName = request.FirstName;
            customers.LastName = request.LastName;
            customers.DateOfBirth = Convert.ToDateTime(request.DateOfBirth);
            customers.EmailAddress = request.EmailId;
        }

        private static void UpdateContactInfo(Customers customers, CustomerPersonalInfoUpdateCommand request, List<VacManageAccountUpdateAttributes> updatedAttributes)
        {
            // Update Email
            if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.Email))
                VaCustomerDocContactDetailsMapper.UpdateEmailDataMapperCustomer(customers, request);

            // Update Phone
            if (updatedAttributes.Contains(VacManageAccountUpdateAttributes.PhoneNumber))
                VaCustomerDocContactDetailsMapper.UpdatePhoneDataMapperCustomer(customers, request);
        }
    }
}

