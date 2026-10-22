using VAA.EAPI.GraphQL.Domain.Account.Constants;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class VoucherMapperTests
    {
        public VoucherMapperTests()
        {
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void VoucherResponseMapper_ShouldMapVoucher_WithDisplayTextEnums_AndCategoryOrdering()
        {
            var now = DateTime.UtcNow;
            var input = new List<VoucherDetail>
            {
                CreateVoucherDetail(
                    voucherName: "Flying Club reward voucher - Virgin Red US credit card",
                    source: "VIRGIN MONEY",
                    category: "DISCOUNT",
                    voucherNumber: "DISC-001",
                    maxValue: "300",
                    startDate: now.AddDays(-1).ToString("O"),
                    expiryDate: now.AddDays(20).ToString("O")),
                CreateVoucherDetail(
                    voucherName: "Gold Lounge Pass",
                    source: "VIRGIN ATLANTIC AIRWAYS",
                    category: "REWARD",
                    voucherNumber: "REW-001",
                    maxValue: "100",
                    startDate: now.AddDays(-2).ToString("O"),
                    expiryDate: now.AddDays(10).ToString("O")),
                CreateVoucherDetail(
                    voucherName: "Flying Club reward voucher - UK Reward credit card",
                    source: "VIRGIN CREDIT CARD UK",
                    category: "CLUBHOUSE",
                    voucherNumber: "CLB-001",
                    maxValue: "200",
                    startDate: now.AddDays(-3).ToString("O"),
                    expiryDate: now.AddDays(30).ToString("O"))
            };

            var result = VoucherMapper.VoucherResponseMapper(input, page: 0, pageSize: 0);

            var vouchers = result.Vouchers.ToList();
            var categories = result.Categories.ToList();

            Assert.Equal(3, vouchers.Count);
            Assert.Collection(
                vouchers,
                first =>
                {
                    Assert.Equal(VoucherCategory.REWARD, first.Category);
                    Assert.Equal(VoucherTypes.GOLD_LOUNGE_PASS, first.Type);
                    Assert.Equal(VoucherSource.VIRGIN_ATLANTIC_AIRWAYS, first.Source);
                },
                second => Assert.Equal(VoucherCategory.CLUBHOUSE, second.Category),
                third => Assert.Equal(VoucherCategory.DISCOUNT, third.Category));

            Assert.Collection(
                categories,
                first =>
                {
                    Assert.Equal(VoucherCategory.REWARD, first.Code);
                    Assert.Equal(0, first.SortOrder);
                },
                second =>
                {
                    Assert.Equal(VoucherCategory.CLUBHOUSE, second.Code);
                    Assert.Equal(1, second.SortOrder);
                });

            Assert.Equal(300, result.PointsThreshold);
        }

        [Theory]
        [InlineData("Partially Used", "Gold Lounge Pass", "VIRGIN MONEY")]
        [InlineData("Available", "Type Not Configured", "VIRGIN MONEY")]
        [InlineData("Available", "Gold Lounge Pass", "Random Source")]
        public void VoucherResponseMapper_ShouldExcludeVoucher_WhenStatusTypeOrSourceValuesAreUnrecognized(
            string status,
            string voucherName,
            string source)
        {
            var now = DateTime.UtcNow;
            var input = new[]
            {
                CreateVoucherDetail(
                    voucherName: voucherName,
                    source: source,
                    category: "REWARD",
                    voucherNumber: "UNK-001",
                    status: status,
                    startDate: now.AddDays(-1).ToString("O"),
                    expiryDate: now.AddDays(5).ToString("O"))
            };

            var result = VoucherMapper.VoucherResponseMapper(input, page: 0, pageSize: 0);

            Assert.Empty(result.Vouchers);

            var categories = result.Categories.ToList();
            Assert.Collection(
                categories,
                first => Assert.Equal(VoucherCategory.REWARD, first.Code),
                second => Assert.Equal(VoucherCategory.CLUBHOUSE, second.Code));

            Assert.Equal(0, result.PointsThreshold);
        }

        [Fact]
        public void VoucherResponseMapper_ShouldIncludeOnlyAvailableStatusVouchers()
        {
            var now = DateTime.UtcNow;
            var input = new[]
            {
                CreateVoucherDetail(status: "Cancelled", voucherNumber: "EX-1", expiryDate: now.AddDays(5).ToString("O")),
                CreateVoucherDetail(status: "Used", voucherNumber: "EX-2", expiryDate: now.AddDays(6).ToString("O")),
                CreateVoucherDetail(status: "Expired", voucherNumber: "EX-3", expiryDate: now.AddDays(7).ToString("O")),
                CreateVoucherDetail(status: "Gifted", voucherNumber: "EX-4", expiryDate: now.AddDays(8).ToString("O")),
                CreateVoucherDetail(status: "Active", voucherNumber: "EX-5", expiryDate: now.AddDays(9).ToString("O")),
                CreateVoucherDetail(status: "Available", voucherNumber: "IN-1", maxValue: "150", expiryDate: now.AddDays(8).ToString("O"))
            };

            var result = VoucherMapper.VoucherResponseMapper(input, page: 0, pageSize: 0);

            var voucher = Assert.Single(result.Vouchers);
            Assert.Equal("IN-1", voucher.Number);
            Assert.Equal(VoucherStatus.Available, voucher.Status);
            Assert.Equal(150, result.PointsThreshold);
        }

        [Fact]
        public void VoucherResponseMapper_ShouldFilterOutPastExpiry_AndApplyPaging()
        {
            var now = DateTime.UtcNow;
            var input = new[]
            {
                CreateVoucherDetail(category: "REWARD", voucherNumber: "R-1", maxValue: "100", expiryDate: now.AddDays(3).ToString("O")),
                CreateVoucherDetail(category: "CLUBHOUSE", voucherNumber: "C-1", maxValue: "200", expiryDate: now.AddDays(4).ToString("O")),
                CreateVoucherDetail(category: "DISCOUNT", voucherNumber: "D-1", maxValue: "300", expiryDate: now.AddDays(5).ToString("O")),
                CreateVoucherDetail(category: "REWARD", voucherNumber: "OLD-1", maxValue: "999", expiryDate: now.AddDays(-1).ToString("O"))
            };

            var paged = VoucherMapper.VoucherResponseMapper(input, page: 2, pageSize: 1);

            var voucher = Assert.Single(paged.Vouchers);
            Assert.Equal("C-1", voucher.Number);

            var categories = paged.Categories.ToList();
            Assert.Collection(
                categories,
                first => Assert.Equal(VoucherCategory.REWARD, first.Code),
                second => Assert.Equal(VoucherCategory.CLUBHOUSE, second.Code));

            Assert.Equal(300, paged.PointsThreshold);
        }

        [Fact]
        public void VoucherResponseMapper_ShouldCalculatePointsThreshold_FromAllFilteredVouchers_NotPagedVouchers()
        {
            var now = DateTime.UtcNow;
            var input = new[]
            {
                CreateVoucherDetail(category: "REWARD", voucherNumber: "R-1", maxValue: "100", expiryDate: now.AddDays(3).ToString("O")),
                CreateVoucherDetail(category: "CLUBHOUSE", voucherNumber: "C-1", maxValue: "900", expiryDate: now.AddDays(4).ToString("O")),
                CreateVoucherDetail(category: "DISCOUNT", voucherNumber: "D-1", maxValue: "300", expiryDate: now.AddDays(5).ToString("O"))
            };

            var paged = VoucherMapper.VoucherResponseMapper(input, page: 1, pageSize: 1);

            var singlePagedVoucher = Assert.Single(paged.Vouchers);
            Assert.Equal("R-1", singlePagedVoucher.Number);
            Assert.Equal(900, paged.PointsThreshold);
        }

        private static VoucherDetail CreateVoucherDetail(
            string voucherName = "Gold Lounge Pass",
            string source = "VIRGIN MONEY",
            string category = "REWARD",
            string voucherNumber = "V-001",
            string status = "Available",
            string maxValue = "100",
            string? startDate = null,
            string? expiryDate = null)
        {
            var now = DateTime.UtcNow;

            return new VoucherDetail
            {
                VoucherName = voucherName,
                VoucherNumber = voucherNumber,
                Description = "desc",
                Status = status,
                StartDate = startDate ?? now.AddDays(-1).ToString("O"),
                ExpiryDate = expiryDate ?? now.AddDays(10).ToString("O"),
                Source = source,
                MaxValue = maxValue,
                Category = category,
                IsGifted = false
            };
        }
    }
}
