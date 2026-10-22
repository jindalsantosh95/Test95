using System.Collections.Concurrent;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class TripListResponseStore
    {
        public ConcurrentBag<TripListResponseItem> TripListResponse { get; } = [];

        public static explicit operator TripListResponseStore(
            ConcurrentBag<Tuple<TripsDetailsRequest, TripsDetailsResponse>> source)
        {
            var store = new TripListResponseStore();

            foreach (var item in source)
            {
                store.TripListResponse.Add(new TripListResponseItem
                {
                    TripSearchRequest = item.Item1,
                    TripDetails = item.Item2
                });
            }

            return store;
        }
    }
}