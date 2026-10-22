using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public sealed class ScvBookingProvider(IHttpConnector connector, IOAuthTokenProvider tokenProvider) : IScvBookingProvider
    {
        private readonly IHttpConnector _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        private readonly IOAuthTokenProvider _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

        public async Task<IReadOnlyList<Booking>> LookupAsync(string pnr, string flightDate, string givenName, string surname, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentException.ThrowIfNullOrWhiteSpace(pnr);
            ArgumentException.ThrowIfNullOrWhiteSpace(flightDate);
            ArgumentException.ThrowIfNullOrWhiteSpace(givenName);
            ArgumentException.ThrowIfNullOrWhiteSpace(surname);

            if (!DateTime.TryParseExact(flightDate.Trim(), "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new ArgumentException("Flight date must use ddMMMyy format, for example 31AUG25.", nameof(flightDate));

            // Token acquisition may be shared/cached. Stop this request's wait without
            // cancelling token acquisition for other callers.
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.OrderDataAppId)).WaitAsync(cancellationToken);
            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"{ConfigReader.Get(Environment.OrderDataMSUrl).TrimEnd('/')}/scv/orders/by-flight-date")
            {
                Content = new StringContent(Json.Serialize(new { Pnr = pnr.Trim().ToUpperInvariant(), FlightDate = flightDate.Trim().ToUpperInvariant(), FirstName = givenName.Trim(), LastName = surname.Trim() }), Encoding.UTF8, "application/json")
            };
            request.Headers.Add(Application.OcpKeyHeader, ConfigReader.Get(Environment.OrderDataOcpKey));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            var response = await _connector.SendAsync<List<Booking>>(request, cancellationToken);

            var bookings = response.Response ?? throw new InvalidOperationException("SCV booking lookup returned an invalid response.");
            if (bookings.Count != 1)
                return Array.Empty<Booking>();

            if (ConfigReader.Try(Environment.ExcludeGroupBookings, true) &&
                bookings.Any(booking => booking.ReservationType?.Trim().Is("group") == true))
                throw new InvalidOperationException("Group bookings are not supported");

            return bookings.Select(booking =>
            {
                // Copy only trip members: inherited holiday payment and summary data
                // must remain unset even if unexpectedly supplied by the downstream API.
                var result = new Booking
                {
                    Id = booking.Id,
                    Status = booking.Status,
                    ReservationType = booking.ReservationType,
                    Pos = booking.Pos,
                    Passengers = booking.Passengers,
                    Product = booking.Product
                };
                foreach (var product in result.Product ?? [])
                {
                    product.CheckInEligible = false;
                    product.AccessibleTravelEligible = false;
                }
                return result;
            }).ToArray();
        }
    }
}
