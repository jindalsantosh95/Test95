using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Validations.Common
{
    public class OrderSearchResponseValidation
    {
        public static IEnumerable<ErrorDetail> ValidateOrderSearchNullResponse(Air4OrderSearchMSResponse orderSearchResponse)
        {
            if (orderSearchResponse == null)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "914_6" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderSearchRS(IEnumerable<ErrorDetails> errorDetails)
        {
            var errors = new List<ErrorDetail>();
            foreach (var errorDetail in errorDetails)
            {
                if (Constants.Application.OrderSearchErrorCodesGeneral.Split(";").Contains(errorDetail.Code))
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "911_22" });

                if (Constants.Application.OrderSearchErrorCodesSpecific.Split(";").Contains(errorDetail.Code))
                    errors.Add(new() { IsDynamicMessage = false, ErrorCode = "912_20" });

                if (errorDetail.Code == "1079") errors.Add(new() { IsDynamicMessage = false, ErrorCode = "129_2" });

                if (errorDetail.Code == "1100") errors.Add(new() { IsDynamicMessage = false, ErrorCode = "118_2" });
            }

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateOrderSearchRSForVSTicks(OrderSearchResponse orderSearchResponse)
        {
            if (orderSearchResponse.OrderItems.First().AccountableDocumentRefs.Any(x => x == null || !x.DocumentNumber.StartsWith("932")))
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "129_4" } };

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderSearchRSForGDSAgent(OrderSearchResponse orderSearchResponse, string iataNum)
        {
            if (orderSearchResponse.PointOfSale?.IATANumber != iataNum)
                return new List<ErrorDetail>() { new() { IsDynamicMessage = false, ErrorCode = "368_2" } };

            return Enumerable.Empty<ErrorDetail>();
        }
    }
}
