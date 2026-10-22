using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class CustomerDocumentUpdateMapper
    {
        public static Customers UpdateEmailDataMapperCustomer(Customers customerDocument, string newEmaiID)
        {
            Contact updateEmail = customerDocument.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y");

            customerDocument.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            customerDocument.LastUpdatedOn = DateTime.UtcNow.ToString();
            updateEmail.ContactValue = newEmaiID;
            updateEmail.LastUpdatedOn = DateTime.UtcNow.ToString();
            updateEmail.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            updateEmail.Description = Constants.Application.WebEmailUpdateDescription;

            return customerDocument;
        }

        public static Customers UpdateCustomerAddresMapper(Customers customerDoc, AccountAddressDetail ad)
        {
            Address address = customerDoc.AddressDetails.First(x => x.Primary == "Y");

            address.AddressType = ad?.AddressType.ToString() ?? string.Empty;
            address.AddressLine1 = ad?.AddressLine1;
            address.AddressLine2 = ad?.AddressLine2;
            address.AddressLine3 = ad?.AddressLine3;
            address.AddressVerified = ad.AddressVerified == true ? "Y" : "N";
            address.LandlordOrCompanyName = ad?.CompanyName;
            address.City = ad?.City;
            address.PostCode = ad?.PostalCode.Replace("-", string.Empty);
            address.Province = ad?.Region;
            address.Country = ad.Country;
            address.Primary = ad?.PreferredFlag;
            address.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            address.LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat);

            customerDoc.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            customerDoc.LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat);

            return customerDoc;
        }


        public static CustomerSearch UpdateCustomerSearchAddresMapper(CustomerSearch customerDoc, AccountAddressDetail ad)
        {

            customerDoc.Province = ad?.Region;
            customerDoc.PostCode = ad?.PostalCode.Replace("-", string.Empty);
            customerDoc.Country = ad?.Country;

            return customerDoc;
        }

        public static Customers UpdatePhoneDataMapperCustomer(Customers customerDocument, UpdatePhoneNumberCommand updatePhoneNumberCommand)
        {
            Contact updateEmail = customerDocument.ContactDetails.First(x => x.ContactType == Constants.Application.PhoneContactType && x.Primary == "Y");

            customerDocument.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            customerDocument.LastUpdatedOn = DateTime.UtcNow.ToString();
            updateEmail.ContactValue = updatePhoneNumberCommand.PhoneNumber;
            updateEmail.LastUpdatedOn = DateTime.UtcNow.ToString();
            updateEmail.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            updateEmail.Description = Constants.Application.WebEmailUpdateDescription;
            updateEmail.CountryCode = updatePhoneNumberCommand.CountryCode;
            return customerDocument;

        }

        public static Customers BulkUpdateMemberStatusMapper(Customers customerDocument, string status)
        {
            customerDocument.LastUpdatedBy = Constants.Application.WebMyaChannelID;
            customerDocument.LastUpdatedOn = DateTime.UtcNow.ToString();
            customerDocument.Status = status;

            return customerDocument;
        }
    }
}
