using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class CosmosFixture : IDisposable
    {
        public static readonly CosmosClientOptions DefaultCosmosClientOptions = new()
        {
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = new TimeSpan(0, 30, 0),
        };

        private static readonly ConcurrentDictionary<string, Entry> cache = new();

        private static Entry GetOrCreateEntry(string accountEndpoint)
        {
            VAA.EAPI.Automation.Core.Xunit.Fixtures.CosmosFixture.Entry entry = (Entry)accountEndpoint;

            while (!cache.ContainsKey(entry.Key) && !cache.TryAdd(entry.Key, entry))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[entry.Key];
        }

        private static readonly object @lock = new();

        private static Entry FindClient<T>()
        {
            lock (@lock)
            {
                return cache.Values.FirstOrDefault(x => x.Types.ContainsKey(typeof(T)));
            }
        }

        public void RegisterClient(string accountEndpoint, string authKeyOrResourceToken, CosmosClientOptions clientOptions = null)
        {
            if (accountEndpoint.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(accountEndpoint));
            if (authKeyOrResourceToken.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(authKeyOrResourceToken));

            VAA.EAPI.Automation.Core.Xunit.Fixtures.CosmosFixture.Entry entry = GetOrCreateEntry(accountEndpoint);

            if (entry.Client != null)
                return;

            entry.Client = new CosmosClient(entry.Key, authKeyOrResourceToken, clientOptions ?? DefaultCosmosClientOptions);
        }

        public void RegisterType<T>(string accountEndpoint, string databaseId, string containerId)
        {
            if (accountEndpoint.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(accountEndpoint));
            if (databaseId.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(databaseId));
            if (containerId.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(containerId));

            VAA.EAPI.Automation.Core.Xunit.Fixtures.CosmosFixture.Entry entry = GetOrCreateEntry(accountEndpoint);

            if (entry.Types.ContainsKey(typeof(T)))
                return;

            while (!entry.Types.ContainsKey(typeof(T)) && !entry.Types.TryAdd(typeof(T), new[] { databaseId, containerId }))
                if (!Thread.Yield())
                    Thread.Sleep(0);
        }

        public T QueryItem<T>(string query, params KeyValuePair<string, object>[] parameters) => QueryItems<T>(query, parameters).FirstOrDefault();

        public IEnumerable<T> QueryItems<T>(string query, params KeyValuePair<string, object>[] parameters)
        {
            Microsoft.Azure.Cosmos.QueryDefinition queryDefinition = new QueryDefinition(query);

            foreach (KeyValuePair<string, object> parameter in parameters ?? Enumerable.Empty<KeyValuePair<string, object>>())
                queryDefinition.WithParameter(parameter.Key.StartsWith('@') ? parameter.Key : $"@{parameter.Key}", parameter.Value);

            return CosmosQueryItems<T>(queryDefinition);
        }

        public T CosmosQueryItem<T>(QueryDefinition query) => CosmosQueryItems<T>(query).FirstOrDefault();

        public IEnumerable<T> CosmosQueryItems<T>(QueryDefinition query, int maxItemCount = 20)
        {
            if (maxItemCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxItemCount), "Maximum item count must be greater than zero.");

            List<T> value = new();

            VAA.EAPI.Automation.Core.Xunit.Fixtures.CosmosFixture.Entry entry = FindClient<T>();

            if (entry == null)
                return value;

            CosmosClient client = entry.Client;
            string databaseId = entry.Types[typeof(T)][0];
            string containerId = entry.Types[typeof(T)][1];

            Container readContainer = client.GetContainer(databaseId, containerId);

            using (FeedIterator<T> feedIterator = readContainer.GetItemQueryIterator<T>(query))
                while (value.Count < maxItemCount && feedIterator.HasMoreResults)
                {
                    FeedResponse<T> page = feedIterator.ReadNextAsync().GetAwaiter().GetResult();
                    value.AddRange(page.Take(maxItemCount - value.Count));
                }

            return value;
        }

        public T GetScalar<T>(string accountEndpoint, string databaseId, string containerId, string query)
        {
            if (accountEndpoint.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(accountEndpoint));
            if (databaseId.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(databaseId));
            if (containerId.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(containerId));

            T value = default;

            VAA.EAPI.Automation.Core.Xunit.Fixtures.CosmosFixture.Entry entry = GetOrCreateEntry(accountEndpoint);

            if (entry == null)
                return value;

            Container readContainer = entry.Client.GetContainer(databaseId, containerId);

            using (FeedIterator<T> feedIterator = readContainer.GetItemQueryIterator<T>(new QueryDefinition(query)))
                while (feedIterator.HasMoreResults)
                    value = feedIterator.ReadNextAsync().GetAwaiter().GetResult().FirstOrDefault() ?? value;

            return value;
        }

        public ItemResponse<T> CreateItem<T>(T document, string partitionKey)
        {
            VAA.EAPI.Automation.Core.Xunit.Fixtures.CosmosFixture.Entry entry = FindClient<T>();

            if (entry == null)
                return default;

            CosmosClient client = entry.Client;
            string databaseId = entry.Types[typeof(T)][0];
            string containerId = entry.Types[typeof(T)][1];

            return client.GetContainer(databaseId, containerId)
                         .CreateItemAsync(document, new PartitionKey(partitionKey))
                         .GetAwaiter()
                         .GetResult();
        }

        private sealed class Entry
        {
            public static explicit operator Entry(Uri source) => new(source);
            public static explicit operator Entry(string source) => new(source);
            public static explicit operator Entry(CosmosClient source) => new(source.Endpoint) { Client = source };

            public Entry(Uri key) : this(key.ToString()) { }
            public Entry(string key)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                Key = key.Trim('/').ToLower();
                Types = new ConcurrentDictionary<Type, string[]>();
            }

            public string Key { get; }
            public CosmosClient Client { get; set; }
            public ConcurrentDictionary<Type, string[]> Types { get; }
        }
        public void Dispose()
        {
        }
    }
}
