using Microsoft.Azure.Functions.Worker;
using System.Collections.Immutable;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestFunctionDefinition : FunctionDefinition
    {
        private readonly FunctionParameter[] parameters;
        private readonly string entryPoint;
        private readonly string id;
        private readonly string name;
        private readonly Dictionary<string, BindingMetadata> inputBindings = [];
        private readonly Dictionary<string, BindingMetadata> outputBindings = [];

        public TestFunctionDefinition() : this(Guid.NewGuid().ToString("N"), nameof(TestFunctionDefinition), $"{typeof(TestFunctionDefinition).FullName}.Test", new() { { "req", new TestBindingMetadata("req", "httpTrigger", BindingDirection.In) } }) { }
        public TestFunctionDefinition(Dictionary<string, BindingMetadata> inputBindings) : this(Guid.NewGuid().ToString("N"), nameof(TestFunctionDefinition), $"{typeof(TestFunctionDefinition).FullName}.Test", inputBindings) { }
        public TestFunctionDefinition(Dictionary<string, BindingMetadata> inputBindings, Dictionary<string, BindingMetadata> outputBindings) : this(Guid.NewGuid().ToString("N"), nameof(TestFunctionDefinition), $"{typeof(TestFunctionDefinition).FullName}.Test", inputBindings, outputBindings) { }
        public TestFunctionDefinition(Dictionary<string, BindingMetadata> inputBindings, Dictionary<string, BindingMetadata> outputBindings, FunctionParameter[] parameters) : this(Guid.NewGuid().ToString("N"), nameof(TestFunctionDefinition), $"{typeof(TestFunctionDefinition).FullName}.Test", inputBindings, outputBindings, parameters) { }
        public TestFunctionDefinition(string id, string name, string entryPoint) : this(id, name, entryPoint, new() { { "req", new TestBindingMetadata("req", "httpTrigger", BindingDirection.In) } }) { }
        public TestFunctionDefinition(string id, string name, string entryPoint, Dictionary<string, BindingMetadata> inputBindings) : this(id, name, entryPoint, inputBindings, []) { }
        public TestFunctionDefinition(string id, string name, string entryPoint, Dictionary<string, BindingMetadata> inputBindings, Dictionary<string, BindingMetadata> outputBindings) : this(id, name, entryPoint, inputBindings, outputBindings, []) { }
        public TestFunctionDefinition(string id, string name, string entryPoint, Dictionary<string, BindingMetadata> inputBindings, Dictionary<string, BindingMetadata> outputBindings, FunctionParameter[] parameters)
        {
            this.id = id;
            this.name = name;
            this.entryPoint = entryPoint;
            this.inputBindings = inputBindings;
            this.outputBindings = outputBindings;
            this.parameters = parameters;
        }

        public override ImmutableArray<FunctionParameter> Parameters => [.. parameters];

        public override string PathToAssembly => Path.GetFullPath(".");

        public override string EntryPoint => entryPoint;

        public override string Id => id;

        public override string Name => name;

        public override IImmutableDictionary<string, BindingMetadata> InputBindings => inputBindings.ToImmutableDictionary();

        public override IImmutableDictionary<string, BindingMetadata> OutputBindings => outputBindings.ToImmutableDictionary();
    }
}
