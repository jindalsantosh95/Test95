using Microsoft.Extensions.DependencyModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using VAA.EAPI.Automation.Core.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Runners;
using Xunit;
using Xunit.Runners;


namespace VAA.EAPI.Automation.Utility.TestRunner.Service.Application
{
    public sealed class AssemblyRunnerLoader : IAssemblyRunnerLoader
    {
        private readonly ILogger<AssemblyRunnerLoader> logger;
        private readonly Trace<AssemblyRunnerLoader> trace;

        public AssemblyRunner[] Runners { get; }

        public void DisconnectAll() => Xunit.Runners.AssemblyRunner.DisconnectAll();

        public AssemblyRunnerLoader(ILogger<AssemblyRunnerLoader> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            trace = logger.CreateTrace();

            var edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"\Microsoft\Edge\Application\msedge.exe");
            var chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe");

            if (!File.Exists(edgePath))
                edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"\Microsoft\Edge\Application\msedge.exe");

            if (!File.Exists(chromePath))
                chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"\Google\Chrome\Application\chrome.exe");

            var isEdgeInstalled = File.Exists(edgePath);
            var isChromeInstalled = File.Exists(chromePath);
            var edgeVersion = isEdgeInstalled ? FileVersionInfo.GetVersionInfo(edgePath.ToString()).FileVersion : "0.0.0.0";
            var chromeVersion = isChromeInstalled ? FileVersionInfo.GetVersionInfo(chromePath.ToString()).FileVersion : "0.0.0.0";
            var processPath = Path.GetDirectoryName(Environment.ProcessPath)!;

            trace.Diagnostics.LogWarning($"Browser check...{Environment.NewLine}isEdgeInstalled: {isEdgeInstalled}-{edgeVersion}{Environment.NewLine}isChromeInstalled: {isChromeInstalled}-{chromeVersion}");
            trace.Diagnostics.LogWarning($"Loading assemblies, {Config.TestRun.Assemblies}({Config.TestRun.Filter}), located in {new DirectoryInfo(processPath).FullName}");
            Stopwatch loadTimer = Stopwatch.StartNew();

            Runners = Directory.GetFiles(processPath, "VAA.EAPI.Automation.*.dll", SearchOption.TopDirectoryOnly)
                               .Where(x => !Config.TestRun.Assemblies.IsNullOrWhiteSpace() && RegexCompiled.IsMatch(x, Config.TestRun.Assemblies))
                               .Where(x => Assembly.LoadFrom(x).GetTypes().Any(t => t.GetMethods().Any(m => m.GetCustomAttributes().Any(a => a is TheoryAttribute or FactAttribute))))
                               .Select(x =>
                                {
                                    Assembly[] assemblies = GetReferencingAssemblies(Path.GetFileNameWithoutExtension(x));
                                    List<string> tags = new();

                                    var assemblyRunner = Xunit.Runners.AssemblyRunner.WithoutAppDomain(x);

                                    assemblyRunner.OnDiscoveryComplete = (e) =>
                                    {
                                        trace.Diagnostics.LogWarning($"Discovered: {e.TestCasesDiscovered}, {e.TestCasesToRun} test cases to run");

                                        if (e.TestCasesToRun == 0)
                                        {
                                            trace.Diagnostics.LogError(new InvalidOperationException(
                                                $"Heimdall discovered no runnable tests in '{Path.GetFileName(x)}'. " +
                                                $"Check Configuration:TestRun:Assemblies ('{Config.TestRun.Assemblies}') and " +
                                                $"Configuration:TestRun:Filter ('{Config.TestRun.Filter}')."));
                                        }
                                    };

                                    assemblyRunner.OnTestStarting = (e) => { };
                                    assemblyRunner.OnTestPassed = (e) =>
                                    {
                                        trace.Diagnostics.LogWarning($"Test [PASSED] : {e.TestDisplayName.IfIsNullOrWhiteSpace($"{e.TypeName}.{e.MethodName}")}, Tag: {e.GetTraitValue("tag", assemblies)}");

                                        tags.Add(e.GetTraitValue("tag", assemblies));
                                    };
                                    assemblyRunner.OnTestFailed = (e) =>
                                    {
                                        trace.Diagnostics.LogWarning($"Test [FAILED] : {e.TestDisplayName.IfIsNullOrWhiteSpace(e.MethodName)}");

                                        var test = e.TestDisplayName.IfIsNullOrWhiteSpace($"{e.TypeName}.{e.MethodName}");

                                        trace.Diagnostics.LogError((FailedTestException)(
                                            test,
                                            DiagnosticSanitizer.Sanitize(e.ExceptionMessage),
                                            DiagnosticSanitizer.Sanitize(e.ExceptionStackTrace),
                                            e.ExceptionType,
                                            e.ExecutionTime,
                                            DiagnosticSanitizer.Sanitize(
                                                $"{e.Output}, Tag: {e.GetTraitValue("tag", assemblies)}")));

                                        tags.Add(e.GetTraitValue("tag", assemblies));
                                    };
                                    assemblyRunner.OnTestSkipped = (e) => trace.Diagnostics.LogWarning($"Test [SKIPPED] {e.TestDisplayName.IfIsNullOrWhiteSpace($"{e.TypeName}.{e.MethodName}")}: {e.SkipReason.MakeInterpolationSafe()}");
                                    assemblyRunner.OnTestOutput = (e) => { };
                                    assemblyRunner.OnTestFinished = (e) => { };

                                    assemblyRunner.OnErrorMessage = (e) =>
                                        trace.Diagnostics.LogError(new InvalidOperationException(
                                            $"xUnit runner error in '{Path.GetFileName(x)}': {e.ExceptionType}: " +
                                            DiagnosticSanitizer.Sanitize(
                                                $"{e.ExceptionMessage}{Environment.NewLine}{e.ExceptionStackTrace}")));
                                    assemblyRunner.OnDiagnosticMessage = (e) => { };

                                    assemblyRunner.OnExecutionComplete = (e) => trace.Diagnostics.LogWarning($"Finished: {e.TotalTests} tests in {e.ExecutionTime}s ({e.TotalTests - (e.TestsFailed + e.TestsSkipped)} passed, {e.TestsFailed} failed, {e.TestsSkipped} skipped), Tags: {string.Join(',', tags.Distinct())}");

                                    assemblyRunner.TestCaseFilter = (testCase) =>
                                    {
                                        if (Config.TestRun.Filter.IsNullOrWhiteSpace()) return true;

                                        var expression = (FilterExpression)Config.TestRun.Filter;

                                        return expression.IsValid(testCase);
                                    };

                                    return assemblyRunner;
                               })
                               .ToArray();

            if (Runners.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Heimdall loaded no test assemblies from '{processPath}'. " +
                    $"Check Configuration:TestRun:Assemblies ('{Config.TestRun.Assemblies}') and the deployed publish output.");
            }

            trace.Diagnostics.LogWarning($"Loading of {Runners.Length} assemblies executed in {loadTimer.ElapsedMilliseconds}ms");
        }

        public void Dispose()
        {
            if (!Runners.IsNullOrEmpty())
                foreach (var runner in Runners)
                    runner.Dispose();
        }

        private static Assembly[] GetReferencingAssemblies(string assemblyName)
            => DependencyContext.Default.RuntimeLibraries.Where(x => IsCandidateLibrary(x, assemblyName))
                                                         .Select(x => Assembly.Load(new AssemblyName(x.Name)))
                                                         .ToArray();
        private static bool IsCandidateLibrary(RuntimeLibrary library, string assemblyName)
            => library.Name == assemblyName || library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
    }
}
