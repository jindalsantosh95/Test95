using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Refunds.Application;
using VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Refunds.Tests;

public sealed class RefundTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public RefundTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {

        ConfigReader.Set("EMailNotificationServiceBusConnection", "https://tempuri.org/reference");

        var azureServiceBusFactory = new MessageBusFactory();


        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<IDistributedCache>(DistributedCache.Instance)
                                                  .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                  .AddSingleton<ID365RefundGateway, D365RefundGateway>()
                                                  .AddSingleton<IRefundNotificationPublisher, RefundNotificationPublisher>()
                                                  .AddSingleton<IRefundStatusCommandHandler, RefundStatusCommandHandler>()
                                                  .AddSingleton<ISubmitRefundCommandHandler, SubmitRefundCommandHandler>()
                                                  .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                                                  .BuildServiceProvider();

        AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
    }

    [Fact]
    public async Task TestRefundStatusCommandHandler()
    {
        var handler = _serviceProvider.GetRequiredService<IRefundStatusCommandHandler>();
        var requestForm = new Models.Refunds.RefundRequestForm()
        {
            Entries = new List<RefundRequestItem>
            {
                new RefundRequestItem {
                    PassengerRef = "01.01",
                    PassengerName = "John Doe",
                    FlightDate = DateOnly.Parse("2026-03-01"),
                    FlightNumber = 1,
                    TicketNumber = "1234567890",
                    CouponNumber = "1"
                },
                new RefundRequestItem {
                    PassengerRef = "01.01",
                    PassengerName = "John Doe",
                    FlightDate = DateOnly.Parse("2026-03-05"),
                    FlightNumber = 2,
                    TicketNumber = "1234567890",
                    CouponNumber = "2"
                },
                new RefundRequestItem {
                    PassengerRef = "02.01",
                    PassengerName = "Jane Doe",
                    FlightDate = DateOnly.Parse("2026-03-01"),
                    FlightNumber = 1,
                    TicketNumber = "0987654321",
                    CouponNumber = "1"
                },
                new RefundRequestItem {
                    PassengerRef = "02.01",
                    PassengerName = "John Doe",
                    FlightDate = DateOnly.Parse("2026-03-05"),
                    FlightNumber = 2,
                    TicketNumber = "0987654321",
                    CouponNumber = "2"
                }
            }
        };

        var result = await handler.HandleAsync(requestForm, CancellationToken.None);

        // Assert that we get a result back for each item in the request
        Assert.Equal(requestForm.Entries.Count, result.Entries.Count);

        // And for the items with FlightNumber 1, we should have a null or whitespace claim number
        Assert.All(result.Entries.Where(x => x.FlightNumber == 1), item => Assert.True(string.IsNullOrWhiteSpace(item.ClaimNumber)));

        // As per the mocked response in D365RefundGateway, only the items with FlightNumber 2 will have a claim number returned, so we can assert against that
        Assert.All(result.Entries.Where(x => x.FlightNumber == 2), item => Assert.False(string.IsNullOrWhiteSpace(item.ClaimNumber)));

    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(2, "2")]
    public async Task TestSubmitRefund(int fltNo, string coupNo)
    {
        var handler = _serviceProvider.GetRequiredService<ISubmitRefundCommandHandler>();

        /*
        var messageBus = (MessageBus)_serviceProvider.GetRequiredService<IMessageBus>();

        CloudEvent? message = default;
        messageBus.OnMessage += (m) => message = m; // capture message sent to service bus for validation
        */


        var request = new Models.Refunds.RefundRequestForm
        {
            RequestId = Guid.NewGuid().ToString(),
            Requester = new Models.Refunds.RefundRequestRequester
            {
                FirstName = "John",
                LastName = "Doe",
                BookingReference = "ABC123",
                Email = "john.doe@example.com",
                BookingDate = DateOnly.Parse("2026-01-01"),
                Reason = "Test refund submission",
            },
            Entries = new List<RefundRequestItem>
            {
                new Models.Refunds.RefundRequestItem {
                    PassengerRef = "01.01",
                    PassengerName = "John Doe",
                    FlightDate = DateOnly.Parse("2026-03-01"),
                    FlightNumber = fltNo,
                    TicketNumber = "9321234567890",
                    CouponNumber = coupNo
                },
                new Models.Refunds.RefundRequestItem {
                    PassengerRef = "02.01",
                    PassengerName = "Jane Doe",
                    FlightDate = DateOnly.Parse("2026-03-01"),
                    FlightNumber = fltNo,
                    TicketNumber = "9321234567891",
                    CouponNumber = coupNo
                }
            }
        };

        if (fltNo == 1)
        {
            // For flight number 1, we expect the submission to succeed and a claim number to be returned, as per the mocked response in D365RefundGateway
            var result = await handler.HandleAsync(request, CancellationToken.None);

            Assert.False(result.ClaimNumber.IsNullOrWhiteSpace());
            /*
            await Task.Delay(100); // allow some time for the message to be published to the bus    

            Assert.NotNull(message);

            Assert.Equal(Constants.Application.ServiceBusSource, message!.Source);
            Assert.Equal(Constants.Application.RefundRequestEmailNotificationSubject, message.Subject);

            Assert.NotNull(message.Data);

            JObject data = Json.Deserialize<JObject>(message.Data.ToString());

            Assert.Equal(result.ClaimNumber, data.SelectToken("claimNumber")!.ToString());
            Assert.Equal(request.Requester.FirstName, data.SelectToken("request.requester.firstName")!.ToString());
            Assert.Equal(request.Requester.LastName, data.SelectToken("request.requester.lastName")!.ToString());
            Assert.Equal(request.Requester.Email, data.SelectToken("request.requester.email")!.ToString());
            Assert.Equal(request.Requester.Reason, data.SelectToken("request.requester.reason")!.ToString());
            Assert.Equal(request.Requester.BookingReference, data.SelectToken("request.requester.bookingReference")!.ToString());
            Assert.Equal(request.Requester.BookingDate.ToString("yyyy-MM-dd"), data.SelectToken("request.requester.bookingDate")!.ToString());
            Assert.Equal(request.Entries.Count(), data.SelectToken("request.entries")!.Count());
            */
        }
        else
        {
            // For flight number 2, we expect the submission to fail with an InvalidOperationException, as the validation in SubmitRefundCommandHandler should detect that there are existing claim numbers for these entries (as per the mocked response in D365RefundGateway)
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.HandleAsync(request, CancellationToken.None));
        }
    }

}

