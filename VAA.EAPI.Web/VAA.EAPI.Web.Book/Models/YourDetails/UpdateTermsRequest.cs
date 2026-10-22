using MediatR;
using System.IO;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.YourDetails
{
    public class UpdateTermsRequest : IRequest<YourDetailsResponse>
    {
        public string BasketId { get; set; }
        public bool Accepted { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string AuthToken { get; set; }

        public UpdateTermsRequest()
        { }

        public UpdateTermsRequest(string basketId, string authToken, Stream body)
        {
            var fromJson = Json.Deserialize<UpdateTermsRequest>(new StreamReader(body).ReadToEndAsync().Result);
            BasketId = basketId;
            Accepted = fromJson.Accepted;
            Link = fromJson.Link;
            Validation = fromJson.Validation;
            AuthToken = authToken;
        }
    }
}
