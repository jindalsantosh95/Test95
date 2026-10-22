using Microsoft.Azure.Functions.Worker;
using System.Collections.Immutable;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestFunctionDefinition : FunctionDefinition
    {
        private readonly Dictionary<string, BindingMetadata> inputBindings = new();
        private readonly Dictionary<string, BindingMetadata> outputBindings = new();

        public override ImmutableArray<FunctionParameter> Parameters => throw new NotImplementedException();

        public override string PathToAssembly => Path.GetFullPath(".");

        public override string EntryPoint => throw new NotImplementedException();

        public override string Id => throw new NotImplementedException();

        public override string Name => throw new NotImplementedException();

        public override IImmutableDictionary<string, BindingMetadata> InputBindings => inputBindings.ToImmutableDictionary();

        public override IImmutableDictionary<string, BindingMetadata> OutputBindings => outputBindings.ToImmutableDictionary();
    }
}
