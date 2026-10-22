using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.EndtoEnd
{
    public static class E2ETestData
    {
        public const string AdHocScenarioFileEnvironmentVariable = "BOOK_AD_HOC_SCENARIO_FILE";

        private const string LegacyBookGraphQLRegression = "BookGraphQLRegression";
        private const string LegacyBookE2ERegression = "BookE2ERegression";

        private static readonly Lazy<List<E2EScenario>> _scenarios = new(LoadScenarios);

        public static IEnumerable<object[]> AllScenarios()
        {
            foreach (E2EScenario scenario in _scenarios.Value)
            {
                yield return new object[] { scenario };
            }
        }

        public static IEnumerable<object[]> SanityScenarios()
        {
            return ScenariosForCategories(Traits.BookGraphQLPSSITSanity);
        }

        public static IEnumerable<object[]> RegressionScenarios()
        {
            return ScenariosForCategories(LegacyBookGraphQLRegression, Traits.BookGraphQLRegression);
        }

        public static IEnumerable<object[]> E2ERegressionScenarios()
        {
            return ScenariosForCategories(LegacyBookE2ERegression, Traits.BookE2ERegression);
        }

        public static IEnumerable<object[]> CoreRegressionScenarios()
        {
            return ScenariosForCategories(Traits.BookCoreRegression);
        }

        public static IEnumerable<object[]> FunctionalScenarios()
        {
            return ScenariosForCategories(Traits.BookFunctional);
        }

        public static IEnumerable<object[]> SharedServicesSignOffScenarios()
        {
            return ScenariosForCategories(Traits.SharedServicesSignOff);
        }

        private static IEnumerable<object[]> ScenariosForCategories(params string[] expectedCategories)
        {
            return AllScenarios().Where(scenario =>
                HasAnyCategory((E2EScenario)scenario[0], expectedCategories));
        }

        private static bool HasAnyCategory(E2EScenario scenario, params string[] expectedCategories)
        {
            return scenario.Categories != null
                && scenario.Categories.Any(category =>
                    expectedCategories.Any(expectedCategory => category.Is(expectedCategory)));
        }

        private static List<E2EScenario> LoadScenarios()
        {
            string adHocScenarioFile = System.Environment.GetEnvironmentVariable(AdHocScenarioFileEnvironmentVariable);
            if (!adHocScenarioFile.IsNullOrWhiteSpace())
            {
                if (!File.Exists(adHocScenarioFile))
                {
                    throw new FileNotFoundException(
                        $"Ad-hoc Book scenario file from {AdHocScenarioFileEnvironmentVariable} was not found.",
                        adHocScenarioFile);
                }

                return DeserialiseScenarios(File.ReadAllText(adHocScenarioFile));
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .First(n => n.EndsWith("e2e-scenarios.json"));

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            return DeserialiseScenarios(json);
        }

        private static List<E2EScenario> DeserialiseScenarios(string json)
        {
            JObject root = JObject.Parse(json);
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            };

            JToken scenarios = root["scenarios"]
                ?? throw new JsonException("Book E2E scenario JSON must contain a 'scenarios' array.");

            return scenarios.ToObject<List<E2EScenario>>(
                JsonSerializer.Create(settings));
        }
    }
}
