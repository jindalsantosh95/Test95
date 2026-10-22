using HotChocolate.Language;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Middleware
{
    /// <summary>
    /// Applies the request-boundary contract that is unique to account-linking proof creation.
    /// Global authentication remains operation-agnostic and performs authoritative token validation.
    /// </summary>
    public sealed class AccountLinkingProofRequestMiddleware : IFunctionsWorkerMiddleware
    {
        private const string OperationField = "accountCreateLinkingProof";
        private static readonly JwtSecurityTokenHandler JwtHandler = new();

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var inspection = await InspectOperation(context);
            if (!inspection.ContainsProof)
            {
                await next(context);
                return;
            }

            if (!inspection.IsValid)
            {
                SetError(context, HttpStatusCode.BadRequest, "The account-linking operation shape is invalid", "400");
                return;
            }

            var authorization = context.GetHeader("authorization");
            if (authorization.IsNullOrWhiteSpace())
            {
                await next(context);
                return;
            }

            var principalFeature = context.Features.Get<JwtPrincipalFeature>();
            var principal = principalFeature?.Principal;
            if (!TryGetBearerToken(authorization, out var token) ||
                !(principal?.Identity?.IsAuthenticated ?? false) ||
                !StringComparer.Ordinal.Equals(principalFeature?.AccessToken?.Token, token) ||
                !AccountLinkingProofTokenBoundary.HasTrustedIssuerAudience(token))
            {
                SetError(context, HttpStatusCode.Unauthorized, "Authorization token is missing/invalid", "401");
                return;
            }

            await next(context);
        }

        private static bool TryGetBearerToken(string value, out string token)
        {
            token = null;
            if (value.Contains(',') ||
                !value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) || value.Length <= 7 ||
                value.AsSpan(7).Contains(' ')) return false;

            token = value[7..];
            return JwtHandler.CanReadToken(token);
        }

        private static async Task<(bool ContainsProof, bool IsValid)> InspectOperation(FunctionContext context)
        {
            var request = await context.GetHttpRequestDataAsync();
            if (request is null || !request.Url.AbsolutePath.TrimEnd('/').Is("/api/v1/GraphQL"))
                return (false, false);

            var bodyStream = request.Body;
            var originalPosition = bodyStream.CanSeek ? bodyStream.Position : 0;
            var bytes = new byte[AccountLinkingProofRequestLimitFeature.MaximumInspectionBytes + 1];
            var count = 0;
            while (count < bytes.Length)
            {
                var read = await bodyStream.ReadAsync(bytes.AsMemory(count), context.CancellationToken);
                if (read == 0) break;
                count += read;
            }
            if (bodyStream.CanSeek)
                bodyStream.Position = originalPosition;
            else
                context.Features.Set<IHttpRequestDataFeature>(new BufferedHttpRequestDataFeature(
                    new BufferedHttpRequestData(request, new PrefixReadStream(bytes, count, bodyStream))));
            if (count > AccountLinkingProofRequestLimitFeature.MaximumInspectionBytes)
            {
                // Only the new proof resolver rejects this marker; unrelated operations keep their full original body.
                context.Features.Set(new AccountLinkingProofRequestLimitFeature());
                return (false, false);
            }
            using var buffer = new MemoryStream(bytes, 0, count, false);
            string body;
            using (var reader = new StreamReader(buffer, Encoding.UTF8, true, 1024, true))
                body = await reader.ReadToEndAsync();

            var containsProof = false;
            try
            {
                var payload = body.IsNullOrWhiteSpace() ? new JObject() : JToken.Parse(body);
                if (payload is JArray batch)
                {
                    foreach (var item in batch)
                    {
                        if (item is not JObject batchEnvelope) continue;
                        if (ContainsProof(batchEnvelope, request.Url)) return (true, false);
                    }
                    return (false, false);
                }

                if (payload is not JObject envelope) return (false, false);
                if (!ContainsProof(envelope, request.Url)) return (false, false);
                containsProof = true;
                if (envelope.Properties().Where(property => property.Name.Is("query") || property.Name.Is("operationName") || property.Name.Is("pqId"))
                    .GroupBy(property => property.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
                    return (true, false);
                var (operationQuery, resolved) = ResolveQuery(envelope, request.Url);
                if (!resolved) return (true, false);
                if (operationQuery.IsNullOrWhiteSpace()) return (false, false);

                var document = Utf8GraphQLParser.Parse(operationQuery);
                if (!GraphQLDocumentInspector.ContainsField(document, OperationField)) return (false, false);

                var operationName = envelope.GetValue("operationName", StringComparison.OrdinalIgnoreCase)?.Value<string>();
                var valid = resolved && request.Method.Is("POST") && !operationName.IsNullOrWhiteSpace() &&
                    document.Definitions.Count == 1 && document.Definitions[0] is OperationDefinitionNode operation &&
                    operation.Name?.Value == operationName && operation.SelectionSet.Selections.Count == 1 &&
                    operation.SelectionSet.Selections[0] is FieldNode field && field.Name.Value == OperationField &&
                    field.Alias is null && operation.Directives.Count == 0 && field.Directives.Count == 0;
                return (true, valid);
            }
            catch (Exception exception) when (exception is JsonException or SyntaxException or ArgumentException or InvalidCastException)
            {
                return (containsProof, false);
            }
        }

        private static bool ContainsProof(JObject envelope, Uri requestUrl)
        {
            // Inspect every casing variant before rejecting ambiguity, but leave unrelated payloads unchanged.
            foreach (var property in envelope.Properties())
            {
                if (property.Name.Is("query") && HasProofField(property.Value.Type == JTokenType.String ? property.Value.Value<string>() : null))
                    return true;
                if (property.Name.Is("pqId") && property.Value.Type == JTokenType.String)
                {
                    if (HasPersistedProof(new JObject { ["pqId"] = property.Value }, requestUrl)) return true;
                }
            }
            return HasPersistedProof(new JObject(), requestUrl);
        }

        private static bool HasPersistedProof(JObject envelope, Uri requestUrl)
        {
            try { return HasProofField(ResolveQuery(envelope, requestUrl).Query); }
            catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException) { return false; }
        }

        private static bool HasProofField(string query)
        {
            if (query.IsNullOrWhiteSpace()) return false;
            try { return GraphQLDocumentInspector.ContainsField(Utf8GraphQLParser.Parse(query), OperationField); }
            catch (SyntaxException) { return false; }
        }

        private static (string Query, bool Resolved) ResolveQuery(JObject envelope, Uri requestUrl)
        {
            var queryParameters = System.Web.HttpUtility.ParseQueryString(requestUrl.Query);
            var persistedQueryId = queryParameters["pqId"] ?? envelope.GetValue("pqId", StringComparison.OrdinalIgnoreCase)?.Value<string>();
            if (!persistedQueryId.IsNullOrWhiteSpace())
            {
                var inlineQuery = envelope.GetValue("query", StringComparison.OrdinalIgnoreCase)?.Value<string>();
                if (!persistedQueryId.Is(Path.GetFileName(persistedQueryId)) ||
                    !inlineQuery.IsNullOrWhiteSpace())
                    return (null, false);

                var queryStore = ConfigReader.Try(Common.Constants.Environment.PersistedQueryStore);
                if (queryStore.IsNullOrWhiteSpace()) return (null, false);
                var queryFile = Path.Combine(
                    Common.Constants.Application.FunctionAppDirPath,
                    queryStore,
                    $"{persistedQueryId}.graphql");
                try
                {
                    return File.Exists(queryFile) ? (File.ReadAllText(queryFile), true) : (null, false);
                }
                catch (IOException)
                {
                    return (null, false);
                }
                catch (UnauthorizedAccessException)
                {
                    return (null, false);
                }
            }
            return (envelope.GetValue("query", StringComparison.OrdinalIgnoreCase)?.Value<string>(), true);
        }

        private static void SetError(FunctionContext context, HttpStatusCode statusCode, string message, string code) =>
            context.SetHttpResponseData(statusCode, (string)Json.Serialize(new
            {
                Errors = new[] { new { Message = message, Extensions = new { Code = code } } }
            }));

        private sealed class BufferedHttpRequestDataFeature(HttpRequestData request) : IHttpRequestDataFeature
        {
            public ValueTask<HttpRequestData?> GetHttpRequestDataAsync(FunctionContext context) =>
                ValueTask.FromResult<HttpRequestData?>(request);
        }

        private sealed class PrefixReadStream(byte[] prefix, int count, Stream remainder) : MemoryStream
        {
            private bool _buffered;
            private bool _buffering;

            // The existing gateway consumes MemoryStream.ToArray(). Defer complete buffering
            // until downstream consumption, keeping account-linking inspection bounded.
            private void EnsureBuffered()
            {
                if (_buffered || _buffering) return;
                _buffering = true;
                try
                {
                    base.Write(prefix, 0, count);
                    remainder.CopyToAsync(this).GetAwaiter().GetResult();
                    base.Position = 0;
                    _buffered = true;
                }
                finally { _buffering = false; }
            }

            public override byte[] ToArray() { EnsureBuffered(); return base.ToArray(); }
            public override long Length { get { EnsureBuffered(); return base.Length; } }
            public override long Position
            {
                get { EnsureBuffered(); return base.Position; }
                set { EnsureBuffered(); base.Position = value; }
            }
            public override long Seek(long offset, SeekOrigin origin) { EnsureBuffered(); return base.Seek(offset, origin); }
            public override int Read(byte[] buffer, int offset, int length) { EnsureBuffered(); return base.Read(buffer, offset, length); }
            public override int Read(Span<byte> buffer) { EnsureBuffered(); return base.Read(buffer); }
            public override int ReadByte() { EnsureBuffered(); return base.ReadByte(); }
            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            { EnsureBuffered(); return base.ReadAsync(buffer, cancellationToken); }
            public override Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken) =>
                ReadAsync(buffer.AsMemory(offset, length), cancellationToken).AsTask();
            public override void CopyTo(Stream destination, int bufferSize) { EnsureBuffered(); base.CopyTo(destination, bufferSize); }
            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            { EnsureBuffered(); return base.CopyToAsync(destination, bufferSize, cancellationToken); }
        }

        private sealed class BufferedHttpRequestData(HttpRequestData source, Stream body) : HttpRequestData(source.FunctionContext)
        {
            public override Stream Body => body;
            public override HttpHeadersCollection Headers => source.Headers;
            public override IReadOnlyCollection<IHttpCookie> Cookies => source.Cookies;
            public override Uri Url => source.Url;
            public override IEnumerable<System.Security.Claims.ClaimsIdentity> Identities => source.Identities;
            public override string Method => source.Method;
            public override HttpResponseData CreateResponse() => source.CreateResponse();
        }
    }
}
