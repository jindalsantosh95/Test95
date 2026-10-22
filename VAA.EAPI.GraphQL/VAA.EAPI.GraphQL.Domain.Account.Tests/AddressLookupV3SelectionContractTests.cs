using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public sealed class AddressLookupV3SelectionContractTests
    {
        [Fact]
        public void SuggestionInput_ExposesAddressFieldsAndRetainsExistingQueryAndReference()
        {
            var schema = SchemaBuilder.New()
                .AddQueryType(descriptor => descriptor.Name("Query")
                    .Field("test").Type<StringType>().Resolve("test"))
                .AddType<InputObjectType<AddressLookupV3SuggestionInput>>()
                .Create();

            var input = schema.GetType<InputObjectType>(nameof(AddressLookupV3SuggestionInput));

            Assert.Equal(new[] { "addressLine1", "countryCode", "postCode", "query", "reference" },
                input.Fields.Select(field => field.Name.ToString()).OrderBy(name => name));
        }

        [Fact]
        public void SelectionInput_OnlyExposesTheAddressReferenceInTheSchema()
        {
            var schema = SchemaBuilder.New()
                .AddQueryType(descriptor => descriptor.Name("Query")
                    .Field("test").Type<StringType>().Resolve("test"))
                .AddType<InputObjectType<AddressLookupV3SelectionInput>>()
                .Create();

            var input = schema.GetType<InputObjectType>(nameof(AddressLookupV3SelectionInput));

            Assert.Equal("reference", Assert.Single(input.Fields).Name);
        }
    }
}
