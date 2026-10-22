using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.Utility.TestRunner.Service.Application;

namespace VAA.EAPI.Automation.Utility.TestRunner.Service
{
    public class Program
    {
        public static readonly StringBuilder Output = new();

        public static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("VAA_HEIMDALL_RUNNER", "true");

            if (!Debugger.IsAttached)
                Console.SetOut(new StringWriter(Output));

            var path = ConfigReader.Get("PATH").TrimEnd(';');
            var dotNetPaths = string.Empty;

            if (!RegexCompiled.IsMatch(path, @"C:\\Program Files\(x86\)\\dotnet") && Directory.Exists(@"C:\Program Files(x86)\dotnet"))
                dotNetPaths += @";C:\Program Files(x86)\dotnet";

            if (!RegexCompiled.IsMatch(path, @"C:\\Program Files\\dotnet") && Directory.Exists(@"C:\Program Files\dotnet"))
                dotNetPaths += @";C:\Program Files\dotnet";

            if (!dotNetPaths.IsNullOrWhiteSpace())
                ConfigReader.Set("PATH", $"{path}{dotNetPaths};");

            var rootFolder = Path.GetDirectoryName(Environment.ProcessPath)!;
            var agentName = Path.GetFileName(rootFolder);
            var eventLogLoggerProvider = new EventLogLoggerProvider(new EventLogSettings() { SourceName = $"Heimdall agent '{agentName}'" });
            var logger = eventLogLoggerProvider.CreateLogger($"Heimdall agent '{agentName}'");
            var trace = logger.CreateTrace<Program>();

            if (args.Any(x => x.Is("--install") || x.Is("--uninstall")))
            {
                var install = args.Any(x => x.Is("--install"));
                var uninstall = args.Any(x => x.Is("--uninstall"));
                var arguments = string.Empty;

                if (install && !uninstall)
                    arguments = $"create \"Heimdall agent '{agentName}'\" binpath=\"{Environment.ProcessPath}\" start= auto displayName= \"Heimdall agent '{agentName}'\"";
                else if (!install && uninstall)
                    arguments = $"delete \"Heimdall agent '{agentName}'\"";

                if (!arguments.IsNullOrWhiteSpace())
                {
                    var process = new Process()
                    {
                        EnableRaisingEvents = true,
                        StartInfo = new()
                        {
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            FileName = "sc",
                            Arguments = arguments
                        }
                    };

                    Console.WriteLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data is not null)
                            Console.WriteLine(e.Data);
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data is not null)
                            Console.WriteLine(e.Data);
                    };

                    process.Start();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                        Environment.Exit(process.ExitCode);
                }

                return;
            }

            if (!Directory.GetCurrentDirectory().Is(rootFolder))
            {
                try
                {
                    trace.Diagnostics.LogWarning($"Setting current directory from '{Directory.GetCurrentDirectory()}' to '{rootFolder}'.");

                    Directory.SetCurrentDirectory(rootFolder);
                }
                catch (Exception ex)
                {
                    try
                    {
                        trace.Diagnostics.LogError(ex, ex.Message);
                    }
                    catch
                    {
                    }
                }
            }

            trace.Diagnostics.LogWarning($"Agent: {agentName}, Current directory: {Directory.GetCurrentDirectory()}");

            ConfigReader.LoadEnvironmentSettingsFile("appSettings.json");

            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddWindowsService(options => options.ServiceName = $"Heimdall agent '{agentName}'");

            var applicationInsightsServiceOptions = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                EnableQuickPulseMetricStream = false
            };

            builder.Services.AddApplicationInsightsTelemetryWorkerService(applicationInsightsServiceOptions);

            builder.Services.AddSingleton<IAssemblyRunnerLoader, AssemblyRunnerLoader>();

#pragma warning disable CA1416 // Validate platform compatibility
            LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
#pragma warning restore CA1416 // Validate platform compatibility

            builder.Services.AddHostedService<WindowsBackgroundService>();

            var host = builder.Build();

            host.Run();
        }
    }
}
