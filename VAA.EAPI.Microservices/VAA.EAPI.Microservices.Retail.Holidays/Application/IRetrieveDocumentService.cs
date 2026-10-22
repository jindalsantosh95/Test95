using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Holidays.Domain;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;


namespace VAA.EAPI.Microservices.Retail.Holidays.Application
{
    public interface IRetrieveDocumentService
    {
        Task<Domain.Documents.HolidaysDocumentListResponse> Handle(RetrieveDocumentRequest request, CancellationToken cancellationToken);
        Task<Domain.Documents.HolidaysPendingDocumentListResponse> RetrievePendingDocumentList(RetrieveDocumentRequest retrieveDocumentRequest, CancellationToken cancellationToken);
        Task<HolidaysDocumentLinkItem> RetrieveDocumentDownloadLink(DocumentLinkRequest request, CancellationToken cancellationToken);
        Task<HolidaysDocumentLinkListResponse> RetrieveDocumentDownloadLinkList(RetrieveDocumentLinkListRequest request, CancellationToken cancellationToken);
#nullable enable annotations
        Task<RetrieveDocumentDownloadResponse?> RetrieveDocument(RetrieveDocumentDownloadRequest request, CancellationToken cancellationToken);
#nullable restore annotations

    }
}
