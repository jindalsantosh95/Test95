using Microsoft.Azure.Functions.Worker;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestBindingMetadata : BindingMetadata
    {
        private string name;
        private string type;
        private BindingDirection direction;

        public TestBindingMetadata(string name, string type, BindingDirection direction)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.type = type ?? throw new ArgumentNullException(nameof(type));
            this.direction = direction;
        }

        public override string Name => name;

        public override string Type => type;

        public override BindingDirection Direction => direction;
    }
}
