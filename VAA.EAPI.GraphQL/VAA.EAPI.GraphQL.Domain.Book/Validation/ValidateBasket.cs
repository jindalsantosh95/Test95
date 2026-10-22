using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Validation
{
    public class ValidateBasket
    {
        public static async Task<PassengerUpdateRequest> ValidatePassenger(PassengerUpdateRequest request, OfferResponse offer, Basket basket, ICustomerProvider _customerProvider = null)
        {
            request.People.Validation = Constants.Application.ValidStatus;

            #region Check DoB and Age
            if (request.People.DateOfBirth == null)
            {
                request.People.Validation = Constants.Application.InvalidStatus;
            }
            else
            {
                request.People.Type = basket.People.FirstOrDefault(x => x.Id == request.People.Id)?.Type;
                var departureTime = DateTime.Parse(offer.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime));
                var passengerAgeonDepartureDate = CalculatePassengerAge(Convert.ToDateTime(request.People.DateOfBirth.ToString()), departureTime);
                ValidatePassengerAge(request, passengerAgeonDepartureDate);
            }
            #endregion

            // Check FQT Programme
            if (_customerProvider != null)
            {
                if (request.People.MembershipDetails == null)
                {
                    request.People.MembershipDetails = new();
                }

                request.People.MembershipDetails.FQTValidityStatus = Constants.FQTValidationStatus.Unchecked;
                if (request.People.MembershipDetails.FQTProgramme == Constants.Application.VSFQTProgramme)
                {
                    if (request.People.MembershipDetails.FQTNumber.IsNullOrWhiteSpace())
                    {
                        request.People.MembershipDetails.FQTValidityStatus = Constants.FQTValidationStatus.InvalidAccount;
                    }
                    else
                    {
                        try
                        {
                            var customer = await _customerProvider.GetCustomer(string.Empty, request.People.MembershipDetails.FQTNumber);
                            if (customer.Customer.FirstName.Equals(request.People.FirstName, StringComparison.InvariantCultureIgnoreCase) && customer.Customer.LastName.Equals(request.People.LastName, StringComparison.InvariantCultureIgnoreCase))
                                request.People.MembershipDetails.FQTValidityStatus = Constants.FQTValidationStatus.Valid;
                            else
                                request.People.MembershipDetails.FQTValidityStatus = Constants.FQTValidationStatus.NameMismatch;

                        }
                        catch (HttpRequestException ex)
                        {
                            if (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity) // No Customer found will throw this, with 422 status code
                                request.People.MembershipDetails.FQTValidityStatus = Constants.FQTValidationStatus.InvalidAccount;

                            else
                                throw; //anything else, propogate up
                        }
                    }
                    // If FQT is not valid, then set the overall validation status to INVALID
                    request.People.MembershipDetails.Validation = request.People.MembershipDetails.FQTValidityStatus == Constants.FQTValidationStatus.Valid ? Constants.Application.ValidStatus : Constants.Application.InvalidStatus;
                    if (request.People.MembershipDetails.FQTValidityStatus != Constants.FQTValidationStatus.Valid)
                        request.People.Validation = Constants.Application.InvalidStatus;
                }
                else if (request.People.MembershipDetails.FQTProgramme.IsNullOrWhiteSpace() || request.People.MembershipDetails.FQTNumber.IsNullOrWhiteSpace())
                {
                    request.People.MembershipDetails.FQTProgramme = null;
                    request.People.MembershipDetails.FQTNumber = null;
                }
            }

            return request;
        }

        private static PassengerUpdateRequest ValidatePassengerAge(PassengerUpdateRequest request, int passengerAge)
        {
            switch (request.People.Type)
            {
                case "ADULT":
                    if (passengerAge < 16)
                    {
                        request.People.Validation = Constants.Application.InvalidStatus;
                    }
                    break;

                case "CHILD":
                    if (passengerAge < 2 || passengerAge > 11)
                    {
                        request.People.Validation = Constants.Application.InvalidStatus;
                    }
                    break;

                case "TEEN":
                    if (passengerAge < 12 || passengerAge > 15)
                    {
                        request.People.Validation = Constants.Application.InvalidStatus;
                    }
                    break;

                case "INFANT":
                    if (passengerAge >= 2)
                    {
                        request.People.Validation = Constants.Application.InvalidStatus;
                    }
                    break;
            }
            return request;
        }

        public static int CalculatePassengerAge(DateTime dateofBirth, DateTime departureDate)
        {
            int age, dayofyearDOB = dateofBirth.DayOfYear, dayofyearDepartureDate = departureDate.DayOfYear;

            if (DateTime.IsLeapYear(dateofBirth.Year) && dateofBirth.DayOfYear >= 60)
            {
                dayofyearDOB = dateofBirth.DayOfYear - 1;
            }
            if (DateTime.IsLeapYear(departureDate.Year) && departureDate.DayOfYear >= 60)
            {
                dayofyearDepartureDate = departureDate.DayOfYear - 1;
            }

            age = departureDate.Year - dateofBirth.Year;

            if (dayofyearDepartureDate < dayofyearDOB)
            {
                age--;
            }

            return age;
        }
    }
}
