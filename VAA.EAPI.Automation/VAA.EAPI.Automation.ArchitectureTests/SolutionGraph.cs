using System.Xml.Linq;

namespace VAA.EAPI.Automation.ArchitectureTests
{
    /// <summary>
    /// The project-reference graph of VAA.EAPI.Automation.sln. The project set is taken
    /// from the .sln itself - the authoritative list, matching what `dotnet build`
    /// validates - and each project's references are read from its .csproj.
    /// Architecture fitness rules are asserted against this graph.
    /// See docs/HARNESS_ENGINEERING.md (Thread 3).
    /// </summary>
    internal sealed class SolutionGraph
    {
        private const string SolutionFileName = "VAA.EAPI.Automation.sln";
        private const string ThisProject = "VAA.EAPI.Automation.ArchitectureTests";

        private readonly Dictionary<string, IReadOnlyList<string>> _references;

        private SolutionGraph()
        {
            SolutionDirectory = FindSolutionDirectory();
            _references = LoadReferences(SolutionDirectory);
        }

        /// <summary>Absolute path to the directory containing VAA.EAPI.Automation.sln.</summary>
        public string SolutionDirectory { get; }

        /// <summary>Every project in the solution, excluding the architecture-test project itself.</summary>
        public IEnumerable<string> Projects => _references.Keys;

        /// <summary>Builds the graph from the solution file and its .csproj files.</summary>
        public static SolutionGraph Load() => new();

        /// <summary>Classifies a project into an architectural layer by its name.</summary>
        public static ProjectLayer LayerOf(string project)
        {
            // Order matters: Core.UI ends in ".UI" but is a Core library, not a UI suite.
            if (project.Equals("VAA.EAPI.Automation.Core", StringComparison.Ordinal) ||
                project.StartsWith("VAA.EAPI.Automation.Core.", StringComparison.Ordinal))
            {
                return ProjectLayer.Core;
            }

            if (project.StartsWith("VAA.EAPI.Automation.Utility.", StringComparison.Ordinal))
            {
                return ProjectLayer.Utility;
            }

            if (project.EndsWith(".UI", StringComparison.Ordinal))
            {
                return ProjectLayer.Ui;
            }

            return ProjectLayer.Domain;
        }

        /// <summary>The projects directly referenced by <paramref name="project"/>.</summary>
        public IReadOnlyList<string> ReferencesOf(string project) =>
            _references.TryGetValue(project, out var references) ? references : Array.Empty<string>();

        private static string FindSolutionDirectory()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException(
                $"Could not locate {SolutionFileName} above '{AppContext.BaseDirectory}'.");
        }

        private static Dictionary<string, IReadOnlyList<string>> LoadReferences(string solutionDirectory)
        {
            var graph = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            var pathSeparators = new[] { '\\', '/' };

            foreach (var csproj in EnumerateSolutionProjects(solutionDirectory))
            {
                var name = Path.GetFileNameWithoutExtension(csproj);

                // The architecture-test project observes the graph; it is not part of
                // it. A .sln entry whose file is missing on disk is skipped, not thrown.
                if (name.Equals(ThisProject, StringComparison.Ordinal) || !File.Exists(csproj))
                {
                    continue;
                }

                var references = XDocument.Load(csproj)
                    .Descendants("ProjectReference")
                    .Select(element => (string?)element.Attribute("Include"))
                    .Where(include => !string.IsNullOrWhiteSpace(include))
                    .Select(include => include!.Split(pathSeparators).Last())
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();

                graph[name] = references;
            }

            return graph;
        }

        /// <summary>
        /// The .csproj files listed in VAA.EAPI.Automation.sln - the authoritative project
        /// set. Enumerating *.csproj on disk would also pick up projects that are not in
        /// the solution (and so are not built by `dotnet build`).
        /// </summary>
        private static IEnumerable<string> EnumerateSolutionProjects(string solutionDirectory)
        {
            var solutionPath = Path.Combine(solutionDirectory, SolutionFileName);

            // Each project line is: Project("{type}") = "Name", "relative\path", "{guid}".
            // The project path is the quoted token ending in ".csproj"; solution folders
            // and non-C# projects (e.g. .deployproj) have no such token and are skipped.
            foreach (var line in File.ReadLines(solutionPath))
            {
                if (!line.TrimStart().StartsWith("Project(", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var token in line.Split('"'))
                {
                    if (token.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return Path.GetFullPath(Path.Combine(
                            solutionDirectory,
                            token.Replace('\\', Path.DirectorySeparatorChar)));
                        break;
                    }
                }
            }
        }
    }
}
