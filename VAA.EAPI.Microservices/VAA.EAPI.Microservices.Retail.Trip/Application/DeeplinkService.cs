using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink;
using VAA.EAPI.Microservices.Retail.Trip.Domain.Enum;
using VAA.EAPI.Microservices.Retail.Trip.Helper;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public class DeeplinkService : IDeeplinkService
    {
        private readonly ILogger<DeeplinkService> _logger;
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly IDeltaEncryptionService _deltaEncryptionService;
        private readonly IDeltaEncryptionPrivateService _deltaEncryptionPrivateService;
        private readonly IBidForUpgradeOffersService _bidForUpgradeOffersService;
        private readonly bool _switchToPrivateEndpoints = ConfigReader.Try<bool>(Constant.Environment.SwitchToPrivateEndpoints);

        public DeeplinkService(ILogger<DeeplinkService> logger, IDistributedCacheService distributedCacheService, IDeltaEncryptionService deltaEncryptionService, IDeltaEncryptionPrivateService deltaEncryptionPrivateService, IBidForUpgradeOffersService bidForUpgradeOffersService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _deltaEncryptionService = deltaEncryptionService ?? throw new ArgumentNullException(nameof(deltaEncryptionService));
            _deltaEncryptionPrivateService = deltaEncryptionPrivateService ?? throw new ArgumentNullException(nameof(deltaEncryptionPrivateService));
            _bidForUpgradeOffersService = bidForUpgradeOffersService ?? throw new ArgumentNullException(nameof(bidForUpgradeOffersService));
        }


        public async Task<Result<Response<DeeplinkResponse>>> GetDeeplinkUrlDetails(DeepLinkRequestPayload request)
        {
            try
            {
                DeeplinkResponse deeplinkResponse = null;
                string proxyDeeplinkUrl = string.Empty;
                string deeplinkCacheKey = Guid.NewGuid().ToString();
                TimeSpan redisAbsoluteExpiration = TimeSpan.FromMinutes(ConfigReader.Get<double>(Constant.Environment.DeeplinkExpiryLimitInMinutes));

                //write switch case for request.SelectionOption to call respective services
                switch (request.DeeplinkOption)
                {
                    case DeeplinkOption.TravelInsurance:

                        //Get the configured URL basis POSCountry
                        if (request.POSCountryCode.IsNullOrWhiteSpace()) throw new ArgumentNullException("POSCountry is required for Travel Insurance Deeplink");

                        var configKey = string.Format(Constant.Environment.TravelInsuranceDeeplinkUrl, request.POSCountryCode);
                        proxyDeeplinkUrl = ConfigReader.Try(configKey);

                        if (proxyDeeplinkUrl.IsNullOrWhiteSpace()) throw new NotImplementedException($"Unable to locate config for '{configKey}'");

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = proxyDeeplinkUrl,
                            DeeplinkOption = DeeplinkOption.TravelInsurance
                        };

                        break;
                    case DeeplinkOption.IROP_ViewDetails or DeeplinkOption.FlyReady or DeeplinkOption.FlightChange:

                        if (request.TripReservationId.IsNullOrWhiteSpace()) throw new ArgumentNullException($"TripReservationId is required for {request.DeeplinkOption} Deeplink");

                        DeeplinkEndpointDetails cacheObject = (DeeplinkEndpointDetails)request;
                        proxyDeeplinkUrl = ConfigReader.Try(Constant.Environment.ProxyDeeplinkURL);

                        if (cacheObject is null
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.EndpointUrl)
                            || string.IsNullOrEmpty(proxyDeeplinkUrl)
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.Http_Method)
                            || cacheObject.Parameters.IsNullOrEmpty())
                        {
                            throw new NotImplementedException(string.Format("{0} option is not implemented ", request.DeeplinkOption));
                        }

                        await _distributedCacheService.Set(deeplinkCacheKey, cacheObject, false, TimeSpan.FromSeconds(0), redisAbsoluteExpiration);

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = string.Format(proxyDeeplinkUrl, deeplinkCacheKey),
                            DeeplinkOption = request.DeeplinkOption
                        };

                        break;
                    case DeeplinkOption.IROP_FAF:

                        //IROP FAF Logic
                        if (request.TripReservationId.IsNullOrWhiteSpace()) throw new ArgumentNullException("TripReservationId is required for IROP Find Alternative Flight Deeplink");

                        cacheObject = (DeeplinkEndpointDetails)request;
                        proxyDeeplinkUrl = ConfigReader.Try(Constant.Environment.ProxyDeeplinkURL);

                        if (cacheObject is null
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.EndpointUrl)
                            || string.IsNullOrEmpty(proxyDeeplinkUrl)
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.Http_Method)
                            || cacheObject.Parameters.IsNullOrEmpty())
                        {
                            throw new NotImplementedException(string.Format("{0} option is not implemented ", DeeplinkOption.IROP_FAF));
                        }

                        await _distributedCacheService.Set(deeplinkCacheKey, cacheObject, false, TimeSpan.FromSeconds(0), redisAbsoluteExpiration);

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = string.Format(proxyDeeplinkUrl, deeplinkCacheKey),
                            DeeplinkOption = DeeplinkOption.IROP_FAF
                        };

                        break;
                    case DeeplinkOption.POPSMeal:

                        //POPS Logic 
                        if (string.IsNullOrEmpty(request.TripReservationId)) throw new ArgumentNullException("TripReservationId is required for POPS Meal Link");
                        if (string.IsNullOrEmpty(request.FlightNumber)) throw new ArgumentNullException("FlightNumber is required for POPS Meal Link");

                        var encryptedToken = await DeeplinkTokenHelper.GenerateTokenForPOPSLink(request);
                        proxyDeeplinkUrl = ConfigReader.Try<string>(Constant.Environment.POPSMealUrl);

                        if (proxyDeeplinkUrl.IsNullOrWhiteSpace()) throw new NotImplementedException($"Unable to locate config for '{Constant.Environment.POPSMealUrl}'");

                        if (encryptedToken is null) throw new NotImplementedException(string.Format("{0} option is not implemented ", DeeplinkOption.POPSMeal));

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = string.Format(proxyDeeplinkUrl, encryptedToken),
                            DeeplinkOption = DeeplinkOption.POPSMeal
                        };

                        break;
                    case DeeplinkOption.FlightCancel:

                        if (request.TripReservationId.IsNullOrWhiteSpace()) throw new ArgumentNullException("TripReservationId is required for FlightCancel Deeplink");

                        var dlBookingId = await BookingIdHelper.GetBookingId(request.TripReservationId);
                        var encryptedData = _switchToPrivateEndpoints ? await _deltaEncryptionPrivateService.GetEncryptedBookingDetailsAsync(dlBookingId) : await _deltaEncryptionService.GetEncryptedBookingDetailsAsync(dlBookingId);
                        cacheObject = (DeeplinkEndpointDetails)(request, encryptedData);
                        proxyDeeplinkUrl = ConfigReader.Try(Constant.Environment.ProxyDeeplinkURL);

                        if (cacheObject is null
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.EndpointUrl)
                            || string.IsNullOrEmpty(proxyDeeplinkUrl)
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.Http_Method)
                            || cacheObject.Parameters.IsNullOrEmpty())
                        {
                            throw new NotImplementedException(string.Format("{0} option is not implemented ", DeeplinkOption.IROP_ViewDetails));
                        }

                        await _distributedCacheService.Set(deeplinkCacheKey, cacheObject, false, TimeSpan.FromSeconds(0), redisAbsoluteExpiration);

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = string.Format(proxyDeeplinkUrl, deeplinkCacheKey),
                            DeeplinkOption = DeeplinkOption.FlightCancel
                        };

                        break;
                    case DeeplinkOption.ManageHoliday:

                        if (request.TripReservationId.IsNullOrWhiteSpace()) throw new ArgumentNullException("TripReservationId is required for ManageHoliday deeplink. Required fields: TripReservationId.");

                        cacheObject = (DeeplinkEndpointDetails)request;
                        proxyDeeplinkUrl = ConfigReader.Try(Constant.Environment.ProxyDeeplinkURL);

                        if (cacheObject is null
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.EndpointUrl)
                            || string.IsNullOrEmpty(proxyDeeplinkUrl)
                            || string.IsNullOrEmpty(cacheObject?.Endpoint?.Http_Method)
                            || cacheObject.Parameters.IsNullOrEmpty())
                        {
                            throw new NotImplementedException($"{nameof(DeeplinkOption.ManageHoliday)} option is not implemented ");
                        }

                        await _distributedCacheService.Set(deeplinkCacheKey, cacheObject, false, TimeSpan.FromSeconds(0), redisAbsoluteExpiration);

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = string.Format(proxyDeeplinkUrl, deeplinkCacheKey),
                            DeeplinkOption = DeeplinkOption.ManageHoliday
                        };

                        break;
                    case DeeplinkOption.BidForUpgrade:

                        if (!string.IsNullOrEmpty(request.TripReservationId))
                        {
                            var recordLocator = await BookingIdHelper.GetPnrLocator(request.TripReservationId);
                            var surName = await BookingIdHelper.GetSurName(request.TripReservationId);

                            var bidForUpgradeResponse = await _bidForUpgradeOffersService.GetBidForUpgradeOffersAsync(new Infrastructure.Proxy.PlusGrade.BidForUpgradeRequest { Pnr = recordLocator, LastName = surName });

                            if (bidForUpgradeResponse.ResponseStatus == "ELIGIBLE" && bidForUpgradeResponse.OfferUrl.IsNullOrEmpty() == false)
                            {
                                deeplinkResponse = new DeeplinkResponse()
                                {
                                    DeeplinkUrl = bidForUpgradeResponse.OfferUrl,
                                    DeeplinkOption = DeeplinkOption.BidForUpgrade
                                };
                            }
                            else
                            {
                                return new Response<DeeplinkResponse>()
                                {
                                    Status = Status.BusinessException,
                                    Errors =
                                       [
                                        new ()
                                            {
                                             Code = "PU002",
                                             Message = "No eligible bid for upgrade offers found.",
                                             DeveloperMessage = $"No eligible bid for upgrade offers found. | response status : {bidForUpgradeResponse?.ResponseStatus} | ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                                             ErrorSource = "PlusGrade",
                                            }
                                       ]
                                };
                            }
                        }
                        else
                        {
                            return new Response<DeeplinkResponse>()
                            {
                                Status = Status.BadRequest,
                                Errors =
                                       [
                                        new ()
                                            {
                                             Code = "PU001",
                                             Message = "TripReservationId is required for BidForUpgrade Deeplink",
                                             DeveloperMessage = $"TripReservationId is required for BidForUpgrade Deeplink.| ErrorID: {AsyncContext.Current.CorrelationId} | TimeStamp: {DateTime.UtcNow}",
                                             ErrorSource = "Source System",
                                            }
                                       ]
                            };
                        }

                        break;
                    case DeeplinkOption.BaggageAllowance:

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = ConfigReader.Try<string>(Constant.Environment.BaggageAllowanceUrl),
                            DeeplinkOption = DeeplinkOption.BaggageAllowance
                        };

                        break;
                    case DeeplinkOption.FlightDisruption:

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = ConfigReader.Try<string>(Constant.Environment.FlightDisruptionUrl),
                            DeeplinkOption = DeeplinkOption.FlightDisruption
                        };

                        break;
                    case DeeplinkOption.ConciergeTripAware:

                        if (request.TripReservationId.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.TripReservationId), "TripReservationId is required for ConciergeTripAware deeplink.");

                        cacheObject = (DeeplinkEndpointDetails)request;

                        if (cacheObject.Items.IsNullOrEmpty())
                            throw new ArgumentException("TripReservationId must contain at least one valid trip reservation token for ConciergeTripAware deeplink.", nameof(request.TripReservationId));

                        proxyDeeplinkUrl = ConfigReader.Try(Constant.Environment.ProxyDeeplinkURL);

                        if (proxyDeeplinkUrl.IsNullOrWhiteSpace())
                            throw new NotImplementedException($"Unable to locate config for '{Constant.Environment.ProxyDeeplinkURL}'");

                        await _distributedCacheService.Set(deeplinkCacheKey, cacheObject, false, TimeSpan.FromSeconds(0), redisAbsoluteExpiration);

                        deeplinkResponse = new DeeplinkResponse()
                        {
                            DeeplinkUrl = string.Format(proxyDeeplinkUrl, deeplinkCacheKey),
                            DeeplinkOption = DeeplinkOption.ConciergeTripAware
                        };

                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid Deeplink Option");
                }

                return new Response<DeeplinkResponse>()
                {
                    Status = Status.OK,
                    Value = deeplinkResponse
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Deeplink option: {request.DeeplinkOption} | Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)}  - Exception -  {Json.Serialize(ex.Data["Response"])} | Trip reservation id : {request.TripReservationId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deeplink option: {request.DeeplinkOption} | Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)}  - Exception - {ex} | Trip reservation id : {request.TripReservationId}");
                throw;
            }
        }

        public async Task<Result<Response<DeeplinkEndpointDetails>>> GetDeeplinkDetailsForProxy(string cacheId)
        {
            try
            {
                var postDeeplinkDetails = await _distributedCacheService.Get<DeeplinkEndpointDetails>(cacheId);

                if (postDeeplinkDetails == null)
                {
                    throw new KeyNotFoundException("Deeplink details not found for the provided cache Id");
                }
                return Result.Success(new Response<DeeplinkEndpointDetails>
                {
                    Status = Status.OK,
                    Value = postDeeplinkDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)}  - Exception - {ex}");
                throw;
            }
        }
    }
}
