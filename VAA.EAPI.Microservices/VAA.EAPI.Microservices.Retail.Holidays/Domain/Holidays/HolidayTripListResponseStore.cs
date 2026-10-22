using System;
using System.Collections.Concurrent;
using VAA.EAPI.Microservices.Retail.Holidays.Domain;

namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{

    public class HolidayTripListResponseStore
    {
        public ConcurrentBag<HolidayTripListResponseItem> HolidayTripListResponse { get; } = [];

        public static explicit operator HolidayTripListResponseStore(
            ConcurrentBag<Tuple<RetrieveBookingRequest, HolidayTripResponse>> source)
        {
            var store = new HolidayTripListResponseStore();

            foreach (var item in source)
            {
                store.HolidayTripListResponse.Add(new HolidayTripListResponseItem
                {
                    HolidayRequest = item.Item1,
                    HolidayResponse = item.Item2
                });
            }

            return store;
        }
    }

}
