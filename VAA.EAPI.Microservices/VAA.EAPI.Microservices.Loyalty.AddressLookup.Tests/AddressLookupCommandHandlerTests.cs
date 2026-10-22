using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressLookupCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<AddressLookUpCommandHandler>> _mockLogger;
        private readonly AddressLookUpCommandHandler _mockHandler;

        public AddressLookupCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AddressLookUpCommandHandler>>();
            _mockHandler = new AddressLookUpCommandHandler(_mockMediator.Object, _mockLogger.Object);
            AddressLookupTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Handle_NullRequest_ShouldThrowNullArgument()
        {
            // Act
            async Task result() => await _mockHandler.Handle(null, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }
        [Fact]
        public async Task Handle_Request_ReturnsSucessResponce()
        {
            //Arrange
            var request = new AddressLookupEventProcessCommand()
            {
                Data = "{\"FirstName\":\"abc\",\"AddressDetails\":[{\"PreferredFlag\":\"Y\"}]}"
            };
            //Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);
            //Assert
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task Handle_Exception()
        {
            //Arrange
            var request = new AddressLookupEventProcessCommand()
            {
                Data = "{\"FirstName\":\"abc\",\"AddressDetails\":[{\"PreferredFlag\":\"Y\",\"CountryCode\":\"GB\"}]}"
            };
            _mockMediator.Setup(r => r.Send(It.IsAny<AddressLookUpRepoCommand>(), CancellationToken.None)).Throws<ArgumentNullException>();
            //Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);
            //Assert
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task Handle_RequestWithCountryGB_ReturnsSucessResponces()
        {
            //Arrange
            var request = new AddressLookupEventProcessCommand()
            {
                Data = "{\"ChannelUserId\":\"AXIS\",\"FirstName\":\"Shelloier\",\"LastName\":\"Izzyer\",\"MiddleName\":\"\",\"PreferredGender\":\"M\",\"PreferredName\":\"Shelly\",\"PreferredSalutation\":\"DearMr\",\"PreferredPronouns\":\"He\",\"Gender\":\"M\",\"Title\":\"Mr\",\"CustomerType\":\"Loyalty\",\"DateOfBirth\":\"1997-08-10\",\"Password\":\"\",\"AddressDetails\":[{\"AddressType\":\"Home\",\"AddressLine1\":\"11FriendsClose\",\"AddressLine2\":\"HomeLocality\",\"AddressLine3\":\"\",\"CompanyName\":\"VAA\",\"City\":\"Colchester\",\"PostalCode\":\"RH117TY\",\"Region\":\"Essex\",\"CountryCode\":\"GB\",\"CountryName\":\"GB\",\"PreferredFlag\":\"Y\",\"AddressVerified\":true,\"MetroArea\":\"GB\"}],\"ContactDetails\":{\"EmailDetails\":[{\"Usage\":null,\"EmailId\":\"andrewteeery_zz01@gmail.com\",\"PreferredFlag\":\"Y\",\"Description\":\"PersonalEmail\"}],\"PhoneDetails\":[{\"PhoneType\":1,\"Usage\":null,\"CountryCode\":\"44\",\"PhoneNumber\":\"8677899888\",\"PreferredFlag\":\"Y\",\"Description\":\"PersonalPhone\"}]},\"MemberType\":\"Individual\",\"ProgramName\":\"VirginAtlanticFlyingClub\",\"MemberNumber\":\"1111113334440\",\"FlyingClubCardNumber\":\"1111113334440\",\"Status\":\"Active\",\"CampaignCode\":\"\",\"TestMember\":false,\"MediaCode\":\"A50\",\"EnrollmentChannel\":\"AXIS\",\"TermsAndConditionsAccepted\":false,\"GuardianDetails\":{\"Name\":\"Alexander\",\"DateOfBirth\":\"1975-01-27\",\"CardNumber\":\"1075810877\",\"PhoneDetails\":{\"PhoneType\":0,\"Usage\":0,\"CountryCode\":\"44\",\"PhoneNumber\":\"9898987787\",\"PreferredFlag\":\"Y\",\"Description\":\"PersonalPhone\"},\"EmailDetails\":{\"Usage\":0,\"EmailId\":\"alex.123@noreply.com\",\"PreferredFlag\":\"Y\",\"Description\":\"PersonalEmail\"},\"GuardianTermsAndConditionsAccepted\":true},\"PartnerCode\":\"\",\"StartDate\":\"\",\"StatusEnhancementCode\":\"\",\"PartnerStatusCode\":null,\"PartnerMembershipMappingId\":null,\"ReasonForTravel\":\"Business\",\"Preferences\":[{\"PreferenceType\":11,\"PreferenceValue\":\"\"}],\"CustomerAttribute\":[{\"Type\":0,\"Value\":\"ABC\"},{\"Type\":1,\"Value\":\"XYZ\"}],\"SecurityQuestions\":[{\"Question\":1,\"Answer\":\"jyhfvghtsytrfg\"}],\"CustomerId\":\"5c87acd9-bd0e-4f02-a90c-2f8b4025ce39\",\"FcNumber\":\"1003573183\"}"
            };
            _mockMediator.Setup(r => r.Send(It.IsAny<AddressLookUpRepoCommand>(), CancellationToken.None)).ReturnsAsync(true);
            //Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);
            //Assert
            Assert.True(result.IsSuccess);
        }
    }
}
