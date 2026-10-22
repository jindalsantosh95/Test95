using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class ValidateVSClaimInputsCommandHandlerTests
    {
        private readonly InMemoryAzureBlobService _blobService = new();
        private readonly ValidateVSClaimInputsCommandHandler _handler;

        public ValidateVSClaimInputsCommandHandlerTests()
        {
            ConfigReader.Set(Constants.Environment.VSClaimInputValidationBlobStorageUri, "https://storage.example.test");
            ConfigReader.Set(Constants.Environment.VSClaimInputValidationBlobContainerName, "vs-retro");
            ConfigReader.Set(Constants.Environment.VSClaimInputValidationBlobSasKey, "test-sas");

            _handler = new ValidateVSClaimInputsCommandHandler(
                _blobService,
                NullLogger<ValidateVSClaimInputsCommandHandler>.Instance);
        }

        [Fact]
        public async Task Handle_ReturnsFalseWithBookingNotEligible_WhenTicketDesignatorIsListed()
        {
            _blobService.SetCodes(Constants.Application.TicketDesignatorCodesBlobName, ["DISCOUNT"]);
            _blobService.SetCodes(Constants.Application.FareBasisCodesBlobName, []);
            _blobService.SetCodes(Constants.Application.PassengerTypeCodesBlobName, []);

            var result = await _handler.Handle(
                new ValidateVSClaimInputsCommand(new ValidateVSClaimInputsRequest
                {
                    TicketDesignator = "DISCOUNT",
                    FareBasis = "FLEX",
                    PassengerType = "ADT"
                }),
                CancellationToken.None);

            Assert.False(result.Value.Value.Success);
            Assert.Equal(Constants.Application.BookingNotEligible, result.Value.Value.ErrorMessage);
        }

        [Fact]
        public async Task Handle_ReturnsFalseWithBookingNotEligible_WhenFareBasisOrPassengerTypeIsListed()
        {
            _blobService.SetCodes(Constants.Application.TicketDesignatorCodesBlobName, []);
            _blobService.SetCodes(Constants.Application.FareBasisCodesBlobName, ["LIGHT"]);
            _blobService.SetCodes(Constants.Application.PassengerTypeCodesBlobName, ["INF"]);

            var fareBasisResult = await _handler.Handle(
                new ValidateVSClaimInputsCommand(new ValidateVSClaimInputsRequest
                {
                    TicketDesignator = "STANDARD",
                    FareBasis = "LIGHT",
                    PassengerType = "ADT"
                }),
                CancellationToken.None);
            var passengerTypeResult = await _handler.Handle(
                new ValidateVSClaimInputsCommand(new ValidateVSClaimInputsRequest
                {
                    TicketDesignator = "STANDARD",
                    FareBasis = "FLEX",
                    PassengerType = "INF"
                }),
                CancellationToken.None);

            Assert.False(fareBasisResult.Value.Value.Success);
            Assert.Equal(Constants.Application.BookingNotEligible, fareBasisResult.Value.Value.ErrorMessage);
            Assert.False(passengerTypeResult.Value.Value.Success);
            Assert.Equal(Constants.Application.BookingNotEligible, passengerTypeResult.Value.Value.ErrorMessage);
        }

        [Fact]
        public async Task Handle_ReturnsTrue_WhenNoInputIsListed()
        {
            _blobService.SetCodes(Constants.Application.TicketDesignatorCodesBlobName, []);
            _blobService.SetCodes(Constants.Application.FareBasisCodesBlobName, []);
            _blobService.SetCodes(Constants.Application.PassengerTypeCodesBlobName, []);

            var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

            Assert.True(result.Value.Value.Success);
            Assert.Null(result.Value.Value.ErrorMessage);
        }

        [Fact]
        public async Task Handle_ReturnsTrue_WhenOnlyOneInputIsSuppliedAndNotListed()
        {
            _blobService.SetCodes(Constants.Application.TicketDesignatorCodesBlobName, []);
            _blobService.SetCodes(Constants.Application.FareBasisCodesBlobName, []);
            _blobService.SetCodes(Constants.Application.PassengerTypeCodesBlobName, []);

            var result = await _handler.Handle(
                new ValidateVSClaimInputsCommand(new ValidateVSClaimInputsRequest
                {
                    FareBasis = "FLEX"
                }),
                CancellationToken.None);

            Assert.True(result.Value.Value.Success);
            Assert.Null(result.Value.Value.ErrorMessage);
        }

        [Fact]
        public async Task Handle_ReturnsFalse_WhenNoInputsAreSupplied()
        {
            var result = await _handler.Handle(
                new ValidateVSClaimInputsCommand(new ValidateVSClaimInputsRequest()),
                CancellationToken.None);

            Assert.False(result.Value.Value.Success);
            Assert.Equal(Constants.Application.VSClaimInputsRequired, result.Value.Value.ErrorMessage);
        }

        private static ValidateVSClaimInputsCommand CreateCommand() => new(new ValidateVSClaimInputsRequest
        {
            TicketDesignator = "STANDARD",
            FareBasis = "FLEX",
            PassengerType = "ADT"
        });

        private sealed class InMemoryAzureBlobService : IAzureBlobService
        {
            private readonly Dictionary<string, List<string>> _codes = new(StringComparer.Ordinal);

            public void SetCodes(string blobName, List<string> codes) => _codes[blobName] = codes;

            public Task<T> BlobJsonAsAsync<T>(string name, CancellationToken cancellationToken)
            {
                if (_codes.TryGetValue(name, out var codes))
                {
                    return Task.FromResult((T)(object)codes);
                }

                throw new InvalidOperationException($"Codes for '{name}' have not been configured.");
            }

            public Task DownloadBlobAsync(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken) => Task.CompletedTask;

            public Task<byte[]> BlobBytesAsync(string name, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<string> BlobStringAsync(string name, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<T> BlobXmlAsAsync<T>(string name, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task WriteToBlobAsync(string name, string content, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task DownloadBlobAsync(Uri blobUri, string name, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task UploadBlobAsync(string storageAccountUrl, string container, string sasKey, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task UploadBlobAsync(Uri containerUrl, string name, string content, BlobType type, bool overwrite, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<IEnumerable<string>> GetBlobsAsync(string storageAccountUrl, string container, string sasKey, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<IEnumerable<string>> GetBlobsAsync(Uri containerUrl, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<IEnumerable<string>> FindBlobsAsync(string storageAccountUrl, string container, string sasKey, string prefix, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<IEnumerable<string>> FindBlobsAsync(Uri containerUrl, string prefix, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task DeleteBlobAsync(string storageAccountUrl, string container, string sasKey, string name, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task DeleteBlobAsync(Uri containerUrl, string name, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task Download(string storageAccountUrl, string container, string name, string sasKey, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task Download(Uri blobUri, string name, CancellationToken cancellationToken) => throw new NotSupportedException();
        }
    }
}
