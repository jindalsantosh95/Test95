Virgin Atlantic GraphQL REST Gateway
------------------------------------

This project (`VAA.EAPI.GraphQL.Rest`) serves as a REST gateway to the underlying GraphQL API. 
Instead of interacting directly with the GraphQL endpoint, consumers can use RESTful routes exposed by this gateway.

## REST Gateway Endpoints

- Primary REST endpoints follow the pattern: `v1/{queryMutationName}`
- Additional endpoints may be available under: `/mcp`
- These endpoints translate REST requests into GraphQL queries or mutations and return the results in a REST-friendly format.

**Example REST Calls:**

    GET /v1/customer/{customerId}
    GET /v1/notes/{memberNumber}
    POST /v1/addItem
    POST /v1/updateItem

Refer to the API documentation or OpenAPI/Swagger definition for the full list of available endpoints and their request/response schemas.

---

## Design Principles (for GraphQL Domain Libraries)

> The following guidance applies to the underlying GraphQL domain libraries and is included here for reference.  
> If you are contributing to the GraphQL domain projects, follow these patterns:

- Create class library projects per business domain inside the Domain solution folder (`VAA.EAPI.GraphQL.Domain.{NAME}`).
- Create a Query and Mutator resolver in each project following the design below.
- Ensure the `ExtendObjectType` attribute is used on each class with the type (`Query` / `Mutation`).
- Add the project reference to the main `VAA.EAPI.GraphQL` project.
- Update `Program.cs` to add the resolver types.
- Each class library should contain its own mapping logic and return objects.

### MutationResolver Example

    using HotChocolate.Types;
    using VAA.EAPI.GraphQL.StarWars.DataContext;

    namespace VAA.EAPI.GraphQL.Domain.CustomDomain.Mutation
    {
        [ExtendObjectType("Mutation")]
        public class MutationResolver
        {
            public Item AddItem(Item item)
            {
                // Logic here
            }

            public Item UpdateItem(Item item)
            {
                // Logic here
            }
        }
    }

### QueryResolver Example

    using HotChocolate.Data;
    using HotChocolate.Types;
    using System.Collections.Generic;
    using System.Linq;

    namespace VAA.EAPI.GraphQL.Domain.CustomDomain.Query
    {
        [ExtendObjectType("Query")]
        public class QueryResolver
        {
            [UseFiltering]
            [UseSorting]
            public ICollection<Item> GetItems() 
            {
                // Logic here        
            }

            public Item GetItem(int id)
            {
                // Logic here        
            }
        }
    }

### Program.cs Registration

    services.AddGraphQLServer()
            .AddQueryType(q => q.Name("Query"))
            .AddType<CustomResolver>()                      // Add new query types after this line
            .AddMutationType(q => q.Name("Mutation"))
            .AddType<CustomMutationResolver>()              // Add new mutation types after this line
            .AddFiltering()
            .AddSorting();

---

## Notes

- This file provides both REST gateway usage and generic GraphQL domain development guidance.
- If you are consuming the REST API, use the `/v1/{queryMutationName}` and `/mcp` endpoints.
- If you are developing or extending the GraphQL domain libraries, follow the design principles above.

If you have questions about which guidance applies, please contact the project maintainers.