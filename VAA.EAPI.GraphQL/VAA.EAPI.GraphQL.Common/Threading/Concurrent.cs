using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Common.Threading
{
    public static class Concurrent
    {
        public static async Task WhenAll(params ThreadStart[] threadStarts) => await WhenAll(threadStarts.Select(t => new Thread(t) { IsBackground = true }).ToArray());
        public static async Task WhenAll(params Thread[] threads)
        {
            var trace = Diagnostics.Trace<Thread>.Create();

            var waitTask = new Task(() =>
            {
                // run threads concurrently
                foreach (var t in threads)
                    t.Start();

                while (!threads.All(t => t.ThreadState.HasFlag(System.Threading.ThreadState.Stopped)))
                    if (!Thread.Yield())
                        Thread.Sleep(100);
            });

            waitTask.Start();

            await waitTask;

            trace.Diagnostics.LogInformation($"{{0}}> WhenAll executed {threads.Length} in {{2}}ms");
        }
    }
}
