using System.Reflection;
using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.Core.Xunit.Extensions
{
    public static class TestExecutedInfoExtensions
    {
        public static string GetTraitValue(this global::Xunit.Runners.TestExecutedInfo source, string name, params Assembly[] assemblies)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(name));
            if (assemblies.IsNullOrEmpty())
                throw new ArgumentOutOfRangeException(nameof(assemblies));

            Type type = assemblies.FirstOrDefault(a => a.GetType(source.TypeName) is not null)?.GetType(source.TypeName);

            if (type is null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Unable to locate type '{source.TypeName}' in assemblies.");

            CustomAttributeData typeTraitAttributeData = type.GetCustomAttributesData()
                                             .FirstOrDefault(x => x.AttributeType == typeof(TraitAttribute) && x.ConstructorArguments[0].Value.ToString().Is(name));

            CustomAttributeData methodTraitAttributeData = type.GetMethod(source.MethodName)
                                               .GetCustomAttributesData()
                                               .FirstOrDefault(x => x.AttributeType == typeof(TraitAttribute) && x.ConstructorArguments[0].Value.ToString().Is(name));

            CustomAttributeData traitAttributeData = methodTraitAttributeData ?? typeTraitAttributeData;

            return traitAttributeData?.ConstructorArguments[1].Value.ToString() ?? string.Empty;
        }
    }
}
