using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Mapper.ErrorMapper
{
    public static class ErrorMapper
    {
        public static Response<IATA_OrderViewRS> GetErrorDetails(ResponseMessage responseMessage)
        {
            return new Response<IATA_OrderViewRS>()
            {
                Status = ErrorCodeMapper.MapErrorResultCode(responseMessage),
                Value = MapToErrorOrderViewRs(responseMessage)
            };
        }

        public static IATA_OrderViewRS MapToErrorOrderViewRs(ResponseMessage errors)
        {
            return new IATA_OrderViewRS { Items = GetErrors(errors).ToArray() };
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

        public static IATA_OrderViewRS MapWarnings(IEnumerable<NDCWarning> warnings)
        {
            return new IATA_OrderViewRS { Items = warnings.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.Type,
                TagText = x.TagText
            }).ToArray()};
        }
    }
}
