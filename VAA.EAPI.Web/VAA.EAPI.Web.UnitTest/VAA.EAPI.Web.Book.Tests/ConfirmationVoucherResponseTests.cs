using System.Linq;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Common.Serialisation;
using Xunit;

namespace VAA.EAPI.Web.Book.Tests
{
    public class ConfirmationVoucherResponseTests
    {
        [Theory]
        [InlineData("COMPANION", 2)]
        [InlineData("UPGRADE", 1)]
        public void ConfirmationResponse_DeserializesVoucherRedemptionDetailsFromOrderResponse(string redemptionType, int optionId)
        {
            string response = $$"""
                {
                  "summary": {
                    "voucherApplied": true,
                    "voucherPoints": "50000",
                    "pointsToPay": "25000",
                    "voucherDetails": {
                      "bestOption": { "optionId": {{optionId}}, "redemptionType": "{{redemptionType}}" },
                      "redemptionOptions": [{ "optionId": {{optionId}}, "redemptionType": "{{redemptionType}}", "totalPointsUsed": 50000, "pointsToPay": 25000, "appliedVouchers": [{ "voucherId": "voucher-1", "pointsUsed": 50000 }] }]
                    },
                    "voucherRedemptionSelection": { "maxVouchersToUse": 2, "selectedVoucherIds": ["voucher-1"], "selectedOptionId": {{optionId}} }
                  }
                }
                """;

            var confirmationResponse = Json.Deserialize<ConfirmationResponse>(response);
            var summary = confirmationResponse.Summary;

            Assert.True(summary.VoucherApplied);
            Assert.Equal("50000", summary.VoucherPoints);
            Assert.Equal("25000", summary.PointsToPay);
            Assert.NotNull(summary.VoucherDetails);
            Assert.NotNull(summary.VoucherRedemptionSelection);
            Assert.Equal(redemptionType, summary.VoucherDetails.BestOption.RedemptionType);
            Assert.Equal(optionId, summary.VoucherDetails.BestOption.OptionId);
            Assert.Equal(50000, summary.VoucherDetails.RedemptionOptions.Single().TotalPointsUsed);
            Assert.Equal(50000, summary.VoucherDetails.RedemptionOptions.Single().AppliedVouchers.Single().PointsUsed);
            Assert.Equal(["voucher-1"], summary.VoucherRedemptionSelection.SelectedVoucherIds);
            Assert.Equal(optionId, summary.VoucherRedemptionSelection.SelectedOptionId);
        }

        [Fact]
        public void ConfirmationResponse_DeserializesStandardRewardControlWithoutVoucherDetails()
        {
            const string response = """
                {
                  "summary": {
                    "voucherApplied": false,
                    "voucherPoints": "0",
                    "pointsToPay": "75000",
                    "voucherDetails": null,
                    "voucherRedemptionSelection": null
                  }
                }
                """;

            var summary = Json.Deserialize<ConfirmationResponse>(response).Summary;

            Assert.False(summary.VoucherApplied);
            Assert.Equal("0", summary.VoucherPoints);
            Assert.Equal("75000", summary.PointsToPay);
            Assert.Null(summary.VoucherDetails);
            Assert.Null(summary.VoucherRedemptionSelection);
        }
    }
}
