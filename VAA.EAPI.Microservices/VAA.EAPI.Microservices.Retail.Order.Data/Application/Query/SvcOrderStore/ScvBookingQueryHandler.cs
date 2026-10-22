using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public sealed class ScvBookingQueryHandler(IScvBookingRepository repository) : IScvBookingQuery
    {
        private readonly IScvBookingRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<Response<IReadOnlyCollection<TripDetailsResponse>>> HandleAsync(string pnr, string flightDate, string firstName, string lastName, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pnr);
            ArgumentException.ThrowIfNullOrWhiteSpace(flightDate);
            ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
            ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

            if (!DateTime.TryParseExact(flightDate.Trim(), "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new ArgumentException("Flight date must use ddMMMyy format, for example 31AUG25.", nameof(flightDate));

            cancellationToken.ThrowIfCancellationRequested();
            var normalizedPnr = pnr.Trim().ToUpperInvariant();
            if (normalizedPnr.Length != 6)
                return EmptyResponse();

            var departureDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var candidateIds = await _repository.GetOrderIdsByPnrAsync(normalizedPnr, cancellationToken);
            var matchingIds = new List<string>();
            foreach (var candidateId in candidateIds.Distinct(StringComparer.Ordinal))
            {
                // A PNR is the whole prefix before the seven-character creation date.
                if (candidateId is null || candidateId.Length != normalizedPnr.Length + 7 ||
                    !candidateId.StartsWith(normalizedPnr, StringComparison.Ordinal))
                    continue;

                var matches = await _repository.GetMatchingOrderIdsAsync(candidateId, departureDate, firstName.Trim(), lastName.Trim(), cancellationToken);
                matchingIds.AddRange(matches.Where(id => id == candidateId));
                if (matchingIds.Count > 1)
                    return EmptyResponse();
            }

            if (matchingIds.Count != 1)
                return EmptyResponse();

            var orderId = matchingIds[0];
            var bookings = await _repository.GetByOrderIdAsync(orderId, cancellationToken);
            // Recheck after retrieval so a changed document cannot bypass the lookup criteria.
            var accepted = bookings.Where(booking => booking.OrderID == orderId &&
                booking.SegmentDetails?.Any(segment =>
                    segment.ScheduledDepartureTime?.LocalDateTime?.StartsWith(departureDate + "T", StringComparison.Ordinal) == true) == true &&
                booking.CustomerDetails?.Any(customer =>
                    customer.FirstName?.Trim().Is(firstName.Trim()) == true &&
                    customer.LastName?.Trim().Is(lastName.Trim()) == true) == true).ToList();

            return accepted.Count == 1
                ? new Response<IReadOnlyCollection<TripDetailsResponse>> { Status = Status.OK, Value = [ScvBookingResponseMapper.Map(accepted[0])] }
                : EmptyResponse();
        }

        private static Response<IReadOnlyCollection<TripDetailsResponse>> EmptyResponse()
            => new() { Status = Status.OK, Value = Array.Empty<TripDetailsResponse>() };
    }
}
