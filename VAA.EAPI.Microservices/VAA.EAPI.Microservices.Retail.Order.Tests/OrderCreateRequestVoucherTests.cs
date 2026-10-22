using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderCreateRequestVoucherTests
    {
        [Fact]
        public void Create_WhenVoucherApplied_ShouldPopulateVoucherDetails()
        {
            var request = new OrderCreateRequest
            {
                VoucherApplied = true,
                VoucherDetails = new VoucherDetails
                {
                    VoucherOwnerFcAccount = "1234567890",
                    AppliedVouchers = [new VoucherRedemption { VoucherId = "TEST-VOUCHER-001", PointsUsed = 55000L }],
                    TotalPointsUsed = 55000L
                }
            };

            Assert.True(request.VoucherApplied);
            Assert.NotNull(request.VoucherDetails);
            Assert.Equal("1234567890", request.VoucherDetails.VoucherOwnerFcAccount);
            Assert.Equal(55000L, request.VoucherDetails.TotalPointsUsed);

            var appliedVoucher = Assert.Single(request.VoucherDetails.AppliedVouchers);
            Assert.Equal("TEST-VOUCHER-001", appliedVoucher.VoucherId);
            Assert.Equal(55000L, appliedVoucher.PointsUsed);
        }
    }
}
