using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.UI.Xunit.Features.Atttributes;
using Xunit.Sdk;

namespace VAA.EAPI.Automation.Core.UI.Xunit.Features
{
    public static class ScenarioRunner
    {
        public static async Task Run(Scenario scenario, params object[] dependencies)
        {
            var bindings = scenario.Feature.Class
                                           .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                           .Select(x =>
                                           {
                                               var attribute = x.GetCustomAttributes(false)
                                                                .FirstOrDefault(a => a is GivenAttribute or WhenAttribute or AndAttribute or ThenAttribute or ButAttribute);

                                               if (attribute is not DescriptionAttribute desc) return null;

                                               return new StepBinding(desc.Description, x);
                                           })
                                           .Where(x => x != null)
                                           .ToList();

            var instance = Activator.CreateInstance(scenario.Feature.Class, dependencies);

            try
            {
                foreach (var rawStep in scenario.Steps)
                {
                    var stepKey = StepText.Normalize(rawStep);
                    var (methodInfo, args) = ResolveStep(bindings, stepKey, rawStep);

                    Task action;

                    if (methodInfo.ReturnType == typeof(Task))
                        action = (Task)methodInfo.Invoke(instance, args)!;
                    else
                        action = Task.Run(() => methodInfo.Invoke(instance, args));

                    await action;
                }
            }
            catch (Exception ex)
            {
                if (instance is IScenarioFailureHandler failureHandler)
                    failureHandler.OnFailure(ex);

                throw;
            }
            finally
            {
                if (instance is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        private static (MethodInfo method, object[] args) ResolveStep(List<StepBinding> bindings, string stepKey, string rawStep)
        {
            // Try exact match first (no parameters)
            var exact = bindings.FirstOrDefault(b => b.Pattern.Equals(stepKey, StringComparison.OrdinalIgnoreCase));
            if (exact != null)
                return (exact.Method, null);

            // Try regex match for parameterised steps
            foreach (var binding in bindings)
            {
                var match = Regex.Match(stepKey, $"^{binding.Pattern}$", RegexOptions.IgnoreCase);
                if (!match.Success) continue;

                var parameters = binding.Method.GetParameters();
                var groups = match.Groups;

                if (groups.Count - 1 != parameters.Length) continue;

                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = Convert.ChangeType(groups[i + 1].Value, parameters[i].ParameterType);
                }

                return (binding.Method, args);
            }

            throw new XunitException($"No binding for step: {rawStep}");
        }

        private sealed record StepBinding(string Pattern, MethodInfo Method);
    }
}