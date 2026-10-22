using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Common.Helpers
{
    public static class DefaultError
    {
        public static ResponseMessage DefaultErrorMesaage()
        {
            var responseMessage = new ResponseMessage()
            {
                Errors = new List<ErrorDetails>()
                    {
                        new ErrorDetails()
                        {
                            Code = Constants.Application.TechError,
                            Message = Constants.Application.TechErrorMsg,
                            StatusText = Constants.Application.NotProcessed,
                            TagText = Constants.Application.TagText
                        }
                    }
            };
            return responseMessage;
        }
    }
}
