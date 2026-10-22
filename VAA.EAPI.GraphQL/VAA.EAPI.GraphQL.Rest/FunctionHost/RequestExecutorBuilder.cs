using HotChocolate;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost
{
    internal sealed class RequestExecutorBuilder(IServiceCollection Services) : IRequestExecutorBuilder
    {
        private readonly IServiceCollection _services = Services ?? throw new ArgumentNullException(nameof(Services));

        public NameString Name => "InternalRequestExecutorBuilderContainer";

        public IServiceCollection Services => _services;
    }
}
