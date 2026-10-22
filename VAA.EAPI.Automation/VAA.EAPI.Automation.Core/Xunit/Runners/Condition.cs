using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Xunit.Runners
{
    internal sealed class Condition : IOperation
    {
        public static explicit operator Condition((Group parent, string filter) source)
        {
            if (!RegexCompiled.IsMatch(source.filter, "!~|~|!=|="))
                source.filter = $"FullyQualifiedName~{source.filter}";

            Match parsed = RegexCompiled.Match(source.filter, "([^!~=]+)(!~|~|!=|=)(.+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return new Condition()
            {
                Parent = source.parent,
                Property = parsed.Groups[1].Value,
                Operator = parsed.Groups[2].Value,
                Value = parsed.Groups[3].Value
            };
        }

        public Group Parent { get; set; }
        public string Property { get; internal set; }
        public string Operator { get; internal set; }
        public string Value { get; internal set; }

        public bool IsValid(ITestCase testCase)
        {
            string[] value = Property.ToLower() switch
            {
                "fullyqualifiedname" => new[] { $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}" },
                "displayname" => new[] { $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.DisplayName}" },
                "category" => testCase.Traits.Where(x => x.Key.Is("Category"))
                                             .SelectMany(x => x.Value)
                                             .ToArray(),
                "tag" => testCase.Traits.Where(x => x.Key.Is("Tag"))
                                        .SelectMany(x => x.Value)
                                        .ToArray(),
                _ => Array.Empty<string>()
            };

            if (value.IsNullOrEmpty()) return false;

            return Operator switch
            {
                "=" => value.Any(x => Value.Is(x)),
                "!=" => !value.Any(x => Value.Is(x)),
                "~" => !value.Any(x => Value.Has(x)),
                "!~" => !value.Any(x => !Value.Has(x)),
                _ => false,
            };
        }
    }
}
