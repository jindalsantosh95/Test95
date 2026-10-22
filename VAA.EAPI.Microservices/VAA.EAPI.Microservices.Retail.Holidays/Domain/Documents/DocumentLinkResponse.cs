using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
{
    public class DocumentLinkResponse
    {
        public string DownloadLink { get; set; }
        public DocumentLinkResponseStatus ResponseStatus { get; set; }

        public static explicit operator DocumentLinkResponse(HolidaysDocumentDownloadResponse source)
        {
            // null response — data array was null or empty
            if (source == null)
            {
                return new()
                {
                    DownloadLink = null,
                    ResponseStatus = new DocumentLinkResponseStatus
                    {
                        HttpStatusCode = 404,
                    }
                };
            }

            // error response — errors populated from api
            if (source.Errors?.Any() == true)
            {
                return new()
                {
                    DownloadLink = null,
                    ResponseStatus = new DocumentLinkResponseStatus
                    {
                        HttpStatusCode = 500,
                        Errors = [.. source.Errors.Select(e => (DocumentLinkError)e)]
                    }
                };
            }

            // success
            return new()
            {
                DownloadLink = source.DownloadLink,
                ResponseStatus = new DocumentLinkResponseStatus
                {
                    HttpStatusCode = 200,
                }
            };
        }
    }
}
