using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonEnvironment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public sealed class VoucherRedemptionProvider : IVoucherRedemptionProvider
    {
        private readonly IHttpConnector _connector;
        private readonly IOAuthTokenProvider _tokenProvider;

        public VoucherRedemptionProvider(IHttpConnector connector, IOAuthTokenProvider tokenProvider)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public async Task RedeemAsync(VoucherRedemptionRequest request)
        {
            using var redemptionResponse = await SendAsync("redeemvoucher", HttpMethod.Post, request.CustomerId, request, noWait: true);
            redemptionResponse.EnsureSuccessStatusCode();
            try
            {
                var callbackUri = GetVRedCallbackUri(redemptionResponse, "Voucher redemption");
                await InvokeVRedCallbackAsync(callbackUri);
            }
            catch (Exception exception)
            {
                throw new VoucherRedemptionAcceptedException(request.VoucherDetails?.VoucherNumber, exception);
            }
        }

        public Task UpdatePnrAsync(VoucherPnrUpdateRequest request) => SendAndEnsureSuccessAsync("voucherRelatedTxnsPnr", HttpMethod.Patch, request.CustomerId, request);

        public async Task RollbackAsync(VoucherRollbackRequest request)
        {
            using var rollbackResponse = await SendAsync("rollbackVoucherRedemption", HttpMethod.Post, request.CustomerId, request, noWait: true);
            rollbackResponse.EnsureSuccessStatusCode();

            var callbackUri = GetVRedCallbackUri(rollbackResponse, "Voucher rollback");
            await InvokeVRedCallbackAsync(callbackUri);
        }

        private async Task SendAndEnsureSuccessAsync(string operation, HttpMethod method, string customerId, object request)
        {
            using var response = await SendAsync(operation, method, customerId, request);
            response.EnsureSuccessStatusCode();
        }

        private static Uri GetVRedCallbackUri(HttpResponseMessage operationResponse, string operationName)
        {
            var callbackUri = operationResponse.Headers.Location;
            if (callbackUri is null || !callbackUri.IsAbsoluteUri || !IsVRedCallbackUri(callbackUri))
            {
                throw new InvalidOperationException($"{operationName} did not return a valid VRed callback location.");
            }

            return callbackUri;
        }

        private async Task InvokeVRedCallbackAsync(Uri callbackUri)
        {
            var vRedToken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(CommonEnvironment.VRedAppId));
            var vRedHeaders = new Dictionary<string, string>
            {
                { Application.OcpKeyHeader, ConfigReader.Get(CommonEnvironment.VRedOcpSubID) }
            };

            using var callbackResponse = await _connector.SendAsync(
                callbackUri.ToString(),
                vRedHeaders,
                vRedToken,
                HttpMethod.Get,
                string.Empty,
                noWait: true);
            callbackResponse.EnsureSuccessStatusCode();
        }

        private async Task<HttpResponseMessage> SendAsync(string operation, HttpMethod method, string customerId, object request, bool noWait = false)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("A customer id is required for voucher redemption.", nameof(customerId));

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(CommonEnvironment.CustTransAppId));
            var headers = new Dictionary<string, string>
            {
                { Application.OcpKeyHeader, ConfigReader.Get(CommonEnvironment.CustTransOcpKey) }
            };
            var uri = $"{ConfigReader.Get(CommonEnvironment.CustTransactionMsUrl).TrimEnd('/')}/{customerId}/{operation}";

            return await _connector.SendAsync(uri, headers, token, method, Json.Serialize(request), noWait);
        }

        private static bool IsVRedCallbackUri(Uri callbackUri)
        {
            if (!Uri.TryCreate(ConfigReader.Get(CommonEnvironment.VredMsUrl), UriKind.Absolute, out var vRedBaseUri))
            {
                return false;
            }

            var vRedBasePath = $"{vRedBaseUri.AbsolutePath.TrimEnd('/')}/";
            return vRedBaseUri.Scheme.Is(callbackUri.Scheme)
                   && vRedBaseUri.Host.Is(callbackUri.Host)
                   && vRedBaseUri.Port == callbackUri.Port
                   && callbackUri.AbsolutePath.StartsWith(vRedBasePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
