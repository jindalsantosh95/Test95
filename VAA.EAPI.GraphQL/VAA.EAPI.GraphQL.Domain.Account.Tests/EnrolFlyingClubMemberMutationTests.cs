using Azure;
using Grpc.Core;
using HotChocolate;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class EnrolFlyingClubMemberMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IStampingPNRsHandler> _stampingPNRsHandler;
        private readonly IEnrolmentProvider _mockEnrolmentProvider;
        private readonly EnrolFlyingClubMemberMutation _mutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;

        public EnrolFlyingClubMemberMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _stampingPNRsHandler = new Mock<IStampingPNRsHandler>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _mockEnrolmentProvider = new EnrolmentProvider(_tokenProvider.Object, _httpConnector, _stampingPNRsHandler.Object);
            _mutation = new EnrolFlyingClubMemberMutation(_mockEnrolmentProvider);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Mutation_ShouldReturn_TokenMissingInRequest()
        {
            //Arrange
            var enrolRequest = TestDataProvider.GetTestData<CreateCustomerAccountRequest>("Account//EnrolmentRequestEnrolTokenMissing");

            _ = await Assert.ThrowsAsync<GraphQLException>(async () => await _mutation.EnrolFcMember(enrolRequest));
        }

        [Fact]
        public async Task Mutation_ShouldReturn_FcNumber_ForGoodRequest()
        {
            //Arrange
            var enrolRequest = TestDataProvider.GetTestData<CreateCustomerAccountRequest>("Account//EnrolmentRequest");

            //Act
            var result = await _mutation.EnrolFcMember(enrolRequest);

            //Assert
            Assert.NotNull(result.MemberDetails?.MemberNumber);
            Assert.IsType<FlyingClubSignUpResponse>(result);
            Assert.Equal("1501033656", result.MemberDetails?.MemberNumber);
            Assert.Equal("Active", result.MemberDetails?.Status);
        }
    }
}
