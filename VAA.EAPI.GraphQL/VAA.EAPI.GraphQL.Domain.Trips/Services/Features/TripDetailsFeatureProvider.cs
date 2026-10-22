using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.Features;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using Booking = VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.Booking;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services.Features
{
    public class TripDetailsFeatureProvider : ITripDetailsFeatureProvider
    {
        private readonly IOperationalScheduleProvider _operationalScheduleProvider;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TripDetailsFeatureProvider(IOperationalScheduleProvider operationalScheduleProvider, IDateTimeProvider dateTimeProvider = null) 
        {
            _operationalScheduleProvider = operationalScheduleProvider ?? throw new ArgumentNullException(nameof(operationalScheduleProvider));
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        }

        public async Task<Booking> AddTripDetailFeature(Infrastructure.Models.TripMS.Booking bookingDetails, IEnumerable<CabinRefData> cabinDetails, string bookingId, IEnumerable<AirportRefData> airportRefDatas, IEnumerable<AirlineRefData> airlineRefDatas = null, Infrastructure.Models.CheckInMS.TravelRequirementsResponse travelRequirements = null)
        {
            if (ConfigReader.Try(Common.Constants.Environment.TripAircraftDetailsFeature, true))
            {
                var lookupTasks = new ConcurrentDictionary<string, Task<OperationalScheduleResponse>>();

                // TODO: Add aircraft details to each product in the booking
                if (bookingDetails?.Product != null)
                {
                    foreach (var product in bookingDetails.Product)
                    {
                        if (product?.Slices == null) continue;

                        foreach (var slice in product.Slices)
                        {
                            if (slice?.Segments == null) continue;

                            // collect segments to process so we can await sequentially (or build tasks for parallel)
                            foreach (var segment in slice.Segments.Where(filter => filter.OperatingCarrier?.CarrierCode == "VS"))
                            {
                                if (segment.ScheduledDepartureUtcDateTime.Date <= _dateTimeProvider.UtcNow.Date.AddDays(3))
                                {
                                    // explicit operator exists: OperationScheduleRequest from Segment
                                    var req = (OperationScheduleRequest)segment;
                                    var key = segment.Id;

                                    // ensure only one call per unique request key
                                    lookupTasks.GetOrAdd(key, _ => _operationalScheduleProvider.GetOperationalScheduleAsync(req));
                                }
                            }
                        }
                    }


                    // await all lookups (best-effort; exceptions will propagate)
                    try
                    {
                        await Task.WhenAll(lookupTasks.Values).ConfigureAwait(false);
                    }
                    catch
                    {
                        // If some lookups fail, swallow here to allow best-effort enrichment below.
                        // You may want to log the exception(s) instead of swallowing.
                    }
                }

                if(lookupTasks.Count == 0)
                {
                    return (Booking)(bookingDetails, cabinDetails, bookingId, airportRefDatas, airlineRefDatas, travelRequirements);
                }

                // Now enrich the booking details with aircraft info where available
                foreach (var product in bookingDetails.Product)
                {
                    if (product?.Slices == null) continue;
                    foreach (var slice in product.Slices)
                    {
                        if (slice?.Segments == null) continue;
                        foreach (var segment in slice.Segments.Where(filter => filter.OperatingCarrier?.CarrierCode == "VS"))
                        {
                            if (segment.ScheduledDepartureUtcDateTime.Date <= DateTime.UtcNow.Date.AddDays(3))
                            {
                                var key = segment.Id;
                                if (lookupTasks.TryGetValue(key, out var lookupTask))
                                {
                                    try
                                    {
                                        var operationalSchedule = await lookupTask.ConfigureAwait(false);
                                        segment.Legs?.ToList().ForEach(leg =>
                                        {
                                            // explicit operator exists: Aircraft from (Aircraft, OperationalScheduleResponse)
                                            leg.Aircraft = (Infrastructure.Models.TripMS.Aircraft)(leg.Aircraft, operationalSchedule);
                                        });
                                    }
                                    catch
                                    {
                                        // Swallow exceptions for individual lookups to allow best-effort enrichment.
                                        // You may want to log the exception instead of swallowing.
                                    }
                                }
                            }
                        }
                    }
                }

                return (Booking)(bookingDetails, cabinDetails, bookingId, airportRefDatas, airlineRefDatas, travelRequirements);
            }
            return (Booking)(bookingDetails, cabinDetails, bookingId, airportRefDatas, airlineRefDatas, travelRequirements);
        }
    }
}
