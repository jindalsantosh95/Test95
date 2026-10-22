using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Validation
{
    public class ValidateOrderSearchRequest
    {
        public static Result<Response<string>> OrderSearchRequestValidator(OrderSearchResponseQuery requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>()
            {
                Status = Status.OK,
                Errors = null
            };
            bool isPnrRequest = false;
            bool isTicketRequest = false;

            if (!requestData.HttpRequest.Headers.Contains("ChannelId")) return GetErrorResponse(errorList, ErrorCodes.MissingOrInvalidChannelId);

            var channelId = requestData.HttpRequest.Headers.GetValues("ChannelId").FirstOrDefault();

            if (channelId != Constants.Application.NDCChannelId && channelId != Constants.Application.SelfServiceChannelId)
                response = GetErrorResponse(errorList, ErrorCodes.MissingOrInvalidChannelId);

            if (requestData?.OrderSearchRequest?.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC != null)
                isPnrRequest = true;

            if (requestData?.OrderSearchRequest?.SearchFilterCriteria?.accountableDocumentFilterCriteria?.accountableDocumentReference?.DocumentNumber != null)
                isTicketRequest = true;

            if (!isPnrRequest && !isTicketRequest)
                response = GetErrorResponse(errorList, ErrorCodes.InvalidAttributeLength);
            else if (isPnrRequest)
            {
                //Validating PNR and its length should be 6 characters
                if (requestData?.OrderSearchRequest?.SearchFilterCriteria?.bookingReferenceCriteria?.BookingReference?.RLOC?.Length != Constants.Application.PNRLength)
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAttributeLength);
            }
            //Validating eTicket and its length should be 13 characters
            else if (requestData?.OrderSearchRequest?.SearchFilterCriteria?.accountableDocumentFilterCriteria?.accountableDocumentReference?.DocumentNumber?.Length != Constants.Application.ETicketLength)
                response = GetErrorResponse(errorList, ErrorCodes.InvalidETicketNumber);

            return response;
        }

        private static Response<string> GetErrorResponse(List<ErrorDetails> errorList, string errorCode)
        {
            errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());

            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = errorList
            };
        }

        public static bool VerifyCustomerName(OrderSearchResponseQuery requestData, RetrieveResponseType retrieveResponse)
        {
            if (requestData.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName != null)
            {
                List<string> hashKeysFromResponse = new();

                if (requestData.HttpRequest.Headers.GetValues("ChannelId").First() == Constants.Application.SelfServiceChannelId)
                {
                    var hashKeyFromHeaders = requestData.HttpRequest.Headers.GetValues("HashKey").First();

                    foreach (var passenger in retrieveResponse.TeardownContextResponse.Pnr.Passenger)
                        hashKeysFromResponse.Add(Hash.SHA256Managed($"{passenger.FirstName}{passenger.LastName}"));

                    return hashKeysFromResponse.Any(x => x == hashKeyFromHeaders);
                }

                return retrieveResponse.TeardownContextResponse.Pnr.Passenger.Any(item => item.FirstName == requestData.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.GivenName.ToUpper() && item.LastName == requestData.OrderSearchRequest.SearchFilterCriteria.bookingReferenceCriteria.customerName.SurName.ToUpper());
            }

            return true;
        }
    }
}
