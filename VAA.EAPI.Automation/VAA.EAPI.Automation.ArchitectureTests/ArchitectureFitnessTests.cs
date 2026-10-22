using Xunit;

namespace VAA.EAPI.Automation.ArchitectureTests
{
    /// <summary>
    /// Architecture fitness functions for VAA.EAPI.Automation.sln. Each test encodes a
    /// project-layering invariant from docs/FRAMEWORK_ARCHITECTURE.md that holds today;
    /// a future change that breaks it fails the test run (and CI / scripts/verify.ps1).
    /// See docs/HARNESS_ENGINEERING.md (Thread 3).
    /// </summary>
    public sealed class ArchitectureFitnessTests
    {
        private static readonly IReadOnlyList<string> ForbiddenUiDriverTokens = new[]
        {
            "using Microsoft.Playwright",
            "Microsoft.Playwright.",
            ".Page.",
            ".Locator(",
            ".GetByRole(",
            ".GetByText(",
            ".GetByLabel(",
            ".GetByTestId(",
            ".GetByPlaceholder(",
            ".GetByAltText(",
            ".GetByTitle(",
            ".FrameLocator(",
            ".GotoAsync(",
            ".GoBackAsync(",
            ".GoForwardAsync(",
            ".ReloadAsync(",
            ".WaitForURLAsync(",
            ".WaitForLoadStateAsync(",
            ".WaitForSelectorAsync(",
            ".ClickAsync(",
            ".FillAsync(",
            ".PressAsync(",
            ".SelectOptionAsync(",
            ".CheckAsync(",
            ".UncheckAsync(",
            ".HoverAsync(",
            ".ScreenshotAsync(",
            ".EvaluateAsync",
            ".PauseAsync("
        };

        private readonly SolutionGraph _graph = SolutionGraph.Load();

        [Fact]
        public void CoreProjects_DependOnlyOnOtherCoreProjects()
        {
            var violations = (
                from project in _graph.Projects
                where SolutionGraph.LayerOf(project) == ProjectLayer.Core
                from reference in _graph.ReferencesOf(project)
                where SolutionGraph.LayerOf(reference) != ProjectLayer.Core
                select $"{project} -> {reference}").ToList();

            Assert.True(
                violations.Count == 0,
                "Core projects are the foundation layer and must reference only other " +
                "Core projects:" + Bullets(violations));
        }

        [Fact]
        public void UiSuites_AreReferencedOnlyByUiOrUtilityProjects()
        {
            var violations = (
                from project in _graph.Projects
                from reference in _graph.ReferencesOf(project)
                where SolutionGraph.LayerOf(reference) == ProjectLayer.Ui
                let layer = SolutionGraph.LayerOf(project)
                where layer != ProjectLayer.Ui && layer != ProjectLayer.Utility
                select $"{project} ({layer}) -> {reference}").ToList();

            Assert.True(
                violations.Count == 0,
                "UI suites sit above the Core and API layers; only UI or Utility " +
                "projects may reference them:" + Bullets(violations));
        }

        [Fact]
        public void UtilityProjects_AreReferencedByNoProject()
        {
            var violations = (
                from project in _graph.Projects
                from reference in _graph.ReferencesOf(project)
                where SolutionGraph.LayerOf(reference) == ProjectLayer.Utility
                select $"{project} -> {reference}").ToList();

            Assert.True(
                violations.Count == 0,
                "Utility projects sit at the top of the graph and must not be " +
                "referenced by any project:" + Bullets(violations));
        }

        [Fact]
        public void ProjectDependencyGraph_IsAcyclic()
        {
            var cycle = FindCycle();

            Assert.True(
                cycle is null,
                cycle is null
                    ? string.Empty
                    : "The project dependency graph must be acyclic. Cycle detected:\n  " +
                      string.Join(" -> ", cycle));
        }

        [Fact]
        public void UiTestsAndFlows_DoNotUseBrowserDriverPrimitivesDirectly()
        {
            List<string> violations = Directory
                .EnumerateDirectories(_graph.SolutionDirectory, "*.UI", SearchOption.TopDirectoryOnly)
                .SelectMany(path => new[]
                {
                    Path.Combine(path, "Tests"),
                    Path.Combine(path, "Flows")
                })
                .Where(Directory.Exists)
                .SelectMany(path => Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
                .Where(path => !path.Contains(
                    $"{Path.DirectorySeparatorChar}Tests{Path.DirectorySeparatorChar}Setup{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal))
                .Select(path => (Path: path, Content: File.ReadAllText(path)))
                .SelectMany(source => FindForbiddenUiDriverTokens(source.Content)
                    .Select(token => $"{Path.GetRelativePath(_graph.SolutionDirectory, source.Path)} contains '{token}'"))
                .ToList();

            Assert.True(
                violations.Count == 0,
                "UI tests must express intent through flows and page objects; browser-driver " +
                "primitives belong in Pages or browser infrastructure:" + Bullets(violations));
        }

        [Theory]
        [InlineData("await session.Page.GetByText(\"Continue\").ClickAsync();")]
        [InlineData("await session.Page.GotoAsync(baseUrl);")]
        [InlineData("await page.GetByLabel(\"Surname\").FillAsync(\"Test\");")]
        [InlineData("await page.WaitForURLAsync(\"**/summary\");")]
        [InlineData("await page.ScreenshotAsync();")]
        public void UiDriverPrimitiveSensor_DetectsCommonPlaywrightCalls(string source)
        {
            Assert.NotEmpty(FindForbiddenUiDriverTokens(source));
        }

        private static IEnumerable<string> FindForbiddenUiDriverTokens(string source)
        {
            return ForbiddenUiDriverTokens.Where(token => source.Contains(token, StringComparison.Ordinal));
        }

        private IReadOnlyList<string>? FindCycle()
        {
            var completed = new HashSet<string>(StringComparer.Ordinal);

            foreach (var project in _graph.Projects)
            {
                var cycle = Walk(project, completed, new List<string>());
                if (cycle is not null)
                {
                    return cycle;
                }
            }

            return null;
        }

        private IReadOnlyList<string>? Walk(string project, HashSet<string> completed, List<string> path)
        {
            var loopStart = path.IndexOf(project);
            if (loopStart >= 0)
            {
                return path.Skip(loopStart).Append(project).ToList();
            }

            if (!completed.Add(project))
            {
                return null;
            }

            path.Add(project);
            foreach (var reference in _graph.ReferencesOf(project))
            {
                var cycle = Walk(reference, completed, path);
                if (cycle is not null)
                {
                    return cycle;
                }
            }

            path.RemoveAt(path.Count - 1);
            return null;
        }

        private static string Bullets(IReadOnlyList<string> lines) =>
            lines.Count == 0 ? string.Empty : "\n  " + string.Join("\n  ", lines);
    }
}
