using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.FlyingClubToken;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public sealed class FlyingClubAccountSummaryProvider : IFlyingClubAccountSummaryProvider
    {
        private readonly IFlyingClubTokenProvider _tokenProvider;
        private readonly IAccountCustomerService _customerService;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _membershipProvider;
        private readonly ICustomerTransactionQueryProvider<CustomerTransactionDocument> _tierProvider;
        private readonly IVredPointProvider _pointsProvider;
        public FlyingClubAccountSummaryProvider(IFlyingClubTokenProvider tokenProvider, IAccountCustomerService customerService,
            ICustomerTransactionQueryProvider<FcMemberDetails> membershipProvider,
            ICustomerTransactionQueryProvider<CustomerTransactionDocument> tierProvider, IVredPointProvider pointsProvider)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _membershipProvider = membershipProvider ?? throw new ArgumentNullException(nameof(membershipProvider));
            _tierProvider = tierProvider ?? throw new ArgumentNullException(nameof(tierProvider));
            _pointsProvider = pointsProvider ?? throw new ArgumentNullException(nameof(pointsProvider));
        }

        public async Task<FlyingClubAccountSummary> GetAsync(string token, CancellationToken cancellationToken = default)
        {
            using var totalDeadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            totalDeadline.CancelAfter(TimeSpan.FromSeconds(5));
            FlyingClubTokenValidationResponse validation;
            try { validation = await _tokenProvider.ValidateAsync(token).WaitAsync(TimeSpan.FromSeconds(2), totalDeadline.Token); }
            catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized)
            { throw InvalidBrokerToken(); }
            catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Forbidden)
            { throw DependencyUnavailable(); }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested && IsTransient(exception))
            { throw DependencyUnavailable(); }
            if (validation?.FlyingClubAccountNumber.IsNullOrWhiteSpace() ?? true)
                throw InvalidBrokerToken();

            CustomerDTO customer;
            try { customer = await ReadAsync(() => _customerService.GetCustomer(null, validation.FlyingClubAccountNumber), totalDeadline.Token); }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw DependencyUnavailable(); }
            var customerId = customer?.Customer?.CustomerId;
            if (customerId.IsNullOrWhiteSpace()) throw UpstreamInvalid();

            var baseUrl = ConfigReader.Get(Environment.CustTransactionMsUrl).TrimEnd('/');
            var membershipPath = Common.Constants.Application.GetFcMemberDetails.Replace("{{customerId}}", Uri.EscapeDataString(customerId));
            var membershipTask = ReadAsync(() => _membershipProvider.CustomerTransactionQuery($"{baseUrl}/{membershipPath.TrimStart('/')}", customerId), totalDeadline.Token);
            var tierTask = ReadAsync(() => _tierProvider.CustomerTransactionQuery($"{baseUrl}/{customerId}/tierDetails", customerId), totalDeadline.Token);
            var pointsTask = ReadAsync(() => _pointsProvider.VredPointBalance(validation.FlyingClubAccountNumber), totalDeadline.Token);
            try { await Task.WhenAll(membershipTask, tierTask, pointsTask); }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw DependencyUnavailable(); }
            var membership = (await membershipTask)?.FlyingClubMemberDocuments?.FirstOrDefault();
            var tier = await tierTask;
            var pointsValue = (await pointsTask)?.Current;
            var points = 0L;
            if (membership is null || (tier?.TierCode.IsNullOrWhiteSpace() ?? true) ||
                !long.TryParse(pointsValue, NumberStyles.None, CultureInfo.InvariantCulture, out points) || points < 0)
                throw UpstreamInvalid();
            return new FlyingClubAccountSummary
            {
                Tier = tier.TierCode,
                VirginPoints = points,
                TierPoints = tier.TierPoints,
                TierPointsResetDate = tier.DateTierPointsReset == default ? null : DateTime.SpecifyKind(tier.DateTierPointsReset, DateTimeKind.Utc)
            };
        }

        private static async Task<T> ReadAsync<T>(Func<Task<T>> read, CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    return await read().WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
                }
                catch (HttpRequestException exception) when (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    throw DependencyUnavailable();
                }
                catch (HttpRequestException exception) when (IsRejected(exception))
                {
                    throw new RequestValidationException("ACCOUNT_SUMMARY_UPSTREAM_REJECTED");
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested && IsTransient(exception))
                {
                    if (attempt == 2) throw DependencyUnavailable();
                    var jitter = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(25, 76));
                    await Task.Delay(CappedRetryAfter(exception) + jitter, cancellationToken);
                }
            }
            throw DependencyUnavailable();
        }

        private static bool IsRejected(HttpRequestException exception) =>
            exception.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError &&
            exception.StatusCode is not HttpStatusCode.RequestTimeout and not HttpStatusCode.TooManyRequests;

        private static bool IsTransient(Exception exception) => exception is TimeoutException or TaskCanceledException ||
            exception is HttpRequestException requestException &&
            (requestException.StatusCode is null or HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests or
             HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout);

        private static TimeSpan CappedRetryAfter(Exception exception)
        {
            if (exception is not HttpRequestException { InnerException: HttpResponseException responseException }) return TimeSpan.Zero;
            var retryAfter = responseException.Response.Headers.RetryAfter;
            var delay = retryAfter?.Delta ?? (retryAfter?.Date - DateTimeOffset.UtcNow) ?? TimeSpan.Zero;
            return delay <= TimeSpan.Zero ? TimeSpan.Zero : delay > TimeSpan.FromMilliseconds(250) ? TimeSpan.FromMilliseconds(250) : delay;
        }

        private static Exception DependencyUnavailable() =>
            new("ACCOUNT_SUMMARY_DEPENDENCY_UNAVAILABLE");

        private static RequestValidationException UpstreamInvalid() =>
            new("ACCOUNT_SUMMARY_UPSTREAM_INVALID");

        internal static Exception InvalidBrokerToken() =>
            new UnauthorizedAccessException(nameof(HttpStatusCode.Unauthorized));
    }
}
