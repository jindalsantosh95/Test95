using Microsoft.Azure.Functions.Worker;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestBindingContext : BindingContext
    {
        private readonly IReadOnlyDictionary<string, object?> bindingData;

        public TestBindingContext(IReadOnlyDictionary<string, object?> bindingData) => this.bindingData = bindingData;

        public override IReadOnlyDictionary<string, object?> BindingData => bindingData;
    }
}
