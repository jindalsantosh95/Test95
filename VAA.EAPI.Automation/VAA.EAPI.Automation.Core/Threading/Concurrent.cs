using System.Diagnostics;

namespace VAA.EAPI.Automation.Core.Threading
{
    public static class Concurrent
    {
        public static async Task WhenAll(params ThreadStart[] threadStarts) => await WhenAll(threadStarts.Select(t => new Thread(t) { IsBackground = true }).ToArray());
        public static async Task WhenAll(params Thread[] threads)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            System.Threading.Tasks.Task waitTask = new Task(() =>
            {
                // run threads concurrently
                foreach (System.Threading.Thread t in threads)
                    t.Start();

                while (!threads.All(t => t.ThreadState.HasFlag(System.Threading.ThreadState.Stopped)))
                    if (!Thread.Yield())
                        Thread.Sleep(100);
            });

            waitTask.Start();

            await waitTask;

            stopwatch.Stop();

            Debug.WriteLine($"WhenAll executed {threads.Length} in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
