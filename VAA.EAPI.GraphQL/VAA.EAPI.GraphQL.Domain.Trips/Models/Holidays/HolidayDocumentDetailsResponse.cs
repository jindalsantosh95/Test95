using HotChocolate;
using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayDocumentDetailsResponse")]
    public class HolidayDocumentDetailsResponse
    {
        public IEnumerable<DocumentDetails>? DocumentInformation { get; set; }

        public static explicit operator HolidayDocumentDetailsResponse((HolidaysDocumentLinkListResponse documentLinksMSResponse, HolidaysDocumentListResponse documentListMSResponse) data)
        {
            var (documentLinksMSResponse, documentListMSResponse) = data;

            if (documentListMSResponse == null ||
                documentLinksMSResponse == null ||
                documentListMSResponse.Documents == null ||
                documentLinksMSResponse.HolidaysDocumentLinkResponses == null)
            {
                return default;
            }

            var linkByDocId = documentLinksMSResponse.HolidaysDocumentLinkResponses
                .Where(l => l?.Request?.DocumentId != null)
                .GroupBy(l => l!.Request!.DocumentId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var documentInformation = documentListMSResponse.Documents
                .Select(doc =>
                {
                    linkByDocId.TryGetValue(doc.DocumentId, out var matchingLink);

                    var status = matchingLink?.Response?.ResponseStatus?.HttpStatusCode ?? 404;
                    var downloadLink = status == 200 ? matchingLink?.Response?.DownloadLink : null;

                    return new DocumentDetails
                    {
                        DocumentId = doc.DocumentId.ToString(),
                        BookingReference = matchingLink?.Request?.BookingId ?? doc.BookingId.ToString(),
                        DocumentUrl = downloadLink,
                        Status = status,
                        DocumentType = doc.DocumentType,
                        Name = MapDocumentName(doc.DocumentName),
                        Errors = MapErrors(matchingLink?.Response?.ResponseStatus?.Errors)
                    };
                })
                .Where(d => d.Status == (int)HttpStatusCode.OK)
                .ToList();

            if (!documentInformation.Any(d => d.Name == "Tickets"))
            {
                documentInformation.Add(new DocumentDetails
                {
                    Name = "Tickets",
                    DocumentType = "TRVDOC",
                    Status = (int)HttpStatusCode.NotFound,
                    BookingReference = documentListMSResponse.Documents.FirstOrDefault()?.BookingId.ToString(),
                });
            }

            return new HolidayDocumentDetailsResponse
            {
                DocumentInformation = documentInformation
            };
        }

        private static IEnumerable<ErrorDetails> MapErrors(IEnumerable<DocumentLinkError> errors)
        {
            return errors?.Select(error => new ErrorDetails
            {
                Code = error.Code,
                Message = error.Message
            });
        }

        private static string MapDocumentName(string documentName)
        {
            return documentName switch
            {
                "Travel Document" => "Tickets",
                "Travel Document - Email" => "Tickets - Email",
                _ => documentName
            };
        }
    }
}
