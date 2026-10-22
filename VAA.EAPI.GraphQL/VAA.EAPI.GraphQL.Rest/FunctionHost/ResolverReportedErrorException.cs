using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost
{
    public sealed class ResolverReportedErrorException(IReadOnlyList<IError> errors) : Exception(errors?.FirstOrDefault()?.Message ?? "Resolver reported errors.")
    {
        public IReadOnlyList<IError> Errors { get; } = errors ?? [];
    }
}
