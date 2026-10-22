using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.StressTests
{
    public sealed class Cosmos : IClassFixture<CosmosFixture>
    {
        private CosmosFixture cosmosFixture;

        public Cosmos(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            string accountEndpoint = Vault.Default.GetSecretValue("TESTING-Cosmos-NDC-Automation-Url");
            string authKeyOrResourceToken = Vault.Default.GetSecretValue("TESTING-Cosmos-NDC-Automation-Key");
            CosmosClientOptions? clientOptions = default;

            string databaseId = "Ndc";
            string containerId = "Offers";

            cosmosFixture.RegisterClient(accountEndpoint, authKeyOrResourceToken, clientOptions);

            cosmosFixture.RegisterType<JObject>(accountEndpoint, databaseId, containerId);
        }

        [Trait("Category", "Local-Run")]
        //This will run stress against the cosmos database for read and write setup for NDC 
        [Fact]
        public async Task AsyncReadWriteStress()
        {
            TimeSpan duration = TimeSpan.FromSeconds(60);
            Stopwatch stopwatch = Stopwatch.StartNew();

            var writeLoad = 7_200;// * 10;// 2.6; // number of writes
            var writeChunck = (int)Math.Max(writeLoad / 60.0 / 60.0, 1.0);
            var writeDelay = (int)Math.Round(1_000.00 / writeChunck);

            var readLoad = Math.Max(writeLoad * 0.2, 1.0);
            var readChunk = (int)Math.Max(readLoad / 60.0 / 60.0, 1.0);
            var readDelay = (int)Math.Round(1_000.00 / readChunk);

            var writeFailures = 0;
            var readFailures = 0;

            ConcurrentBag<Thread> writeThreads = new();
            ConcurrentBag<Thread> readThreads = new();
            ConcurrentQueue<Guid> idQueue = new();

            var writes = Task.Run(async () =>
            {
                var payload = File.ReadAllText(@".\Payloads\NDCCosmosResponse.json");
                var threadCount = 0;

                var timer = new Timer(async (state) =>
                {
                    for (int i = 0; i < writeChunck; i++)
                    {
                        Interlocked.Increment(ref threadCount);

                        var thread = new Thread(() =>
                        {
                            try
                            {
                                var id = Guid.NewGuid();
                                var idValue = id.ToString();
                                var document = JsonConvert.DeserializeObject<JObject>(payload)!;

                                document["ShoppingId"] = idValue;
                                document["id"] = idValue;

                                ((JArray)document["Offers"]!)[0]["OfferId"] = idValue;

                                var result = cosmosFixture.CreateItem(document, idValue);

                                Assert.NotNull(result);
                                Assert.Equal(System.Net.HttpStatusCode.Created, result.StatusCode);

                                idQueue.Enqueue(id);
                            }
                            catch (Exception ex)
                            {
                                Interlocked.Increment(ref writeFailures);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref threadCount);
                            }
                        })
                        {
                            IsBackground = true
                        };

                        writeThreads.Add(thread);

                        thread.Start();

                        await Task.Delay(writeDelay);
                    }
                }, null, 0, 1_000);

                await Task.Delay((int)duration.TotalMilliseconds);
            });
            var reads = Task.Run(async () =>
            {
                var threadCount = 0;

                var timer = new Timer(async (state) =>
                {
                    for (int i = 0; i < readChunk; i++)
                    {
                        Interlocked.Increment(ref threadCount);

                        var thread = new Thread(() =>
                        {
                            Guid id = Guid.Empty;

                            try
                            {
                                if (idQueue.TryDequeue(out id))
                                {
                                    var item = cosmosFixture.QueryItem<JObject>("SELECT TOP 1 [Offers] as Offers  FROM c join Offers in c.Offers where Offers.OfferId=@OfferID", new KeyValuePair<string, object>("OfferID", id));

                                    Assert.NotNull(item);

                                    Debug.WriteLine($"id: {id}, item[\"Offers\"][0][\"OfferId\"]: {item["Offers"]![0]!["OfferId"]}");
                                }
                            }
                            catch (Exception ex)
                            {
                                idQueue.Enqueue(id);

                                Interlocked.Increment(ref readFailures);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref threadCount);
                            }
                        })
                        {
                            IsBackground = true
                        };

                        readThreads.Add(thread);

                        thread.Start();

                        await Task.Delay(readDelay);
                    }
                }, null, 0, 1_000);

                await Task.Delay((int)duration.TotalMilliseconds);
            });

            Task[] tasks = { reads, writes };

            await Task.WhenAll(tasks);

            Debug.WriteLine($"Runtime: {stopwatch.ElapsedMilliseconds}, writeThreads: {writeThreads.Count}, readThreads: {readThreads.Count}, idQueue: {idQueue.Count}, writeFailures: {writeFailures}, readFailures: {readFailures}");
        }
    }
}