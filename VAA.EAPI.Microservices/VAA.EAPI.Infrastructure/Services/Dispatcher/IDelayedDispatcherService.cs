using System;

namespace VAA.EAPI.Infrastructure.Services.Dispatcher
{
    public delegate string GenerateIdDelegate<T>(T item);

    public interface IDelayedDispatcherService<T> : IDisposable
    {
        GenerateIdDelegate<T> GenerateId { get; set; }
        int Count { get; }

        bool Schedule(T item, TimeSpan delay);
    }
}
