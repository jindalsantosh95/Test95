namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class HolidaysDocumentLinkItem
    {
        public DocumentLinkRequest Request { get; set; }
        public DocumentLinkResponse Response { get; set; }

        public static explicit operator HolidaysDocumentLinkItem((DocumentLinkRequest detail, HolidaysDocumentDownloadResponse source) data)
        {
            var (detail, source) = data;

            return new()
            {
                Request = new DocumentLinkRequest
                {
                    DocumentId = detail?.DocumentId,
                    BookingId = detail?.BookingId
                },
                Response = (DocumentLinkResponse)source
            };
        }
    }
}
