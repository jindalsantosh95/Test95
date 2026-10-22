using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.FunctionHost.WebSockets;

namespace VAA.EAPI.GraphQL.FunctionHost
{
    public static class DomainSchemaRegistration
    {
        public static IRequestExecutorBuilder RegisterDomainSchema(this IRequestExecutorBuilder builder, bool isSecure, IEnumerable<Action<IRequestExecutorBuilder, bool>> queries, IEnumerable<Action<IRequestExecutorBuilder, bool>> mutations, IEnumerable<Action<IRequestExecutorBuilder, bool>> subscriptions)
        {
            foreach (var register in queries)
            {
                register(builder, isSecure);
            }

            builder.AddQueryType(q => q.Name("Query"));

            var initial = builder.Services.Count;

            foreach (var register in mutations)
            {
                register(builder, isSecure);
            }

            if (initial < builder.Services.Count)
            {
                builder.AddMutationType(q => q.Name("Mutation"));
            }

            initial = builder.Services.Count;

            foreach (var register in subscriptions)
            {
                register(builder, isSecure);
            }

            if (initial < builder.Services.Count)
            {
                if (builder.Services.Any(service => service.ServiceType == typeof(RedisSubscriptionSessionStore)))
                {
                    builder.AddRedisSubscriptions(provider => provider.GetRequiredService<RedisSubscriptionSessionStore>().Connection);
                }
                else
                {
                    builder.AddInMemorySubscriptions();
                }
                builder.AddSubscriptionType(q => q.Name("Subscription"));
            }

            return builder;
        }
    }
}
