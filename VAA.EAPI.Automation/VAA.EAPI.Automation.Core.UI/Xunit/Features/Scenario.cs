namespace VAA.EAPI.Automation.Core.UI.Xunit.Features
{
    public sealed class Scenario(Feature feature, string name)
    {
        public static explicit operator Scenario((Feature feature, string name) source) => new(source.feature, source.name);

        public Feature Feature { get; } = feature;
        public string Name { get; } = name;
        public List<string> Steps { get; } = [];

        public override string ToString() => Name;
    }
}
