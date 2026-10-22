using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;

namespace VAA.EAPI.Infrastructure.Services.Dispatcher
{
    public sealed class DelayedDispatcherService<T> : IDelayedDispatcherService<T> where T : class, new()
    {
        public enum InsertMode
        {
            FirstWriteWins = 0,
            LastWriteWins
        }

        internal sealed class ScheduledItem : IDisposable
        {
            public static explicit operator ScheduledItem((T item, TimeSpan delay, GenerateIdDelegate<T> generateIdDelegate, ILogger logger) source) => new(source.item, source.delay, source.generateIdDelegate, source.logger);

            private readonly int _retryWindowInMs = ConfigReader.Try($"{AppDomain.CurrentDomain.FriendlyName}.DelayedDispatcherService:RetryWindowInMs", 5000);
            private readonly int _maxRetryCount = ConfigReader.Try($"{AppDomain.CurrentDomain.FriendlyName}.DelayedDispatcherService:MaxRetryCount", 5);
            private readonly Timer _timer;
            private ILogger _logger;
            private bool _disposed;
            private int _state = 0;

            public event Func<object, T, Task> InvokedAsync;

            public string Id { get; set; }
            public T Item { get; set; }
            public TimeSpan Delay { get; set; }
            public DateTime Stamp { get; set; }
            public int Retry { get; set; } = 0;
            public TimeSpan Tick => Stamp.Add(Delay) - DateTime.UtcNow;
            public bool Valid => Tick > TimeSpan.Zero;
            public bool Processing => Volatile.Read(ref _state) == 1;

            public bool TryStartProcessing() => Interlocked.CompareExchange(ref _state, 1, 0) == 0;

            public void StopProcessing() => Interlocked.CompareExchange(ref _state, 0, 1);

            public bool TryMarkRemoved() => Interlocked.CompareExchange(ref _state, 2, 0) == 0;

            private void Invoke(T state) => InvokeAsync(state).GetAwaiter().GetResult();

            private async Task InvokeAsync(T state)
            {
                using var trace = _logger.CreateTrace<ScheduledItem>();

                try
                {
                    var handlers = InvokedAsync;

                    if (handlers is null) return;

                    foreach (var handler in handlers.GetInvocationList())
                        await ((Func<object, T, Task>)handler).Invoke(this, state).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Item<{typeof(T).Name}>: {Id} callback invocation failed before reschedule logic was reached.");
                }
            }

            public ScheduledItem()
            {
                _timer = new(state => Invoke((T)state), Item, Timeout.Infinite, Timeout.Infinite);
            }

            private ScheduledItem(T item, TimeSpan delay, GenerateIdDelegate<T> generateIdDelegate, ILogger logger)
            {
                Id = generateIdDelegate(item) ?? throw new ArgumentNullException(nameof(generateIdDelegate));
                Item = item ?? throw new ArgumentNullException(nameof(item));
                Delay = delay;
                Stamp = DateTime.UtcNow;

                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _timer = new(state => Invoke((T)state), Item, (int)Tick.TotalMilliseconds, Timeout.Infinite);
            }

            public void Reschedule(ILogger logger)
            {
                if (Retry >= _maxRetryCount) return;

                _logger ??= logger ?? throw new ArgumentNullException(nameof(logger));

                using var trace = _logger.CreateTrace<ScheduledItem>();

                Retry++;

                Stamp = DateTime.UtcNow;

                _timer.Change(_retryWindowInMs, Timeout.Infinite);

                trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {Id} rescheduled, Retry: {Retry}, Delay: {_retryWindowInMs}ms.");
            }

            internal void Initialise(ILogger logger)
            {
                if (Valid)
                {
                    _logger ??= logger ?? throw new ArgumentNullException(nameof(logger));

                    using var trace = _logger.CreateTrace<ScheduledItem>();

                    _timer.Change((int)Tick.TotalMilliseconds, Timeout.Infinite);

                    trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {Id} initialised, Delay: {Tick.TotalMilliseconds}ms.");
                }
            }

            public override string ToString() => $"Id: {Id}, Delay: {Delay}, Stamp: {Stamp}, Tick: {Tick}, Valid: {Valid}, Item<{Item.GetType().Name}>: {Json.Serialize(Item)}";

