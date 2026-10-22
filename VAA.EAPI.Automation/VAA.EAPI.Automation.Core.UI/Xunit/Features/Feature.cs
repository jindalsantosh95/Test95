namespace VAA.EAPI.Automation.Core.UI.Xunit.Features
{
    public sealed class Feature(Type @class, string name, string sourcePath)
    {
        public static explicit operator Feature((Type @class, string name, string sourcePath) source)
            => new(source.@class, source.name, source.sourcePath);

        public Type Class { get; } = @class;
        public string Name { get; } = name;
        public List<Scenario> Scenarios { get; } = [];
        public string SourcePath { get; } = sourcePath;

        public override string ToString() => Name;
    }
}
