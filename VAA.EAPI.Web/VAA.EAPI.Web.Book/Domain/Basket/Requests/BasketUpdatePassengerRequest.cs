using MediatR;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;

namespace VAA.EAPI.Web.Book.Domain.Basket.Requests
{
    public class BasketUpdatePassengerRequest : BasketBase, IRequest<YourDetailsResponse>
    {
        public UpdatePeopleRequest UpdatePeopleRequest { get; set; }
        public BasketUpdatePassengerRequest(UpdatePassengerRequest source)
        {
            UpdatePeopleRequest = new()
            {
                BasketId = source.BasketId,
                People = new()
                {
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    DateOfBirth = source.DateOfBirth,
                    Gender = source.Gender,
                    Title = source.Title,
                    Validation = source.Validation,
                    ProfilePassenger = source.ProfilePassenger,
                    Id = source.PassengerId.ToString(),
                    Type = source.Type,
                    MembershipDetails = source.MembershipDetails
                }
            };
        }
    }
}