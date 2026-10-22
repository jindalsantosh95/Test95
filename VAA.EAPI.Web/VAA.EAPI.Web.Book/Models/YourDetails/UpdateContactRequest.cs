using MediatR;
using System.IO;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.YourDetails
{
    public class UpdateContactRequest : IRequest<YourDetailsResponse>
    {
        public string BasketId { get; set; }
        public string PeopleId { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool? FcAutoMatchStamping { get; set; }
        public string AlternativePhoneNumber { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMessage { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string CountryCode { get; set; }
        public string AuthToken { get; set; }

        public UpdateContactRequest()
        { }

        public UpdateContactRequest(string basketId, string authToken, Stream body)
        {
            var fromJson = Json.Deserialize<UpdateContactRequest>(new StreamReader(body).ReadToEndAsync().Result);

            BasketId = basketId;
            PeopleId = fromJson.PeopleId;
            EmailAddress = fromJson.EmailAddress;
            PhoneNumber = fromJson.PhoneNumber;
            FcAutoMatchStamping = fromJson.FcAutoMatchStamping;
            AlternativePhoneNumber = fromJson.AlternativePhoneNumber;
            AcceptPost = fromJson.AcceptPost;
            AcceptTelephone = fromJson.AcceptTelephone;
            AcceptEmail = fromJson.AcceptEmail;
            AcceptTextMessage = fromJson.AcceptTextMessage;
            Link = fromJson.Link;
            Validation = fromJson.Validation;
            CountryCode = fromJson.CountryCode;
            AuthToken = authToken;
        }
    }
}