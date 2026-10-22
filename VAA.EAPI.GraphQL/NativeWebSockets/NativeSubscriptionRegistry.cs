using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.FunctionHost.WebSockets
{
    public sealed class NativeSubscriptionRegistry : BackgroundService
    {
        private readonly IRequestExecutorResolver resolver;
        private readonly ConcurrentDictionary<Guid, Session> sessions = new();
        private readonly SemaphoreSlim capacity = new(128, 128);
        private readonly ILogger<NativeSubscriptionRegistry> logger;
        private readonly RedisSubscriptionSessionStore distributed;

        public NativeSubscriptionRegistry(IRequestExecutorResolver resolver, ILogger<NativeSubscriptionRegistry> logger = null,
            RedisSubscriptionSessionStore distributed = null)
        {
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.logger = logger ?? NullLogger<NativeSubscriptionRegistry>.Instance;
            this.distributed = distributed;
        }

        public static string Owner(ClaimsPrincipal principal)
        {
            var identity = principal?.Identities.FirstOrDefault(x => x.IsAuthenticated);
            if (identity is null)
            {
                return "anonymous";
            }
            return new JArray(identity.Claims.OrderBy(x => x.Type).ThenBy(x => x.Value)
                .Select(x => new JArray(x.Type, x.Value, x.Issuer))).ToString(Newtonsoft.Json.Formatting.None);
        }

        public async Task<JObject> StartAsync(JObject request, ClaimsPrincipal principal, Guid connectionId,
            IReadOnlyDictionary<string, object> properties = null)
        {
            var owner = Owner(principal);
            if (request["variables"] is not (null or JObject) && request["variables"].Type != JTokenType.Null)
            {
                return Error("Variables must be an object.");
            }
            var query = request.Value<string>("query");
            if (query is null || query.Length > 32768)
            {
                return Error("A subscription document of at most 32 KiB is required.");
            }
            DocumentNode document;
            try { document = Utf8GraphQLParser.Parse(query); }
            catch (SyntaxException) { return Error("Invalid GraphQL document."); }
            var operationName = request.Value<string>("operationName");
            var operations = document.Definitions.OfType<OperationDefinitionNode>()
                .Where(x => operationName is null || x.Name?.Value == operationName).ToArray();
            if (operations.Length != 1 || operations[0].Operation != OperationType.Subscription)
            {
                return Error("This endpoint accepts subscription operations only; use the existing HTTP endpoint for queries and mutations.");
            }
            if (!await capacity.WaitAsync(0)) { return Error("Subscription capacity reached."); }
            if (connectionId == Guid.Empty) { capacity.Release(); throw new UnauthorizedAccessException("Connection identity required."); }
            var session = new Session(owner, connectionId);
            try
            {
                var executor = await resolver.GetRequestExecutorAsync(cancellationToken: session.Lifetime.Token);
                var builder = QueryRequestBuilder.New().SetQuery(query).SetOperation(operationName)
                    .SetProperty("ClaimsPrincipal", principal ?? new ClaimsPrincipal(new ClaimsIdentity()));
                if (properties is not null)
                {
                    foreach (var property in properties)
                    {
                        builder.SetProperty(property.Key, property.Value);
                    }
                }
                if (request["variables"] is JObject variables)
                {
                    builder.SetVariableValues(variables.ToObject<Dictionary<string, object>>());
                }
                var result = await executor.ExecuteAsync(builder.Create(), session.Lifetime.Token);
                if (result is not IResponseStream stream)
                {
                    var error = JObject.Parse(result.ToJson());
                    if (result is IAsyncDisposable disposable) { await disposable.DisposeAsync(); }
                    return error;
                }
#if !HOTCHOCOLATE_13
                if (result is ISubscriptionResult subscription && subscription.Errors is { Count: > 0 })
                {
                    await stream.DisposeAsync();
                    return Error("Subscription validation failed.");
                }
#endif
                session.Stream = stream;
                session.Results = stream.ReadResultsAsync().GetAsyncEnumerator(session.Lifetime.Token);
                var id = Guid.NewGuid();
                if (distributed is not null) { await distributed.CreateAsync(id, owner, connectionId); }
                session.Id = id;
                sessions[session.Id] = session;
                if (distributed is not null) { session.Pump = PumpAsync(session, principal); }
                return new JObject { ["sessionId"] = session.Id.ToString() };
            }
            finally
            {
                if (session.Id == Guid.Empty)
                {
                    if (session.Results is not null) { await session.Results.DisposeAsync(); }
                    if (session.Stream is not null) { await session.Stream.DisposeAsync(); }
                    session.Lifetime.Dispose();
                    capacity.Release();
                }
            }
        }

        public async Task<JObject> ReadAsync(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken, Guid connectionId)
        {
            return distributed is null
                ? await ReadLocalAsync(id, principal, cancellationToken, connectionId)
                : await distributed.ReadAsync(id, Owner(principal), connectionId, cancellationToken);
        }

        private async Task<JObject> ReadLocalAsync(Guid id, ClaimsPrincipal principal, CancellationToken cancellationToken, Guid connectionId)
        {
            var session = Find(id, principal, connectionId);
            await session.Gate.WaitAsync(cancellationToken);
            try
            {
                session.Pending ??= session.Results.MoveNextAsync().AsTask();
                var delay = Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
                if (await Task.WhenAny(session.Pending, delay) != session.Pending)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new JObject();
                }
                if (!await session.Pending) { return new JObject { ["complete"] = true }; }
                var result = JObject.Parse(session.Results.Current.ToJson());
                session.Pending = null;
                return new JObject { ["result"] = result };
            }
            finally { session.Gate.Release(); }
        }

        public async Task StopAsync(Guid id, ClaimsPrincipal principal, Guid connectionId)
        {
            if (distributed is not null)
            {
                await distributed.StopAsync(id, Owner(principal), connectionId);
                return;
            }
            var session = Find(id, principal, connectionId);
            if (sessions.TryRemove(id, out _)) { await DisposeSessionAsync(session); }
        }

        private async Task PumpAsync(Session session, ClaimsPrincipal principal)
        {
            JObject terminal = null;
            try
            {
                while (!session.Lifetime.IsCancellationRequested &&
                    await distributed.PublishAsync(session.Id, session.Owner, session.ConnectionId, null))
                {
                    var result = await ReadLocalAsync(session.Id, principal, session.Lifetime.Token, session.ConnectionId);
                    if (result.Value<bool>("complete")) { break; }
                    if (result.HasValues && !await distributed.PublishAsync(session.Id, session.Owner, session.ConnectionId, result)) { break; }
                }
            }
            catch (OperationCanceledException) when (session.Lifetime.IsCancellationRequested)
            {
                terminal = RedisSubscriptionSessionStore.Error("Subscription lifetime ended; reconnect and subscribe again.");
            }
            catch (Exception exception)
            {
                logger.LogWarning("Distributed subscription ended: {ExceptionType}", exception.GetType().Name);
                terminal = RedisSubscriptionSessionStore.Error("Subscription ended; reconnect and subscribe again.");
            }
            finally
            {
                try { await distributed.PublishAsync(session.Id, session.Owner, session.ConnectionId, terminal, complete: true); }
                catch (Exception exception) { logger.LogWarning("Subscription mailbox cleanup failed: {ExceptionType}", exception.GetType().Name); }
                if (sessions.TryRemove(session.Id, out var removed)) { await DisposeSessionAsync(removed); }
            }
        }

        private Session Find(Guid id, ClaimsPrincipal principal, Guid connectionId)
        {
            var owner = Owner(principal);
            if (!sessions.TryGetValue(id, out var session) || session.Owner != owner || session.ConnectionId != connectionId)
            {
                throw new UnauthorizedAccessException("Subscription unavailable for this caller.");
            }
            return session;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    foreach (var pair in sessions)
                    {
                        if (pair.Value.ExpiresAt <= DateTimeOffset.UtcNow && sessions.TryRemove(pair.Key, out var session))
                        {
                            await DisposeSessionAsync(session);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            finally
            {
                var pumps = sessions.Values.Select(session => session.Pump).Where(task => task is not null).ToArray();
                foreach (var pair in sessions)
                {
                    if (sessions.TryRemove(pair.Key, out var session)) { await DisposeSessionAsync(session); }
                }
                await Task.WhenAll(pumps);
            }
        }

        private async Task DisposeSessionAsync(Session session)
        {
            session.Lifetime.Cancel();
            await session.Gate.WaitAsync();
            try
            {
                if (session.Pending is not null)
                {
                    try { await session.Pending; }
                    catch (Exception) { /* A faulted enumerator must still be disposed. */ }
                }
                try { await session.Results.DisposeAsync(); }
                finally { await session.Stream.DisposeAsync(); }
            }
            catch (Exception exception)
            {
                logger.LogWarning("Native subscription cleanup failed: {ExceptionType}", exception.GetType().Name);
            }
            finally
            {
                session.Gate.Release();
                session.Lifetime.Dispose();
                capacity.Release();
            }
        }

        private static JObject Error(string message) => new()
        {
            ["errors"] = new JArray(new JObject { ["message"] = message })
        };

        private sealed class Session
        {
            public Session(string owner, Guid connectionId) { Owner = owner; ConnectionId = connectionId; }
            public Guid ConnectionId { get; }
            public Guid Id { get; set; }
            public string Owner { get; }
            public DateTimeOffset ExpiresAt { get; } = DateTimeOffset.UtcNow.AddMinutes(5);
            public CancellationTokenSource Lifetime { get; } = new(TimeSpan.FromMinutes(5));
            public SemaphoreSlim Gate { get; } = new(1, 1);
            public IResponseStream Stream { get; set; }
            public IAsyncEnumerator<IQueryResult> Results { get; set; }
            public Task<bool> Pending { get; set; }
            public Task Pump { get; set; }
        }
    }
}
