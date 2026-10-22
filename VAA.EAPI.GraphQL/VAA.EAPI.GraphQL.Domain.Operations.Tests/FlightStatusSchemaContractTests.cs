using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace VAA.EAPI.GraphQL.Domain.Operations.Tests;

public sealed class FlightStatusSchemaContractTests
{
    [Fact]
    public async Task PublicSchema_ExposesNamedFlightStatusArgumentsAndResponseTypes()
    {
        using var services = CreateServices(isSecure: false);
        var executor = await services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
        var fields = executor.Schema.QueryType.Fields;

        var route = fields["flightStatusByRoute"];
        Assert.Equal("FlightStatusRoute", route.Type.NamedType().Name);
        Assert.Equal(
            ["departureAirportCode", "arrivalAirportCode", "scheduledDepartureDate", "timeRange", "stops", "first"],
            route.Arguments.Select(argument => argument.Name));

        var number = fields["flightStatusByNumber"];
        Assert.Equal("FlightStatusFlight", number.Type.NamedType().Name);
        Assert.Equal(
            ["flightNumber", "departureDate", "departureAirportCode", "selectedLegId"],
            number.Arguments.Select(argument => argument.Name));
        Assert.IsType<NonNullType>(number.Arguments.Single(argument => argument.Name == "departureAirportCode").Type);

        Assert.DoesNotContain(route.Arguments, argument => argument.Name == "input" || argument.Name == "cancellationToken");
        Assert.DoesNotContain(number.Arguments, argument => argument.Name == "input" || argument.Name == "cancellationToken");
    }

    [Fact]
    public async Task SecureSchema_DoesNotExposeFlightStatusOperations()
    {
        using var services = CreateServices(isSecure: true);
        var executor = await services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
        var names = executor.Schema.QueryType.Fields.Select(field => field.Name);

        Assert.DoesNotContain("flightStatusByRoute", names);
        Assert.DoesNotContain("flightStatusByNumber", names);
    }

    private static ServiceProvider CreateServices(bool isSecure)
    {
        var services = new ServiceCollection();
        var builder = services.AddGraphQL()
            .AddFiltering()
            .AddSorting()
            .AddQueryType(descriptor => descriptor.Name("Query"));
        new Startup().RegisterQueries(builder, isSecure);
        return services.BuildServiceProvider();
    }
}
