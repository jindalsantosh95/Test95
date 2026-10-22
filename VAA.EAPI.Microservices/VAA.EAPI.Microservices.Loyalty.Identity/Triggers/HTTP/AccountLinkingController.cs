using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http
{
    public sealed class AccountLinkingController
    {
        private readonly IAccountLinkingProofHandler _handler;
        private readonly ILinkingTransactionValidator _transactionValidator;

        public AccountLinkingController(IAccountLinkingProofHandler handler, ILinkingTransactionValidator transactionValidator)
        {
            _handler = handler;
            _transactionValidator = transactionValidator;
        }

        [Function("CreateAccountLinkingProof")]
        [RoleAuthorization("Loyalty.Identity.AccountLinkingProof.Create")]
        [OpenApiOperation(operationId: "CreateAccountLinkingProof", tags: new[] { "Identity" }, Summary = "Creates a bound one-time account-linking proof")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(CreateLinkingProofRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CreateLinkingProofResponse))]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(List<ErrorDetails>))]
        [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(List<ErrorDetails>), Summary = "Unauthorized")]
        [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/json", typeof(List<ErrorDetails>), Summary = "Forbidden")]
        [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(List<ErrorDetails>), Summary = "Proof creation failed")]
        public async Task<HttpResponseData> CreateAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account-linking-proof")] HttpRequestData request,
            FunctionContext context,
            CancellationToken cancellationToken)
        {
            var input = await ReadAsync<CreateLinkingProofRequest>(request, cancellationToken);
            try
            {
                if (input is not null)
                {
                    input.AuthenticatedCallerApplicationId = Caller(context);
                    var transaction = _transactionValidator.ValidateCreation(input);
                    input.TransactionId = transaction.TransactionId;
                    input.TransactionBindingHash = transaction.BindingHash;
                    input.AssertionReplayId = transaction.AssertionReplayId;
                    input.Nonce = transaction.Nonce;
                    input.ProofReplayId = transaction.ProofReplayId;
                    input.ExpiresAtUtc = transaction.ExpiresAtUtc;
                }
            }
            catch (Exception exception) when (IsInvalidTransaction(exception))
            {
                return await ResultExtention<CreateLinkingProofResponse>.ExecuteResultAsync(request, InvalidTransaction<CreateLinkingProofResponse>());
            }

            return await ResultExtention<CreateLinkingProofResponse>.ExecuteResultAsync(
                request,
                await _handler.CreateAsync(input, cancellationToken));
        }

        [Function("RedeemAccountLinkingProof")]
        [RoleAuthorization("Loyalty.Identity.AccountLinkingProof.Redeem")]
        [OpenApiOperation(operationId: "RedeemAccountLinkingProof", tags: new[] { "Identity" }, Summary = "Atomically redeems a bound account-linking proof")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RedeemLinkingProofRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RedeemLinkingProofResponse))]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(List<ErrorDetails>))]
        [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(List<ErrorDetails>), Summary = "Unauthorized")]
        [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/json", typeof(List<ErrorDetails>), Summary = "Forbidden")]
        [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(List<ErrorDetails>), Summary = "Proof redemption failed")]
        public async Task<HttpResponseData> RedeemAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account-linking-proof/redeem")] HttpRequestData request,
            FunctionContext context,
            CancellationToken cancellationToken)
        {
            var input = await ReadAsync<RedeemLinkingProofRequest>(request, cancellationToken);
            try
            {
                if (input is not null)
                {
                    input.AuthenticatedCallerApplicationId = Caller(context);
                    var transaction = _transactionValidator.ValidateRedemption(input);
                    input.TransactionId = transaction.TransactionId;
                    input.TransactionBindingHash = transaction.BindingHash;
                }
            }
            catch (Exception exception) when (IsInvalidTransaction(exception))
            {
                return await ResultExtention<RedeemLinkingProofResponse>.ExecuteResultAsync(request, InvalidTransaction<RedeemLinkingProofResponse>());
            }
            return await ResultExtention<RedeemLinkingProofResponse>.ExecuteResultAsync(request, await _handler.RedeemAsync(input, cancellationToken));
        }

        private static async Task<T> ReadAsync<T>(HttpRequestData request, CancellationToken cancellationToken) where T : class
        {
            const int maximumCharacters = 32768;
            using var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8, true, 1024, leaveOpen: true);
            var buffer = new char[maximumCharacters + 1];
            var count = 0;
            while (count < buffer.Length)
            {
                var read = await reader.ReadAsync(buffer.AsMemory(count), cancellationToken);
                if (read == 0) break;
                count += read;
            }
            if (count > maximumCharacters) return null;
            var body = new string(buffer, 0, count);
            if (body.IsNullOrWhiteSpace()) return null;
            try { return Json.Deserialize<T>(body); }
            catch (JsonException) { return null; }
        }

        private static string Caller(FunctionContext context)
        {
            var principal = context.Features.Get<JwtPrincipalFeature>()?.Principal;
            return principal?.FindFirst(Constants.Application.AuthorizedPartyClaim)?.Value
                ?? principal?.FindFirst(Constants.Application.ApplicationIdClaim)?.Value;
        }

        private static bool IsInvalidTransaction(Exception exception) =>
            exception is SecurityTokenException or ArgumentException or JsonException or FormatException;

        private static Result<Response<T>> InvalidTransaction<T>() => Result.Success(new Response<T>
        {
            Status = Status.BadRequest,
            Errors = new List<ErrorDetails>
            {
                new()
                {
                    Code = "ACCOUNT_LINKING_TRANSACTION_INVALID",
                    Message = "The account-linking transaction is invalid.",
                    ErrorSource = "Consuming application"
                }
            }
        });
    }
}
