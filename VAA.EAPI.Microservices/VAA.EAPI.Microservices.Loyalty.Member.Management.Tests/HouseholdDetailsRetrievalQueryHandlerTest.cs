using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseholdDetailsRetrievalQueryHandlerTests :
        IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public HouseholdDetailsRetrievalQueryHandlerTests(
            HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(
                    httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()

                .AddSingleton<IMediator>(_ => Mock.Of<IMediator>())

                .AddSingleton<GroupDocumentServiceQueryHandler>(sp =>
                {
                    var httpConnector =
                        sp.GetRequiredService<IHttpConnector>();

                    var logger =
                        sp.GetRequiredService<ILogger<GroupDocumentServiceQueryHandler>>();
                    var groupDocHandler = new Mock<GroupDocumentServiceQueryHandler>(httpConnector, logger);
                    groupDocHandler.Setup(Object => Object.Handle(It.IsAny<GroupDocumentServiceQuery>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync((Result.Success(new Response<HouseHoldGroup>
                                       {
                                           Status = Status.OK,
                                           IgnoreNullValues = false,
                                           Value = new HouseHoldGroup()
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
                                                                HoHManaged = true
                                                            }
                                                                }
                                           }
                                       })));

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

                    var memberAccountHandler =
                        new Mock<MemberAccountDetailsQueryHandler>(
                            mediator,
                            logger);

                    memberAccountHandler
                        .Setup(x => x.Handle(
                            It.IsAny<MemberAccountDetailsQuery>(),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(
                            Result.Success(
                                new Response<MemberAccountResponse>
                                {
                                    Status = Status.OK,
                                    IgnoreNullValues = false,
                                    Value = new MemberAccountResponse
                                    {
                                        loyaltyMemberAccount =
                                            new LoyaltyMemberAccount
                                            {
                                                CustomerId = "CHILD002",
                                                RetainCoreTierPoints = "200",
                                                RollingCoreTierPoints = "100",
                                                RetainPointTotal = "200",
                                                RollingPointTotal = "100"
                                            }
                                    }
                                }));

                    return memberAccountHandler.Object;
                })

                .AddSingleton<IGroupDetailsService, GroupDetailsServiceHandler>()

                .AddSingleton<IHouseholdDetailsRetrievalQuery,
                    HouseholdDetailsRetrievalQueryHandler>()

                .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
        {
            var handler =
                _serviceProvider.GetRequiredService<IHouseholdDetailsRetrievalQuery>();

            var request = new HouseholdDetailsRetrievalQuery
            {
                CustomerId = "13097607-8469-4abf-aea5-d61f3730083"
            };

            var result = await handler.Handle(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);

            Assert.NotNull(result.Value);
            Assert.Equal(Status.OK, result.Value.Status);

            var response = result.Value.Value;

            Assert.NotNull(response);
            Assert.NotNull(response.HouseholdMembers);
            Assert.NotNull(response.HouseholdHistory);

            Assert.Single(response.HouseholdMembers);
            Assert.Single(response.HouseholdHistory);

            var member = response.HouseholdMembers.First();

            Assert.Equal("CHILD002", member.CustomerId);
            Assert.Equal("Linked Member-Child", member.RelationshipType);
            Assert.Equal("234489121", member.GroupDocId);
            Assert.Equal(
                "200",
                result.Value.Value.HouseholdMembers.First().RetainPointTotal);
            Assert.Equal(
                "100",
                result.Value.Value.HouseholdMembers.First().RollingPointTotal);

            var history = response.HouseholdHistory.First();

            Assert.Equal("CHILD001", history.CustomerId);
            Assert.Equal("Linked Member", history.RelationshipType);
            Assert.Equal("234489121", history.GroupDocId);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCustomerIdIsNull()
        {
            var handler =
                _serviceProvider.GetRequiredService<IHouseholdDetailsRetrievalQuery>();

            var request = new HouseholdDetailsRetrievalQuery
            {
                CustomerId = null
            };

            var result = await handler.Handle(
                request,
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            Assert.Equal(
                "Request cannot be null. (Parameter 'request')",
                result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCustomerIdIsEmpty()
        {
            var handler =
                _serviceProvider.GetRequiredService<IHouseholdDetailsRetrievalQuery>();

            var request = new HouseholdDetailsRetrievalQuery
            {
                CustomerId = string.Empty
            };

            var result = await handler.Handle(
                request,
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
        }
    }
}