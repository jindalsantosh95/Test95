using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class RegisterCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IMediator _mediator;

        public RegisterCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<IServiceBusTopicMessageEventCommandHandler>(sp => sp.GetRequiredService<ServiceBusTopicMessageEventCommandHandler>())
                .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>()
                .AddMediatR(config => config.RegisterServicesFromAssemblyContaining<RegisterCommand>())
                .BuildServiceProvider();

            _mediator = serviceProvider.GetRequiredService<IMediator>();
        }

        [Fact]
        public async Task Handle_Should_NormalizeKnownGenderValues_ToSingleLetter()
        {
            // Arrange
            var registerCommand = new RegisterCommand
            {
                CustomerDetails = new CustomerAccountDetails
                {
                    Gender = "Female",
                    FcNumber = "FC123",
                    ContactDetails = new AccountContactDetails
                    {
                        EmailDetails = new System.Collections.Generic.List<AccountEmailDetail>
                        {
                            new AccountEmailDetail { EmailId = "test@domain.com" }
                        }
                    },
                    AddressDetails = new System.Collections.Generic.List<AccountAddressDetail>
                    {
                        new AccountAddressDetail { Country = "GB" }
                    },
                    LoyaltyMember = new AccountLoyaltyMember { MemberType = AccountLoyaltyMember.AccountMemberTypes.Individual }
                }
            };

            // Act
            await _mediator.Send(registerCommand, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("F", registerCommand.CustomerDetails.Gender);
        }

        [Fact]
        public async Task Handle_Should_NotAlter_UnknownGenderValue()
        {
            // Arrange
            var originalGender = "NonBinary";
            var registerCommand = new RegisterCommand
            {
                CustomerDetails = new CustomerAccountDetails
                {
                    Gender = originalGender,
                    FcNumber = "FC123",
                    ContactDetails = new AccountContactDetails
                    {
                        EmailDetails = new System.Collections.Generic.List<AccountEmailDetail>
                        {
                            new AccountEmailDetail { EmailId = "test@domain.com" }
                        }
                    },
                    AddressDetails = new System.Collections.Generic.List<AccountAddressDetail>
                    {
                        new AccountAddressDetail { Country = "GB" }
                    },
                    LoyaltyMember = new AccountLoyaltyMember { MemberType = AccountLoyaltyMember.AccountMemberTypes.Individual }
                }
            };

            // Act
            await _mediator.Send(registerCommand, TestContext.Current.CancellationToken);

            Assert.Equal(originalGender, registerCommand.CustomerDetails.Gender);
        }

        [Fact]
        public async Task Handle_Should_Cleanup_PhoneCountryCode_And_Default_Preferred_AddressType()
        {
            // Arrange
            var phone = new AccountPhoneDetail
            {
                CountryCode = "  +44 ",
                PhoneNumber = "07123456789",
                PreferredFlag = "Y"
            };

            var addr = new AccountAddressDetail
            {
                AddressType = null,
                PreferredFlag = "Y",
                Country = "GB",
                PostalCode = "SW1A1AA"
            };

            var registerCommand = new RegisterCommand
            {
                CustomerDetails = new CustomerAccountDetails
                {
                    Gender = null,
                    FcNumber = "FC123",
                    ContactDetails = new AccountContactDetails
                    {
                        EmailDetails = new System.Collections.Generic.List<AccountEmailDetail>
                        {
                            new AccountEmailDetail { EmailId = "test@domain.com" }
                        },
                        PhoneDetails = new System.Collections.Generic.List<AccountPhoneDetail> { phone }
                    },
                    AddressDetails = new System.Collections.Generic.List<AccountAddressDetail> { addr },
                    LoyaltyMember = new AccountLoyaltyMember { MemberType = AccountLoyaltyMember.AccountMemberTypes.Individual }
                }
            };

            // Act
            await _mediator.Send(registerCommand, TestContext.Current.CancellationToken);

            // Assert - phone country code trimmed and '+' removed
            Assert.Equal("44", registerCommand.CustomerDetails.ContactDetails.PhoneDetails[0].CountryCode);

            // Assert - preferred address without type defaulted to Home
            Assert.Equal(AccountAddressDetail.AccountAddressTypes.Home, registerCommand.CustomerDetails.AddressDetails[0].AddressType);
        }
    }
}
