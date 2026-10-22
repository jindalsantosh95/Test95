using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Entities.Common
{
    public class ErrorDetails
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string ErrorSource { get; set; }
        public string StatusText { get; set; }
        public string TagText { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionCode { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }

        public static explicit operator ErrorDetails(JObject source)
        {
            return new ErrorDetails()
            {
                Id = source["Id"]?.ToString(),
                Code = source["Code"]?.ToString(),
                Message = source["Message"]?.ToString(),
                DeveloperMessage = source["DeveloperMessage"]?.ToString(),
                ErrorSource = source["ErrorSource"]?.ToString(),
                StatusText = source["StatusText"]?.ToString(),
                TagText = source["TagText"]?.ToString(),
                ExceptionCode = source["ExceptionCode"]?.ToString(),
                ExceptionType = source["ExceptionType"]?.ToString(),
                Url = source["Url"]?.ToString(),
                Title = source["Title"]?.ToString()
            };
        }
    }
}
