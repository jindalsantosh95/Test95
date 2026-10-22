using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderListRS;

namespace VAA.EAPI.NDC.Order.Helper
{
    public static class OrderListErrorMapper
    {
        public static Response<IATA_OrderListRS> GetErrorDetails(ResponseMessage responseMessage)
        {
            return new Response<IATA_OrderListRS>()
            {
                Status = ErrorCodeMapper.MapErrorResultCode(responseMessage),
                Value = MapToErrorOrderViewRs(responseMessage)
            };
        }

        public static IATA_OrderListRS MapToErrorOrderViewRs(ResponseMessage errors)
        {
            return new IATA_OrderListRS { Items = GetErrors(errors).ToArray() };
        }

        public static IEnumerable<ErrorType> GetErrors(ResponseMessage errors)
        {
            return errors.Errors.Select(x => new ErrorType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.Message,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.StatusText,
                TagText = x.TagText
            });
        }
    }
}
