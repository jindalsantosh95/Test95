using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Xunit.Runners
{
    internal sealed class Operator : IOperation
    {
        public static explicit operator Operator(string source)
        {
            if (!RegexCompiled.IsMatch(source, @"^\||&$", System.Text.RegularExpressions.RegexOptions.Singleline)) throw new ArgumentOutOfRangeException(nameof(source), "Unexpected value, was expecting one of four possible values: =, !=, ~, !~");

            return new Operator()
            {
                Value = source
            };
        }

        public Group Parent { get; set; }
        public string Value { get; internal set; }
    }
}
