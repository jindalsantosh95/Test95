using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass;
using VAA.EAPI.Microservices.Loyalty.Wallet.Helper;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Service
{
    public class BoardingPassWalletService(ILogger<BoardingPassWalletService> logger, IAirshipWalletService airshipWalletService, IDistributedCacheService distributedCacheService) : IBoardingPassWalletService
    {
        private sealed class TemplateFlightRequest
        {
            public TemplateCode TemplateCode { get; init; }
            public FlightRequestModel Flight { get; init; }
        }

        private readonly ILogger<BoardingPassWalletService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAirshipWalletService _airshipWalletService = airshipWalletService ?? throw new ArgumentNullException(nameof(airshipWalletService));
        private readonly IDistributedCacheService _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        public async Task<Result<Response<WalletResponse>>> GenerateWalletLinksAsync(WalletPassRequest walletPassRequest)
        {
            ArgumentNullException.ThrowIfNull(walletPassRequest);
            ArgumentNullException.ThrowIfNull(walletPassRequest.Airports);
            ArgumentNullException.ThrowIfNull(walletPassRequest.Segments);
            ArgumentNullException.ThrowIfNull(walletPassRequest.BoardingPasses);
            ArgumentNullException.ThrowIfNull(walletPassRequest.Passengers);

            AirshipBoardingPassResponse airshipBoardingPassResponse = null;
            var walletPassResponses = new List<WalletPassResponse>();
            string pnrMultiUrl = null;
            string pnrMultiUrlIos = null;
            string pnrMultiUrlAndroid = null;

            try 
            {
                var adaptivePassBaseUrl = ConfigReader.Get(Constants.Environment.AirShipWalletAdaptivePassUrlPrefix);

                var flightTasks = walletPassRequest.Segments.Select(async segment =>
                {
                    if (segment?.OperatingCarrier?.CarrierCode.IsNullOrWhiteSpace() == true ||
                        !int.TryParse(segment?.OperatingCarrier?.FlightNum, out var flightNum) ||
                        segment?.ScheduledDepartureLocalDateTime is null)
                    {
                        throw new ArgumentException($"Segment is missing required operating carrier / flight number / departure date for {walletPassRequest.BoardingPasses?.FirstOrDefault()?.PnrLocator}");
                    }

                    string operatingCarrierCode = segment.OperatingCarrier.CarrierCode.Trim();
                    string flightExternalId = $"{operatingCarrierCode}-{flightNum}-{segment.ScheduledDepartureLocalDateTime.Value.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture)}".Replace(" ", "-");
                    string operatingFlightNumber = $"{operatingCarrierCode}{flightNum}";

                    var segmentBoardingPasses = walletPassRequest.BoardingPasses
                        .Where(boardingPass => boardingPass.OriginAirportCode.Is(segment.Origin.Code) &&
                                               boardingPass.OperatingFlightNumber.Is(operatingFlightNumber))
                        .ToList();

                    FlightResponse flightResponse = await _distributedCacheService.Get<FlightResponse>(flightExternalId);

                    var flightRequest = (FlightRequest)(
                        segment,
                        segmentBoardingPasses.FirstOrDefault(),
                        walletPassRequest.Airports.FirstOrDefault(x => x.AirportCode.Is(segment.Origin.Code)),
                        walletPassRequest.Airports.FirstOrDefault(x => x.AirportCode.Is(segment.Destination.Code))
                    );

                    bool airshipFlightUpdated = !AirshipWalletHelper.IsAirshipUptoDate(flightRequest, flightResponse?.Fields);

                    if (flightResponse == null || airshipFlightUpdated)
                    {
                        try
                        {
                            if (airshipFlightUpdated)
                                flightResponse = await _airshipWalletService.PatchFlightAsync(flightExternalId, flightRequest);
                            else
                                flightResponse = await _airshipWalletService.PostFlightAsync(flightExternalId, flightRequest);
                        }

                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Airship Flight API Failed. FlightExternal id : {FlightExternalId}.", flightExternalId);
                            flightResponse = await _airshipWalletService.GetFlightAsync(flightExternalId);
                        }

                        await _distributedCacheService.Set(flightExternalId, flightResponse, false, TimeSpan.FromSeconds(0), TimeSpan.FromHours(ConfigReader.Get<int>(Constants.Environment.BoardingPassSlidingExpiryHours)));
                    }

                    return segmentBoardingPasses
                        .GroupBy(bp => AirshipWalletHelper.GetTemplateCode(operatingCarrierCode, bp.CabinName))
                        .Select(group => new TemplateFlightRequest
                        {
                            TemplateCode = group.Key,
                            Flight = new FlightRequestModel
                            {
                                FlightExternalId = flightResponse.FlightExternalId,
                                Passengers = group
                                    .Select(bp => (PassengerRequest)bp)
                                    .Where(passenger => passenger is not null)
                            }
                        });
                });
                var boardingPassesByTemplate = (await Task.WhenAll(flightTasks))
                    .SelectMany(flights => flights)
                    .ToList();
                var boardingPassesBySegment = boardingPassesByTemplate.Select(flight => flight.Flight).ToList();

                var firstBoardingPass = walletPassRequest.BoardingPasses.FirstOrDefault();
                var primaryPassenger = walletPassRequest.Passengers.FirstOrDefault(x => x.PassengerFirstNIN.Is("01") && x.PassengerLastNIN.Is("01"));
                var firstSegment = walletPassRequest.Segments.FirstOrDefault(x => x.Ordinal == 0);

                if (firstBoardingPass is null || primaryPassenger is null || firstSegment?.Origin?.Code.IsNullOrWhiteSpace() == true)
                    throw new ArgumentException("Request is missing required boarding pass / primary passenger / origin segment information.");

                string boardingPassId = $"{firstBoardingPass.PnrLocator}.{primaryPassenger.Surname}.{firstSegment.Origin.Code}.{string.Join("-", boardingPassesByTemplate
                    .SelectMany(flight => flight.Flight.Passengers.Select(passenger => $"{passenger.AdaptiveLinkExternalId}:{flight.TemplateCode}"))
                    .OrderBy(templateAssignment => templateAssignment))}";

                airshipBoardingPassResponse = await _distributedCacheService.Get<AirshipBoardingPassResponse>(boardingPassId);

                if (airshipBoardingPassResponse == null || AirshipWalletHelper.HaveBoardingPassesChanged(boardingPassesBySegment, airshipBoardingPassResponse.Links))
                {
                    var responses = await Task.WhenAll(boardingPassesByTemplate
                        .GroupBy(flight => flight.TemplateCode)
                        .Select(async group => await _airshipWalletService.PostBoardingPassAsync(new AirshipBoardingPassRequest
                        {
                            IosTemplateId = AirshipWalletHelper.GetIosTemplateCode(group.Key),
                            AndroidTemplateId = AirshipWalletHelper.GetAndroidTemplateCode(group.Key),
                            Payload = new WalletPayload { Flights = group.Select(flight => flight.Flight) }
                        })));

                    airshipBoardingPassResponse = new AirshipBoardingPassResponse
                    {
                        Links = responses.SelectMany(response => response?.Links ?? [])
                    };

                    await _distributedCacheService.Set(boardingPassId, airshipBoardingPassResponse, false, TimeSpan.FromSeconds(0), TimeSpan.FromHours(ConfigReader.Get<int>(Constants.Environment.BoardingPassSlidingExpiryHours)));
                }

                var idsByPassenger = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                var idsByPassengerSeen = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                foreach (var boardingPass in walletPassRequest.BoardingPasses)
                {
                    if (boardingPass.CheckedIn && !boardingPass.QrCodeData.IsNullOrEmpty())
                    {
                        var adaptiveExternalLinkId = $"{boardingPass.OperatingFlightNumber}-{boardingPass.DepartureLocal}-{boardingPass.PnrLocator}-{boardingPass.PassengerNIN}";
                        var link = airshipBoardingPassResponse.Links.FirstOrDefault(x => x.AdaptiveLinkExternalId == adaptiveExternalLinkId);
                        if (link == null)
                        {
                            _logger.LogWarning("No Airship link found for AdaptiveLinkExternalId {AdaptiveLinkExternalId}", adaptiveExternalLinkId);
                            continue;
                        }

                        var response = (WalletPassResponse)(link, boardingPass);
                        walletPassResponses.Add(response);

                        if (!boardingPass.PassengerNIN.IsNullOrWhiteSpace() && !link.AdaptiveLinkId.IsNullOrWhiteSpace())
                        {
                            if (!idsByPassenger.TryGetValue(boardingPass.PassengerNIN, out var ids))
                            {
                                ids = new List<string>();
                                idsByPassenger[boardingPass.PassengerNIN] = ids;
                                idsByPassengerSeen[boardingPass.PassengerNIN] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            }

                            if (idsByPassengerSeen[boardingPass.PassengerNIN].Add(link.AdaptiveLinkId))
                                ids.Add(link.AdaptiveLinkId);
                        }
                    }
                }

                if (!adaptivePassBaseUrl.IsNullOrWhiteSpace() && walletPassResponses.Count > 0)
                {
                    var isMultiSegmentBooking = walletPassRequest.Segments.Skip(1).Any();

                    var passengerNinsWithIds = idsByPassenger
                        .Where(kvp => kvp.Value.Count > 0)
                        .Select(kvp => kvp.Key)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var pnrIds = passengerNinsWithIds
                        .SelectMany(passengerNin =>
                            idsByPassenger.TryGetValue(passengerNin, out var passengerIds)
                                ? passengerIds
                                : Enumerable.Empty<string>())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    // Only create a PNR-level multi-pass URL when there are at least two passengers with wallet ids
                    // (otherwise the passenger-level links already cover the use case), and the unique id list is within
                    // the Airship adaptive pass limit (max 10 ids per request).
                    if (passengerNinsWithIds.Count > 1 && pnrIds.Count > 0 && pnrIds.Count <= 10)
                    {
                        pnrMultiUrl = AirshipWalletHelper.BuildAdaptivePassUrl(adaptivePassBaseUrl, pnrIds);
                        pnrMultiUrlIos = AirshipWalletHelper.BuildAdaptivePassUrl(adaptivePassBaseUrl, pnrIds, Constants.Application.IosDeviceType);
                        pnrMultiUrlAndroid = AirshipWalletHelper.BuildAdaptivePassUrl(adaptivePassBaseUrl, pnrIds, Constants.Application.AndroidDeviceType);
                    }
                    else
                    {
                        pnrMultiUrl = null;
                        pnrMultiUrlIos = null;
                        pnrMultiUrlAndroid = null;
                    }

                    if (isMultiSegmentBooking)
                    {
                        var passengerMultiIdsByNin = idsByPassenger
                            .Where(kvp => kvp.Value.Count > 1)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => (IReadOnlyCollection<string>)kvp.Value,
                                StringComparer.OrdinalIgnoreCase);

                        foreach (var response in walletPassResponses)
                        {
                            if (response.PassengerNIN.IsNullOrWhiteSpace())
                                continue;

                            if (!passengerMultiIdsByNin.TryGetValue(response.PassengerNIN, out var passengerIds))
                                continue;

                            response.BoardingPassAdaptiveLink = AirshipWalletHelper.BuildAdaptivePassUrl(adaptivePassBaseUrl, passengerIds);
                            response.IosBoardingPassWalletLink = AirshipWalletHelper.BuildAdaptivePassUrl(adaptivePassBaseUrl, passengerIds, Constants.Application.IosDeviceType);
                            response.AndroidBoardingPassWalletLink = AirshipWalletHelper.BuildAdaptivePassUrl(adaptivePassBaseUrl, passengerIds, Constants.Application.AndroidDeviceType);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                var pnr = walletPassRequest.BoardingPasses.FirstOrDefault()?.PnrLocator;
                _logger.LogError(ex, "Airship wallet API failed. Pnr: {Pnr}.", pnr);

                return Result.Failure<Response<WalletResponse>>(
                    $"Airship wallet API failed. Pnr: {pnr}");
            }

            return Result.Success(new Response<WalletResponse>
            {
                Status = Status.OK,
                Value = new WalletResponse
                {
                    WalletPasses = walletPassResponses,
                    IosPnrMultiBoardingPassUrl = pnrMultiUrlIos,
                    AndroidPnrMultiBoardingPassUrl = pnrMultiUrlAndroid,
                    PnrMultiBoardingPassUrl = pnrMultiUrl
                }
            });
        }
    }
}
