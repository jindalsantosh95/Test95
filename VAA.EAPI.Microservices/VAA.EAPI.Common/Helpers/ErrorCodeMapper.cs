using System.Linq;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Common.Helpers
{
    public class ErrorCodeMapper
    {
        public static Status MapErrorResultCode(ResponseMessage responseMessage)
        {
            return responseMessage.Errors.Any(x => x.Message.Contains(Constants.Application.Subscription)) ? Status.Unauthorized : responseMessage.Errors.Any(x => x.Message.Contains(Constants.Application.TechErrorMsg)) ? Status.Error : Status.BusinessException;
        }


        public static int MapHttpStatusCode(ResponseMessage responseMessage)
        {
            if (responseMessage?.Errors?.Any() != true)
            {
                return Constants.Application.Error;
            }
            if (responseMessage.Errors.Any(x => x.Message?.Contains(Constants.Application.Subscription) == true))
            {
                return Constants.Application.Unauthorized;
            }

            if (responseMessage.Errors.Any(x => x.Message?.Contains(Constants.Application.TechErrorMsg) == true))
            {
                return Constants.Application.Error;
            }

            return Constants.Application.BusinessException;
        }
    }
}
