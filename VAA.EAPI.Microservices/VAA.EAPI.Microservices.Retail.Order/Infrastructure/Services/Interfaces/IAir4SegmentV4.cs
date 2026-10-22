using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.SegmentV4Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.Interfaces
{
    public interface IAir4SegmentV4
    {
        public CancelAndRebookResponseType CancelAndRebookSegments(CancelAndRebookRequestType cancelAndRebookRequestType);
        public InsertResponseType InsertSegments(InsertRequestType insertRequestType);
        public DeleteResponseType DeleteSegments(DeleteRequestType deleteRequestType);
    }
}
