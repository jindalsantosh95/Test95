using MediatR;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;

namespace VAA.EAPI.Web.Book.Domain.Basket.Requests
{
    public class BasketUpdateContactRequest : IRequest<YourDetailsResponse>
    {
        public UpdateContactDetailsRequest UpdateContactDetailsRequest { get; set; }
        public BasketUpdateContactRequest(UpdateContactRequest source)
        {
            UpdateContactDetailsRequest = new()
            {
                BasketId = source.BasketId,
                ContactDetails = new()
                {
                    EmailAddress = source.EmailAddress,
                    PeopleId = source.PeopleId,
                    PhoneNumber = source.PhoneNumber,
                    AlternativePhoneNumber = source.AlternativePhoneNumber,
                    AcceptPost = source.AcceptPost,
                    AcceptTelephone = source.AcceptTelephone,
                    AcceptEmail = source.AcceptEmail,
                    AcceptTextMessage = source.AcceptTextMessage,
                    Validation = source.Validation,
                    CountryCode = source.CountryCode,
                    FcAutoMatchStamping = source.FcAutoMatchStamping
                }
            };
        }
    }
}