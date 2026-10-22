using System.Globalization;
using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Query.Store;

public class StoreHandler : IStoreHandler
{
    private readonly IStoreRepository _storeLocatorRepository;
    private readonly IGoogleService _googleDistanceMatrixService;

    public StoreHandler(IStoreRepository storeLocatorRepository, IGoogleService googleDistanceMatrixService)
    {
        _storeLocatorRepository = storeLocatorRepository;
        _googleDistanceMatrixService = googleDistanceMatrixService;
    }

    public async Task<Result<Response<NearbyStoreResult>>> GetNearByStoresAsync(StoreSearchRequest request)
    {
        ValidateOrigin(request);

        var stores = (await _storeLocatorRepository.GetStoresAsync()).ToList();
        if (stores.Count == 0)
        {
            return Result.Failure<Response<NearbyStoreResult>>("No stores found.");
        }

        var normalizedSearchParameter = request.SearchParameter.Trim();
        var isCoordinateSearch = IsCoordinateSearch(normalizedSearchParameter);
        var distanceByStore = await _googleDistanceMatrixService.GetDistanceMetricsAsync(normalizedSearchParameter, stores);

        var rankedStores = stores
            .Select(store => new
            {
                Store = store,
                SearchRank = GetStoreSearchRank(store, normalizedSearchParameter),
                DistanceInMeters = distanceByStore.TryGetValue(store.StoreId, out var result) ? result.DistanceInMeters : DistanceMatrixResult.UnknownDistanceInMeters
            })
            .ToList();

        var hasAnySearchMatch = rankedStores.Any(store => store.SearchRank < 4);
        var hasAnyDistanceMatch = rankedStores.Any(store => store.DistanceInMeters < DistanceMatrixResult.UnknownDistanceInMeters);
        var shouldUseBluewaterFallback = !isCoordinateSearch && !hasAnySearchMatch && !hasAnyDistanceMatch;

        var orderedStores = shouldUseBluewaterFallback
            ? rankedStores
                .OrderBy(store => IsBluewaterStore(store.Store) ? 0 : 1)
                .ThenBy(store => store.DistanceInMeters)
                .ThenBy(store => store.Store.StoreId)
            : isCoordinateSearch
            ? rankedStores
                .OrderBy(store => store.DistanceInMeters)
                : hasAnySearchMatch
            ? rankedStores
               .OrderBy(store => store.SearchRank)
               .ThenBy(store => store.DistanceInMeters)
            : rankedStores
              .OrderBy(store => store.DistanceInMeters);

        var sortedStores = orderedStores
            .Select(store => store.Store)
            .ToList();

        for (var index = 0; index < sortedStores.Count; index++)
        {
            var store = sortedStores[index];

            if (distanceByStore.TryGetValue(store.StoreId, out var distance))
            {
                if (!string.IsNullOrWhiteSpace(distance.DistanceText))
                {
                    store.Distance = distance.DistanceText;
                }

                if (!string.IsNullOrWhiteSpace(distance.DurationText))
                {
                    store.DrivingTime = distance.DurationText;
                }
            }

            store.IsClosest = index == 0;

            if (!store.IsClosest)
            {
                store.FullDetails = null;
            }
        }

        var response = new Response<NearbyStoreResult>
        {
            Status = Status.OK,
            Value = new NearbyStoreResult
            {
                Stores = sortedStores
            }
        };

        return Result.Success(response);
    }

    public async Task<Result<Response<StoreResult>>> GetStoreByIdAsync(StoreSearchRequest request)
    {
        ValidateOrigin(request);
        ValidateStoreId(request);

        var store = await _storeLocatorRepository.GetStoreByIdAsync(request.StoreId);
        if (store is null)
        {
            return Result.Failure<Response<StoreResult>>($"Store with id {request.StoreId} was not found.");
        }

        var distanceByStore = await _googleDistanceMatrixService.GetDistanceMetricsAsync(request.SearchParameter.Trim(), new[] { store });

        if (distanceByStore.TryGetValue(store.StoreId, out var distance))
        {
            if (!string.IsNullOrWhiteSpace(distance.DistanceText))
            {
                store.Distance = distance.DistanceText;
            }

            if (!string.IsNullOrWhiteSpace(distance.DurationText))
            {
                store.DrivingTime = distance.DurationText;
            }
        }

        store.IsClosest = true;

        return Result.Success(new Response<StoreResult>
        {
            Status = Status.OK,
            Value = store
        });
    }

