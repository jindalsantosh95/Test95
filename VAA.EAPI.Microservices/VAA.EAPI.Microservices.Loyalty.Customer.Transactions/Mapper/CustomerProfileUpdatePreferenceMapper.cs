using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class CustomerProfileUpdatePreferenceMapper
    {
        public static CustomerProfileDTO UpdatePreferenceMapper(CustomerProfile customerProfileResponse, CustomerProfileCommand request)
        {
            var updatedPreferences = MergePreferences(customerProfileResponse.Preferences, request);

            return new CustomerProfileDTO
            {
                Preferences = updatedPreferences,
                LastUpdatedOn = DateTime.UtcNow,
                LastUpdatedBy = request.LastUpdatedBy,
                CreatedBy = customerProfileResponse.CreatedBy,
                Brand = customerProfileResponse.Brand,
                Id = customerProfileResponse.Id,
                CustomerId = customerProfileResponse.CustomerId,
                CreatedOn = customerProfileResponse.CreatedOn,
                DocType = ConfigReader.Get(Constants.EnvironmentSettings.CustomerProfileDocType),
                SchemaVersion = customerProfileResponse.SchemaVersion,
                SourceIdentifier = customerProfileResponse.SourceIdentifier,
                SourceSystem = customerProfileResponse.SourceSystem,
                PartitionKey = customerProfileResponse.CustomerId
            };
        }

        private static Preference[] MergePreferences(Preference[] existingPreferences, CustomerProfileCommand request)
        {
            var requestedPreferences = request.Preference;

            if (existingPreferences == null || existingPreferences.Length == 0)
            {
                return [.. requestedPreferences
                    .Select(rp => new Preference
                    {
                        PreferenceType = rp.PreferenceType.ToString(),
                        PreferenceValue = GetPreferenceValue(rp),
                        LastUpdatedBy = request.LastUpdatedBy,
                        LastUpdatedOn = DateTime.UtcNow,
                        CreatedBy = request.LastUpdatedBy,
                        CreatedOn = DateTime.UtcNow,
                        PreferenceId = Utility.GetId(),
                        Persona = "Default",
                        Active = "Y",
                    })];
            }

            var updatedPreferences = existingPreferences
            .Select(pref =>
            {
                var requestedPref = requestedPreferences.FirstOrDefault(rp => rp.PreferenceType.ToString() == pref.PreferenceType);
                return new Preference
                {
                    PreferenceType = pref.PreferenceType,
                    PreferenceValue = requestedPref != null ? GetPreferenceValue(requestedPref) : pref.PreferenceValue,
                    LastUpdatedBy = requestedPref != null ? request.LastUpdatedBy : pref.LastUpdatedBy,
                    LastUpdatedOn = requestedPref != null ? DateTime.UtcNow : pref.LastUpdatedOn,
                    CreatedBy = pref.CreatedBy,
                    CreatedOn = pref.CreatedOn,
                    PreferenceId = pref.PreferenceId,
                    Persona = pref.Persona,
                    Active = pref.Active
                };
            })
            .ToList();

            var newPreferences = requestedPreferences
                .Where(rp => !existingPreferences.Any(ep => ep.PreferenceType == rp.PreferenceType.ToString()))
                .Select(rp => new Preference
                {
                    PreferenceType = rp.PreferenceType.ToString(),
                    PreferenceValue = GetPreferenceValue(rp),
                    LastUpdatedBy = request.LastUpdatedBy,
                    LastUpdatedOn = DateTime.UtcNow,
                    CreatedBy = request.LastUpdatedBy,
                    CreatedOn = DateTime.UtcNow,
                    PreferenceId = Utility.GetId(),
                    Persona = "Default",
                    Active = "Y",
                });

            return [.. updatedPreferences, .. newPreferences];
        }

        private static string GetPreferenceValue(Domain.ManageAccount.Preference preference)
        {
            return preference.PreferenceType == Domain.ManageAccount.PreferenceTypes.PreferredPartnerFFP
                ? preference.PreferenceValue
                : (preference.PreferenceValue == "Y" ? "Y" : "N");
        }
    }
}