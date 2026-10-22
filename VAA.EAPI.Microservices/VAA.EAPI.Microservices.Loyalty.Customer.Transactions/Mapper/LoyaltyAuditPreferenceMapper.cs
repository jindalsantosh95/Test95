using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public static class LoyaltyAuditPreferenceMapper
    {
        public static PreferenceAuditData MaployaltyAuditPreferenceMapper(CustomerProfileCommand request, CustomerProfileDTO customerProfileDocResponse)
        {
            var prefs = customerProfileDocResponse.Preferences ?? [];    
            var preferenceAuditDocument = new PreferenceAuditDocument()
            {
                CustomerId = request.CustomerId ?? customerProfileDocResponse.CustomerId,
                PreferenceUpdatedBy = !string.IsNullOrEmpty(request.LastUpdatedBy) ? request.LastUpdatedBy : Constants.Application.Channel,
                PreferenceUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                DoNotCall = prefs.Any(x => x.PreferenceType == Constants.Application.DoNotCall) ? prefs.First(x => x.PreferenceType == Constants.Application.DoNotCall).PreferenceValue : Constants.Application.preferenceDisabled,
                DoNotSMS = prefs.Any(x => x.PreferenceType == Constants.Application.DoNotSMS) ? prefs.First(x => x.PreferenceType == Constants.Application.DoNotSMS).PreferenceValue : Constants.Application.preferenceDisabled,
                DoNotEmail = prefs.Any(x => x.PreferenceType == Constants.Application.DoNotEmail) ? prefs.First(x => x.PreferenceType == Constants.Application.DoNotEmail).PreferenceValue : Constants.Application.preferenceDisabled,
                VAMarketingPreference = prefs.FirstOrDefault(x => x.PreferenceType == Constants.Application.VAAMarketingPermissions)?.PreferenceValue,
                PreferredPartnerFFP = prefs.FirstOrDefault(x => x.PreferenceType == Domain.CustomerProfile.PreferenceTypes.PreferredPartnerFFP.ToString())?.PreferenceValue,
            };

            return new PreferenceAuditData()
            {
                preferenceAuditDocument = preferenceAuditDocument
            };

        }

    }
}
