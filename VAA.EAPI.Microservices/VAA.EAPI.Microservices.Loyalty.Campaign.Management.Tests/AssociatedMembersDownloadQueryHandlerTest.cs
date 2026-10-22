using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignsDownload;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class AssociatedMembersDownloadQueryHandlerTest
    {
        private readonly Mock<IReportingDBServices> _reportingDBServices;
        private readonly Mock<IAssociatedMemberListDownloadService> _associatedMemberListDownloadService;
        private readonly Mock<ILogger<AssociatedMembersDownloadQueryHandler>> _mockLogger;
        private readonly AssociatedMembersDownloadQueryHandler _handler;

        public AssociatedMembersDownloadQueryHandlerTest()
        {
            _mockLogger = new Mock<ILogger<AssociatedMembersDownloadQueryHandler>>();
            _reportingDBServices = new Mock<IReportingDBServices>();
            _associatedMemberListDownloadService = new Mock<IAssociatedMemberListDownloadService>();
            _handler = new AssociatedMembersDownloadQueryHandler(_reportingDBServices.Object, _associatedMemberListDownloadService.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AssociatedMembersDownloadQueryHandler(_reportingDBServices.Object, _associatedMemberListDownloadService.Object, null));

            Assert.Throws<ArgumentNullException>(() =>
            new AssociatedMembersDownloadQueryHandler(null, _associatedMemberListDownloadService.Object, _mockLogger.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new AssociatedMembersDownloadQueryHandler(_reportingDBServices.Object, null, _mockLogger.Object));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var response = new AssociatedMembersDownloadResponse() { AssociatedMembersFileURL = "dummyurl" };
            List<AssociatedMemberData> memberData = new List<AssociatedMemberData>() { new AssociatedMemberData() { MemberNumber = "123" } };

            _reportingDBServices.Setup(m => m.GetAssociatedMembersData(It.IsAny<string>())).ReturnsAsync(memberData);
            _associatedMemberListDownloadService.Setup(m => m.CreateXlStatementAndUploadToBlob("XYZ", memberData)).ReturnsAsync("dummyurl");

            AssociatedMembersDownloadQuery query = new() { CampaignCode = "XYZ" };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationFailure()
        {
            AssociatedMembersDownloadQuery query = new() { CampaignCode = "" };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldFailWhenNoDataFound()
        {
            List<AssociatedMemberData> memberData = new List<AssociatedMemberData>() { };

            _reportingDBServices.Setup(m => m.GetAssociatedMembersData(It.IsAny<string>())).ReturnsAsync(memberData);

            AssociatedMembersDownloadQuery query = new() { CampaignCode = "XYZ" };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithException()
        {
            _reportingDBServices.Setup(m => m.GetAssociatedMembersData(It.IsAny<string>())).ThrowsAsync(new Exception());

            AssociatedMembersDownloadQuery query = new() { CampaignCode = "XYZ" };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }
    }
}