            public void Dispose()
            {
                if (_disposed) return;

                InvokedAsync = null;

                Interlocked.Exchange(ref _state, 2);

                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                _timer.Dispose();

                _disposed = true;
            }
        }

        internal sealed class ScheduledItems : ConcurrentDictionary<string, ScheduledItem>, IDisposable
        {
            public event Func<object, T, Task> ScheduledItemInvokedAsync;

            public DateTime LastModified { get; set; }
            public InsertMode Mode { get; set; } = InsertMode.FirstWriteWins;

            public void RebindEvents()
            {
                foreach (var item in this)
                {
                    item.Value.InvokedAsync -= ScheduledItemInvokedAsync;
                    item.Value.InvokedAsync += ScheduledItemInvokedAsync;
                }
            }

            new public bool TryAdd(string key, ScheduledItem value)
            {
                if (Mode == InsertMode.LastWriteWins)
                    if (TryGetValue(key, out var current))
                    {
                        if (!current.TryMarkRemoved()) return false;
                        if (!TryRemove(key, out _)) return false;
                    }

                if (!base.TryAdd(key, value)) return false;

                value.InvokedAsync += ScheduledItemInvokedAsync;

                LastModified = DateTime.UtcNow;

                return true;
            }

            new public bool TryRemove(string key, [MaybeNullWhen(false)] out ScheduledItem value)
            {
                if (!base.TryRemove(key, out value)) return false;

                value.InvokedAsync -= ScheduledItemInvokedAsync;

                value.Dispose();

                LastModified = DateTime.UtcNow;

                return true;
            }

            [Obsolete("Only TryAdd(string key, ...) & TryRemove(string key, ...) are supported")]
            new public bool TryRemove(KeyValuePair<string, ScheduledItem> item) => throw new NotImplementedException();

            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public bool TryUpdate(string key, ScheduledItem newValue, ScheduledItem comparisonValue) => throw new NotImplementedException();

            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public ScheduledItem GetOrAdd(string key, Func<string, ScheduledItem> valueFactory) => throw new NotImplementedException();
            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public ScheduledItem GetOrAdd<TArg>(string key, Func<string, TArg, ScheduledItem> valueFactory, TArg factoryArgument) => throw new NotImplementedException();
            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public ScheduledItem GetOrAdd(string key, ScheduledItem value) => throw new NotImplementedException();

            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public ScheduledItem AddOrUpdate<TArg>(string key, Func<string, TArg, ScheduledItem> addValueFactory, Func<string, ScheduledItem, TArg, ScheduledItem> updateValueFactory, TArg factoryArgument) => throw new NotImplementedException();
            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public ScheduledItem AddOrUpdate(string key, Func<string, ScheduledItem> addValueFactory, Func<string, ScheduledItem, ScheduledItem> updateValueFactory) => throw new NotImplementedException();
            [Obsolete("Only TryAdd & TryRemove are supported")]
            new public ScheduledItem AddOrUpdate(string key, ScheduledItem addValue, Func<string, ScheduledItem, ScheduledItem> updateValueFactory) => throw new NotImplementedException();

            public void Dispose()
            {
                foreach (var item in this)
                    item.Value?.Dispose();
            }
        }

        private readonly IMessageBus _messageBus;
        private readonly IDistributedCache _distributedCache;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly ILogger<DelayedDispatcherService<T>> _logger;

        private readonly string _serviceName = $"{AppDomain.CurrentDomain.FriendlyName}.DelayedDispatcherService[{typeof(T).FullName}]";
        private readonly ScheduledItems _items = null;
        private readonly Timer _timer;
        private readonly TimeSpan _defaultDelay = TimeSpan.FromSeconds(1);
        private volatile bool _hasLoaded = false;
        private int _isSaving = 0;

        public required string CloudEventSubject { get; set; }
        public required string CloudEventType { get; set; }
        public required GenerateIdDelegate<T> GenerateId { get; set; }
        public int Count => _items.Count;
        public bool UseDefaultDelay { get; set; } = false;
        private InsertMode _mode = InsertMode.FirstWriteWins;

        public InsertMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;

