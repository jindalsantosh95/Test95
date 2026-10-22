using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount
{
    public class CustomerProfileCommand : CloudEvent, IRequest<UpdateMarketingPreferenceResponse>
    {
        public string CustomerId { get; set; }
        public string FcNumber { get; set; }
        public string DocType { get; set; }
        public IEnumerable<Preference> Preference { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string TierCode { get; set; }
        public string LastUpdatedBy { get; set; }

        public static explicit operator CustomerProfileCommand(UpdateCustomerProfilePreference updateCustomerProfilePreference)
        {
            return new()
            {
                CustomerId = updateCustomerProfilePreference.CustomerId,
                FcNumber = updateCustomerProfilePreference.FcNumber,
                Preference = updateCustomerProfilePreference.preference,
                EmailAddress = updateCustomerProfilePreference.EmailAddress,
                FirstName = updateCustomerProfilePreference?.FirstName,
                TierCode = updateCustomerProfilePreference?.TierCode,
                LastUpdatedBy = updateCustomerProfilePreference?.LastUpdatedBy
            };
        }
    }
}
