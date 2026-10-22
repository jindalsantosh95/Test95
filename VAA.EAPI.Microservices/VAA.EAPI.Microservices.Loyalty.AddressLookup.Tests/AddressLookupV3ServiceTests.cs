using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public sealed class AddressLookupV3ServiceTests
    {
        public AddressLookupV3ServiceTests()
        {
            ConfigReader.Set(Constants.Environment.AddressLookupV3MinQueryLength, "3");
            ConfigReader.Set(Constants.Environment.AddressLookupV3SuggestionLimit, "6");
            ConfigReader.Set(Constants.Environment.AddressLookupV3ReferenceLifetimeSeconds, "300");
        }

        [Fact]
        public async Task GetSelectionAsync_WhenCancelledBeforeDispatch_DoesNotCallGbgRetrieve()
        {
            var gbgService = new TestGbgV3Service();
            var service = CreateService(gbgService, new TestReferenceProtector { UnprotectedReference = CreateAddressReference() });
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSelectionAsync(
                new AddressLookupV3SelectionRequest { Reference = "address-reference" },
                cancellation.Token));

            Assert.Equal(0, gbgService.RetrieveCalls);
        }

        [Fact]
        public async Task GetSelectionAsync_WithoutAuditInfrastructure_RetrievesOnceAndMapsAddress()
        {
            var gbgService = new TestGbgV3Service
            {
                RetrieveResponse = new GbgResponse<RetrieveResponse>
                {
                    Items = new[]
                    {
                        new RetrieveResponse
                        {
                            Id = "provider-address-id",
                            Line1 = "10 Downing Street",
                            Line2 = "Westminster",
                            City = "London",
                            PostalCode = "SW1A 2AA",
                            Province = "Greater London",
                            CountryName = "United Kingdom",
                            CountryIso2 = "GB"
                        }
                    }
                }
            };
            var referenceProtector = new TestReferenceProtector
            {
                UnprotectedReference = CreateAddressReference()
            };
            var service = CreateService(gbgService, referenceProtector);

            var response = await service.GetSelectionAsync(
                new AddressLookupV3SelectionRequest { Reference = "address-reference" },
                CancellationToken.None);

            Assert.Equal(1, gbgService.RetrieveCalls);
            Assert.Equal("provider-address-id", gbgService.LastRetrievedProviderId);

            Assert.Equal("flow-id", response.FlowId);
            Assert.NotNull(response.Address);
            Assert.Equal("10 Downing Street", response.Address.AddressLine1);
            Assert.Equal("Westminster", response.Address.AddressLine2);
            Assert.Equal("London", response.Address.City);
            Assert.Equal("SW1A 2AA", response.Address.PostalCode);
            Assert.Equal("Greater London", response.Address.Region);
            Assert.Equal("United Kingdom", response.Address.CountryName);
            Assert.Equal("GB", response.Address.CountryCode);
            Assert.True(response.Address.AddressVerified);
            Assert.Equal("Y", response.Address.Valid);
        }

        [Fact]
        public async Task GetSuggestionsAsync_CreatesOpaqueAddressReferences_WithoutLoggingTheQuery()
        {
            var gbgService = new TestGbgV3Service
            {
                FindResponse = new GbgResponse<FindResponse>
                {
                    Items = new[]
                    {
                        new FindResponse
                        {
                            Id = "provider-address-id",
                            Type = "Address",
                            Text = "10 Downing Street",
                            Description = "London"
                        }
                    }
                }
            };
            var referenceProtector = new TestReferenceProtector { ProtectedReference = "opaque-address-reference" };

            var service = CreateService(gbgService, referenceProtector);

            var response = await service.GetSuggestionsAsync(
                new AddressLookupV3SuggestionRequest { Query = "10 Downing Street", CountryCode = "gb" },
                CancellationToken.None);

            Assert.Equal(1, referenceProtector.ProtectCalls);
            var candidate = Assert.Single(response.Candidates);
            Assert.Same(candidate, Assert.Single(response.Candidates));
            Assert.Equal(1, referenceProtector.ProtectCalls);
            Assert.Equal("opaque-address-reference", candidate.Reference);
            Assert.Equal("Address", candidate.Type);
            Assert.NotNull(gbgService.LastFindRequest);
            Assert.Equal("10 Downing Street", gbgService.LastFindRequest.Text);
            Assert.Null(gbgService.LastFindRequest.Container);
            Assert.Equal("GB", gbgService.LastFindRequest.CountryCode);
            Assert.Equal(6, gbgService.LastFindRequest.Limit);
            Assert.NotNull(referenceProtector.LastProtectedReference);
            Assert.Equal("Address", referenceProtector.LastProtectedReference.ItemType);
            Assert.Equal("provider-address-id", referenceProtector.LastProtectedReference.ProviderId);
            Assert.NotNull(referenceProtector.LastProtectedReference.SelectionIntentId);
            Assert.Null(typeof(AddressLookupV3Reference).GetProperty("CallerBinding"));
        }

        [Fact]
        public async Task SuggestionsThenSelection_WithoutCustomContextOrSession_ReturnsAddress()
        {
            ConfigReader.Set(Constants.Environment.AddressLookupV3ReferenceEncryptionKey, Convert.ToBase64String(new byte[32]));
            var gbgService = new TestGbgV3Service
            {
                FindResponse = new GbgResponse<FindResponse>
                {
                    Items = [new FindResponse { Id = "provider-address-id", Type = "Address", Text = "Test address" }]
                },
                RetrieveResponse = new GbgResponse<RetrieveResponse>
                {
                    Items = [new RetrieveResponse { Id = "provider-address-id", Line1 = "Test address", CountryIso2 = "GB" }]
                }
            };
            var service = CreateService(gbgService, new AddressLookupV3ReferenceProtector());

            var suggestions = await service.GetSuggestionsAsync(
                new AddressLookupV3SuggestionRequest { Query = "Test address", CountryCode = "GB" },
                CancellationToken.None);
            Assert.Equal(0, gbgService.RetrieveCalls);
            var selected = await service.GetSelectionAsync(
                new AddressLookupV3SelectionRequest { Reference = Assert.Single(suggestions.Candidates).Reference }, CancellationToken.None);

            Assert.Equal(suggestions.FlowId, selected.FlowId);
            Assert.Equal("Test address", selected.Address.AddressLine1);
            Assert.Equal(1, gbgService.RetrieveCalls);
        }

        [Theory]
        [InlineData("expired")]
        [InlineData("container")]
        [InlineData("old-version")]
        [InlineData("selection-intent")]
        public async Task GetSelectionAsync_WhenReferenceValidationFails_DoesNotDispatch(string invalidField)
        {
            var reference = CreateAddressReference();
            switch (invalidField)
            {
                case "old-version": reference.ContractVersion = "v1"; break;
                case "expired": reference.ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1); break;
                case "container": reference.ItemType = "Container"; break;
                case "selection-intent": reference.SelectionIntentId = ""; break;
            }
            var gbgService = new TestGbgV3Service();
            var service = CreateService(gbgService, new TestReferenceProtector { UnprotectedReference = reference });

            var exception = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.GetSelectionAsync(
                new AddressLookupV3SelectionRequest { Reference = "address-reference" }, CancellationToken.None));

            Assert.Equal("InvalidReference", exception.Code);
            Assert.Equal(0, gbgService.RetrieveCalls);
        }

        [Fact]
        public void SelectionRequest_OnlyExposesTheAddressReference()
        {
            var property = Assert.Single(typeof(AddressLookupV3SelectionRequest).GetProperties());

            Assert.Equal(nameof(AddressLookupV3SelectionRequest.Reference), property.Name);
        }

        [Theory]
        [InlineData(false, "success")]
        [InlineData(false, "failed")]
        [InlineData(false, "cancelled")]
        [InlineData(true, "success")]
        [InlineData(true, "failed")]
        [InlineData(true, "cancelled")]
        public async Task FindTelemetry_CorrelatesInitialAndContainerOutcomesWithoutSensitiveData(bool container, string outcome)
        {
            using var channel = new TestTelemetryChannel();
            using var configuration = new TelemetryConfiguration { ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001", TelemetryChannel = channel };
            var reference = CreateAddressReference();
            reference.ItemType = "Container";
            var gbg = new TestGbgV3Service
            {
                FindException = outcome == "cancelled" ? new OperationCanceledException() : outcome == "failed" ? new InvalidOperationException("provider failed") : null
            };
            var service = CreateService(gbg, new TestReferenceProtector { UnprotectedReference = reference }, new TelemetryClient(configuration));
            var request = container
                ? new AddressLookupV3SuggestionRequest { Reference = "sensitive-reference" }
                : new AddressLookupV3SuggestionRequest { Query = "Sensitive test address", CountryCode = "GB" };
            AddressLookupV3SuggestionResponse? response = null;
            if (outcome == "success") response = await service.GetSuggestionsAsync(request, CancellationToken.None);
            else await Assert.ThrowsAnyAsync<Exception>(() => service.GetSuggestionsAsync(request, CancellationToken.None));

            var item = Assert.Single(channel.Items.OfType<EventTelemetry>());
            Assert.Equal("AddressLookupV3Find", item.Name);
            Assert.Equal(outcome, item.Properties["outcome"]);
            Assert.Equal(container ? "container" : "initial", item.Properties["operation"]);
            if (container) Assert.Equal(reference.FlowId, item.Properties["flowId"]);
            else if (response is not null) Assert.Equal(response.FlowId, item.Properties["flowId"]);
            else Assert.True(Guid.TryParse(item.Properties["flowId"], out _));
            Assert.Equal(new[] { "flowId", "operation", "outcome" }, item.Properties.Keys.OrderBy(key => key));
            Assert.Empty(item.Metrics);
        }

        [Theory]
        [InlineData("completed")]
        [InlineData("not-found")]
        [InlineData("cancelled")]
        [InlineData("failed")]
        [InlineData("provider-error")]
        public async Task RetrieveTelemetry_CorrelatesAllOutcomesWithoutRetryOrSensitiveData(string outcome)
        {
            using var channel = new TestTelemetryChannel();
            using var configuration = new TelemetryConfiguration { ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001", TelemetryChannel = channel };
            var gbg = new TestGbgV3Service
            {
                RetrieveException = outcome switch
                {
                    "cancelled" => new OperationCanceledException(),
                    "failed" => new InvalidOperationException("provider failed"),
                    "provider-error" => new AddressLookupV3Exception(HttpStatusCode.ServiceUnavailable, "Unavailable", "Provider unavailable"),
                    _ => null
                },
                RetrieveResponse = new GbgResponse<RetrieveResponse>
                {
                    Items = outcome == "not-found" ? [] : [new RetrieveResponse { Line1 = "Sensitive test address", CountryIso2 = "GB" }]
                }
            };
            var service = CreateService(gbg, new TestReferenceProtector { UnprotectedReference = CreateAddressReference() }, new TelemetryClient(configuration));
            var request = new AddressLookupV3SelectionRequest { Reference = "sensitive-reference" };
            if (outcome == "completed") await service.GetSelectionAsync(request, CancellationToken.None);
            else if (outcome == "cancelled")
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetSelectionAsync(request, CancellationToken.None));
            else
            {
                var exception = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.GetSelectionAsync(request, CancellationToken.None));
                Assert.Equal(outcome == "not-found" ? HttpStatusCode.UnprocessableEntity : HttpStatusCode.ServiceUnavailable, exception.StatusCode);
                Assert.Equal(outcome == "not-found" ? "AddressUnavailable" : outcome == "provider-error" ? "Unavailable" : "RetrieveOutcomeUnknown", exception.Code);
            }

            var item = Assert.Single(channel.Items.OfType<EventTelemetry>());
            Assert.Equal("AddressLookupV3Retrieve", item.Name);
            Assert.Equal(outcome == "completed" || outcome == "not-found" ? outcome : "uncertain", item.Properties["outcome"]);
            Assert.Equal("flow-id", item.Properties["flowId"]);
            Assert.Equal("selection-intent-id", item.Properties["selectionIntentId"]);
            Assert.True(Guid.TryParseExact(item.Properties["dispatchId"], "N", out _));
            Assert.Equal(new[] { "dispatchId", "flowId", "outcome", "selectionIntentId" }, item.Properties.Keys.OrderBy(key => key));
            Assert.Empty(item.Metrics);
            Assert.Equal(1, gbg.RetrieveCalls);
        }

        [Fact]
        public async Task RepeatedSelection_RetrievesOncePerRequestWithDistinctDiagnosticAttemptIds()
        {
            using var channel = new TestTelemetryChannel();
            using var configuration = new TelemetryConfiguration { ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001", TelemetryChannel = channel };
            var gbg = new TestGbgV3Service
            {
                RetrieveResponse = new GbgResponse<RetrieveResponse>
                {
                    Items = [new RetrieveResponse { Line1 = "Test address", CountryIso2 = "GB" }]
                }
            };
            var service = CreateService(gbg, new TestReferenceProtector { UnprotectedReference = CreateAddressReference() }, new TelemetryClient(configuration));
            var request = new AddressLookupV3SelectionRequest { Reference = "address-reference" };

            await service.GetSelectionAsync(request, CancellationToken.None);
            Assert.Equal(1, gbg.RetrieveCalls);
            await service.GetSelectionAsync(request, CancellationToken.None);
            Assert.Equal(2, gbg.RetrieveCalls);

            var events = channel.Items.OfType<EventTelemetry>().ToList();
            Assert.Equal(2, events.Count);
            Assert.All(events, item => Assert.Equal("selection-intent-id", item.Properties["selectionIntentId"]));
            Assert.NotEqual(events[0].Properties["dispatchId"], events[1].Properties["dispatchId"]);
        }

        [Theory]
        [InlineData("ReferenceKeyUnavailable")]
        [InlineData("ReferenceLifetimeUnavailable")]
        public async Task GetSuggestionsAsync_WhenReferenceCreationFails_RecordsProcessingFailureWithoutChangingFindOutcome(string errorCode)
        {
            using var channel = new TestTelemetryChannel();
            using var configuration = new TelemetryConfiguration { ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001", TelemetryChannel = channel };
            var gbg = new TestGbgV3Service
            {
                FindResponse = new GbgResponse<FindResponse>
                {
                    Items = [new FindResponse { Id = "provider-address-id", Type = "Address", Text = "Test address" }]
                }
            };
            var protector = new TestReferenceProtector
            {
                ProtectException = new AddressLookupV3Exception(HttpStatusCode.ServiceUnavailable, errorCode, "Address lookup is temporarily unavailable.")
            };
            var service = CreateService(gbg, protector, new TelemetryClient(configuration));

            var exception = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.GetSuggestionsAsync(
                new AddressLookupV3SuggestionRequest { Query = "Test address", CountryCode = "GB" },
                TestContext.Current.CancellationToken));

            Assert.Equal(errorCode, exception.Code);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
            var events = channel.Items.OfType<EventTelemetry>().ToArray();
            Assert.Equal(2, events.Length);
            var find = Assert.Single(events, item => item.Name == "AddressLookupV3Find");
            Assert.Equal("success", find.Properties["outcome"]);
            var processing = Assert.Single(events, item => item.Name == "AddressLookupV3SuggestionProcessing");
            Assert.Equal("failed", processing.Properties["outcome"]);
            Assert.Equal("references", processing.Properties["stage"]);
            Assert.Equal(find.Properties["flowId"], processing.Properties["flowId"]);
            Assert.Equal(new[] { "flowId", "outcome", "stage" }, processing.Properties.Keys.OrderBy(key => key));
            Assert.Equal(0, gbg.RetrieveCalls);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task FindTelemetry_WhenExpansionFails_KeepsSuccessfulRootOutcome(bool containerRoot, bool cancelled)
        {
            using var channel = new TestTelemetryChannel();
            using var configuration = new TelemetryConfiguration { ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001", TelemetryChannel = channel };
            var reference = CreateAddressReference();
            reference.ItemType = "Container";
            var gbg = new TestGbgV3Service
            {
                FindImplementation = (request, _) => request.Container == "child"
                    ? Task.FromException<GbgResponse<FindResponse>>(cancelled ? new OperationCanceledException() : new InvalidOperationException("provider failure"))
                    : Task.FromResult(new GbgResponse<FindResponse> { Items = [new FindResponse { Id = "child", Type = "Container" }] })
            };
            var service = CreateService(gbg, new TestReferenceProtector { UnprotectedReference = reference }, new TelemetryClient(configuration));
            var request = containerRoot
                ? new AddressLookupV3SuggestionRequest { Reference = "sensitive-reference" }
                : new AddressLookupV3SuggestionRequest { Query = "Sensitive test address", CountryCode = "GB" };

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetSuggestionsAsync(request, TestContext.Current.CancellationToken));

            var events = channel.Items.OfType<EventTelemetry>().ToArray();
            var finds = events.Where(item => item.Name == "AddressLookupV3Find").ToArray();
            Assert.Equal(2, finds.Length);
            Assert.Equal("success", finds[0].Properties["outcome"]);
            Assert.Equal(containerRoot ? "container" : "initial", finds[0].Properties["operation"]);
            Assert.Equal(cancelled ? "cancelled" : "failed", finds[1].Properties["outcome"]);
            Assert.Equal("container", finds[1].Properties["operation"]);
            var processing = Assert.Single(events, item => item.Name == "AddressLookupV3SuggestionProcessing");
            Assert.Equal("expansion", processing.Properties["stage"]);
            Assert.Equal(cancelled ? "cancelled" : "failed", processing.Properties["outcome"]);
            Assert.All(events, item => Assert.Equal(finds[0].Properties["flowId"], item.Properties["flowId"]));
            Assert.Equal(2, gbg.FindCalls);
            Assert.Equal(0, gbg.RetrieveCalls);
        }

        // Scheduling tests use controllable provider tasks; HTTP payload behaviour is covered by AddressLookupV3ExpansionTests.
        [Fact]
        public async Task GetSuggestionsAsync_ExpandsInBatchesOfAtMostFive()
        {
            ConfigReader.Set(Constants.Environment.AddressLookupV3SuggestionLimit, "20");
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellation.CancelAfter(TimeSpan.FromSeconds(15));
            var firstBatchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondBatchStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstBatchRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondBatchRelease = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var started = new ConcurrentQueue<string>();
            var active = 0;
            var peak = 0;
            var gbg = new TestGbgV3Service
            {
                FindImplementation = async (request, token) =>
                {
                    if (request.Container is null)
                        return new GbgResponse<FindResponse> { Items = Enumerable.Range(1, 12).Select(index => new FindResponse { Id = $"container-{index}", Type = "Container" }).ToArray() };
                    var current = Interlocked.Increment(ref active);
                    int previous;
                    do { previous = Volatile.Read(ref peak); }
                    while (current > previous && Interlocked.CompareExchange(ref peak, current, previous) != previous);
                    started.Enqueue(request.Container);
                    if (started.Count == 5) firstBatchStarted.TrySetResult();
                    if (started.Count == 10) secondBatchStarted.TrySetResult();
                    try
                    {
                        var index = int.Parse(request.Container["container-".Length..], System.Globalization.CultureInfo.InvariantCulture);
                        if (index <= 5) await firstBatchRelease.Task.WaitAsync(token);
                        else if (index <= 10) await secondBatchRelease.Task.WaitAsync(token);
                        return new GbgResponse<FindResponse> { Items = [new FindResponse { Id = $"address-{index}", Type = "Address" }] };
                    }
                    finally { Interlocked.Decrement(ref active); }
                }
            };
            var service = CreateService(gbg, new TestReferenceProtector());
            var pending = service.GetSuggestionsAsync(new AddressLookupV3SuggestionRequest { Query = "Test", CountryCode = "GB" }, cancellation.Token);
            try
            {
                await firstBatchStarted.Task.WaitAsync(cancellation.Token);
                Assert.Equal(5, started.Count);
                Assert.False(pending.IsCompleted);
                firstBatchRelease.SetResult();
                await secondBatchStarted.Task.WaitAsync(cancellation.Token);
                Assert.Equal(10, started.Count);
                secondBatchRelease.SetResult();
                var result = await pending;
                Assert.Equal(12, result.Candidates.Count());
                Assert.Equal(5, peak);
                Assert.Equal(13, gbg.FindCalls);
                Assert.Equal(0, gbg.RetrieveCalls);
            }
            finally
            {
                firstBatchRelease.TrySetResult();
                secondBatchRelease.TrySetResult();
                await pending;
            }
        }

        [Fact]
        public async Task GetSuggestionsAsync_WhenExpansionBudgetIsExhausted_RequestsNarrowerSearch()
        {
            using var channel = new TestTelemetryChannel();
            using var configuration = new TelemetryConfiguration { ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000001", TelemetryChannel = channel };
            var gbg = new TestGbgV3Service
            {
                FindImplementation = (request, _) =>
                {
                    var next = request.Container is null ? 0 : int.Parse(request.Container, System.Globalization.CultureInfo.InvariantCulture) + 1;
                    return Task.FromResult(new GbgResponse<FindResponse> { Items = [new FindResponse { Id = next.ToString(System.Globalization.CultureInfo.InvariantCulture), Type = "Container" }] });
                }
            };
            var service = CreateService(gbg, new TestReferenceProtector(), new TelemetryClient(configuration));
            var exception = await Assert.ThrowsAsync<AddressLookupV3Exception>(() => service.GetSuggestionsAsync(
                new AddressLookupV3SuggestionRequest { Query = "Test", CountryCode = "GB" }, TestContext.Current.CancellationToken));

            Assert.Equal("SearchTooBroad", exception.Code);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
            Assert.Equal(21, gbg.FindCalls); // One initial Find plus at most twenty expansion calls.
            Assert.Equal(0, gbg.RetrieveCalls);
            var finds = channel.Items.OfType<EventTelemetry>().Where(item => item.Name == "AddressLookupV3Find").ToArray();
            Assert.Equal(21, finds.Length);
            Assert.All(finds, item => Assert.Equal("success", item.Properties["outcome"]));
            var processing = Assert.Single(channel.Items.OfType<EventTelemetry>(), item => item.Name == "AddressLookupV3SuggestionProcessing");
            Assert.Equal("expansion", processing.Properties["stage"]);
            Assert.Equal("failed", processing.Properties["outcome"]);
        }

        [Fact]
        public async Task GetSuggestionsAsync_WhenCancelledDuringBatch_DoesNotStartNextBatch()
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellation.CancelAfter(TimeSpan.FromSeconds(15));
            var allStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var started = 0;
            var gbg = new TestGbgV3Service
            {
                FindImplementation = async (request, token) =>
                {
                    if (request.Container is null)
                        return new GbgResponse<FindResponse> { Items = Enumerable.Range(1, 6).Select(index => new FindResponse { Id = $"container-{index}", Type = "Container" }).ToArray() };
                    if (Interlocked.Increment(ref started) == 5) allStarted.TrySetResult();
                    await release.Task.WaitAsync(token);
                    return new GbgResponse<FindResponse>();
                }
            };
            var service = CreateService(gbg, new TestReferenceProtector());
            var pending = service.GetSuggestionsAsync(new AddressLookupV3SuggestionRequest { Query = "Test", CountryCode = "GB" }, cancellation.Token);
            await allStarted.Task.WaitAsync(cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            Assert.Equal(6, gbg.FindCalls);
            Assert.Equal(0, gbg.RetrieveCalls);
        }

        private sealed class TestTelemetryChannel : ITelemetryChannel
        {
            public ConcurrentQueue<ITelemetry> Items { get; } = new();
            public bool? DeveloperMode { get; set; }
            public string EndpointAddress { get; set; } = "https://telemetry.example.test";
            public void Send(ITelemetry item) => Items.Enqueue(item);
            public void Flush() { }
            public void Dispose() { }
        }

        private static AddressLookupV3Service CreateService(IGbgV3Service gbgService, IAddressLookupV3ReferenceProtector referenceProtector, TelemetryClient? telemetry = null)
        {
            return new AddressLookupV3Service(
                gbgService,
                referenceProtector,
                telemetry ?? new TelemetryClient(new TelemetryConfiguration()));
        }

        private AddressLookupV3Reference CreateAddressReference() => new()
        {
            ContractVersion = "v2",
            ProviderId = "provider-address-id",
            ItemType = "Address",
            FlowId = "flow-id",
            SelectionIntentId = "selection-intent-id",
            CountryCode = "GB",
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1)
        };

        private sealed class TestGbgV3Service : IGbgV3Service
        {
            private int findCalls;
            public int FindCalls => findCalls;
            public Func<GbgV3FindRequest, CancellationToken, Task<GbgResponse<FindResponse>>>? FindImplementation { get; set; }
            public GbgResponse<FindResponse> FindResponse { get; set; } = new();
            public GbgResponse<RetrieveResponse> RetrieveResponse { get; set; } = new();
            public GbgV3FindRequest LastFindRequest { get; private set; }
            public string? LastRetrievedProviderId { get; private set; }
            public int RetrieveCalls { get; private set; }
            public Exception? FindException { get; set; }
            public Exception? RetrieveException { get; set; }

            public Task<GbgResponse<FindResponse>> FindAsync(GbgV3FindRequest request, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref findCalls);
                LastFindRequest = request;
                if (FindImplementation is not null) return FindImplementation(request, cancellationToken);
                if (FindException is not null) return Task.FromException<GbgResponse<FindResponse>>(FindException);

                return Task.FromResult(FindResponse);
            }

            public Task<GbgResponse<RetrieveResponse>> RetrieveAsync(string providerAddressId, CancellationToken cancellationToken)
            {
                RetrieveCalls++;
                LastRetrievedProviderId = providerAddressId;
                if (RetrieveException is not null) return Task.FromException<GbgResponse<RetrieveResponse>>(RetrieveException);

                return Task.FromResult(RetrieveResponse);
            }
        }

        private sealed class TestReferenceProtector : IAddressLookupV3ReferenceProtector
        {
            public int ProtectCalls { get; private set; }
            public Exception? ProtectException { get; set; }
            public AddressLookupV3Reference LastProtectedReference { get; private set; }
            public string ProtectedReference { get; set; }
            public AddressLookupV3Reference UnprotectedReference { get; set; }

            public string Protect(AddressLookupV3Reference reference)
            {
                ProtectCalls++;
                if (ProtectException is not null) throw ProtectException;
                LastProtectedReference = reference;

                return ProtectedReference;
            }

            public AddressLookupV3Reference Unprotect(string protectedReference)
            {
                return UnprotectedReference;
            }
        }

    }
}
