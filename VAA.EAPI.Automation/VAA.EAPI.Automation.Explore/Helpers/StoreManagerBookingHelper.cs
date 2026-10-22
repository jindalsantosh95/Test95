using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class StoreManagerBookingHelper
    {
        public static async Task<StoreManagerCancelledBooking> CancelBooking(string storeId, string bookingId, string bearer)
        {
            HttpResponseMessage response = GraphQL.StoreManagerCancelBooking(storeId, bookingId, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            StoreManagerCancelBookingResponse? result = JsonConvert.DeserializeObject<StoreManagerCancelBookingResponse>(responseBody);

            if (result == null)
            {
                throw new InvalidOperationException("StoreManagerCancelBooking response could not be deserialized.");
            }

            if (result.data == null)
            {
                throw new InvalidOperationException("StoreManagerCancelBooking response data is missing.");
            }

            if (result.data.storeManagerCancelBooking == null)
            {
                throw new InvalidOperationException("StoreManagerCancelBooking response object is missing.");
            }

            StoreManagerCancelledBooking cancelledBooking = result.data.storeManagerCancelBooking;

            if (!cancelledBooking.id.Is(bookingId))
            {
                throw new InvalidOperationException($"Cancellation booking ID mismatch. Expected {bookingId}, received {cancelledBooking.id}.");
            }

            if (!cancelledBooking.storeId.Is(storeId))
            {
                throw new InvalidOperationException($"Cancellation store ID mismatch. Expected {storeId}, received {cancelledBooking.storeId}.");
            }

            if (!cancelledBooking.status.Is("CANCELLED"))
            {
                throw new InvalidOperationException($"Expected booking status CANCELLED but received {cancelledBooking.status}.");
            }

            return cancelledBooking;
        }
    }
}
