using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro
{
    public class ValidateVSClaimInputsCommandHandler : IRequestHandler<ValidateVSClaimInputsCommand, Result<Response<ValidateVSClaimInputsResponse>>>
    {
        private readonly IAzureBlobService _azureBlobService;
        private readonly ILogger<ValidateVSClaimInputsCommandHandler> _logger;

        public ValidateVSClaimInputsCommandHandler(
            IAzureBlobService azureBlobService,
            ILogger<ValidateVSClaimInputsCommandHandler> logger)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<ValidateVSClaimInputsResponse>>> Handle(
            ValidateVSClaimInputsCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.TicketDesignator.IsNullOrWhiteSpace()
                && request.FareBasis.IsNullOrWhiteSpace()
                && request.PassengerType.IsNullOrWhiteSpace())
            {
                return CreateSuccessResponse(false, Constants.Application.VSClaimInputsRequired);
            }

            try
            {
                var codes = await Task.WhenAll(
                    GetCodesAsync(Constants.Application.TicketDesignatorCodesBlobName, cancellationToken),
                    GetCodesAsync(Constants.Application.FareBasisCodesBlobName, cancellationToken),
                    GetCodesAsync(Constants.Application.PassengerTypeCodesBlobName, cancellationToken));

                if (codes[0].Contains(request.TicketDesignator)
                    || codes[1].Contains(request.FareBasis)
                    || codes[2].Contains(request.PassengerType))
                {
                    var a = CreateSuccessResponse(false, Constants.Application.BookingNotEligible);
                    return a;
                }

                return CreateSuccessResponse(true, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to load VS retro claim validation codes from blob storage.");
                return Result.Failure<Response<ValidateVSClaimInputsResponse>>("Unable to validate VS claim inputs.");
            }
        }

        private async Task<IReadOnlyCollection<string>> GetCodesAsync(string blobName, CancellationToken cancellationToken)
        {
            await _azureBlobService.DownloadBlobAsync(
                ConfigReader.Get(Constants.Environment.VSClaimInputValidationBlobStorageUri),
                ConfigReader.Get(Constants.Environment.VSClaimInputValidationBlobContainerName),
                blobName,
                ConfigReader.Get(Constants.Environment.VSClaimInputValidationBlobSasKey),
                cancellationToken);

            return await _azureBlobService.BlobJsonAsAsync<List<string>>(blobName, cancellationToken)
                ?? [];
        }

        private static Result<Response<ValidateVSClaimInputsResponse>> CreateSuccessResponse(bool success, string? errorMessage)
        {
            return Result.Success(
                new Response<ValidateVSClaimInputsResponse>
                {
                    Status = Status.OK,
                    Value = new ValidateVSClaimInputsResponse
                    {
                        Success = success,
                        ErrorMessage = errorMessage
                    }
                });
        }
    }
}
