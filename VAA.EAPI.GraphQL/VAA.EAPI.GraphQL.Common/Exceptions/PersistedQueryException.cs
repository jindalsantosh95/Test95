using System;

namespace VAA.EAPI.GraphQL.Common.Exceptions
{
    public class PersistedQueryException : Exception
    {
        public PersistedQueryException(string Message) : base(Message) { }
    }
}
