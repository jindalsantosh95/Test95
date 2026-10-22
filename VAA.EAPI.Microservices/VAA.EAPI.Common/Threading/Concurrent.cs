using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Common.Threading
{
    public static class Concurrent
    {
        public static async Task WhenAll(IEnumerable<ThreadStart> threadStarts) => await WhenAll(threadStarts.ToArray());
        public static async Task WhenAll(params ThreadStart[] threadStarts) => await WhenAll(threadStarts.Select(t => new Thread(t) { IsBackground = true }));

        public static async Task WhenAll(IEnumerable<Thread> threads) => await WhenAll(threads.ToArray());
        public static async Task WhenAll(params Thread[] threads)
        {
            var trace = Diagnostics.Trace<Thread>.Create();

            var waitTask = new Task(() =>
            {
                // run threads concurrently
                foreach (var t in threads)
                    t.Start();

                while (!threads.All(t => t.ThreadState.HasFlag(ThreadState.Stopped)))
                    if (!Thread.Yield())
                        Thread.Sleep(100);
            });

            waitTask.Start();

            await waitTask;

            trace.Diagnostics.LogInformation($"{{0}}> WhenAll executed {threads.Length} in {{2}}ms");
        }
    }
}
