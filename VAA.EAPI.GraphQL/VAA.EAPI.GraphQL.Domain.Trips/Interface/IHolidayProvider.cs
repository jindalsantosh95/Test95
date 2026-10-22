using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IHolidayProvider
    {
        public Task<TripsDetailsResponse> RetrieveBooking(Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest request);
        public Task<TripListResponseStore> RetrieveBookingList(IEnumerable<Infrastructure.Models.HolidaysMS.TripsCore.TripsDetailsRequest> request);
        public Task<HolidaysDocumentListResponse> RetrieveDocumentList(RetrieveDocumentRequest request);
        public Task<HolidaysPendingDocumentListResponse> RetrievePendingDocumentList(RetrieveDocumentRequest request);
        public Task<HolidaysDocumentLinkListResponse> RetrieveDocumentDownloadLinkList(RetrieveDocumentLinkListRequest request);
#nullable enable annotations
        public Task<RetrieveDocumentDownloadResponse?> RetrieveDocument(RetrieveDocumentDownloadRequest request, CancellationToken cancellationToken);
#nullable restore annotations
    }
}
