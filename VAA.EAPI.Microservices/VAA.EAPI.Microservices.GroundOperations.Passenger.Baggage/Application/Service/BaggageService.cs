using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.CheckPaymentEligibility;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.OrderEmail;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.SaveBags;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.UpdateBags;
using Environment = VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Constants.Environment;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Service
{
    public class BaggageService : IBaggageService
    {
        public readonly IDeltaSsciService _deltaSsciService;
        public readonly IAzureBlobService _azureBlobService;
        public readonly IEmailService _emailService;
        private readonly ILogger<BaggageService> _logger;

        public BaggageService(IDeltaSsciService deltaSsciService, IAzureBlobService azureBlobService, ILogger<BaggageService> logger, IEmailService emailService)
        {
            _deltaSsciService = deltaSsciService ?? throw new ArgumentNullException(nameof(deltaSsciService));
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService)); ;
        }

        public async Task<Result<Response<BaggageAllowanceResponse>>> GetBaggageAllowance(string travelPartyId, bool hidePaidBaggage = false)
        {
            if (travelPartyId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(travelPartyId));

            BagsData baggageObj;
            BaggageAdditionalDetails baggageAdditionalDetails;

            var baggageTask = _deltaSsciService.GetBaggageInfoAsync(travelPartyId);

            await _azureBlobService.DownloadBlobAsync(ConfigReader.Get(Environment.BlobStorageUri), ConfigReader.Get(Environment.ContainerName), ConfigReader.Get(Environment.AllAboutBagsBlob), ConfigReader.Get(Environment.SasKey), CancellationToken.None);

            baggageAdditionalDetails = Json.Deserialize<BaggageAdditionalDetails>(await _azureBlobService.BlobStringAsync(ConfigReader.Get(Environment.AllAboutBagsBlob), CancellationToken.None));

            try
            {
                baggageObj = await baggageTask;

                if (baggageObj.PassengerBagInfo.Any(item => item.BaggagePrices != null && item.BaggagePrices.Any(j => j.DecimalPrecisionCnt != 0)))
                {
                    var decimalPrecisionValue = baggageObj.PassengerBagInfo?
                                                 .FirstOrDefault(item => item.BaggagePrices?.Any(j => j.DecimalPrecisionCnt != 0) == true)?
                                                 .BaggagePrices?
                                                 .FirstOrDefault(j => j.DecimalPrecisionCnt != 0)?
                                                 .DecimalPrecisionCnt;

                    _logger.LogInformation("DecimalPrecisionCnt found with non-zero values for {TravelPartyId}; value: {DecimalPrecisionCount}.",
                        travelPartyId, decimalPrecisionValue);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Baggage allowance retrieval failed for {TravelPartyId}.", travelPartyId);

                return Result.Success(new Response<BaggageAllowanceResponse>
                {
                    Status = Status.OK,
                    Value = new BaggageAllowanceResponse { ShowBaggageAllowance = false, TravelPartyId = travelPartyId }
                });
            }

            var baggageAllowanceResponse = (BaggageAllowanceResponse)(baggageObj, baggageAdditionalDetails, hidePaidBaggage);
            
            baggageAllowanceResponse.TravelPartyId = travelPartyId;

            return Result.Success(new Response<BaggageAllowanceResponse> { Status = Status.OK, Value = baggageAllowanceResponse });
        }

        public async Task<Result<Response<SaveBaggageResponse>>> SaveBagsAsync(SaveBaggageRequest request)
        {
            if (request.TravelPartyId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.TravelPartyId));

            var baggageObj = await _deltaSsciService.SaveBaggageAsync(request.TravelPartyId, (UpdateBagsRequest)request);

            if (baggageObj.NextAction.Is("Success"))
            {
                return Result.Success(new Response<SaveBaggageResponse> { Status = Status.OK, Value = new SaveBaggageResponse { TravelPartyId = request.TravelPartyId } });
            }

            return Result.Success(new Response<SaveBaggageResponse> { Status = Status.Error });
        }

        public async Task<Result<Response<CheckPaymentResponse>>> CheckPaymentAsync(CheckPaymentRequest request)
        {
            if (request.TravelPartyId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.TravelPartyId));

            try
            {
                var baggageObj = await _deltaSsciService.CheckPaymentAsync(request.TravelPartyId, (UpdateBagsRequest)request);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Baggage payment eligibility check failed for {TravelPartyId}.", request?.TravelPartyId);
                return Result.Success(new Response<CheckPaymentResponse> { Status = Status.Error });
            }

            return Result.Success(new Response<CheckPaymentResponse> { Status = Status.OK, Value = new CheckPaymentResponse { TravelPartyId = request.TravelPartyId } });
        }

        public async Task<Result<Response<ConfirmBaggageResponse>>> ConfirmBaggageAsync(ConfirmBaggageRequest request)
        {
            BagsPurchaseReceipt bagsPurchaseReceipt;

            try
            {
                bagsPurchaseReceipt = await _deltaSsciService.ConfirmBaggageAsync(request.TravelPartyId, (PurchaseRequest)request);

                if (bagsPurchaseReceipt == null)
                {
                    _logger.LogError("Confirm baggage operation failed for {PnrLocator}: downstream response was null.", request?.PnrLocator);

                    return Result.Success(new Response<ConfirmBaggageResponse>
                    {
                        Status = Status.Error,
                        Errors =
                        [
                            new ErrorDetails
                            {
                                Title = "Baggage payment failed",
                                Message = "Baggage payment failed",
                                DeveloperMessage = "Downstream response from SSCI was null"
                            }
                        ]
                    });
                }

                if (!bagsPurchaseReceipt.NextAction.Is("Success") || !bagsPurchaseReceipt.ErrorMsg.IsNullOrEmpty())
                {
                    _logger.LogWarning("Baggage payment failed for PNR: {PnrLocator} with next action {NextAction}.", request?.PnrLocator, bagsPurchaseReceipt?.NextAction);

                    return Result.Success(new Response<ConfirmBaggageResponse>
                    {
                        Status = Status.Error,
                        Errors =
                        [
                            new ErrorDetails
                            {
                                Title = "Baggage payment failed",
                                Message = "Baggage payment failed",
                                DeveloperMessage = bagsPurchaseReceipt?.ErrorMsg
                            }
                        ]
                    });
                }

                if (!HasValidBaggageFeeProduct(bagsPurchaseReceipt))
                {
                    _logger.LogWarning(
                        "Baggage payment receipt did not contain valid baggage fee products for {Pnr}.",
                        request?.PnrLocator);

                    return Result.Success(new Response<ConfirmBaggageResponse>
                    {
                        Status = Status.Error,
                        Errors =
                        [
                            new ErrorDetails
                            {
                                Title = "Baggage payment failed",
                                Message = "Baggage payment failed",
                                DeveloperMessage = "Downstream receipt did not contain valid baggage fee products."
                            }
                        ]
                    });
                }

                if (bagsPurchaseReceipt.NextAction.Is("Success") && bagsPurchaseReceipt.PassengerList?.SelectMany(p => p.ProductList).Sum(p => decimal.TryParse(p.ProductTotalAmt, out var amt) ? amt : 0) > 0)
                {
                    _ = _emailService.AddEventForEmailReceiptAsync((BagsOrderEmailRequest)request);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Confirm baggage operation failed for {PnrLocator}.", request?.PnrLocator);
                return Result.Success(new Response<ConfirmBaggageResponse> { Status = Status.Error });
            }

            return Result.Success(new Response<ConfirmBaggageResponse> { Status = Status.OK, Value = (ConfirmBaggageResponse)bagsPurchaseReceipt });
        }

        private static bool HasValidBaggageFeeProduct(BagsPurchaseReceipt receipt) =>
            receipt.PassengerList?.Any(passenger =>
            {
                var baggageFeeProduct = passenger.ProductList?.FirstOrDefault(product => product.Description.Is("Baggage Fee"));

                return baggageFeeProduct is not null &&
                    Domain.ConfirmBaggage.Passenger.TryGetBagAmount(
                        baggageFeeProduct.ProductTotalAmt,
                        baggageFeeProduct.ProductTotalAmtDecimal,
                        out _);
            }) == true;
    }
}
