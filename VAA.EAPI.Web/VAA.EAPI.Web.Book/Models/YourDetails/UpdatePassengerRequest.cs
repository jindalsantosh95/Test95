using MediatR;
using System;
using System.IO;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.YourDetails
{
    public class UpdatePassengerRequest : IRequest<YourDetailsResponse>
    {
        public string BasketId { get; set; }
        public int PassengerId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? ProfilePassenger { get; set; } = null;
        public string Link { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
        public string Age { get; set; }
        public string AuthToken { get; set; }
        public FQTMembership MembershipDetails { get; set; }
        public UpdatePassengerRequest() { }

        public UpdatePassengerRequest(string basketId, int passengerId, string authToken, Stream body)
        {
            var fromJson = Json.Deserialize<UpdatePassengerRequest>(new StreamReader(body).ReadToEndAsync().Result);

            BasketId = basketId;
            PassengerId = passengerId;
            Title = fromJson.Title;
            FirstName = fromJson.FirstName;
            LastName = fromJson.LastName;
            DateOfBirth = fromJson.DateOfBirth;
            Gender = fromJson.Gender;
            ProfilePassenger = fromJson.ProfilePassenger;
            Link = fromJson.Link;
            Validation = fromJson.Validation;
            Type = fromJson.Type;
            Age = fromJson.Age;
            AuthToken = authToken;
            MembershipDetails = new()
            {
                FQTProgramme = fromJson.MembershipDetails.FQTProgramme,
                FQTNumber = fromJson.MembershipDetails.FQTNumber,
            };
        }
    }
}