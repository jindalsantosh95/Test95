using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Services;

public sealed class YoungFlyerAccountRequestFactory(IReferenceDataProvider referenceDataProvider = null)
{
    private const string DummyEmailDomainConfigurationKey = "DummyEmailDomain";
    private const string DefaultDummyEmailDomain = "U18.virgin.com";
    private readonly IReferenceDataProvider _referenceDataProvider = referenceDataProvider;
    private readonly SemaphoreSlim _countryCacheLock = new(1, 1);
    private IReadOnlyList<CountryRefData>? _countryDetails;

    public async Task<string> ApplyManagedYoungFlyerDetailsAsync(CustomerAccountDetails youngFlyer, CustomerCustMS parent)
    {
        ArgumentNullException.ThrowIfNull(youngFlyer);
        ArgumentNullException.ThrowIfNull(parent);

        var dummyEmail = $"{Guid.NewGuid().ToString("N")}@{ConfigReader.Try(DummyEmailDomainConfigurationKey, DefaultDummyEmailDomain)}";
        youngFlyer.Password = null;
        youngFlyer.AddressDetails ??= [];
        youngFlyer.AddressDetails.Clear();
        if (parent.AddressDetails is not null)
        {
            foreach (var address in parent.AddressDetails)
            {
                youngFlyer.AddressDetails.Add(new AccountAddressDetail
                {
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    AddressLine3 = address.AddressLine3,
                    CompanyName = address.CompanyName,
                    City = address.City,
                    PostalCode = address.PostalCode,
                    Region = address.Region,
                    Country = await ResolveIataCountryCodeAsync(address.Country),
                    PreferredFlag = address.PreferredFlag,
                    AddressVerified = address.AddressVerified.Is("Y"),
                });
            }
        }
        youngFlyer.ContactDetails = new AccountContactDetails
        {
            PhoneDetails = parent.ContactDetails?.PhoneDetails?.Select(phone => new AccountPhoneDetail
            {
                PhoneType = Enum.TryParse<AccountPhoneDetail.AccountPhoneTypes>(phone.PhoneType, true, out var phoneType) ? phoneType : null,
                Usage = Enum.TryParse<AccountPhoneDetail.AccountPhoneUsages>(phone.Usage, true, out var usage) ? usage : null,
                CountryCode = phone.CountryCode,
                PhoneNumber = phone.PhoneNumber,
                PreferredFlag = phone.PreferredFlag,
                Description = phone.Description
            }).ToList() ?? [],
            EmailDetails = [new AccountEmailDetail
            {
                EmailId = dummyEmail,
                PreferredFlag = "Y",
                Description = "Managed Young Flyer account",
                Valid = "N",
                Usage = AccountEmailDetail.AccountEmailUsages.Personal
            }]
        };
        youngFlyer.LoyaltyMember ??= new AccountLoyaltyMember();

        var preferredParentPhone = parent.ContactDetails?.PhoneDetails?.FirstOrDefault(phone => phone.PreferredFlag.Is("Y"));
        var preferredParentEmail = parent.ContactDetails?.EmailDetails?.FirstOrDefault(email => email.PreferredFlag.Is("Y"));

        youngFlyer.LoyaltyMember.GuardianDetails = new AccountGuardianDetails
        {
            Name = $"{parent.FirstName} {parent.LastName}",
            DateOfBirth = parent.DateOfBirth.ToString("yyyy-MM-dd"),
            CardNumber = parent.LoyaltyNumber,
            EmailDetails = preferredParentEmail?.emailId.IsNullOrWhiteSpace() == false
                ? new AccountEmailDetail
                {
                    EmailId = preferredParentEmail.emailId,
                    PreferredFlag = "Y",
                    Usage = AccountEmailDetail.AccountEmailUsages.Personal
                }
                : null,
            GuardianTermsAndConditionsAccepted = true,
            PhoneDetails = preferredParentPhone?.PhoneNumber.IsNullOrWhiteSpace() == false
                ? new AccountPhoneDetail
                {
                    PhoneType = Enum.TryParse<AccountPhoneDetail.AccountPhoneTypes>(preferredParentPhone.PhoneType, true, out var phoneType) ? phoneType : null,
                    Usage = Enum.TryParse<AccountPhoneDetail.AccountPhoneUsages>(preferredParentPhone.Usage, true, out var usage) ? usage : null,
                    CountryCode = preferredParentPhone.CountryCode,
                    PhoneNumber = preferredParentPhone.PhoneNumber,
                    PreferredFlag = "Y",
                    Description = preferredParentPhone.Description
                }
                : null
        };

        return dummyEmail;
    }

    private async Task<string> ResolveIataCountryCodeAsync(string country)
    {
        if (_referenceDataProvider is null || country.IsNullOrWhiteSpace())
            return country;

        var countryDetails = _countryDetails;
        if (countryDetails is null)
        {
            await _countryCacheLock.WaitAsync();
            try
            {
                countryDetails = _countryDetails;
                if (countryDetails is null)
                {
                    var retrievedCountries = await _referenceDataProvider.RetrieveCountriesAsync();
                    if (retrievedCountries is null)
                        return country;

                    countryDetails = [.. retrievedCountries];
                    _countryDetails = countryDetails;
                }
            }
            finally
            {
                _countryCacheLock.Release();
            }
        }

        return countryDetails.FirstOrDefault(x => x.CountryName.Is(country))?.IATACountryCode ?? country;
    }
}
