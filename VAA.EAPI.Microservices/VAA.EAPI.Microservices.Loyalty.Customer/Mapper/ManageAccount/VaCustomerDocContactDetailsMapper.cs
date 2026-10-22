using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class VaCustomerDocContactDetailsMapper
    {
        public static Customers UpdateEmailDataMapperCustomer(Customers customerDocument, CustomerPersonalInfoUpdateCommand request)
        {
            Contact updateEmail = customerDocument.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y");

            updateEmail.ContactValue = request?.EmailId;
            updateEmail.LastUpdatedOn = DateTime.UtcNow.ToString();
            updateEmail.LastUpdatedBy = request.LastUpdatedBy;
            updateEmail.Description = request.LastUpdatedBy.Is(Constants.Application.WebMyaChannelID) ? Constants.Application.WebEmailUpdateDescription : Constants.Application.PowerAppUpdateDescription;

            return customerDocument;
        }

        public static Customers UpdatePhoneDataMapperCustomer(Customers customerDocument, CustomerPersonalInfoUpdateCommand request)
        {
            Contact updatePhone = customerDocument.ContactDetails.First(x => x.ContactType == Constants.Application.PhoneContactType && x.Primary == "Y");

            updatePhone.ContactValue = request.PhoneNumber;
            updatePhone.LastUpdatedOn = DateTime.UtcNow.ToString();
            updatePhone.LastUpdatedBy = request.LastUpdatedBy;
            updatePhone.Description = request.LastUpdatedBy.Is(Constants.Application.WebMyaChannelID) ? Constants.Application.WebEmailUpdateDescription : Constants.Application.PowerAppUpdateDescription;
            updatePhone.CountryCode = request.CountryCode.TrimStart('+');

            return customerDocument;

        }
    }
}
