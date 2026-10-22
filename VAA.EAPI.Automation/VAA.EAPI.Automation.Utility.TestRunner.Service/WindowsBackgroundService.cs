using System.Diagnostics;
using System.Text;
using VAA.EAPI.Automation.Core.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Utility.TestRunner.Service.Application;

namespace VAA.EAPI.Automation.Utility.TestRunner.Service
{
    public class WindowsBackgroundService : BackgroundService
    {
        private readonly IAssemblyRunnerLoader assemblyRunnerLoader;
        private readonly Trace<WindowsBackgroundService> trace;
        private Timer? timer = null;
        private bool isRunning = false;

        public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, IAssemblyRunnerLoader assemblyRunnerLoader)
        {
            ArgumentNullException.ThrowIfNull(logger);

            this.assemblyRunnerLoader = assemblyRunnerLoader ?? throw new ArgumentNullException(nameof(assemblyRunnerLoader));

            trace = logger.CreateTrace();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var cronExpression = NCrontab.Advanced.CrontabSchedule.Parse(Config.TestRun.TimerInterval, NCrontab.Advanced.Enumerations.CronStringFormat.WithSeconds);
                var firstOccurrence = cronExpression.GetNextOccurrence(DateTime.Now);
                var secondOccurrence = cronExpression.GetNextOccurrence(firstOccurrence);
                var first = firstOccurrence - DateTime.Now;
                var span = secondOccurrence - firstOccurrence;

                timer = new Timer((state) =>
                {
                    if (isRunning) return;

                    isRunning = true;

                    try
                    {
                        assemblyRunnerLoader.DisconnectAll();

                        TerminateProcesses(GetChildProcessIds());
                        TerminateProcesses(GetChromePids(span));

                        var actions = assemblyRunnerLoader.Runners.Select(x => new Action(() =>
                        {
                            x.Start();

                            while (x.Status != Xunit.Runners.AssemblyRunnerStatus.Idle)
                                if (!Thread.Yield())
                                    Thread.Sleep(500);
                        }))
                        .ToArray();

                        trace.Diagnostics.LogWarning($"Preparing execute tests {(Config.TestRun.InParallel ? "in parallel" : "sequentially")}...");

                        if (Config.TestRun.InParallel)
                        {
                            var threadStarts = actions.Select(x => new ThreadStart(x))
                                                      .ToArray();

                            Core.Threading.Concurrent.WhenAll(threadStarts).GetAwaiter().GetResult();
                        }
                        else
                            foreach (var action in actions)
                                action();
                    }
                    catch (Exception ex)
                    {
                        trace.Diagnostics.LogError(ex);
                    }
                    finally
                    {



                        isRunning = false;
                    }

                }, null, (int)first.TotalMilliseconds, (int)span.TotalMilliseconds);

                while (timer is not null)
                    await Task.Delay((int)span.TotalMilliseconds, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                trace.Logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
            finally
            {
                CleanUpProcesses();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            CleanUpProcesses();

            return base.StopAsync(cancellationToken);
        }

        private void CleanUpProcesses()
        {
            timer?.Dispose();
            timer = null;
        }

        public static int[] GetChildProcessIds()
        {
            var processId = Environment.ProcessId;

            var stringBuilder = new StringBuilder();

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "wmic",
                    Arguments = $"process where (ParentProcessId={processId}) get ProcessId",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
            };

            process.OutputDataReceived += (sender, args) => stringBuilder.AppendLine(args.Data ?? string.Empty);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return stringBuilder.ToString()
                                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                                .Skip(1)
                                .Select(x => x.TryParse(out int pid) ? pid : -1)
                                .Where(x => x > 0)
                                .ToArray();
        }
        public static int[] GetChromePids(TimeSpan age)
        {
            var stringBuilder = new StringBuilder();

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "wmic",
                    Arguments = $"process where (Name=\"chrome.exe\") get ProcessId",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
            };

            process.OutputDataReceived += (sender, args) => stringBuilder.AppendLine(args.Data ?? string.Empty);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return stringBuilder.ToString()
                                .Split(Environment.NewLine)
                                .Skip(1)
                                .Select(x => !x.IsNullOrWhiteSpace() && x.TryParse(out int pid) ? pid : -1)
                                .Where(x =>
                                {
                                    bool hasExpired = false;

                                    try
                                    {
                                        hasExpired = x > 0 && Process.GetProcessById(x).StartTime.Add(age) < DateTime.Now;
                                    }
                                    catch
                                    {
                                        // we might not have access to the process at this time so
                                        // we don't want to fail, we will just carry on to the next
                                    }

                                    return hasExpired;
                                })
                                .ToArray();
        }

        public static void TerminateProcesses(int[] pids)
        {
            if (pids.Length == 0) return;

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "taskkill",
                    Arguments = $"/f /t {string.Join(' ', pids.Select(x => $"/pid {x}"))}",
                    RedirectStandardOutput = true
                },
            };

            if (process.Start())
                process.WaitForExit();
        }
    }
}
