using System.Reflection;
using System.Xml.Linq;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

internal static class NativeUiFailureSnapshot
{
    private static readonly HashSet<string> KnownIds = new[]
        { typeof(NativeSearchAccessibilityIds), typeof(NativeBookingAccessibilityIds) }
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToHashSet(StringComparer.Ordinal);

    internal static XElement Sanitise(string source)
    {
        const int maximumElements = 2000;
        XElement snapshot = new("NativeUiSnapshot", new XAttribute("labelsAndValues", "omitted"));
        XElement[] elements = XDocument.Parse(source).Descendants().Take(maximumElements + 1).ToArray();
        snapshot.Add(new XAttribute("truncated", elements.Length > maximumElements));
        foreach (XElement element in elements.Take(maximumElements))
        {
            XElement safe = new("Element");
            string type = element.Attribute("type")?.Value ?? element.Name.LocalName;
            if (RegexCompiled.IsMatch(type, "^(XCUIElementType[A-Za-z]+|android\\.[A-Za-z.]+)$"))
            {
                safe.Add(new XAttribute("type", type));
            }

            string identifier = element.Attribute("name")?.Value ?? string.Empty;
            if (KnownIds.Contains(identifier) || RegexCompiled.IsMatch(identifier,
                "^NativeFlightResult[0-9]{1,4}(Carrier[A-Z0-9]{2,3})?$"))
            {
                safe.Add(new XAttribute("id", identifier));
            }

            foreach (string state in new[] { "enabled", "visible", "selected", "accessible" })
            {
                if (bool.TryParse(element.Attribute(state)?.Value, out bool value))
                {
                    safe.Add(new XAttribute(state, value));
                }
            }

            snapshot.Add(safe);
        }

        return snapshot;
    }
}
