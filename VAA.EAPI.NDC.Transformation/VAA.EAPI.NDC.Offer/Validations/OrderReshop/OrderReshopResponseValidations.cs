using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Validations.OrderReshop
{
    public static class OrderReshopResponseValidations
    {
        public static Response<T> ValidateReshopOfferMS<T>(OfferResponse offerResponse, IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<T>.GetErrors(ValidateReshopResponseErrors(offerResponse), hostEnvironment);
        }

        public static IEnumerable<ErrorDetail> ValidateReshopResponseErrors(OfferResponse offerResponse)
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
                if (offerResponse.Error.Any(x => x.Code == "G042"))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "100_2",
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
    }
}
