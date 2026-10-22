using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Serialization;
using VAA.EAPI.GraphQL.Domain.Account.Constants;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class VoucherMapper
    {
        private static readonly IReadOnlyDictionary<string, VoucherStatus> VoucherStatusesByDisplayText =
            Enum.GetValues<VoucherStatus>().ToDictionary(GetEnumMemberValue, value => value, StringComparer.OrdinalIgnoreCase);
        private static readonly IReadOnlyDictionary<string, VoucherTypes> VoucherTypesByDisplayText =
            Enum.GetValues<VoucherTypes>().ToDictionary(GetEnumMemberValue, value => value, StringComparer.OrdinalIgnoreCase);
        private static readonly IReadOnlyDictionary<string, VoucherSource> VoucherSourcesByDisplayText =
            Enum.GetValues<VoucherSource>().ToDictionary(GetEnumMemberValue, value => value, StringComparer.OrdinalIgnoreCase);

        public static IEnumerable<Voucher> VoucherResponseMapper(ILogger logger, string customerId, IEnumerable<Award> awardResponse, IEnumerable<TransactionDocument> transResponse, IEnumerable<ProductDetails> productRefData, IEnumerable<PartnerProfileDetails> partnerProfileRefData, int page, int pageSize)
        {
            var excludedVoucherStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Cancelled",
                "Gifted",
                "Used",
                "Expired"
            };

            var awards = awardResponse.Where(award => !string.IsNullOrEmpty(award.Status) && !excludedVoucherStatuses.Contains(award.Status)).ToList();

            if (awards.Count == 0)
                return null;

            var vouchers = awards
                .Select(award => new Voucher()
                {
                    Number = award.AwardNumber,
                    IssueDate = award.StartDate.Date.ToString(Common.Constants.Application.ISODateFormat),
                    ExpiryDate = award.ExpiryDate.Date.ToString(Common.Constants.Application.ISODateFormat),
                    Status = award.Status,
                    Type = string.IsNullOrEmpty(award.ProductId) ? null : productRefData?.FirstOrDefault(x => x.RowId == award.ProductId)?.ProductName,
                    Source = string.IsNullOrEmpty(award.TransactionId) ? Constants.Application.VirginMoney : GetSourceFromPartnerProfile(award.TransactionId, partnerProfileRefData, transResponse),
                    IsGifted = award.GiftedAwardDate != DateTime.MinValue
                })
                .Where(a => !string.IsNullOrEmpty(a.Number) && !string.IsNullOrEmpty(a.IssueDate) &&
                            !string.IsNullOrEmpty(a.ExpiryDate) &&
                            DateTime.TryParseExact(a.ExpiryDate, Common.Constants.Application.ISODateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var expiryDate) &&
                            expiryDate.Date >= DateTime.UtcNow.Date &&
                            !string.IsNullOrEmpty(a.Type) &&
                            !string.IsNullOrEmpty(a.Source))
                .ToList();

            var suppressedCount = awards.Count - vouchers.Count;
            if (suppressedCount != 0)
                logger.LogInformation($"For CustomerId: {customerId}: {suppressedCount} data are suppressed");

            if (vouchers.Count == 0)
                return null;

            var orderedVouchers = vouchers.OrderBy(x => x.ExpiryDate).ToList();
            if (page <= 0 || pageSize <= 0)
                return orderedVouchers;

            return orderedVouchers.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private static string GetSourceFromPartnerProfile(string transactionId, IEnumerable<PartnerProfileDetails> partnerProfileDetails, IEnumerable<TransactionDocument> transResponse)
        {
            var partnerId = transResponse?.FirstOrDefault(x => x.TransactionId == transactionId)?.PartnerId;
            if (string.IsNullOrEmpty(partnerId))
            {
                return Constants.Application.VirginMoney;
            }

            var partnerName = partnerProfileDetails?.FirstOrDefault(x => x.RowId == partnerId)?.Name;

            return string.IsNullOrEmpty(partnerName) ? Constants.Application.VirginMoney : partnerName;
        }


        public static VoucherDetails VoucherResponseMapper(IEnumerable<VoucherDetail> voucherResponse, int page, int pageSize)
        {
            var filteredVouchers = new List<(AccountMemberVoucher Voucher, DateTime ExpiryDate)>();
            var categories = new List<VoucherCategoryInfo>
            {
                new VoucherCategoryInfo
                {
                    Code = VoucherCategory.REWARD,
                    SortOrder = GetVoucherCategorySortOrder(VoucherCategory.REWARD)
                },
                new VoucherCategoryInfo
                {
                    Code = VoucherCategory.CLUBHOUSE,
                    SortOrder = GetVoucherCategorySortOrder(VoucherCategory.CLUBHOUSE)
                }
            };

            foreach (var voucherDetail in voucherResponse)
            {
                if (!TryParseVoucherStatus(voucherDetail.Status, out var voucherStatus) || voucherStatus != VoucherStatus.Available)
                    continue;

                if (!TryParseVoucherType(voucherDetail.VoucherName, out var voucherType))
                    continue;

                if (!TryParseDate(voucherDetail.StartDate, out var issueDate) || !TryParseDate(voucherDetail.ExpiryDate, out var expiryDate))
                    continue;

                if (string.IsNullOrEmpty(voucherDetail.VoucherNumber) || string.IsNullOrEmpty(voucherDetail.Source))
                    continue;

                if (!Enum.TryParse(voucherDetail.Category?.Trim(), true, out VoucherCategory voucherCategory))
                    continue;

                if (!TryParseVoucherSource(voucherDetail.Source, out var voucherSource))
                    continue;

                var voucher = new AccountMemberVoucher
                {
                    Type = voucherType,
                    Name = voucherDetail.VoucherName,
                    Number = voucherDetail.VoucherNumber,
                    Description = voucherDetail.Description,
                    Status = voucherStatus,
                    IssueDate = issueDate.ToString(Common.Constants.Application.ISODateTimeFormat),
                    ExpiryDate = expiryDate.ToString(Common.Constants.Application.ISODateTimeFormat),
                    Source = voucherSource,
                    Points = voucherDetail.MaxValue,
                    Category = voucherCategory,
                    IsGifted = voucherDetail.IsGifted
                };

                filteredVouchers.Add((voucher, expiryDate));
            }

            var vouchers = filteredVouchers
                .Where(v => v.ExpiryDate.Date >= DateTime.UtcNow.Date)
                .OrderBy(v => GetVoucherCategorySortOrder(v.Voucher.Category))
                .ThenBy(v => v.ExpiryDate)
                .Select(v => v.Voucher)
                .ToList();

            var pointsThreshold = vouchers
                .Select(v => int.TryParse(v.Points, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var points) ? points : 0)
                .DefaultIfEmpty(0)
                .Max();

            var memberTotalVoucherCount = vouchers.Count;
            var pagedVouchers = (page <= 0 || pageSize <= 0)
                ? vouchers
                : vouchers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new VoucherDetails
            {
                Vouchers = pagedVouchers,
                Categories = categories,
                MemberTotalVoucherCount = memberTotalVoucherCount,
                PointsThreshold = pointsThreshold
            };
        }

        private static int GetVoucherCategorySortOrder(VoucherCategory category)
        {
            return category switch
            {
                VoucherCategory.REWARD => 0,
                VoucherCategory.CLUBHOUSE => 1,
                VoucherCategory.DISCOUNT => 2,
                _ => int.MaxValue
            };
        }

        private static bool TryParseVoucherStatus(string? source, out VoucherStatus voucherStatus)
        {
            voucherStatus = default;

            if (string.IsNullOrWhiteSpace(source))
                return false;

            var trimmedSource = source.Trim();

            if (VoucherStatusesByDisplayText.TryGetValue(trimmedSource, out voucherStatus))
            {
                return true;
            }

            if (Enum.TryParse(NormalizeForEnumParse(trimmedSource), true, out voucherStatus))
            {
                return true;
            }

            voucherStatus = default;
            return false;
        }

        private static bool TryParseVoucherType(string? source, out VoucherTypes voucherType)
        {
            voucherType = default;

            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            var trimmedSource = source.Trim();

            if (VoucherTypesByDisplayText.TryGetValue(trimmedSource, out voucherType))
            {
                return true;
            }

            if (Enum.TryParse(NormalizeForEnumParse(trimmedSource), true, out voucherType))
            {
                return true;
            }

            voucherType = default;
            return false;
        }

        private static bool TryParseVoucherSource(string? source, out VoucherSource voucherSource)
        {
            voucherSource = default;

            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            var trimmedSource = source.Trim();

            if (VoucherSourcesByDisplayText.TryGetValue(trimmedSource, out var parsedVoucherSource))
            {
                voucherSource = parsedVoucherSource;
                return true;
            }

            return Enum.TryParse(NormalizeForEnumParse(trimmedSource), true, out voucherSource);
        }

        private static bool TryParseDate(string? source, out DateTime dateTime)
        {
            return DateTime.TryParse(
                source,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out dateTime);
        }

        private static string GetEnumMemberValue<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var fieldInfo = typeof(TEnum).GetField(value.ToString());
            var enumMemberAttribute = fieldInfo?.GetCustomAttribute<EnumMemberAttribute>();

            return enumMemberAttribute?.Value ?? value.ToString();
        }

        private static string NormalizeForEnumParse(string value)
        {
            return string.Join("_", value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}