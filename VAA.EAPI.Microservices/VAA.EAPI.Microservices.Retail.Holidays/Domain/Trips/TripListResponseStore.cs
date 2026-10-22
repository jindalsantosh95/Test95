using System;
using System.Collections.Concurrent;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TripListResponseStore
    {
        public ConcurrentBag<TripListResponseItem> TripListResponse { get; private set; } = [];

        public static explicit operator TripListResponseStore(ConcurrentBag<Tuple<TripsDetailsRequest, TripsDetailsResponse>> source)
        {
            return new TripListResponseStore
            {
                TripListResponse = [.. source.Select(x => new TripListResponseItem
                {
                    TripSearchRequest = x.Item1,
                    TripDetails = x.Item2
                })]
            };
        }

        public static explicit operator TripListResponseStore(ConcurrentBag<Tuple<RetrieveBookingRequest, TripsDetailsResponse>> source)
        {
            return new TripListResponseStore
            {
                TripListResponse = [.. source.Select(x => new TripListResponseItem
                {
                    TripSearchRequest = (TripsDetailsRequest)x.Item1,
                    TripDetails = x.Item2
                })]
            };
        }
    }
}