namespace VAA.EAPI.Automation.Core.UI.Xunit.Features
{
    public static class FeatureParser
    {
        private static readonly string[] StepPrefixes = ["Given ", "When ", "Then ", "And ", "But "];

        public static Feature Parse<T>(string filePath)
        {
            var lines = File.ReadAllLines(filePath)
                            .Select(x => x.Trim())
                            .Where(x => x.Length > 0)
                            .ToArray();

            Feature feature = null;
            Scenario currentScenario = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("Feature:", StringComparison.OrdinalIgnoreCase))
                {
                    var featureName = line.Split(':', 2)[1].Trim();

                    feature = (Feature)(typeof(T), featureName, filePath);

                    continue;
                }

                if (line.StartsWith("Scenario:", StringComparison.OrdinalIgnoreCase))
                {
                    if (feature is null) throw new InvalidDataException("Invalid feature file.");

                    var scenarioNAme = line.Split(':', 2)[1].Trim();

                    currentScenario = (Scenario)(feature, scenarioNAme);

                    feature.Scenarios.Add(currentScenario);

                    continue;
                }

                if (StepPrefixes.Any(p => line.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    currentScenario?.Steps.Add(line);
            }

            return feature;
        }
    }
}