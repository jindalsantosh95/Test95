using HotChocolate.Language;
using Xunit;

namespace VAA.EAPI.GraphQL.Common.Tests;

public sealed class GraphQLDocumentInspectorTests
{
    [Theory]
    [InlineData("mutation Link { ... on Mutation { accountCreateLinkingProof(input: {}) { linkingProof } } }")]
    [InlineData("mutation Link { ...ProofFields } fragment ProofFields on Mutation { accountCreateLinkingProof(input: {}) { linkingProof } }")]
    public void ContainsField_FindsProofMutationThroughFragments(string query)
    {
        var document = Utf8GraphQLParser.Parse(query);

        Assert.True(GraphQLDocumentInspector.ContainsField(document, "accountCreateLinkingProof"));
    }

    [Fact]
    public void ContainsField_IgnoresUnusedFragment()
    {
        var document = Utf8GraphQLParser.Parse(
            "mutation Other { otherMutation } fragment Unused on Mutation { accountCreateLinkingProof(input: {}) { linkingProof } }");

        Assert.False(GraphQLDocumentInspector.ContainsField(document, "accountCreateLinkingProof"));
    }

    [Fact]
    public void ContainsField_DoesNotThrowForDuplicateFragments()
    {
        var document = Utf8GraphQLParser.Parse(
            "mutation Link { ...ProofFields } fragment ProofFields on Mutation { accountCreateLinkingProof(input: {}) { linkingProof } } fragment ProofFields on Mutation { otherMutation }");

        Assert.True(GraphQLDocumentInspector.ContainsField(document, "accountCreateLinkingProof"));
    }
}
