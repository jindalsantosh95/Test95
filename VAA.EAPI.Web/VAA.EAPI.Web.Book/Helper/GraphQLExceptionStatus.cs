using System.Linq;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;

namespace VAA.EAPI.Web.Book.Helper
{
    public class GraphQLExceptionStatus
    {
        public static Status GetStatusCode(GraphQLException graphQLException, bool IsConfirmation = false)
        {
            if (graphQLException?.Errors == null || !graphQLException.Errors.Any())
            {
                return Status.Error;
            }

            var errorCode = graphQLException.Errors.First().Extensions?.Code;

            return errorCode switch
            {
                "401" => Status.Unauthorized,
                "403" => Status.Forbidden,
                "400" => Status.BadRequest,
                "500" => IsConfirmation ? Status.BusinessException : Status.Error,
                _ => Status.Error
            };

        }

    }
}