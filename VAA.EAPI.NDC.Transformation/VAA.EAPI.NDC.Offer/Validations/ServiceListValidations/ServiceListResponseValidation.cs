using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using DynamicMessagesContent = VAA.EAPI.Domain.Entities.Common.DynamicMessagesContent;

namespace VAA.EAPI.NDC.Offer.Validations.ServiceListValidations
{
    public class ServiceListResponseValidation : IServiceListResponseValidation
    {
        private readonly IHostEnvironment _hostEnvironment;

        public ServiceListResponseValidation(IHostEnvironment hostEnvironment)
        {
            this._hostEnvironment = hostEnvironment;
        }

        public IEnumerable<NDCWarning> PopulateNonVSSegmentWarningsOrder(OrderDocument orderDocument)
        {
            var ndcWarningList = orderDocument.DataList.SegmentDetails
                .Where(s => !(s.MarketingCarrier.Is(Constants.Application.OwnerCode) && string.IsNullOrEmpty(s.OperatingCarrier)))
                .Select(x => new NDCWarning
                {
                    Code = "76H_2",
                    Type = "Warning",
                    IsDynamicMessage = true,
                    DynamicMessagesContents = new List<DynamicMessagesContent>()
                        {
                            new DynamicMessagesContent()
                            {
                                Fieldname = "<<FlightNumber>>",
                                ReplaceWith = x.OperatingCarrier + x.OperatingFlightNumber
                            }
                        }
                });
            return ErrorHelper<NDCWarning>.GetWarnings(ndcWarningList, _hostEnvironment);
        }

        public IEnumerable<NDCWarning> PopulateNonVSSegmentWarningsOffer(OfferResponse offerResponse)
        {
            var ndcWarningList = offerResponse.DataList.FlightSegments
                                               .Where(item => item.OperatingCarrier != null && !item.OperatingCarrier.Is(Constants.Application.OwnerCode))
                                               .Select(item => new NDCWarning
                                               {
                                                   Code = "76H_2",
                                                   Type = "Warning",
                                                   IsDynamicMessage = true,
                                                   DynamicMessagesContents = new List<DynamicMessagesContent>()
                                                     {
                                                            new DynamicMessagesContent()
                                                            {
                                                                Fieldname = "<<FlightNumber>>",
                                                                ReplaceWith = item.OperatingCarrier+item.OperatingFlightNumber
                                                            }
                                                     }
                                               });
            return ErrorHelper<NDCWarning>.GetWarnings(ndcWarningList, _hostEnvironment);
        }
    }
}
