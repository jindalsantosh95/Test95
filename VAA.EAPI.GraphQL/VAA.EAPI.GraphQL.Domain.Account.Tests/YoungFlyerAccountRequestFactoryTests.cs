using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class YoungFlyerAccountRequestFactoryTests
{
    [Fact]
    public async Task ApplyManagedChildDetailsAsync_CreatesNonCommunicationsDummyEmailAndPreservesParentAddress()
    {
        ConfigReader.Set("DummyEmailDomain", "U18.virgin.com");
        using var serviceProvider = new ServiceCollection()
            .AddSingleton<YoungFlyerAccountRequestFactory>()
            .BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<YoungFlyerAccountRequestFactory>();
        var child = new CustomerAccountDetails { Password = "must-not-be-used" };
        var parent = new CustomerCustMS
        {
            AddressDetails = [new AddressCustMS
            {
                AddressLine1 = "1 Test Street",
                City = "London",
                Country = "GB",
                PreferredFlag = "Y",
                AddressVerified = "Y"
            }],
            ContactDetails = new Contact
            {
                PhoneDetails = [new Phone
                {
                    PhoneType = "Mobile",
                    Usage = "Personal",
                    CountryCode = "44",
                    PhoneNumber = "7700900123",
                    PreferredFlag = "Y",
                    Description = "Parent mobile"
                }]
            },
            FirstName = "Parent",
            LastName = "Example",
            LoyaltyNumber = "1234567890",
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        var dummyEmail = await factory.ApplyManagedYoungFlyerDetailsAsync(child, parent);

        var localPart = dummyEmail.Split('@')[0];
        Assert.Equal(32, localPart.Length);
        Assert.DoesNotContain('-', localPart);
        Assert.All(localPart, character => Assert.True(Uri.IsHexDigit(character)));
        Assert.Equal("U18.virgin.com", dummyEmail.Split('@')[1]);
        Assert.Null(child.Password);
        Assert.Single(child.ContactDetails!.EmailDetails!);
        Assert.Equal(dummyEmail, child.ContactDetails.EmailDetails![0].EmailId);
        Assert.Equal("N", child.ContactDetails.EmailDetails[0].Valid);
        Assert.Equal("Parent Example", child.LoyaltyMember!.GuardianDetails!.Name);
        Assert.Equal("1234567890", child.LoyaltyMember.GuardianDetails.CardNumber);
        Assert.Single(child.AddressDetails!);
        Assert.Equal("1 Test Street", child.AddressDetails![0].AddressLine1);
        var childPhone = Assert.Single(child.ContactDetails.PhoneDetails!);
        Assert.Equal("Mobile", childPhone.PhoneType?.ToString());
        Assert.Equal("Personal", childPhone.Usage?.ToString());
        Assert.Equal("44", childPhone.CountryCode);
        Assert.Equal("7700900123", childPhone.PhoneNumber);
        Assert.Equal("Y", childPhone.PreferredFlag);
    }

    [Fact]
    public async Task ApplyManagedChildDetailsAsync_UsesConfiguredDummyEmailDomain()
    {
        ConfigReader.Set("DummyEmailDomain", "managed-child.example");
        var factory = new YoungFlyerAccountRequestFactory();
        var child = new CustomerAccountDetails();
        var parent = new CustomerCustMS
        {
            FirstName = "Parent",
            LastName = "Example",
            LoyaltyNumber = "1234567890",
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        var dummyEmail = await factory.ApplyManagedYoungFlyerDetailsAsync(child, parent);

        Assert.EndsWith("@managed-child.example", dummyEmail);
        Assert.Equal(dummyEmail, child.ContactDetails!.EmailDetails![0].EmailId);
        Assert.Equal("N", child.ContactDetails.EmailDetails[0].Valid);
        Assert.Null(child.LoyaltyMember!.GuardianDetails!.EmailDetails);
        Assert.Null(child.LoyaltyMember.GuardianDetails.PhoneDetails);
    }

    [Fact]
    public async Task ApplyManagedChildDetailsAsync_UsesEmptyAddressList_WhenParentHasNoAddresses()
    {
        var child = new CustomerAccountDetails();
        var parent = new CustomerCustMS
        {
            FirstName = "Parent",
            LastName = "Example",
            LoyaltyNumber = "1234567890",
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        await new YoungFlyerAccountRequestFactory().ApplyManagedYoungFlyerDetailsAsync(child, parent);

        Assert.Empty(child.AddressDetails!);
    }

    [Fact]
    public async Task ApplyManagedChildDetailsAsync_CachesCountriesAcrossAddressesAndInvocations()
    {
        var provider = new CountingReferenceDataProvider(
        [
            new CountryRefData { CountryName = "United Kingdom", IATACountryCode = "GB" }
        ]);
        var factory = new YoungFlyerAccountRequestFactory(provider);
        var parent = new CustomerCustMS
        {
            AddressDetails =
            [
                new AddressCustMS { Country = "United Kingdom" },
                new AddressCustMS { Country = "United Kingdom" }
            ],
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        await factory.ApplyManagedYoungFlyerDetailsAsync(new CustomerAccountDetails(), parent);
        await factory.ApplyManagedYoungFlyerDetailsAsync(new CustomerAccountDetails(), parent);

        Assert.Equal(1, provider.CallCount);
    }

    private sealed class CountingReferenceDataProvider(IEnumerable<CountryRefData> countries) : IReferenceDataProvider
    {
        private readonly IEnumerable<CountryRefData> _countries = countries;
        public int CallCount { get; private set; }

        public Task<IEnumerable<CountryRefData>> RetrieveCountriesAsync()
        {
            CallCount++;
            return Task.FromResult(_countries);
        }
    }
}
