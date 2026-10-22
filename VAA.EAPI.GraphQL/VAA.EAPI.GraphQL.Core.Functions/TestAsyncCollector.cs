using Microsoft.Azure.WebJobs;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestAsyncCollector<T> : IAsyncCollector<T>
    {
        public List<T> Items { get; } = [];

        public Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            Items.Add(item);

            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
