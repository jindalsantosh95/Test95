using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    public sealed class StartupTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DomainRegistersOnlyInPublicSchemaAsync(bool isSecure)
        {
            IStartup startup = new Startup();
            var services = new ServiceCollection();
            var builder = services.AddGraphQLServer()
                .AddTypeExtension(new ObjectTypeExtension(q => q.Name("Query").Field("existingField").Type<StringType>().Resolve(_ => "existing")));
            builder.RegisterDomainSchema(isSecure,
                [startup.RegisterQueries], [startup.RegisterMutations], [startup.RegisterSubscriptions]);
            startup.ConfigureServices(services);
            await using var provider = services.BuildServiceProvider();
            var executor = await provider.GetRequestExecutorAsync();

            Assert.True(executor.Schema.QueryType.Fields.ContainsField("existingField"));
            Assert.Equal(!isSecure, executor.Schema.QueryType.Fields.ContainsField("conciergePing"));
            Assert.Equal(!isSecure, executor.Schema.QueryType.Fields.ContainsField("conciergePong"));
            if (isSecure)
            {
                Assert.Null(executor.Schema.MutationType);
                Assert.Null(executor.Schema.SubscriptionType);
                return;
            }
            Assert.NotNull(executor.Schema.MutationType);
            Assert.True(executor.Schema.MutationType.Fields.ContainsField("postConciergeMessage"));
            Assert.NotNull(executor.Schema.SubscriptionType);
            Assert.True(executor.Schema.SubscriptionType.Fields.ContainsField("conciergeMessageReceived"));
            Assert.Contains(executor.Schema.SubscriptionType.Fields["conciergeMessageReceived"].Arguments, a => a.Name == "conversationId");
        }
    }
}