                if (_items is not null)
                    _items.Mode = value;
            }
        }

        private DelayedDispatcherService()
        {
            _timer = new(state =>
            {
                if (!_hasLoaded) return;
                if (Interlocked.Exchange(ref _isSaving, 1) == 1) return;

                try
                {
                    var keys = _items.Where(x => !x.Value.Valid)
                                     .Select(x => x.Key)
                                     .ToArray();

                    foreach (var key in keys)
                        _items.TryRemove(key, out _);

                    var items = JsonConvert.SerializeObject(_items).Compress();

                    if (items.IsNullOrEmpty()) return;

                    try
                    {
                        // lets try local persistence in case Redis fails us
                        System.IO.File.WriteAllBytes(_serviceName, items);
                    }
                    catch { }

                    _distributedCache?.Set(_serviceName, items, new DistributedCacheEntryOptions() { SlidingExpiration = ConfigReader.Try(_serviceName, TimeSpan.FromDays(2)) });
                }
                catch { }
                finally
                {
                    Volatile.Write(ref _isSaving, 0);
                }

            }, _items, 0, 10_000);
        }

        public DelayedDispatcherService(IMessageBus messageBus, IDistributedCache distributedCache, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, ILogger<DelayedDispatcherService<T>> logger) : this()
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            using var trace = _logger.CreateTrace();

            ScheduledItems fromDisk = null;
            ScheduledItems fromCache = null;

            string json = string.Empty;

            try
            {
                if (System.IO.File.Exists(_serviceName))
                    json = System.IO.File.ReadAllBytes(_serviceName)
                                         .DecompressToString();

                if (!json.IsNullOrWhiteSpace())
                    fromDisk = JsonConvert.DeserializeObject<ScheduledItems>(json);
            }
            catch { }

            try
            {
                json = _distributedCache?.Get(_serviceName)
                                         .DecompressToString();

                if (!json.IsNullOrWhiteSpace())
                    fromCache = JsonConvert.DeserializeObject<ScheduledItems>(json);
            }
            catch { }

            if (fromCache?.LastModified > fromDisk?.LastModified)
            {
                _items = fromCache;

                trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {fromCache.Count} items loaded from cache.");
            }
            else
            {
                _items = fromDisk;

                trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {fromDisk?.Count ?? 0} items loaded from disk.");
            }

            _items ??= [];

            if (_items is not null)
            {
                var keys = _items.Where(x => !x.Value.Valid)
                                 .Select(x => x.Key)
                                 .ToArray();

                foreach (var key in keys)
                    _items.TryRemove(key, out _);

                _items.ScheduledItemInvokedAsync += async (sender, e) =>
                {
                    using var trace = _logger.CreateTrace();

                    var item = (ScheduledItem)sender;

                    if (!item.TryStartProcessing())
                    {
                        trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {item.Id} processing skipped because item is already being processed or has been replaced.");
                        return;
                    }

                    try
                    {
                        trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {item.Id} preparing to be processed.");

                        var cloudEvent = new Domain.Events.CloudEvent()
                        {
                            Id = item.Id,
                            Data = item.Item,
                            Subject = CloudEventSubject,
                            Type = CloudEventType
                        };

                        AsyncContext.Current.MapToCloudEvent(cloudEvent);

                        trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {item.Id} cloud event mapped, CloudEventSubject: {CloudEventSubject}.");

                        await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);

                        trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {item.Id} event store updated.");

                        await _messageBus.PublishMessageAsync(cloudEvent);

                        trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {item.Id} message published.");

                        _items.TryRemove(item.Id, out _);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"An error has occurred dispatching delayed item: {item}, {ex.Message}");

                        item.Reschedule(_logger);
                    }
                    finally
                    {
                        item.StopProcessing();
                    }
                };

                _items.Mode = Mode;

                _items.RebindEvents();

                foreach (var item in _items)
                    item.Value.Initialise(_logger);
            }

            _hasLoaded = true;
        }

        public bool Schedule(T item, TimeSpan delay)
        {
            using var trace = _logger.CreateTrace();

            if (UseDefaultDelay || delay <= TimeSpan.Zero)
                delay = _defaultDelay;

            var scheduledItem = (ScheduledItem)(item, delay, GenerateId, _logger);

            bool scheduled = _items.TryAdd(scheduledItem.Id, scheduledItem);

            if (!scheduled)
                scheduledItem.Dispose();

            trace.Diagnostics.LogInformation($"Item<{typeof(T).Name}>: {scheduledItem.Id} with delay: {delay} {(scheduled ? "scheduled" : "not scheduled")}.");

            return scheduled;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _items?.Dispose();
        }
    }
}
