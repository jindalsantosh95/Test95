namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class Summary
    {
        public Price? Price { get; set; }
        public IEnumerable<Flight>? Flights { get; set; }
        public IEnumerable<Upgrade>? Upgrades { get; set; }
        public FareCalculationSummary? FareCalculationSummary { get; set; }
        public IEnumerable<FareCalculation>? FareCalculations { get; set; }
        public IEnumerable<Link>? Links { get; set; }
        public string? Id { get; set; }
        public string? FareId { get; set; }
        public Guid? BasketId { get; set; }
        public string? UpgradeFareId { get; set; }
        public IEnumerable<FlightPassengerBaggage>? FlightPassengerBaggage { get; set; }
        public string? PromoCodeApplied { get; set; }
        public bool? IsSaverFare { get; set; }
        public string PassengerCounts { get; set; }
        public bool? HasSufficientPoints { get; set; } = null;
        public bool VoucherApplied { get; set; }
        public EarnPoints? EarnPoints { get; set; }
        public decimal? PriceChange { get; set; }

        public VoucherRedemptionDetails? VoucherDetails { get; set; }
        public VoucherEligibility? VoucherEligibility { get; set; }
        public VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }

        public void ApplyVoucherDetails(VoucherRedemptionDetails? voucherDetails, string? availableAwardPoints = null)
        {
            VoucherDetails = voucherDetails;
            var selectedOptionId = VoucherRedemptionSelection is null
                ? voucherDetails?.BestOption?.OptionId
                : VoucherRedemptionSelection.SelectedOptionId;
            var selectedOption = voucherDetails?.RedemptionOptions?.FirstOrDefault(option => option.OptionId == selectedOptionId);
            VoucherApplied = selectedOption is not null;
            if (VoucherApplied && VoucherRedemptionSelection is null)
            {
                VoucherRedemptionSelection = new VoucherRedemptionSelection
                {
                    MaxVouchersToUse = selectedOption!.AppliedVouchers?.Count(),
                    SelectedVoucherIds = selectedOption.AppliedVouchers?
                        .Select(voucher => voucher.VoucherId)
                        .Where(voucherId => !string.IsNullOrWhiteSpace(voucherId))
                        .Cast<string>()
                        .ToArray(),
                    SelectedOptionId = selectedOption.OptionId
                };
            }
            var subtotal = Price?.AwardPoints ?? FareCalculationSummary?.TotalAwardPoints;
            if (!VoucherApplied || Price is null || !long.TryParse(subtotal, out var awardPoints)) return;
            var voucherPoints = Math.Min(selectedOption.TotalPointsUsed, awardPoints);
            Price.AwardPoints = awardPoints.ToString();
            Price.VoucherPoints = voucherPoints.ToString();
            Price.PointsToPay = selectedOption.PointsToPay.ToString();

            if (long.TryParse(availableAwardPoints, out var availablePoints))
            {
                HasSufficientPoints = selectedOption.PointsToPay <= availablePoints;
            }
        }

        public void RemoveVoucherApplication()
        {
            VoucherApplied = false;
            HasSufficientPoints = null;
            VoucherRedemptionSelection = null;
            if (VoucherDetails != null)
            {
                VoucherDetails.BestOption = null;
            }

            var subtotal = Price?.AwardPoints ?? FareCalculationSummary?.TotalAwardPoints;
            if (Price != null && long.TryParse(subtotal, out var awardPoints))
            {
                Price.AwardPoints = awardPoints.ToString();
                Price.VoucherPoints = "0";
                Price.PointsToPay = awardPoints.ToString();
            }
        }
    }
}
