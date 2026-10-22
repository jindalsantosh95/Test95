using System.Globalization;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using BffPrice = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Price;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services
{
    public sealed class VoucherRedemptionService : IVoucherRedemptionService
    {
        private sealed record VoucherCandidate(string VoucherId, long MaxValuePoints);

        public Task<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?> GetVoucherRedemptionOffersForCompanion(
            FlightOfferResponse flightSummaryResponse,
            IEnumerable<VoucherDetail>? availableVouchers,
            int eligibleCompanionPaxCount,
            IEnumerable<BookingJourneyOfferDetails>? bookingJourneyOfferDetails,
            long totalPointsForCompanionToPay,
            long totalAwardPointsNeededToPay,
            int? maxVouchersToUse = null,
            string? voucherIdToUse = null)
        {
            if (eligibleCompanionPaxCount <= 0)
            {
                return Task.FromResult<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?>(null);
            }

            var cashToPay = GetCashToPay(flightSummaryResponse);
            var result = BuildRedemptionOptions(
                cashToPay,
                availableVouchers,
                // A Companion redemption supports no more than two vouchers and
                // cannot use more vouchers than there are eligible companions.
                maximumRedeemableVouchers: Math.Min(2, eligibleCompanionPaxCount),
                totalPointsForCompanionToPay,
                totalAwardPointsNeededToPay,
                maxVouchersToUse,
                voucherIdToUse);

            if (result == null)
            {
                return Task.FromResult<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?>(null);
            }

            var options = result.Value.RedemptionOptions.Select(option =>
            {
                option.CoveredPassengers = CalculateCoveredCompanionPassengers(
                    option.TotalPointsUsed,
                    totalPointsForCompanionToPay,
                    eligibleCompanionPaxCount);
                return option;
            }).ToArray();

            return Task.FromResult<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?>(
                (options, result.Value.BestOption));
        }

        private static int CalculateCoveredCompanionPassengers(
            long voucherPointsUsed,
            long totalCompanionPoints,
            int eligibleCompanionPaxCount)
        {
            if (voucherPointsUsed <= 0 || totalCompanionPoints <= 0 || eligibleCompanionPaxCount <= 0)
            {
                return 0;
            }

            if (voucherPointsUsed >= totalCompanionPoints)
            {
                return eligibleCompanionPaxCount;
            }

            var pointsPerCompanionPassenger = (long)Math.Ceiling((decimal)totalCompanionPoints / eligibleCompanionPaxCount);
            return Math.Min(
                eligibleCompanionPaxCount,
                Math.Max(1, (int)(voucherPointsUsed / pointsPerCompanionPassenger)));
        }

        public Task<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?> GetVoucherRedemptionOffersForUpgrade(
            FlightOfferResponse flightSummaryResponse,
            IEnumerable<VoucherDetail>? availableVouchers,
            int eligiblePaxCountExcludingInfants,
            long totalUpgradePoints,
            long totalAwardPointsNeededToPay,
            long totalBaselinePoints,
            int? maxVouchersToUse = null,
            string? voucherIdToUse = null)
        {
            if (eligiblePaxCountExcludingInfants <= 0 || totalUpgradePoints <= 0 || totalAwardPointsNeededToPay <= 0)
            {
                return Task.FromResult<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?>(null);
            }

            var result = BuildRedemptionOptions(
                GetCashToPay(flightSummaryResponse),
                availableVouchers,
                maximumRedeemableVouchers: 2,
                totalPointsForCompanionToPay: totalUpgradePoints,
                totalAwardPointsNeededToPay: totalAwardPointsNeededToPay,
                maxVouchersToUse: maxVouchersToUse,
                voucherIdToUse: voucherIdToUse);

            if (result == null)
            {
                return Task.FromResult<(IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)?>(null);
            }

            var options = result.Value.RedemptionOptions.Select((option, index) =>
            {
                var appliedVouchers = AllocateUpgradePoints(option.AppliedVouchers, totalUpgradePoints);

                return new RedemptionOption
                {
                    OptionId = index + 1,
                    OptionName = $"Upgrade - {appliedVouchers.Count} Voucher{(appliedVouchers.Count == 1 ? string.Empty : "s")}",
                    RedemptionType = "UPGRADE",
                    AppliedVouchers = appliedVouchers,
                    CoveredPassengers = eligiblePaxCountExcludingInfants,
                    BaselinePoints = totalBaselinePoints,
                    PointsToPay = option.PointsToPay,
                    TotalPointsUsed = appliedVouchers.Sum(voucher => voucher.PointsUsed),
                    CashToPay = option.CashToPay
                };
            }).ToArray();

            var bestOption = options.FirstOrDefault(option => option.OptionId == result.Value.BestOption.OptionId);
            return Task.FromResult(bestOption == null
                ? null
                : ((IEnumerable<RedemptionOption>, BestRedemptionOption)?)(options, new BestRedemptionOption
                {
                    OptionId = bestOption.OptionId,
                    OptionName = bestOption.OptionName,
                    RedemptionType = bestOption.RedemptionType,
                    SelectionReason = result.Value.BestOption.SelectionReason
                }));
        }

        public BestRedemptionOption? SelectBestRedemptionOption(IEnumerable<RedemptionOption>? redemptionOptions)
        {
            var bestOption = redemptionOptions?
                .OrderByDescending(option => option.AppliedVouchers?.Sum(voucher => voucher.PointsUsed) ?? 0)
                .ThenBy(option => option.PointsToPay)
                .ThenByDescending(option => option.RedemptionType == "UPGRADE")
                .ThenByDescending(option => option.AppliedVouchers?.Count() ?? 0)
                .ThenBy(option => option.OptionId)
                .FirstOrDefault();

            return bestOption == null ? null : new BestRedemptionOption
            {
                OptionId = bestOption.OptionId,
                OptionName = bestOption.OptionName,
                RedemptionType = bestOption.RedemptionType,
                SelectionReason = "Highest voucher points utilisation. If options use the same voucher value, the lowest points payable is preferred; if still tied between Upgrade and Companion, Upgrade is preferred; otherwise, the option using more vouchers is preferred."
            };
        }

        internal static (IEnumerable<RedemptionOption> RedemptionOptions, BestRedemptionOption BestOption)? BuildRedemptionOptions(
            decimal cashToPay,
            IEnumerable<VoucherDetail>? availableVouchers,
            int maximumRedeemableVouchers,
            long totalPointsForCompanionToPay,
            long totalAwardPointsNeededToPay,
            int? maxVouchersToUse,
            string? voucherIdToUse)
        {
            static long CalculateWastePoints(long totalVoucherValuePoints, long companionPointsNeeded)
            {
                if (totalVoucherValuePoints <= 0 || companionPointsNeeded <= 0)
                {
                    return 0;
                }

                return totalVoucherValuePoints > companionPointsNeeded
                    ? totalVoucherValuePoints - companionPointsNeeded
                    : 0;
            }

            if (maximumRedeemableVouchers <= 0)
            {
                return null;
            }

            if (totalPointsForCompanionToPay <= 0 || totalAwardPointsNeededToPay <= 0)
            {
                return null;
            }

            var voucherCandidates = ParseVouchers(availableVouchers);
            if (voucherCandidates.Count == 0)
            {
                return null;
            }

            long GetOptionAllowance(int _) => totalPointsForCompanionToPay;

            voucherCandidates = voucherCandidates
                .OrderBy(v => v.MaxValuePoints)
                .ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var effectiveMaxVouchersToUse = Math.Min(voucherCandidates.Count, maximumRedeemableVouchers);

            // Business rule: at most 2 vouchers can be used.
            effectiveMaxVouchersToUse = Math.Min(effectiveMaxVouchersToUse, 2);

            if (maxVouchersToUse.HasValue)
            {
                if (maxVouchersToUse.Value <= 0)
                {
                    return null;
                }

                effectiveMaxVouchersToUse = Math.Min(effectiveMaxVouchersToUse, maxVouchersToUse.Value);
            }

            if (effectiveMaxVouchersToUse <= 0)
            {
                return null;
            }

            if (effectiveMaxVouchersToUse == 1 || maxVouchersToUse == 1)
            {
                var requestedVoucher = !string.IsNullOrWhiteSpace(voucherIdToUse)
                    ? voucherCandidates.FirstOrDefault(v => string.Equals(v.VoucherId, voucherIdToUse, StringComparison.OrdinalIgnoreCase))
                    : null;

                var bestSingleVoucher = requestedVoucher ?? SelectBestSingleVoucher(voucherCandidates, GetOptionAllowance(1));

                if (bestSingleVoucher == null)
                {
                    return null;
                }

                var selection = (IReadOnlyList<VoucherCandidate>)new[] { bestSingleVoucher };
                var (_, pointsToPay, appliedVouchers) = BuildOption(selection, GetOptionAllowance(1), totalAwardPointsNeededToPay);

                var option = new RedemptionOption
                {
                    OptionId = 1,
                    OptionName = $"Companion - 1 Voucher ({bestSingleVoucher.VoucherId})",
                    RedemptionType = "COMPANION",
                    AppliedVouchers = appliedVouchers,
                    CoveredPassengers = 1,
                    PointsToPay = pointsToPay,
                    TotalPointsUsed = appliedVouchers.Sum(voucher => voucher.PointsUsed),
                    CashToPay = cashToPay
                };

                return new(
                    new[] { option },
                    new BestRedemptionOption
                    {
                        OptionId = 1,
                        OptionName = option.OptionName,
                        RedemptionType = option.RedemptionType,
                        SelectionReason = "Single voucher selected. Customer can change the voucher in UI and re-price."
                    });
            }

            var options = new List<RedemptionOption>(capacity: Math.Min(voucherCandidates.Count + 1, 50));
            var ranked = new List<(RedemptionOption Option, long TotalVoucherValuePoints, long CompanionAllowance, string VoucherKey)>(capacity: options.Capacity);

            // Always provide 1-voucher options per voucher so UI can let customer choose which voucher to consume.
            foreach (var voucher in voucherCandidates)
            {
                var selection = (IReadOnlyList<VoucherCandidate>)new[] { voucher };
                var (_, pointsToPay, appliedVouchers) = BuildOption(selection, GetOptionAllowance(1), totalAwardPointsNeededToPay);

                var option = new RedemptionOption
                {
                    OptionId = options.Count + 1,
                    OptionName = $"Companion - 1 Voucher ({voucher.VoucherId})",
                    RedemptionType = "COMPANION",
                    AppliedVouchers = appliedVouchers,
                    CoveredPassengers = 1,
                    PointsToPay = pointsToPay,
                    TotalPointsUsed = appliedVouchers.Sum(voucher => voucher.PointsUsed),
                    CashToPay = cashToPay
                };

                options.Add(option);
                ranked.Add((option, voucher.MaxValuePoints, GetOptionAllowance(1), voucher.VoucherId));
            }

            // Provide the 2-voucher option (only one combination because we cap to 2 vouchers max).
            if (effectiveMaxVouchersToUse >= 2 && voucherCandidates.Count >= 2)
            {
                var selection = SelectBestVoucherSubset(voucherCandidates, 2, GetOptionAllowance(2));
                if (selection.Count == 0)
                {
                    // Fall back to single-voucher options only.
                }
                else
                {
                    var (_, pointsToPay, appliedVouchers) = BuildOption(selection, GetOptionAllowance(2), totalAwardPointsNeededToPay);

                    if (appliedVouchers.Count >= 2)
                    {
                        var option = new RedemptionOption
                        {
                            OptionId = options.Count + 1,
                            OptionName = "Companion - 2 Vouchers",
                            RedemptionType = "COMPANION",
                            AppliedVouchers = appliedVouchers,
                            CoveredPassengers = 2,
                            PointsToPay = pointsToPay,
                            TotalPointsUsed = appliedVouchers.Sum(voucher => voucher.PointsUsed),
                            CashToPay = cashToPay
                        };

                        options.Add(option);

                        var totalVoucherValue = selection.Sum(v => v.MaxValuePoints);
                        var voucherKey = string.Join("|", selection.Select(v => v.VoucherId).OrderBy(v => v, StringComparer.OrdinalIgnoreCase));
                        ranked.Add((option, totalVoucherValue, GetOptionAllowance(2), voucherKey));
                    }
                }
            }

            if (options.Count == 0)
            {
                return null;
            }

            var best = ranked
                .OrderBy(o => o.Option.PointsToPay)
                .ThenBy(o => CalculateWastePoints(o.TotalVoucherValuePoints, o.CompanionAllowance))
                .ThenBy(o => o.Option.AppliedVouchers?.Count() ?? 0)
                .ThenBy(o => o.VoucherKey, StringComparer.OrdinalIgnoreCase)
                .First();

            var bestOption = new BestRedemptionOption
            {
                OptionId = best.Option.OptionId,
                OptionName = best.Option.OptionName,
                RedemptionType = best.Option.RedemptionType,
                SelectionReason = "Lowest points payable. If multiple options have the same payable amount, the option wasting fewer voucher points is preferred; if still tied, fewer vouchers is preferred."
            };

            return new(options, bestOption);

            static VoucherCandidate? SelectBestSingleVoucher(IReadOnlyList<VoucherCandidate> candidates, long pointsNeeded)
            {
                var covering = candidates
                    .Where(v => v.MaxValuePoints >= pointsNeeded)
                    .OrderBy(v => v.MaxValuePoints)
                    .ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                if (covering != null)
                {
                    return covering;
                }

                return candidates
                    .OrderByDescending(v => v.MaxValuePoints)
                    .ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
            }
        }

        private static (long CoveredCompanionPoints, long PointsToPay, IReadOnlyList<AppliedVoucher> AppliedVouchers) BuildOption(
            IReadOnlyList<VoucherCandidate> selection,
            long totalPointsForCompanionToPay,
            long totalAwardPointsNeededToPay)
        {
            var maxCoverable = selection.Sum(v => v.MaxValuePoints);
            var coveredCompanionPoints = Math.Min(maxCoverable, totalPointsForCompanionToPay);
            IReadOnlyList<AppliedVoucher> appliedVouchers = AllocatePoints(selection, coveredCompanionPoints);

            coveredCompanionPoints = appliedVouchers.Sum(voucher => voucher.PointsUsed);
            var pointsToPay = totalAwardPointsNeededToPay - coveredCompanionPoints;
            if (pointsToPay < 0)
            {
                pointsToPay = 0;
            }

            return (coveredCompanionPoints, pointsToPay, appliedVouchers);
        }

        private static decimal GetCashToPay(FlightOfferResponse flightSummaryResponse)
        {
            var selectedOffer = flightSummaryResponse?.Offers?.FirstOrDefault(o => o.Selected);
            if (selectedOffer?.SalePrice == null)
            {
                return 0m;
            }

            var price = (BffPrice)selectedOffer.SalePrice.ToList();
            return price.AmountIncludingTax;
        }

        private static List<VoucherCandidate> ParseVouchers(IEnumerable<VoucherDetail>? availableVouchers)
        {
            if (availableVouchers == null)
            {
                return [];
            }

            var candidates = new List<VoucherCandidate>();

            foreach (var award in availableVouchers)
            {
                if (award == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(award.VoucherNumber))
                {
                    continue;
                }

                if (!TryParsePoints(award.MaxValue, out var maxValuePoints))
                {
                    continue;
                }

                if (maxValuePoints <= 0)
                {
                    continue;
                }

                candidates.Add(new VoucherCandidate(award.VoucherNumber, maxValuePoints));
            }

            return candidates
                .GroupBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(v => v.MaxValuePoints)
                .ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<AppliedVoucher> AllocatePoints(IReadOnlyList<VoucherCandidate> vouchers, long pointsToAllocate)
        {
            var applied = new List<AppliedVoucher>(capacity: vouchers.Count);
            var remainingPoints = pointsToAllocate;

            foreach (var voucher in vouchers.OrderBy(v => v.MaxValuePoints).ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase))
            {
                if (remainingPoints <= 0)
                {
                    break;
                }

                var pointsUsed = Math.Min(voucher.MaxValuePoints, remainingPoints);
                applied.Add(new AppliedVoucher
                {
                    VoucherId = voucher.VoucherId,
                    PointsUsed = pointsUsed
                });

                remainingPoints -= pointsUsed;
            }

            return applied;
        }

        private static List<AppliedVoucher> AllocateUpgradePoints(IEnumerable<AppliedVoucher>? vouchers, long upgradePointsDifference)
        {
            var applied = new List<AppliedVoucher>();
            var remainingPoints = upgradePointsDifference;

            foreach (var voucher in vouchers ?? Enumerable.Empty<AppliedVoucher>())
            {
                if (remainingPoints <= 0)
                {
                    break;
                }

                var pointsUsed = Math.Min(voucher.PointsUsed, remainingPoints);
                if (pointsUsed <= 0)
                {
                    continue;
                }

                applied.Add(new AppliedVoucher
                {
                    VoucherId = voucher.VoucherId,
                    PointsUsed = pointsUsed
                });

                remainingPoints -= pointsUsed;
            }

            return applied;
        }

        private static IReadOnlyList<VoucherCandidate> SelectBestVoucherSubset(IReadOnlyList<VoucherCandidate> vouchers, int count, long pointsNeeded)
        {
            if (count <= 0 || vouchers.Count == 0 || count > vouchers.Count)
            {
                return Array.Empty<VoucherCandidate>();
            }

            // Vouchers are expected to be few. Use exhaustive selection for correctness.
            // If this grows, switch to a bounded heuristic to avoid combinatorial blow-up.
            if (vouchers.Count > 18)
            {
                return SelectBestVoucherSubsetGreedy(vouchers, count, pointsNeeded);
            }

            var ordered = vouchers
                .OrderBy(v => v.MaxValuePoints)
                .ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            IReadOnlyList<VoucherCandidate> best = Array.Empty<VoucherCandidate>();
            var bestPointsToPay = long.MaxValue;
            var bestWaste = long.MaxValue;
            var bestSum = long.MaxValue;

            var buffer = new VoucherCandidate[count];

            void Visit(int startIndex, int depth, long runningSum)
            {
                if (depth == count)
                {
                    var sum = runningSum;
                    var pointsToPay = sum >= pointsNeeded ? 0 : pointsNeeded - sum;
                    var waste = sum > pointsNeeded ? sum - pointsNeeded : 0;

                    if (pointsToPay < bestPointsToPay ||
                        (pointsToPay == bestPointsToPay && waste < bestWaste) ||
                        (pointsToPay == bestPointsToPay && waste == bestWaste && sum < bestSum))
                    {
                        bestPointsToPay = pointsToPay;
                        bestWaste = waste;
                        bestSum = sum;
                        best = buffer.ToArray();
                    }

                    return;
                }

                for (var i = startIndex; i <= ordered.Length - (count - depth); i++)
                {
                    buffer[depth] = ordered[i];
                    Visit(i + 1, depth + 1, checked(runningSum + ordered[i].MaxValuePoints));
                }
            }

            Visit(0, 0, 0);
            return best;
        }

        private static IReadOnlyList<VoucherCandidate> SelectBestVoucherSubsetGreedy(IReadOnlyList<VoucherCandidate> vouchers, int count, long pointsNeeded)
        {
            // Greedy fallback: pick the smallest vouchers that cover pointsNeeded when possible,
            // otherwise pick the largest vouchers to maximise coverage.
            var orderedAsc = vouchers
                .OrderBy(v => v.MaxValuePoints)
                .ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var orderedDesc = orderedAsc.Reverse().ToArray();

            var largest = orderedDesc.Take(count).ToArray();
            var largestSum = largest.Sum(v => v.MaxValuePoints);
            if (largestSum < pointsNeeded)
            {
                return largest.OrderBy(v => v.MaxValuePoints).ToArray();
            }

            var selected = orderedAsc.Take(count).ToList();
            var remaining = orderedAsc.Skip(count).ToList();
            var currentSum = selected.Sum(v => v.MaxValuePoints);

            // Replace the largest selected vouchers with larger remaining vouchers until we cover pointsNeeded.
            while (currentSum < pointsNeeded && remaining.Count > 0)
            {
                selected.Sort((a, b) => b.MaxValuePoints.CompareTo(a.MaxValuePoints));
                var toReplace = selected[0];

                remaining.Sort((a, b) => b.MaxValuePoints.CompareTo(a.MaxValuePoints));
                var replacement = remaining[0];

                if (replacement.MaxValuePoints <= toReplace.MaxValuePoints)
                {
                    break;
                }

                selected.RemoveAt(0);
                remaining.RemoveAt(0);

                currentSum -= toReplace.MaxValuePoints;
                currentSum += replacement.MaxValuePoints;

                selected.Add(replacement);
                remaining.Add(toReplace);
            }

            return selected.OrderBy(v => v.MaxValuePoints).ThenBy(v => v.VoucherId, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static bool TryParsePoints(string? pointsText, out long points)
        {
            return long.TryParse(pointsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out points);
        }
    }
}
