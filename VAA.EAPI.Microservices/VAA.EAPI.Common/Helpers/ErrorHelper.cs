using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Common.Helpers
{
    public static class ErrorHelper<T>
    {
        public static Response<T> GetErrors(IEnumerable<ErrorDetail> errorIds, IHostEnvironment hostEnvironment)
        {
            if (!errorIds.IsNullOrEmpty())
            {
                var errorCodesJson = File.ReadAllText($"{hostEnvironment.ContentRootPath}/Resources/ErrorCodes.json");
                var errorCodes = Json.Deserialize<NDCErrors>(errorCodesJson);

                var errors = errorIds.Select(errorId =>
                {
                    var error = errorCodes.ErrorDetailList.FirstOrDefault(x => x.Id == errorId.ErrorCode);

                    return new ErrorDetails()
                    {
                        Code = error.Code,
                        Id = error.Id,
                        Message = (!errorId.IsDynamicMessage) ? error.Message
                        : errorId.DynamicMessagesContents.Select(r => error.Message.Replace(r.Fieldname, r.ReplaceWith)).FirstOrDefault(),
                        StatusText = error.StatusText,
                        TagText = error.TagText,
                        Url = error?.Url
                    };
                });

                errors = errors.OrderBy(q => q?.Code).Distinct().ToList();

                return GetErrorResult(errors);
            }

            return null;
        }

        public static Response<T> GetErrorResult(IEnumerable<ErrorDetails> errors)
        {
            if (!errors.IsNullOrEmpty())
            {
                return new Response<T>()
                {
                    ResponseMessages = new ResponseMessage()
                    {
                        Errors = errors
                    },
                    Status = Status.BusinessException,
                    ErrorMessage = "Validation error"
                };
            }

            return null;
        }

        public static IEnumerable<NDCWarning> GetWarnings(IEnumerable<NDCWarning> warnings, IHostEnvironment hostEnvironment)
        {
            if (warnings == null || !warnings.Any())
                return Enumerable.Empty<NDCWarning>();

            var errorCodesJson = File.ReadAllText(Path.Combine(hostEnvironment.ContentRootPath, "Resources", "ErrorCodes.json"));
            var errorCodes = Json.Deserialize<NDCErrors>(errorCodesJson);

            return warnings.Select(errorId =>
            {
                var error = errorCodes.ErrorDetailList.FirstOrDefault(x => x.Id == errorId.Code);

                if (error == null) return null; // Skip or handle missing error code

                var shortText = error.Message;

                if (errorId.IsDynamicMessage && errorId.DynamicMessagesContents?.Any() == true)
                {
                    foreach (var msg in errorId.DynamicMessagesContents)
                    {
                        if (!string.IsNullOrEmpty(msg.Fieldname) && msg.ReplaceWith != null)
                        {
                            shortText = shortText.Replace(msg.Fieldname, msg.ReplaceWith);
                        }
                    }
                }

                return new NDCWarning()
                {
                    Code = error.Code,
                    Type = errorId.Type,
                    Status = error.StatusText,
                    TagText = error.TagText,
                    ShortText = shortText,
                    DocURL = errorId?.DocURL,
                };

            }).Where(w => w != null);

        }

    }

}
