using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Validations.AirShopping
{
    public class AirShoppingResponseValidations
    {
        public static Response<T> ValidateMicroServiceResponse<T>(OfferResponse offerResponse, IHostEnvironment hostEnvironment)
        {
            var errorIds = ResponseValidations.ValidateOfferMicroRS(offerResponse).ToList();

            return ErrorHelper<T>.GetErrors(errorIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateCalendarSearchResponse(CalendarSearchResponse calendarSearchResponse, IHostEnvironment hostEnvironment)
        {
            var warningIds = ResponseValidations.ValidateCalendarSearch(calendarSearchResponse).ToList();

            return ErrorHelper<NDCWarning>.GetWarnings(warningIds, hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidatePenaltyCurrencyMismatch(OfferResponse offerResponse, IHostEnvironment hostEnvironment)
        {
            var warningIds = ResponseValidations.ValidatePenaltyCurrencyMismatch(offerResponse).ToList();

            return ErrorHelper<NDCWarning>.GetWarnings(warningIds, hostEnvironment);
        }
    }
}
