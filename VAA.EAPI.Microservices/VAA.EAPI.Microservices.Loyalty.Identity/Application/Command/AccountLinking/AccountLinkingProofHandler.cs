using CSharpFunctionalExtensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountLinking
{
    public sealed class AccountLinkingProofHandler : IAccountLinkingProofHandler
    {
        private const string AccountNumberPattern = "^[0-9]{1,20}$";
        private const int MaximumLifetimeSeconds = 120;
        private readonly IAccountLinkingProofRepository _repository;
        private readonly TimeProvider _timeProvider;

        public AccountLinkingProofHandler(IAccountLinkingProofRepository repository, TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<Result<Response<CreateLinkingProofResponse>>> CreateAsync(CreateLinkingProofRequest request, CancellationToken cancellationToken)
        {
            if (request?.FlyingClubAccountNumber is null ||
                !RegexCompiled.IsMatch(request.FlyingClubAccountNumber, AccountNumberPattern, RegexOptions.CultureInvariant) ||
                !ValidTransaction(request.TransactionId, request.TransactionBindingHash) ||
                request.AssertionReplayId.IsNullOrWhiteSpace() || request.Nonce.IsNullOrWhiteSpace() || request.ProofReplayId.IsNullOrWhiteSpace())
                return Invalid<CreateLinkingProofResponse>("Invalid account-linking proof request.");

            var trustedCaller = ConfigReader.Try(Environment.AccountLinkingTrustedCallerApplicationId);
            if (trustedCaller.IsNullOrWhiteSpace())
                trustedCaller = ConfigReader.Try(Environment.FlyingClubTokenAuthorizedParty);
            if (trustedCaller.IsNullOrWhiteSpace() || !request.AuthenticatedCallerApplicationId.Is(trustedCaller))
                return Forbidden<CreateLinkingProofResponse>();

            var proofBytes = RandomNumberGenerator.GetBytes(32);
            var proof = Base64UrlEncoder.Encode(proofBytes);
            var proofHash = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(proof)));
            var consentReference = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(24));
            var remaining = request.ExpiresAtUtc - _timeProvider.GetUtcNow().UtcDateTime;
            var lifetime = Math.Min(MaximumLifetimeSeconds, (int)Math.Floor(remaining.TotalSeconds));
            if (lifetime <= 0)
                return Invalid<CreateLinkingProofResponse>("The account-linking transaction has expired.");

            DateTime expiresAtUtc;
            try
            {
                expiresAtUtc = await _repository.StoreBoundAsync(proofHash, request, consentReference, TimeSpan.FromSeconds(lifetime), cancellationToken);
            }
            catch (AccountLinkingProofConflictException)
            {
                return Invalid<CreateLinkingProofResponse>("The account-linking proof request conflicts with an existing transaction.");
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<Response<CreateLinkingProofResponse>>("Account-linking proof creation is temporarily unavailable.");
            }
            return Result.Success(new Response<CreateLinkingProofResponse>
            {
                Status = Status.OK,
                Value = new CreateLinkingProofResponse { Proof = proof, ExpiresAtUtc = expiresAtUtc }
            });
        }

        public async Task<Result<Response<RedeemLinkingProofResponse>>> RedeemAsync(RedeemLinkingProofRequest request, CancellationToken cancellationToken)
        {
            if (request?.Proof is null || request.Proof.Length != 43 ||
                !ValidTransaction(request.TransactionId, request.TransactionBindingHash))
                return Invalid<RedeemLinkingProofResponse>("Invalid or expired linking proof.");
            byte[] decoded;
            try { decoded = Base64UrlDecode(request.Proof); }
            catch (FormatException) { return Invalid<RedeemLinkingProofResponse>("Invalid or expired linking proof."); }
            if (decoded.Length != 32 || !Base64UrlEncoder.Encode(decoded).AsSpan().SequenceEqual(request.Proof.AsSpan()))
                return Invalid<RedeemLinkingProofResponse>("Invalid or expired linking proof.");

            var proofHash = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(request.Proof)));
            RedeemLinkingProofResponse value;
            try
            {
                value = await _repository.ConsumeBoundAsync(proofHash, request.TransactionId, request.TransactionBindingHash, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<Response<RedeemLinkingProofResponse>>("Account-linking proof redemption is temporarily unavailable.");
            }
            return value is null
                ? Invalid<RedeemLinkingProofResponse>("Invalid or expired linking proof.")
                : Result.Success(new Response<RedeemLinkingProofResponse> { Status = Status.OK, Value = value });
        }

        private static bool ValidTransaction(string transaction, string binding) =>
            !transaction.IsNullOrWhiteSpace() && transaction.Length <= 128 &&
            !binding.IsNullOrWhiteSpace() && binding.Length <= 128;

        private static Result<Response<T>> Invalid<T>(string message) => Result.Success(new Response<T>
        {
            Status = Status.BadRequest,
            Errors = new List<ErrorDetails> { new() { Code = "ACCOUNT_LINKING_PROOF_INVALID", Message = message, ErrorSource = "Consuming application" } }
        });

        private static Result<Response<T>> Forbidden<T>() => Result.Success(new Response<T>
        {
            Status = Status.Forbidden,
            Errors = new List<ErrorDetails> { new() { Code = "403", Message = "The calling application is not authorized.", ErrorSource = "Authentication context" } }
        });

        private static byte[] Base64UrlDecode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded += new string('=', (4 - padded.Length % 4) % 4);
            return Convert.FromBase64String(padded);
        }
    }
}
