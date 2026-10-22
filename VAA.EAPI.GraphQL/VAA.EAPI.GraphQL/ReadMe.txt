Virgin Atlantic GraphQL
-----------------------

Design principles:

    - Create class library projects per business domain inside Domain solution folder (VAA.EAPI.GraphQL.Domain.{NAME}).
    - Create a Query and Mutator resolver in each project following the following below design.
    - Ensure the ExtendObjectType is decorated on each class with the type (Query / Mutation).
    - Add the project reference to the main VAA.EAPI.GraphQL.
    - Update Program.cs to add the resolver types.
    - Each class libraries should contain their own mapping logic and return objects.



MutationResolver:

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



QueryResolver.cs:

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
            };
        }
    }




Program.cs:

    services.AddGraphQLServer()
            .AddQueryType(q => q.Name("Query"))
            .AddType<CustomResolver>()                      <--- Add new query types after this line
            .AddMutationType(q => q.Name("Mutation"))
            .AddType<CustomMutationResolver>()              <--- Add new mutation types after this line
            .AddFiltering()
            .AddSorting();




Querying the endpoint: 

    POST /api/v1/GraphQL

    {
      "query": "query { items { id, name } }"
    }
    

    {
      "query": "mutation { addItem( item: { name: \"Test\", side: \"Test\" } ) { name } }"
    }


    --CUSTOMER PROFILE (all fields)--

    query
    {
        customer(customerId: "<customerID>")  
        {
            customerId
            firstName 
            dateOfBirth
            addressDetails
            {
                addressType
                companyName
                addressLine1
                addressLine2
                addressLine3
                city
                region
                postalCode
                country
                preferredFlag
            } 
            emailDetails 
            {
                emailAddress
                description
                preferredFlag
            }
            phoneDetails
            {
                phoneNumber
                description
                preferredFlag
                phoneType
            }
            memberNotes
            {
                id
                group
                type
                text
                createdBy
                createdOn
                modifiedBy
                modifiedOn
            }

        }
    }

    --MEMBER NOTES (all fields)--
    query
    {
        notes(memberNumber: "<memberID>") 
        {
            id
            group
            type
            text
            createdBy
            createdOn
            modifiedBy
            modifiedOn
        }
    }