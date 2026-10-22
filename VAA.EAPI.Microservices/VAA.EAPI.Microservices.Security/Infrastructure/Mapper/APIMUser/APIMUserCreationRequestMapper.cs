using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserRequest;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Mapper.APIMUser
{
    public class APIMUserCreationRequestMapper
    {
        public static UserCreationRequest MapToMSAPIMUserRequest(UserAccountDetails request)
        {
            return new UserCreationRequest { Properties = new UserProperties { Email = request.EmailAddress, FirstName = request.FirstName, LastName = request.LastName, Confirmation = "signup" } };

        }

    }
}
