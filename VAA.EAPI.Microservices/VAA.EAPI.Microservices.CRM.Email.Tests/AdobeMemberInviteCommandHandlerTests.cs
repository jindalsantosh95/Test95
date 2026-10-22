using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VAA.EAPI.Infrastructure.Email.Interface;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using Xunit;
using VAA.EAPI.Domain.Entities.Common;
using CSharpFunctionalExtensions;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;
using VAA.EAPI.Microservices.CRM.Email.Domain;
using VAA.EAPI.Microservices.CRM.Email.Mapper;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class AdobeMemberInviteCommandHandlerTests
    {
        private readonly AdobeMemberInviteEmailCommandHandler _handler;
        private readonly Mock<IACMClient> _client;

        public AdobeMemberInviteCommandHandlerTests()
        {
            _client = new Mock<IACMClient>();
            _handler = new AdobeMemberInviteEmailCommandHandler(_client.Object);
            EmailTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Handler_ShouldReturnTrue_ForSuccessfulRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AdobeMemberInviteEmailCommand>("EmailMS\\AdobeInviteMember");

            _client.Setup(_ => _.SendEmail(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var result = _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("Invited Member Successfully", result.Result.Value);
            Assert.Equal(Status.OK, result.Result.Status);

        }

        [Fact]
        public async Task Handler_ShouldThrowException_ForBadRequest()
        {

            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public void Mapper_ShouldCreateYoungFlyerParentTemplateContext()
        {
            var xml = AdobeMemberInviteEmailCommandRequestMapper.MapRequest(new AdobeMemberInviteEmailCommand
            {
                Email = "parent@example.com",
                EmailTemplate = "vaaLoyaltyU18Parent",
                adobeInviteMemberRequest = new AdobeMemberInviteRequest
                {
                    FirstName = "Child",
                    LastName = "ChildLast",
                    MembershipNumber = "1000000002",
                    FirstNameOfHouseholdLead = "Parent",
                    FirstNameOfHouseholdMember = "Child"
                }
            });

            var document = XDocument.Parse(xml);
            var rtEvent = document.Descendants().Single(element => element.Name.LocalName == "rtEvent");
            var context = document.Descendants().Single(element => element.Name.LocalName == "ctx");

            Assert.Equal("vaaLoyaltyU18Parent", rtEvent.Attribute("type")?.Value);
            Assert.Equal("parent@example.com", rtEvent.Attribute("email")?.Value);
            Assert.Equal("Child", context.Element("FirstName")?.Value);
            Assert.Equal("ChildLast", context.Element("LastName")?.Value);
            Assert.Equal("1000000002", context.Element("MembershipNumber")?.Value);
            Assert.Equal("Test", context.Element("HouseHoldInviteLink")?.Value);
            Assert.Equal("Parent", context.Element("FirstNameOfHouseholdLead")?.Value);
            Assert.Equal("Child", context.Element("FirstNameOfHouseholdMember")?.Value);
        }

    }
}
