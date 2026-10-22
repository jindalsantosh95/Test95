using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public static class ErrorHelpers
    {
        public static Exception GetInnermostExceptionMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        public static string AggregateHttpConnectorException(HttpRequestException ex)
        {
            var currentException = ex;
            while(currentException != null && currentException is HttpRequestException)
            {
                var httpEx = currentException as HttpRequestException;
                IEnumerable<string> exceptionData = Enumerable.Empty<string>();
                foreach (var key in httpEx.Data.Keys)
                    if(!key.Equals("Request"))
                    {
                        exceptionData = exceptionData.Append($"{key}: {httpEx.Data[key]}");
                    }
                return string.Join($"Response Data:", Json.Serialize(exceptionData));
            }
            return currentException.Message;
        }

        public static string AggregateException(Exception ex)
        {
            if(ex == null) return string.Empty;
            var innerException = GetInnermostExceptionMessage(ex);
            if(innerException is HttpRequestException)
            {
                return AggregateHttpConnectorException(innerException as HttpRequestException);
            }
            var exceptionData = innerException.Data;
            return string.Join($" Response Data:", ex.Message, Json.Serialize(exceptionData), innerException?.Message);
        }
    }
}
