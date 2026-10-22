using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.CustomerSearch
{
    public class CustomerToCustomerProfileDataMapper
    {
        public static CustomerProfileAuditDTO Mapper(CustomerProfileAuditDTO dto, Customers customer)
        {
            dto.FirstName = customer.FirstName ?? string.Empty;
            dto.LastName = customer.LastName ?? string.Empty;
            dto.MiddleName = customer.MiddleName ?? string.Empty;
            dto.Title = customer.Title ?? string.Empty;
            dto.DoB = Convert.ToDateTime(customer.DateOfBirth);
            dto.Gender = customer.Gender ?? string.Empty;
            dto.CustomerProfileUpdatedOn = Convert.ToDateTime(DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat));
            dto.Status = customer.Status ?? string.Empty;
            dto.NameChangeReason = customer.NameChangeReason ?? string.Empty;
            dto.AuthorisedUser = customer.Authorizations?.FirstOrDefault(x => x.RelationType.ToLower() == Constants.Application.RelationTypeForAuthorizedUserAcc).Name ?? string.Empty;

            return dto;
        }
    }
}
