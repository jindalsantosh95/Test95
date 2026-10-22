using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class CustomerProfileUpdateReferenceMapper
    {
        public static CustomerProfileDTO UpdatePreferenceMapper(CustomerProfile customerProfileResponse, CustomerProfileCommand request)
        {

            CustomerProfileDTO customer = new();

            if (customerProfileResponse.Preferences != null)
            {
                customer.Preferences = GetPreference(customerProfileResponse.Preferences, request).ToArray();
                customer.LastUpdatedOn = DateTime.UtcNow;
                customer.LastUpdatedBy = request.LastUpdatedBy;
                customer.CreatedBy = customerProfileResponse.CreatedBy;
                customer.Brand = customerProfileResponse.Brand;
                customer.Id = customerProfileResponse.Id;
                customer.CustomerId = customerProfileResponse.CustomerId;
                customer.CreatedOn = customerProfileResponse.CreatedOn;
                customer.DocType = ConfigReader.Get(Constants.EnvironmentSettings.CustomerProfileDocType);
                customer.SchemaVersion = customerProfileResponse.SchemaVersion;
                customer.SourceIdentifier = customerProfileResponse.SourceIdentifier;
                customer.SourceSystem = customerProfileResponse.SourceSystem;
                customer.PartitionKey = customerProfileResponse.CustomerId;

            }
            return customer;
        }

        private static IEnumerable<Preference> GetPreference(Preference[] preferences, CustomerProfileCommand request)
        {
            return preferences.Select(p => new Preference()
            {
                Active = p.Active,
                CaptureCannel = p.CaptureCannel,
                LastUpdatedBy = p.PreferenceType.Is(request.Preference?.FirstOrDefault().PreferenceType.ToString()) ? request.LastUpdatedBy : p.LastUpdatedBy,
                LastUpdatedOn = p.PreferenceType.Is(request.Preference?.FirstOrDefault().PreferenceType.ToString()) ? DateTime.UtcNow : p.LastUpdatedOn,
                Persona = p.Persona,
                SourceIdentifier = p.SourceIdentifier,
                SourceSystem = p.SourceSystem,
                CreatedBy = p.CreatedBy,
                CreatedOn = p.CreatedOn,
                PreferenceId = p.PreferenceId,
                PreferenceType = p.PreferenceType,
                PreferenceValue = p.PreferenceType.Is(request.Preference?.FirstOrDefault().PreferenceType.ToString()) ? GetPreferenceValue(request.Preference) : p.PreferenceValue,

            });

        }

        private static string GetPreferenceValue(IEnumerable<Domain.ManageAccount.Preference> preferences)
        {
            var preferenceValue = string.Empty;
            if (preferences?.FirstOrDefault().PreferenceValue == Constants.Application.preferenceEnabled)
            {
                preferenceValue = Constants.Application.preferenceEnabled;
            }
            else
            {
                preferenceValue = Constants.Application.preferenceDisabled;
            }
            return preferenceValue;

        }
    }
}
