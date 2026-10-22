using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using static VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer.AccountPreference;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class VredMarketingPreferenceEventMapper
    {
        public static CloudEvent EventDataMapper(string eventId, CustomerAccountDetails customerDetails)
        {
            if (customerDetails == null) throw new ArgumentNullException(nameof(customerDetails));

            return new CloudEvent
            {
                Id = eventId,
                Type = Constants.Application.VredMarketinPrefEventSubject,
                Data = MapmarketingPrefernceData(customerDetails),
                Subject = Constants.Application.VredMarketinPrefEventSubject,
                Source = ConfigReader.Get(Environment.CustomerSyncEndPoint),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecVersion
            };
        }

        public static ConsentReceiptRequest MapmarketingPrefernceData(CustomerAccountDetails customerDetails)
        {
            return new ConsentReceiptRequest
            {
                Emailid = customerDetails.ContactDetails?.EmailDetails?.First()?.EmailId,
                FirstName= customerDetails?.FirstName,
                LastName= customerDetails?.LastName,
                CountryCode= customerDetails?.AddressDetails?.First()?.Country,
                DateOfBirth= customerDetails?.DateOfBirth,
                LoyaltyMemberNumber = customerDetails?.LoyaltyMember.FlyingClubCardNumber,                
                CustomerId =customerDetails?.CustomerId,
                MarketingPreference = customerDetails?.Preferences?.First(p => p.PreferenceType == AccountPreferenceTypes.VaGroupMarketingPermissions)?.PreferenceValue
            };
        }
    }
}

