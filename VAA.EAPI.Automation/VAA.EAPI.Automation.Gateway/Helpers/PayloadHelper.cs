using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Gateway.Helpers
{
    public static class PayloadHelper
    {
        public static JObject GetVariables(string graphQlPayloadPath)
        {
            string resolvedPath = Path.IsPathRooted(graphQlPayloadPath)
                ? graphQlPayloadPath
                : Path.Combine(AppContext.BaseDirectory, graphQlPayloadPath);

            if (!File.Exists(resolvedPath) && !Path.IsPathRooted(graphQlPayloadPath))
            {
                string? fallbackPath = TryResolveFromTripsProject(graphQlPayloadPath);
                if (!string.IsNullOrWhiteSpace(fallbackPath))
                {
                    resolvedPath = fallbackPath;
                }
            }

            if (!File.Exists(resolvedPath))
            {
                throw new FileNotFoundException(
                    $"GraphQL payload file not found. Looked for '{resolvedPath}'. Ensure the payload JSON is copied to test output (CopyToOutputDirectory) or available under the Trips project.",
                    resolvedPath);
            }

            JObject payload = JObject.Parse(File.ReadAllText(resolvedPath));
            if (payload["variables"] is not JObject variables)
            {
                throw new InvalidOperationException(
                    $"GraphQL payload at '{resolvedPath}' is missing a 'variables' object, or 'variables' is not a JSON object.");
            }
            return variables;
        }

        private static string? TryResolveFromTripsProject(string graphQlPayloadPath)
        {
            string normalizedRelativePath = graphQlPayloadPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string? candidate = TryResolveFromBase(AppContext.BaseDirectory, normalizedRelativePath);
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }

            return TryResolveFromBase(Directory.GetCurrentDirectory(), normalizedRelativePath);
        }

        private static string? TryResolveFromBase(string basePath, string graphQlPayloadPath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                return null;
            }

            DirectoryInfo? current = new DirectoryInfo(basePath);
            for (int i = 0; i < 15 && current != null; i++)
            {
                // Preferred: sibling folder next to the current directory (repo layout).
                string candidate = Path.Combine(current.FullName, "..", "VAA.EAPI.Automation.Trips", graphQlPayloadPath);
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                // Secondary: Trips folder as a child of current (rare, but safe to check).
                candidate = Path.Combine(current.FullName, "VAA.EAPI.Automation.Trips", graphQlPayloadPath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
