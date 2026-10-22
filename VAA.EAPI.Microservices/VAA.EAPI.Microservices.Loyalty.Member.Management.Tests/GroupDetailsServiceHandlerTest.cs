using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class GroupDetailsServiceHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IGroupDetailsService _groupDetailsService;

        public GroupDetailsServiceHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("CustomerTxnMSDetailsEndpoint", "https://dummy.com/123/");
            ConfigReader.Set("loyaltyMbrMgmt:AppId", "123");
            ConfigReader.Set("loyaltyMbrMgmt:ClientSecret", "DUMMY");
            ConfigReader.Set("customerTxnMS:ResourceId", "dummy123");
            ConfigReader.Set("loyaltyMbrMgmt:OcpSubID", "123");
            ConfigReader.Set("azureADEndpoint", "https://dummy.com/123/");
            ConfigReader.Set("customermsurl", "https://dummy.com/123/");
            ConfigReader.Set("customer:appid", "1234");

            var mediatorMock = new Mock<IMediator>();                   

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()

                .AddSingleton<IMediator>(mediatorMock.Object)

                .AddSingleton<GroupDocumentServiceQueryHandler>(sp =>
                {
                    var httpConnector =
                        sp.GetRequiredService<IHttpConnector>();

                    var logger =
                        sp.GetRequiredService<ILogger<GroupDocumentServiceQueryHandler>>();

                    var groupDocHandler =
                        new Mock<GroupDocumentServiceQueryHandler>(
                            httpConnector,
                            logger);

                    groupDocHandler
                        .Setup(x => x.Handle(
                            It.IsAny<GroupDocumentServiceQuery>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(
                            Result.Success(
                                new Response<HouseHoldGroup>
                                {
                                    Status = Status.OK,
                                    IgnoreNullValues = false,
                                    Value = new HouseHoldGroup
                                    {
                                        Id = "234489121",
                                        GroupId = "12097607-8469-4abf-aea5-d61f3730083",
                                        HeadOfGroupCustomerId = "13097607-8469-4abf-aea5-d61f3730083",
                                        Status = "Active",
                                        GroupHistory = new List<GroupHistory>
                                        {
                                            new GroupHistory
                                            {
                                                CustomerId = "CHILD001",
                                                StartDate = DateTime.UtcNow.AddMonths(-6),
                                                CreatedBy = "Test",
                                                CreatedOn = DateTime.UtcNow.AddMonths(-6),
                                                LastUpdatedBy = "Test",
                                                LastUpdatedOn = DateTime.UtcNow,
                                                ExitDate = DateTime.UtcNow.AddMonths(-9)
                                            },
                                            new GroupHistory
                                            {
                                                CustomerId = "CHILD002",
                                                StartDate = DateTime.UtcNow.AddMonths(-6),
                                                CreatedBy = "Test",
                                                CreatedOn = DateTime.UtcNow.AddMonths(-6),
                                                LastUpdatedBy = "Test",
                                                LastUpdatedOn = DateTime.UtcNow,
                                                HoHManaged = true,
                                            }
                                        }
                                    }
                                }));

                    return groupDocHandler.Object;
                })

                .AddSingleton<GroupHistoryDocumentServiceQueryHandler>(sp =>
                {
                    var httpConnector =
                        sp.GetRequiredService<IHttpConnector>();

                    var logger =
                        sp.GetRequiredService<ILogger<GroupHistoryDocumentServiceQueryHandler>>();

                    var groupHistoryDocHandler =
                        new Mock<GroupHistoryDocumentServiceQueryHandler>(
                            httpConnector,
                            logger);

                    groupHistoryDocHandler
                        .Setup(x => x.Handle(
                            It.IsAny<GroupHistoryDocumentServiceQuery>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(
                            Result.Success(
                                new Response<GroupResponse>
                                {
                                    Status = Status.OK,
                                    IgnoreNullValues = false,
                                    Value = new GroupResponse
                                    {
                                        GroupDocuments = new List<HouseHoldGroup>()
                                    }
                                }));

                    return groupHistoryDocHandler.Object;
                })
                
                .AddSingleton<MemberAccountDetailsQueryHandler>(sp =>
                {
                    var mediator =
                        sp.GetRequiredService<IMediator>();

                    var logger =
                        sp.GetRequiredService<ILogger<MemberAccountDetailsQueryHandler>>();

                    var memberAccountDetailsQueryHandler = new Mock<MemberAccountDetailsQueryHandler>(
                        mediator,
                        logger);
                    memberAccountDetailsQueryHandler.Setup(x => x.Handle(
                        It.IsAny<MemberAccountDetailsQuery>(),
                        It.IsAny<CancellationToken>()))
                        .ReturnsAsync(
                            Result.Success(
                                new Response<Domain.Entities.MemberAccountDetails.MemberAccountResponse>
                                {
                                    Value= new MemberAccountResponse
                                    {
                                        loyaltyMemberAccount = new LoyaltyMemberAccount
                                        {
                                            CustomerId = "13097607-8469-4abf-aea5-d61f3730083",
                                            RetainPointTotal = "200",
                                            RollingPointTotal = "100"
                                        }
                                    }
                                }));
                    return memberAccountDetailsQueryHandler.Object;
                })

                .AddSingleton<IGroupDetailsService, GroupDetailsServiceHandler>()

                .BuildServiceProvider();

            _groupDetailsService =
                serviceProvider.GetRequiredService<IGroupDetailsService>();
        }

        [Fact]
        public async Task GetHouseholdAccountDetails_ShouldReturnFailure_WhenCustomerIdIsNull()
        {
            var result =
                await _groupDetailsService.GetHouseholdAccountDetails(
                    null,
                    CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);

            Assert.Equal(
                "CustomerId cannot be null or empty. (Parameter 'customerId')",
                result.Error);
        }

        [Fact]
        public async Task GetHouseholdAccountDetails_ShouldReturnFailure_WhenCustomerIdIsEmpty()
        {
            var result =
                await _groupDetailsService.GetHouseholdAccountDetails(
                    string.Empty,
                    CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);

            Assert.Equal(
                "CustomerId cannot be null or empty. (Parameter 'customerId')",
                result.Error);
        }

        [Fact]
        public async Task GetHouseholdAccountDetails_ShouldReturnFailure_WhenCustomerIdIsWhitespace()
        {
            var result =
                await _groupDetailsService.GetHouseholdAccountDetails(
                    " ",
                    CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);

            Assert.Equal(
                "CustomerId cannot be null or empty. (Parameter 'customerId')",
                result.Error);
        }

        [Fact]
        public async Task GetHouseholdAccountDetails_ShouldReturnSuccess_WhenCustomerIdIsValid()
        {
            const string customerId =
                "13097607-8469-4abf-aea5-d61f3730083";

            var result =
                await _groupDetailsService.GetHouseholdAccountDetails(
                    customerId,
                    CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);

            Assert.Equal(Status.OK, result.Value.Status);

            Assert.NotNull(result.Value.Value);
            Assert.NotNull(result.Value.Value.HouseholdHistory);
            Assert.NotNull(result.Value.Value.HouseholdMembers);

            Assert.Null(result.Value.ErrorMessage);
            Assert.Null(result.Value.Value.HeadOfHousehold);

            Assert.Single(result.Value.Value.HouseholdMembers);
            Assert.Single(result.Value.Value.HouseholdHistory);

            Assert.Equal(
                "Linked Member-Child",
                result.Value.Value.HouseholdMembers.First().RelationshipType);

            Assert.Equal(
                "Linked Member",
                result.Value.Value.HouseholdHistory.First().RelationshipType);
            Assert.Equal(
                "CHILD001",
                result.Value.Value.HouseholdHistory.First().CustomerId);
            Assert.Equal(
                "200",
                result.Value.Value.HouseholdMembers.First().RetainPointTotal);
            Assert.Equal(
                "100",
                result.Value.Value.HouseholdMembers.First().RollingPointTotal);
        }
    }
}