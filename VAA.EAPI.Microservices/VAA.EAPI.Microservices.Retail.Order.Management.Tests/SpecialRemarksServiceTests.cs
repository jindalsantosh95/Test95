using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using Moq;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Services;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Remarks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Xunit;
using AddSpecialRemarkRequest = VAA.EAPI.Microservices.Retail.Order.Management.Domain.Remarks.AddSpecialRemarkRequest;
using SetupContextRequestType = VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy.SetupContextRequestType;
using TeardownContextRequestType = VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy.TeardownContextRequestType;
using TeardownContextResponseType = VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy.TeardownContextResponseType;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class SpecialRemarksServiceTests
    {
        public SpecialRemarksServiceTests()
        {
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set("ApplicationId", "TEST");
            ConfigReader.Set("PoolType", "TEST");
            AsyncContext.Current = new AsyncContext();
            AsyncContext.Current.SetCorrelationId(Guid.NewGuid());
        }

        [Fact]
        public async Task AddAsync_UsesVoucherRedemptionSource_WhenCachedChannelNameDiffers()
        {
            var pnrCommon = new Mock<IAir4PnrCommonV6>();
            var remarksV3 = new Mock<IAir4RemarksV3>();
            var cachedChannel = new Mock<ICachedChannel>();
            var remarksRequests = new System.Collections.Generic.List<AddSpecialRemarkRequestType>();
            cachedChannel.Setup(cache => cache.GetChannelEntity("VHOLS")).ReturnsAsync(CreateChannel());
            pnrCommon.Setup(pnr => pnr.Air4SetupContext(It.IsAny<SetupContextRequestType>()))
                .Returns(new SetupContextResponseType { SessionId = "session-123" });
            pnrCommon.Setup(pnr => pnr.Air4TearDownContext(It.IsAny<TeardownContextRequestType>()))
                .Returns(new TeardownContextResponseType());
            remarksV3.Setup(remarks => remarks.AddSpecialRemarkRequest(It.IsAny<AddSpecialRemarkRequestType>()))
                .Callback<AddSpecialRemarkRequestType>(remarksRequests.Add)
                .ReturnsAsync(new AddSpecialRemarkResponseType());
            var service = new SpecialRemarksService(cachedChannel.Object, pnrCommon.Object, remarksV3.Object);

            var result = await service.AddAsync(CreateVoucherRequest(), "VHOLS", CancellationToken.None);

            Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.OK, result.Value.Status);
            Assert.Equal(2, remarksRequests.Count);
            Assert.Equal("Online FC123456. Upgrade Vchr: 90000 pts. Total pts: 120000", remarksRequests[0].RemarkInfoFreeFormText.FreeformText);
            Assert.Equal("Online FC123456. Upgrade Vchr: 30000 pts. Total pts: 120000", remarksRequests[1].RemarkInfoFreeFormText.FreeformText);
            pnrCommon.Verify(pnr => pnr.Air4SetupContext(It.Is<SetupContextRequestType>(request => request.RetrieveInfo.PassengerInfo.RecordLocator == "ABC123")), Times.Once);
            pnrCommon.Verify(pnr => pnr.Air4TearDownContext(It.Is<TeardownContextRequestType>(request => request.RequestInfo.SessionId == "session-123")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_IgnoresAnUnsupportedRemarkSubtype_WithoutCallingDelta()
        {
            var pnrCommon = new Mock<IAir4PnrCommonV6>();
            var remarksV3 = new Mock<IAir4RemarksV3>();
            var cachedChannel = new Mock<ICachedChannel>();
            var service = new SpecialRemarksService(cachedChannel.Object, pnrCommon.Object, remarksV3.Object);
            var request = new AddSpecialRemarkRequest { RemarkSubType = "FUTURE_REMARK" };

            var result = await service.AddAsync(request, "VHOLS", CancellationToken.None);

            Assert.Equal(VAA.EAPI.Domain.Entities.Common.Status.OK, result.Value.Status);
            Assert.Equal("Remark subtype ignored.", result.Value.Value);
            cachedChannel.Verify(cache => cache.GetChannelEntity(It.IsAny<string>()), Times.Never);
            pnrCommon.Verify(pnr => pnr.Air4SetupContext(It.IsAny<SetupContextRequestType>()), Times.Never);
            remarksV3.Verify(remarks => remarks.AddSpecialRemarkRequest(It.IsAny<AddSpecialRemarkRequestType>()), Times.Never);
            pnrCommon.Verify(pnr => pnr.Air4TearDownContext(It.IsAny<TeardownContextRequestType>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_DoesNotTearDownThePnrSession_WhenAddingARemarkFails()
        {
            var pnrCommon = new Mock<IAir4PnrCommonV6>();
            var remarksV3 = new Mock<IAir4RemarksV3>();
            var cachedChannel = new Mock<ICachedChannel>();

            cachedChannel.Setup(cache => cache.GetChannelEntity("VHOLS")).ReturnsAsync(CreateChannel());
            pnrCommon.Setup(pnr => pnr.Air4SetupContext(It.IsAny<SetupContextRequestType>()))
                .Returns(new SetupContextResponseType { SessionId = "session-123" });
            pnrCommon.Setup(pnr => pnr.Air4TearDownContext(It.IsAny<TeardownContextRequestType>()))
                .Returns(new TeardownContextResponseType());
            remarksV3.Setup(remarks => remarks.AddSpecialRemarkRequest(It.IsAny<AddSpecialRemarkRequestType>()))
                .ThrowsAsync(new InvalidOperationException("Remarks V3 failed"));
            var service = new SpecialRemarksService(cachedChannel.Object, pnrCommon.Object, remarksV3.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(CreateVoucherRequest(), "VHOLS", CancellationToken.None));

            pnrCommon.Verify(pnr => pnr.Air4TearDownContext(It.IsAny<TeardownContextRequestType>()), Times.Never);
        }

        private static AddSpecialRemarkRequest CreateVoucherRequest() => new()
        {
            Pnr = "ABC123",
            Location = "LON",
            RemarkType = "SPECIAL",
            RemarkSubType = "VOUCHER_REDEMPTION",
            VoucherDetails = new VoucherDetails
            {
                LoyaltyNumber = "FC123456",
                RedemptionType = "UPGRADE",
                TotalPoints = 120000,
                Vouchers =
                [
                    new VoucherRemark { VoucherNumber = "VOUCHER-987654", VoucherPointsApplied = 90000 },
                    new VoucherRemark { VoucherNumber = "VOUCHER-123456", VoucherPointsApplied = 30000 }
                ]
            }
        };

        private static VAA.EAPI.Microservices.Retail.Order.Management.Domain.Channel CreateChannel() => new()
        {
            ChannelId = "DFWEB",
            ChannelName = "DFWEB",
            DlApplicationId = "TEST",
            DlAgentId = "AGENT",
            DlPoolType = "TEST",
            DlRoleIndicator = "ROLE"
        };

    }
}
