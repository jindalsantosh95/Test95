using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;

namespace VAA.EAPI.GraphQL.Domain.Book.Validation
{
    public class ValidateCustomerEnrolment
    {
        public static bool ValidateCustomerForEnrolment(CustomerEnrolmentRequest request, OfferResponse offer)
        {
            if (request == null || offer == null)
            {
                return false;
            }

            #region Check DoB and Age
            if (request.CustomerDetails.DateOfBirth == null)
            {
                return false;
            }
            else
            {
                var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                var passengerAgeonDepartureDate = ValidateBasket.CalculatePassengerAge(Convert.ToDateTime(request.CustomerDetails.DateOfBirth.ToString()), departureTime);
                return passengerAgeonDepartureDate > 18 ? true : false;
            }
            #endregion
        }


    }
}
