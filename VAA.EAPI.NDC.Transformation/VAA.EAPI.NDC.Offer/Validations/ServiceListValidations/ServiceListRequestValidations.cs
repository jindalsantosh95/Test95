using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using ErrorDetail = VAA.EAPI.Domain.Entities.Common.ErrorDetail;

namespace VAA.EAPI.NDC.Offer.Validations.ServiceListValidations
{
    public class ServiceListRequestValidations<T> : IServiceListRequestValidations<T>
    {
        private readonly IHostEnvironment _hostEnvironment;

        public ServiceListRequestValidations(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public Response<T> ValidateServiceListRequest(IATA_ServiceListRQ serviceListReq, bool isOfferServiceRq)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateServiceList.ValidateAgentDetails(serviceListReq.DistributionChain));

            if (isOfferServiceRq)
            {
                errorIds.AddRange(ValidateServiceList.ValidateServiceOfferDetails(serviceListReq));
            }
            else
            {
                errorIds.AddRange(ValidateServiceList.ValidateServiceOrderDetails(serviceListReq));
            }
            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateServiceListOrder(OrderDocument orderDocument, bool isOfferServiceRq, OfferResponse offerResponse)
        {
            List<ErrorDetail> errorIds = new();

            if (isOfferServiceRq)
            {
                errorIds.AddRange(ValidateServiceList.ValidateOfferDetails(offerResponse));
            }
            else
            {
                errorIds.AddRange(ValidateServiceList.ValidateOrderDetails(orderDocument));
            }
            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateAgency(ValidationRS validationRS, OrderDocument orderDocument, bool isOfferServiceRq, OfferResponse offerDocument)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(RequestValidations.ValidateAgencyDetails(validationRS));

            if (errorIds.Count == 0 && validationRS.ValidateResponse != null)
            {
                if (isOfferServiceRq)
                    errorIds.AddRange(RequestValidations.ValidateAgentID_OfferServiceList(validationRS.ValidateResponse, offerDocument));
                else
                    errorIds.AddRange(RequestValidations.ValidateAgentIDForServiceList(validationRS.ValidateResponse, orderDocument));
            }
            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }
        
    }
}