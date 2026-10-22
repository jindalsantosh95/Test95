using System.Security.Claims;
using HotChocolate;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using InfraAppointmentBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.AppointmentBooking.AppointmentBookingResponse;
using InfraNearbyStoresResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.NearbyStoresResponse;
using InfraStoreCalendarAvailabilityRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreCalendarAvailabilityRecord;
using InfraStoreRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreRecord;
using InfraStoreSearchRequest = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreSearchRequest;
using InfraStoreSlotsRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreSlotsRecord;
using InfraStoreManagerAvailabilityWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.AvailabilityWindowResponse;
using InfraStoreManagerBookingCancellationRequest = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.BookingCancellationRequest;
using InfraStoreManagerManageBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageBookingResponse;
using InfraStoreManagerManageSlotAllocationResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageSlotAllocationResponse;
using InfraStoreManagerOpeningHoursEntryResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.OpeningHoursEntryResponse;
using InfraStoreManagerSlotRuleResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.SlotRuleResponse;
using InfraStoreManagerStoreClosureRequest = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreClosureRequest;
using InfraStoreManagerStoreClosureResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreClosureResponse;
using InfraStoreManagerStoreResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreResponse;
using InfraStoreManagerWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreManagerWindowResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Services
{
    public class StoreServiceProvider :
        IStoreLocatorServiceProvider,
        IAppointmentBookingServiceProvider,
        IStoreManagerServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = string.IsNullOrWhiteSpace(ConfigReader.Try(Common.Constants.Environment.StoreAppointmentMsOcpKey, string.Empty))
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>
            {
                {
                    Common.Constants.Application.OcpKeyHeader,
                    ConfigReader.Try(Common.Constants.Environment.StoreAppointmentMsOcpKey, string.Empty) ?? string.Empty
                }
            };

        private static readonly string[] StoreIdClaimTypes = { "storeId", "store_id", "extension_storeId" };
        private static readonly string[] UserIdClaimTypes = { ClaimTypes.NameIdentifier, "oid", "sub" };
        private static readonly string[] UsernameClaimTypes = { ClaimTypes.Name, "preferred_username", "name", "email", "emails" };
        private static readonly string[] RoleClaimTypes = { ClaimTypes.Role, "role", "roles" };

        protected readonly IHttpConnector _connector;
        private readonly ILogger<StoreServiceProvider> _logger;

        public StoreServiceProvider(IHttpConnector connector, ILogger<StoreServiceProvider> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<NearbyStoresResponse> GetNearbyStoresAsync(string? searchParameter)
        {
            var uri = BuildUri(Common.Constants.Application.GetNearByStoresDetails);
            var request = new InfraStoreSearchRequest
            {
                SearchParameter = Normalize(searchParameter)
            };

            return await SendWithBodyAsync<InfraNearbyStoresResponse, NearbyStoresResponse>(uri, HttpMethod.Post, request, x => (NearbyStoresResponse)x);
        }

        public async Task<StoreResponse> GetStoreDetailsByIdAsync(string storeId, string origin)
        {
            var uri = BuildUri(Common.Constants.Application.GetStoresByID);
            var request = new InfraStoreSearchRequest
            {
                StoreId = Normalize(storeId),
                SearchParameter = Normalize(origin)
            };

            return await SendWithBodyAsync<InfraStoreRecord, StoreResponse>(uri, HttpMethod.Post, request, x => (StoreResponse)x);
        }

        public async Task<StoreSlotsResponse> GetStoreSlotsByDateAsync(string storeId, DateTime date)
        {
            var uri = BuildUri(Common.Constants.Application.GetSlotsbyDate);
            var request = new InfraStoreSearchRequest
            {
                StoreId = Normalize(storeId),
                SelectedDate = date
            };

            return await SendWithBodyAsync<InfraStoreSlotsRecord, StoreSlotsResponse>(uri, HttpMethod.Post, request, x => (StoreSlotsResponse)x);
        }

        public async Task<StoreCalendarAvailabilityResponse> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month)
        {
            var uri = BuildUri(Common.Constants.Application.GetCalendarAvailability);
            var request = new InfraStoreSearchRequest
            {
                StoreId = Normalize(storeId),
                Year = year,
                Month = month
            };

            return await SendWithBodyAsync<InfraStoreCalendarAvailabilityRecord, StoreCalendarAvailabilityResponse>(uri, HttpMethod.Post, request, x => (StoreCalendarAvailabilityResponse)x);
        }

        public async Task<StoreManagerStoreResponse> GetStoreAsync(string storeId)
        {
            var uri = BuildUri(Route(Application.StoreManagerStoreById, ("id", Normalize(storeId))));
            return (StoreManagerStoreResponse)(await _connector.GetAsync<InfraStoreManagerStoreResponse>(uri, _headers, string.Empty)).Response;
        }

        public async Task<StoreManagerStoreResponse> UpdateStoreAsync(string storeId, StoreRequest request)
        {
            var uri = BuildUri(Route(Application.StoreManagerStoreById, ("id", Normalize(storeId))));
            return await TranslateStoreManagerErrors(() =>
                SendWithBodyAsync<InfraStoreManagerStoreResponse, StoreManagerStoreResponse>(
                    uri,
                    HttpMethod.Put,
                    request,
                    x => (StoreManagerStoreResponse)x));
        }

        public async Task<IEnumerable<OpeningHoursEntryResponse>> GetOpeningHoursAsync(string storeId)
        {
            var uri = BuildUri(Route(Application.StoreManagerOpeningHours, ("id", Normalize(storeId))));
            return await GetListAsync<InfraStoreManagerOpeningHoursEntryResponse, OpeningHoursEntryResponse>(uri, x => (OpeningHoursEntryResponse)x);
        }

        public async Task<IEnumerable<OpeningHoursEntryResponse>> ReplaceOpeningHoursAsync(string storeId, OpeningHoursReplaceRequest request)
        {
            var uri = BuildUri(Route(Application.StoreManagerOpeningHours, ("id", Normalize(storeId))));
            return await TranslateStoreManagerErrors(() =>
                SendListWithBodyAsync<InfraStoreManagerOpeningHoursEntryResponse, OpeningHoursEntryResponse>(
                    uri,
                    HttpMethod.Put,
                    request,
                    x => (OpeningHoursEntryResponse)x));
        }

        public async Task<IEnumerable<SlotRuleResponse>> GetSlotRulesAsync(string storeId)
        {
            var uri = BuildUri(Route(Application.StoreManagerSlotRules, ("storeId", Normalize(storeId))));
            return await GetListAsync<InfraStoreManagerSlotRuleResponse, SlotRuleResponse>(uri, x => (SlotRuleResponse)x);
        }

        public async Task<IEnumerable<SlotRuleResponse>> ReplaceSlotRulesAsync(string storeId, SlotRuleReplaceRequest request)
        {
            var uri = BuildUri(Route(Application.StoreManagerSlotRules, ("storeId", Normalize(storeId))));
            return await TranslateStoreManagerErrors(() =>
                SendListWithBodyAsync<InfraStoreManagerSlotRuleResponse, SlotRuleResponse>(
                    uri,
                    HttpMethod.Put,
                    request,
                    x => (SlotRuleResponse)x));
        }

        public async Task<IEnumerable<StoreManagerWindowResponse>> GetSlotsAsync(string storeId, string from, string to)
        {
            var uri = BuildUri(Route(Application.StoreManagerSlots, ("storeId", Normalize(storeId))));
            uri = AddQueryString(uri, ("from", Normalize(from)), ("to", Normalize(to)));
            return await GetListAsync<InfraStoreManagerWindowResponse, StoreManagerWindowResponse>(uri, x => (StoreManagerWindowResponse)x);
        }

        public async Task<StoreManagerWindowResponse> UpdateSlotStatusAsync(string storeId, string slotId, SlotStatusUpdateRequest request)
        {
            var uri = BuildUri(Route(Application.StoreManagerSlotStatus, ("storeId", Normalize(storeId)), ("slotId", Normalize(slotId))));
            return await SendWithBodyAsync<InfraStoreManagerWindowResponse, StoreManagerWindowResponse>(uri, HttpMethod.Put, request, x => (StoreManagerWindowResponse)x);
        }

        public async Task<ManageSlotAllocationResponse> GetSlotAllocationsAsync(string storeId, string slotId, string startAt)
        {
            var uri = BuildUri(Route(Application.StoreManagerSlotAllocations, ("storeId", Normalize(storeId)), ("slotId", Normalize(slotId))));
            uri = AddQueryString(uri, ("startAt", Normalize(startAt)));
            return await GetAsync<InfraStoreManagerManageSlotAllocationResponse, ManageSlotAllocationResponse>(uri, x => (ManageSlotAllocationResponse)x);
        }

        public async Task<ManageSlotAllocationResponse> UpdateSlotAllocationStatusAsync(string storeId, string slotId, string allocationId, StoreManagerSlotAllocationStatusUpdateRequest request)
        {
            var uri = BuildUri(Route(
                Application.StoreManagerSlotAllocationStatus,
                ("storeId", Normalize(storeId)),
                ("slotId", Normalize(slotId)),
                ("allocationId", Normalize(allocationId))));
            return await SendWithBodyAsync<InfraStoreManagerManageSlotAllocationResponse, ManageSlotAllocationResponse>(uri, HttpMethod.Put, request, x => (ManageSlotAllocationResponse)x);
        }

        public async Task<IEnumerable<AvailabilityWindowResponse>> GetAvailabilityAsync(string storeId, string date)
        {
            var uri = BuildUri(Route(Application.StoreManagerAvailability, ("storeId", Normalize(storeId))));
            uri = AddQueryString(uri, ("date", Normalize(date)));
            return await GetListAsync<InfraStoreManagerAvailabilityWindowResponse, AvailabilityWindowResponse>(uri, x => (AvailabilityWindowResponse)x);
        }

        public async Task<IEnumerable<AvailabilityWindowResponse>> GetAvailabilityRangeAsync(string storeId, string from, string to)
        {
            var uri = BuildUri(Route(Application.StoreManagerAvailabilityRange, ("storeId", Normalize(storeId))));
            uri = AddQueryString(uri, ("from", Normalize(from)), ("to", Normalize(to)));
            return await GetListAsync<InfraStoreManagerAvailabilityWindowResponse, AvailabilityWindowResponse>(uri, x => (AvailabilityWindowResponse)x);
        }

        public async Task<IEnumerable<StoreClosureResponse>> GetClosuresAsync(string storeId, string from, string to)
        {
            var uri = BuildUri(Route(Application.StoreManagerClosures, ("storeId", Normalize(storeId))));
            uri = AddQueryString(uri, ("from", Normalize(from)), ("to", Normalize(to)));
            return await GetListAsync<InfraStoreManagerStoreClosureResponse, StoreClosureResponse>(uri, x => (StoreClosureResponse)x);
        }

        public async Task<StoreClosureResponse> CreateClosureAsync(string storeId, StoreClosureRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var uri = BuildUri(Route(Application.StoreManagerClosures, ("storeId", Normalize(storeId))));
            var infrastructureRequest = new InfraStoreManagerStoreClosureRequest
            {
                ClosureDate = new DateOnly(request.ClosureDate.Year, request.ClosureDate.Month, request.ClosureDate.Day),
                ClosureType = request.ClosureType,
                Reason = request.Reason
            };

            try
            {
                return await SendWithBodyAsync<InfraStoreManagerStoreClosureResponse, StoreClosureResponse>(
                    uri,
                    HttpMethod.Post,
                    infrastructureRequest,
                    x => (StoreClosureResponse)x);
            }
            catch (HttpRequestException exception) when (TryCreateStoreManagerGraphQlException(exception, out var graphQlException))
            {
                throw graphQlException;
            }
        }

        public async Task<bool> DeleteClosureAsync(string storeId, string closureId)
        {
            var uri = BuildUri(Route(Application.StoreManagerClosureById, ("storeId", Normalize(storeId)), ("closureId", Normalize(closureId))));
            return await SendNoContentAsync(uri, HttpMethod.Delete);
        }

        public Task<ManageMeResponse> GetMyStoreUserAsync()
        {
            var currentUser = GetCurrentStoreManagerUser();
            return Task.FromResult(new ManageMeResponse
            {
                UserId = currentUser.UserId,
                Username = currentUser.Username,
                StoreId = currentUser.StoreId,
                Role = currentUser.Role
            });
        }

        public async Task<IEnumerable<ManageBookingResponse>> GetMyStoreBookingsAsync(string date, string status, string query)
        {
            var currentUser = GetCurrentStoreManagerUser();
            var uri = BuildUri(Route(Application.StoreManagerBookings, ("storeId", Normalize(currentUser.StoreId))));
            uri = AddQueryString(uri, ("date", Normalize(date)), ("status", Normalize(status)), ("q", Normalize(query)));
            return await GetListAsync<InfraStoreManagerManageBookingResponse, ManageBookingResponse>(uri, x => (ManageBookingResponse)x);
        }

        public async Task<IEnumerable<ManageBookingResponse>> GetStoreBookingsAsync(
            string storeId,
            string date,
            StoreManagerBookingStatus status)
        {
            var uri = BuildUri(Route(Application.StoreManagerBookings, ("storeId", Normalize(storeId))));
            uri = AddQueryString(
                uri,
                ("date", Normalize(date)),
                ("status", status.ToString().ToUpperInvariant()));
            return await GetListAsync<InfraStoreManagerManageBookingResponse, ManageBookingResponse>(
                uri,
                x => (ManageBookingResponse)x);
        }

        public async Task<ManageBookingResponse> CancelBookingAsync(string storeId, string bookingId, BookingCancellationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (!request.CustomerContactConfirmed)
            {
                throw StoreManagerContextException("Customer contact must be confirmed before cancellation.", "CUSTOMER_CONTACT_NOT_CONFIRMED");
            }

            var principal = AsyncContext.Current?.Context?.Features.Get<JwtPrincipalFeature>()?.Principal;
            if (principal is null)
            {
                throw StoreManagerContextException("Authenticated Store Manager user context is required.", "401");
            }

            var actorUserId = FindClaimValue(principal, UserIdClaimTypes);
            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                throw StoreManagerContextException("Authenticated user id is required for cancellation audit.", "403");
            }

            var uri = BuildUri(Route(
                Application.StoreManagerCancelBooking,
                ("storeId", Normalize(storeId)),
                ("bookingId", Normalize(bookingId))));
            var infrastructureRequest = new InfraStoreManagerBookingCancellationRequest
            {
                Reason = request.Reason switch
                {
                    BookingCancellationReason.StoreClosure => "STORE_CLOSURE",
                    _ => throw StoreManagerContextException("Unsupported booking cancellation reason.", "INVALID_CANCELLATION_REASON")
                },
                CustomerContactConfirmed = request.CustomerContactConfirmed,
                CancelledByUserId = actorUserId,
                CancelledByDisplayName = FindClaimValue(principal, UsernameClaimTypes)
            };

            try
            {
                return await SendWithBodyAsync<InfraStoreManagerManageBookingResponse, ManageBookingResponse>(
                    uri,
                    HttpMethod.Post,
                    infrastructureRequest,
                    x => (ManageBookingResponse)x);
            }
            catch (HttpRequestException exception) when (TryCreateStoreManagerGraphQlException(exception, out var graphQlException))
            {
                throw graphQlException;
            }
        }

        public async Task<IEnumerable<StoreManagerStoreResponse>> GetAdminStoresAsync()
        {
            var uri = BuildUri(Application.StoreManagerManageAdminStores);
            return await GetListAsync<InfraStoreManagerStoreResponse, StoreManagerStoreResponse>(uri, x => (StoreManagerStoreResponse)x);
        }

        public async Task<StoreManagerStoreResponse> GetAdminStoreAsync(string storeId)
        {
            var uri = BuildUri(Route(Application.StoreManagerManageAdminStoreById, ("id", Normalize(storeId))));
            return (StoreManagerStoreResponse)(await _connector.GetAsync<InfraStoreManagerStoreResponse>(uri, _headers, string.Empty)).Response;
        }

        public async Task<StoreManagerStoreResponse> UpdateAdminStoreAsync(string storeId, StoreRequest request)
        {
            var uri = BuildUri(Route(Application.StoreManagerManageAdminStoreById, ("id", Normalize(storeId))));
            return await TranslateStoreManagerErrors(() =>
                SendWithBodyAsync<InfraStoreManagerStoreResponse, StoreManagerStoreResponse>(
                    uri,
                    HttpMethod.Put,
                    request,
                    x => (StoreManagerStoreResponse)x));
        }

        public async Task<AppointmentBookingResponse> SaveAppointmentBookingAsync(AppointmentBookingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var uri = BuildUri(Common.Constants.Application.SaveAppointmentBooking);
            var infraResponse = await SendWithBodyAsync<InfraAppointmentBookingResponse>(uri, HttpMethod.Post, request);
            return (AppointmentBookingResponse)infraResponse;
        }

        private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

        private sealed record StoreManagerUserContext(string UserId, string Username, string StoreId, string Role);

        private async Task<IEnumerable<T>> GetListAsync<T>(string uri)
        {
            var response = await _connector.GetAsync<List<T>>(uri, _headers, string.Empty);
            return response.Response ?? Enumerable.Empty<T>();
        }

        private async Task<IEnumerable<TDomain>> GetListAsync<TInfra, TDomain>(string uri, Func<TInfra, TDomain> converter)
        {
            var response = await GetListAsync<TInfra>(uri);
            return response.Select(converter);
        }

        private async Task<T> GetAsync<T>(string uri)
        {
            return (await _connector.GetAsync<T>(uri, _headers, string.Empty)).Response;
        }

        private async Task<TDomain> GetAsync<TInfra, TDomain>(string uri, Func<TInfra, TDomain> converter)
        {
            var response = await GetAsync<TInfra>(uri);
            return converter(response);
        }

        private async Task<IEnumerable<T>> SendListWithBodyAsync<T>(string uri, HttpMethod method, object payload)
        {
            var response = await SendWithBodyAsync<List<T>>(uri, method, payload);
            return response ?? Enumerable.Empty<T>();
        }

        private async Task<IEnumerable<TDomain>> SendListWithBodyAsync<TInfra, TDomain>(string uri, HttpMethod method, object payload, Func<TInfra, TDomain> converter)
        {
            var response = await SendListWithBodyAsync<TInfra>(uri, method, payload);
            return response.Select(converter);
        }

        private async Task<T> SendWithBodyAsync<T>(string uri, HttpMethod method, object payload)
        {
            var content = payload == null ? "{}" : Json.Serialize(payload);
            return (await _connector.SendAsync<T>(uri, _headers, string.Empty, method, content)).Response;
        }

        private async Task<TDomain> SendWithBodyAsync<TInfra, TDomain>(string uri, HttpMethod method, object payload, Func<TInfra, TDomain> converter)
        {
            var response = await SendWithBodyAsync<TInfra>(uri, method, payload);
            return converter(response);
        }

        private async Task<bool> SendNoContentAsync(string uri, HttpMethod method)
        {
            await _connector.SendAsync(uri, _headers, string.Empty, method, "{}");
            return true;
        }

        private static async Task<T> TranslateStoreManagerErrors<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException exception) when (TryCreateStoreManagerGraphQlException(exception, out var graphQlException))
            {
                throw graphQlException;
            }
        }

        private static StoreManagerUserContext GetCurrentStoreManagerUser()
        {
            var principal = AsyncContext.Current?.Context?.Features.Get<JwtPrincipalFeature>()?.Principal;
            if (principal is null)
            {
                throw StoreManagerContextException("Authenticated Store Manager user context is required.", "401");
            }

            var storeId = FindClaimValue(principal, StoreIdClaimTypes);
            if (string.IsNullOrWhiteSpace(storeId))
            {
                throw StoreManagerContextException("Authenticated Store Manager user context must include a store id claim.", "403");
            }

            return new StoreManagerUserContext(
                FindClaimValue(principal, UserIdClaimTypes) ?? string.Empty,
                FindClaimValue(principal, UsernameClaimTypes) ?? string.Empty,
                storeId,
                ResolveRole(principal) ?? string.Empty);
        }

        private static GraphQLException StoreManagerContextException(
            string message,
            string code,
            JObject? details = null)
        {
            var builder = ErrorBuilder.New()
                .SetMessage(message)
                .SetCode(code);

            if (details is not null)
            {
                foreach (var property in details.Properties())
                {
                    builder.SetExtension(property.Name, ConvertStoreManagerExtensionValue(property.Value));
                }
            }

            return new GraphQLException(builder.Build());
        }

        private static object? ConvertStoreManagerExtensionValue(JToken token)
        {
            return token switch
            {
                JObject jsonObject => jsonObject.Properties().ToDictionary(
                    property => property.Name,
                    property => ConvertStoreManagerExtensionValue(property.Value),
                    StringComparer.Ordinal),
                JArray jsonArray => jsonArray
                    .Select(ConvertStoreManagerExtensionValue)
                    .ToArray(),
                JValue jsonValue => jsonValue.Value,
                _ => token.ToString()
            };
        }

        private static bool TryCreateStoreManagerGraphQlException(HttpRequestException exception, out GraphQLException graphQlException)
        {
            graphQlException = null!;
            if (exception.Data["Response"] is not string response || string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            try
            {
                var payload = JToken.Parse(response);
                var error = payload switch
                {
                    JArray errors => errors.FirstOrDefault(),
                    JObject responseObject => responseObject["Errors"]?.FirstOrDefault()
                        ?? responseObject["errors"]?.FirstOrDefault(),
                    _ => null
                };
                var code = error?["Code"]?.ToString() ?? error?["code"]?.ToString();
                if (string.IsNullOrWhiteSpace(code))
                {
                    return false;
                }

                var message = error?["Message"]?.ToString() ?? error?["message"]?.ToString() ?? "Store Manager request failed.";
                var details = error?["Details"] as JObject ?? error?["details"] as JObject;
                graphQlException = StoreManagerContextException(message, code, details);
                return true;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return false;
            }
        }

        private static string? FindClaimValue(ClaimsPrincipal principal, IEnumerable<string> claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var claimValue = principal.Claims.FirstOrDefault(claim => claim.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrWhiteSpace(claimValue))
                {
                    return claimValue;
                }
            }

            return null;
        }

        private static string? ResolveRole(ClaimsPrincipal principal)
        {
            var roles = principal.Claims
                .Where(claim => RoleClaimTypes.Any(roleType => claim.Type.Equals(roleType, StringComparison.OrdinalIgnoreCase)))
                .SelectMany(claim => claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();

            if (roles.Any(role => role.Equals(Roles.StoreAdmin, StringComparison.OrdinalIgnoreCase)))
            {
                return Roles.StoreAdmin;
            }

            if (roles.Any(role => role.Equals(Roles.StoreManager, StringComparison.OrdinalIgnoreCase)))
            {
                return Roles.StoreManager;
            }

            return roles.FirstOrDefault() ?? FindClaimValue(principal, RoleClaimTypes);
        }

        private static string BuildUri(string route)
        {
            var baseUrl = ConfigReader.Try(Common.Constants.Environment.StoreAppointmentMsUrl, string.Empty);
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException($"{Common.Constants.Environment.StoreAppointmentMsUrl} is not configured.");
            }

            return $"{baseUrl.TrimEnd('/')}{route}";
        }

        private static string Route(string template, params (string Token, string Value)[] replacements)
        {
            var route = template;
            foreach (var (token, value) in replacements)
            {
                route = route.Replace($"{{{token}}}", Uri.EscapeDataString(value ?? string.Empty), StringComparison.OrdinalIgnoreCase);
            }

            return route;
        }

        private static string AddQueryString(string uri, params (string Key, string Value)[] queryParams)
        {
            var query = queryParams
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}")
                .ToArray();

            if (!query.Any())
            {
                return uri;
            }

            return $"{uri}?{string.Join("&", query)}";
        }
    }
}
