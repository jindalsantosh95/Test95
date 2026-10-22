using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Validations
{
    public class ResponseValidations
    {
        public static List<ErrorDetail> ValidateOfferMicroRS(OfferResponse offerResponse)
        {
            var errors = new List<ErrorDetail>();

            if (offerResponse == null)
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "911_22",
                    IsDynamicMessage = false
                });
            }
            else if (offerResponse.Error?.Any() == true)
            {
                if (offerResponse.Error.Any(x => x.Code == "3001" || x.Code == "G042"))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "ITA_NoOffers",
                        IsDynamicMessage = false
                    });
                }
                else
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "911_22",
                        IsDynamicMessage = false
                    });
                }                
            }

            if (errors.Count == 0 && offerResponse.Offers == null)
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "ITA_NoOffers",
                    IsDynamicMessage = false
                });
            }

            return errors;
        }

        public static List<NDCWarning> ValidateCalendarSearch(CalendarSearchResponse calendarSearchResponse)
        {
            var warnings = new List<NDCWarning>();

            if (calendarSearchResponse == null || calendarSearchResponse.FlexibleDateCalendar?.DepartureDetails.Count() == 0 || calendarSearchResponse.Error != null)
            {
                warnings.Add(new NDCWarning
                {
                    Code = "76_H",
                    IsDynamicMessage = false
                });
            }

            return warnings;
        }

        public static List<NDCWarning> ValidateFormOfPaymentType(Infrastructure.Model.ValidationMS.ValidationRS validationMSResponse, bool isCreditCardAccepted)
        {
            var warnings = new List<NDCWarning>();

            if (!isCreditCardAccepted && !validationMSResponse.ValidateResponse.AgencyPermissions.BspActiveStatusInd.Value)
            {
                warnings.Add(new NDCWarning
                {
                    Code = "466_1",
                    IsDynamicMessage = false
                });
            }

            return warnings;
        }

        public static List<NDCWarning> ValidatePenaltyCurrencyMismatch(OfferResponse offerResponse)
        {
            var warnings = new List<NDCWarning>();
            {
                var posCur = offerResponse?.Pos?.CurrencyCode;
                var penaltyCur = offerResponse.DataList.Penalty.First().MaxPrice?.Amount?.BaseCurrency;
                if (posCur != penaltyCur)
                {
                    warnings.Add(new NDCWarning
                    {
                        Code = "71_B",
                        IsDynamicMessage = false
                    });
                }

                return warnings;
            }
        }
    }
}

