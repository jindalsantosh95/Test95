using System.Reflection;
using Xunit;

namespace VAA.EAPI.Automation.Core.Xunit.Attributes
{
    public sealed class CsvDataAttribute : ClassDataAttribute
    {
        public string Path { get; private set; }
        public string Delimiters { get; private set; }
        public bool HasHeaders { get; private set; }
        public bool HasFieldsEnclosedInQuotes { get; private set; }
        public string GroupBy { get; private set; }

        public CsvDataAttribute(string path, string delimiters = ",", bool hasHeaders = true, bool hasFieldsEnclosedInQuotes = false, string groupBy = null) : base(typeof(CsvTestData))
        {
            Path = path;
            Delimiters = delimiters;
            HasHeaders = hasHeaders;
            HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
            GroupBy = groupBy;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            IEnumerable<object[]> data = Activator.CreateInstance(Class, Path, Delimiters, HasHeaders, HasFieldsEnclosedInQuotes, GroupBy) as IEnumerable<object[]>;

            if (data == null)
                throw new ArgumentException($"{Class.FullName} must implement IEnumerable<object[]> to be used as ClassData for the test method named '{testMethod.Name}' on {testMethod.DeclaringType.FullName}");

            return data;
        }
    }
}