    public async Task<Result<Response<StoreSlotsResult>>> GetStoreSlotsByDateAsync(StoreSearchRequest request)
    {
        ValidateStoreId(request);
        ValidateSelectedDate(request);

        var store = await _storeLocatorRepository.GetStoreByIdAsync(request.StoreId);
        if (store is null)
        {
            return Result.Failure<Response<StoreSlotsResult>>($"Store with id {request.StoreId} was not found.");
        }

        var slots = await _storeLocatorRepository.GetStoreSlotsByDateAsync(request.StoreId, request.SelectedDate.Date);

        return Result.Success(new Response<StoreSlotsResult>
        {
            Status = Status.OK,
            Value = new StoreSlotsResult
            {
                StoreId = store.StoreId,
                Slots = slots
            }
        });
    }

    public async Task<Result<Response<StoreCalendarAvailabilityResult>>> GetStoreCalendarAvailabilityAsync(StoreSearchRequest request)
    {
        ValidateStoreId(request);
        ValidateYearMonth(request);

        var store = await _storeLocatorRepository.GetStoreByIdAsync(request.StoreId);
        if (store is null)
        {
            return Result.Failure<Response<StoreCalendarAvailabilityResult>>($"Store with id {request.StoreId} was not found.");
        }

        var days = await _storeLocatorRepository.GetStoreCalendarAvailabilityAsync(request.StoreId, request.Year, request.Month);

        return Result.Success(new Response<StoreCalendarAvailabilityResult>
        {
            Status = Status.OK,
            Value = new StoreCalendarAvailabilityResult
            {
                StoreId = store.StoreId,
                Year = request.Year,
                Month = request.Month,
                Days = days
            }
        });
    }

    private static void ValidateOrigin(StoreSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SearchParameter))
        {
            throw new ArgumentException("Provide either postOrCity or latitude/longitude.", nameof(request));
        }
    }

    private static void ValidateStoreId(StoreSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StoreId))
        {
            throw new ArgumentException("StoreId is required.", nameof(request));
        }
    }

    private static void ValidateSelectedDate(StoreSearchRequest request)
    {
        if (request.SelectedDate == DateTime.MinValue)
        {
            throw new ArgumentException("SelectedDate is required.", nameof(request));
        }
    }
    private static void ValidateYearMonth(StoreSearchRequest request)
    {
        if (request.Year < 1)
        {
            throw new ArgumentException("Year is required.", nameof(request));
        }

        if (request.Month is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.", nameof(request));
        }
    }
    private static int GetStoreSearchRank(StoreResult store, string searchParameter)
    {
        var candidates = GetSearchCandidates(store);
        var bestRank = 4;

        foreach (var candidate in candidates)
        {
            var rank = GetCandidateRank(candidate, searchParameter);
            if (rank < bestRank)
            {
                bestRank = rank;
            }

            if (bestRank == 0)
            {
                break;
            }
        }

        return bestRank;
    }

    private static IEnumerable<string> GetSearchCandidates(StoreResult store)
    {
        if (!string.IsNullOrWhiteSpace(store.Name))
        {
            yield return store.Name;
        }

        if (!string.IsNullOrWhiteSpace(store.Label))
        {
            yield return store.Label;
        }

        var address = store.FullDetails?.Details?.Address;
        if (address is null)
        {
            yield break;
        }

        var addressCandidates = new[]
        {
            address.AddressLine1,
            address.AddressLine2,
            address.City,
            address.County,
            address.PostalCode,
            address.CountryCode
        };

        foreach (var addressLine in addressCandidates.Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            yield return addressLine;
        }
    }

    private static int GetCandidateRank(string candidate, string searchParameter)
    {
        var normalizedCandidate = Normalize(candidate);
        var normalizedSearch = Normalize(searchParameter);

        if (string.IsNullOrEmpty(normalizedCandidate) || string.IsNullOrEmpty(normalizedSearch))
        {
            return 4;
        }

        if (normalizedCandidate.Equals(normalizedSearch, StringComparison.Ordinal))
        {
            return 0;
        }

        if (normalizedCandidate.StartsWith(normalizedSearch, StringComparison.Ordinal))
        {
            return 1;
        }

        if (normalizedCandidate.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(word => word.StartsWith(normalizedSearch, StringComparison.Ordinal)))
        {
            return 2;
        }

        if (normalizedCandidate.Contains(normalizedSearch, StringComparison.Ordinal))
        {
            return 3;
        }

        return 4;
    }
    private static string Normalize(string value)
    {
        var normalizedChars = value
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) ? character : ' ')
            .ToArray();

        return string.Join(' ', new string(normalizedChars)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool IsBluewaterStore(StoreResult store)
    {
        return GetSearchCandidates(store)
            .Select(Normalize)
            .Any(candidate => candidate.Contains(Constants.Constants.NormalizedBluewater, StringComparison.Ordinal));
    }

    private static bool IsCoordinateSearch(string searchParameter)
    {
        var coordinateParts = searchParameter.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (coordinateParts.Length != 2)
        {
            return false;
        }

        if (!double.TryParse(coordinateParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(coordinateParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return false;
        }

        return latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
    }
}
