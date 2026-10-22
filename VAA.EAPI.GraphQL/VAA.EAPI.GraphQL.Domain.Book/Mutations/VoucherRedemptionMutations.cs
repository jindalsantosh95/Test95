using HotChocolate;
using HotChocolate.Types;
using System.Net;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class VoucherRedemptionMutations
    {
        private readonly IBasketServiceProvider _basketServiceProvider;
        private readonly IVRedServiceProvider _vRedServiceProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;

        public VoucherRedemptionMutations(
            IBasketServiceProvider basketServiceProvider,
            IVRedServiceProvider vRedServiceProvider,
            ITokenB2CProvider<string> tokenB2CProvider)
        {
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _vRedServiceProvider = vRedServiceProvider ?? throw new ArgumentNullException(nameof(vRedServiceProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
        }

        [GraphQLName("setVoucherRedemptionSelection")]
        [GraphQLDescription("Stores the customer's voucher selection in the Basket.")]
        public async Task<Models.Basket.Basket> SetVoucherRedemptionSelection(SetVoucherRedemptionSelectionRequest request)
        {
            if (!ConfigReader.Try<bool>(Constants.Application.VoucherRedemptionFeatureFlag, false))
            {
                throw CreateGraphQlException("Voucher redemption is not enabled.", HttpStatusCode.NotFound);
            }

            if (request is null || request.BasketId.IsNullOrWhiteSpace())
            {
                throw CreateGraphQlException("Basket id is required.", HttpStatusCode.BadRequest);
            }

            var voucherIds = (request.SelectedVoucherIds ?? Enumerable.Empty<string>())
                .Where(id => !id.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (voucherIds.Length > 2)
            {
                throw CreateGraphQlException("At most 2 vouchers can be selected.", HttpStatusCode.BadRequest);
            }

            var basket = await _basketServiceProvider.RetrieveBasketAsync(request.BasketId);

            if (basket?.FlightsearchRequest?.isAwardSearch == true && TokenHelper.GetAuthorizationToken() == string.Empty)
                throw new Microsoft.IdentityModel.Tokens.SecurityTokenException("Missing token", new Exception(Common.Constants.Application.MissingB2CTokenErrorCode));

            if (TokenHelper.GetAuthorizationToken() != string.Empty)
            {
                var authenticatedFqtNumber = await TokenHelper.GetFQTNumber(_tokenB2CProvider);
                var basketFqtNumber = basket?.CustomerProfile?.MembershipDetails?.FQTNumber;
                if (!string.IsNullOrEmpty(basketFqtNumber) && !string.Equals(authenticatedFqtNumber, basketFqtNumber, StringComparison.OrdinalIgnoreCase))
                    throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
            }
            else if (basket?.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false)
            {
                throw new RequestValidationException(Common.Constants.Application.AccountMismatchMessage, new Exception(Common.Constants.Application.AccountMismatchErrorCode));
            }

            // An empty selection always means the customer has removed voucher redemption.
            // Do not allow an option without vouchers to keep a redemption applied.
            var option = voucherIds.Length == 0
                ? null
                : FindMatchingOption(basket?.VoucherDetails?.RedemptionOptions, voucherIds);
            if (voucherIds.Length > 0 && option is null)
            {
                throw CreateGraphQlException("The selected vouchers do not match an available redemption option.", HttpStatusCode.BadRequest);
            }

            var fqtNumber = basket.CustomerProfile?.MembershipDetails?.FQTNumber;
            var currentPoints = fqtNumber.IsNullOrWhiteSpace()
                ? null
                : (await _vRedServiceProvider.GetVirginPoints(fqtNumber)).Current;

            ApplySelection(basket!, option, voucherIds, currentPoints);
            await _basketServiceProvider.UpdateVoucherRedemptionAsync(request.BasketId, new VoucherRedemptionUpdateRequest
            {
                HasSufficientPoints = basket.HasSufficientPoints,
                VoucherPoints = basket.VoucherPoints,
                PointsToPay = basket.PointsToPay,
                VoucherApplied = basket.VoucherApplied,
                VoucherDetails = basket.VoucherDetails,
                VoucherRedemptionSelection = basket.VoucherRedemptionSelection
            });

            return basket!;
        }

        private static RedemptionOption? FindMatchingOption(IEnumerable<RedemptionOption>? options, IEnumerable<string> voucherIds)
        {
            var selectedIds = voucherIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return options?
                .Where(option => GetVoucherIds(option)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    .SetEquals(selectedIds))
                .OrderByDescending(option => option.TotalPointsUsed)
                .ThenBy(option => option.PointsToPay)
                .ThenByDescending(option => option.RedemptionType == "UPGRADE")
                .ThenByDescending(option => option.AppliedVouchers?.Count() ?? 0)
                .ThenBy(option => option.OptionId)
                .FirstOrDefault();
        }

        private static void ApplySelection(
            Models.Basket.Basket basket,
            RedemptionOption? option,
            string[] voucherIds,
            string? availableAwardPoints)
        {
            var previousOption = basket.VoucherDetails?.RedemptionOptions?
                .FirstOrDefault(redemptionOption => redemptionOption.OptionId == basket.VoucherRedemptionSelection?.SelectedOptionId);

            basket.VoucherApplied = option is not null;
            basket.VoucherRedemptionSelection = new VoucherRedemptionSelection
            {
                MaxVouchersToUse = option is null ? null : voucherIds.Length,
                SelectedVoucherIds = voucherIds,
                SelectedOptionId = option?.OptionId
            };

            if (basket.VoucherDetails is not null)
            {
                basket.VoucherDetails.BestOption = option is null
                    ? null
                    : new BestRedemptionOption
                    {
                        OptionId = option.OptionId,
                        OptionName = option.OptionName,
                        RedemptionType = option.RedemptionType,
                        SelectionReason = "Selected voucher redemption option."
                    };
            }

            var totalAwardPoints = GetTotalAwardPoints(basket, previousOption);
            var voucherPoints = option is null ? 0 : Math.Min(option.TotalPointsUsed, totalAwardPoints);
            basket.VoucherPoints = voucherPoints.ToString();
            // The option is the quote used to validate redemption in OrderMutation.
            // Persist its value rather than recomputing it from stale Basket totals.
            basket.PointsToPay = (option?.PointsToPay ?? totalAwardPoints).ToString();
            basket.HasSufficientPoints =
                long.TryParse(basket.PointsToPay, out var pointsToPay) &&
                long.TryParse(availableAwardPoints, out var availablePoints) &&
                availablePoints >= pointsToPay;
        }

        private static long GetTotalAwardPoints(Models.Basket.Basket basket, RedemptionOption? previousOption)
        {
            if (long.TryParse(basket.VoucherPoints, out var voucherPoints) &&
                long.TryParse(basket.PointsToPay, out var pointsToPay))
                return voucherPoints + pointsToPay;

            return (previousOption?.TotalPointsUsed ?? 0) + (previousOption?.PointsToPay ?? 0);
        }

        private static IEnumerable<string> GetVoucherIds(RedemptionOption option)
        {
            return (option.AppliedVouchers ?? Enumerable.Empty<AppliedVoucher>())
                .Select(voucher => voucher.VoucherId)
                .Where(id => !id.IsNullOrWhiteSpace())
                .Cast<string>();
        }

        private static GraphQLException CreateGraphQlException(string message, HttpStatusCode statusCode)
        {
            return new GraphQLException(ErrorBuilder.New()
                .SetMessage(message)
                .SetCode(((int)statusCode).ToString())
                .Build());
        }
    }
}
