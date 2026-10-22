using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public sealed class VoucherDetails { public IEnumerable<VoucherRedemptionOption>? RedemptionOptions { get; set; } public BestVoucherRedemptionOption? BestOption { get; set; } public IEnumerable<AvailableVoucher>? AvailableVouchers { get; set; } }
    public sealed class VoucherRedemptionOption { public int OptionId { get; set; } public string? OptionName { get; set; } public string? RedemptionType { get; set; } public IEnumerable<AppliedVoucher>? AppliedVouchers { get; set; } public int? CoveredPassengers { get; set; } public long PointsToPay { get; set; } public long TotalPointsUsed { get; set; } public decimal CashToPay { get; set; } }
    public sealed class AppliedVoucher { public string? VoucherId { get; set; } public long PointsUsed { get; set; } }
    public sealed class BestVoucherRedemptionOption { public int OptionId { get; set; } public string? OptionName { get; set; } public string? RedemptionType { get; set; } public string? SelectionReason { get; set; } }
    public sealed class AvailableVoucher { public string? VoucherId { get; set; } public string? Type { get; set; } public long BalancePoints { get; set; } public DateTime ExpiryDate { get; set; } }
    public sealed class VoucherRedemptionSelection { public int? MaxVouchersToUse { get; set; } public IEnumerable<string>? SelectedVoucherIds { get; set; } public int? SelectedOptionId { get; set; } }
}
