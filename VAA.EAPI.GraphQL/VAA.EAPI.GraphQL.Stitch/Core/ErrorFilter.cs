
using HotChocolate;

namespace VAA.EAPI.GraphQL.Stitch.Core
{
    public sealed class ErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            return error;
        }
    }
}
