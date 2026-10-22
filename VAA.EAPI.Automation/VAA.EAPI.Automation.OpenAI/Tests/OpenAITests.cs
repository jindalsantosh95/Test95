using FuzzySharp;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.OpenAI.Helpers;
using VAA.EAPI.Automation.OpenAI.Requests.Endpoints;
using VAA.EAPI.Automation.OpenAI.Tests.TestCases;

namespace VAA.EAPI.Automation.OpenAI.Tests
{
    public class OpenAITests
    {
        private static readonly string _apiKey = Core.Helpers.ConfigReader.Get("OpenAIKey");
        private static readonly string _systemPromptHelpful = "You are a helpful assistant.";
        private static readonly string _systemPromptFactChecking = "You are a fact-checking assistant. Classify the relationship between a response and a claim as one of: ENTAILMENT, CONTRADICTION, or NEUTRAL.";

        public class OpenAIClientTests
        {
            [Theory]
            [Trait("Category", GlobalTraits.LocalRun)]
            [MemberData(nameof(FlightSearchTestData.TestCases), MemberType = typeof(FlightSearchTestData))]
            public async Task OpenAITest_Poc(string prompt, string expectedFuzzy, string[] requiredKeywords)
            {
                // Arrange
                string payload = RequestHelper.OpenAIRequest(prompt, _systemPromptHelpful);

                // Act
                var openAIresponse = OpenAIEndpoint.OpenAIPost(payload, _apiKey).EnsureIsSuccessStatusCode();

                var dynamicOpenAIResponse = JsonConvert.DeserializeObject<dynamic>(await openAIresponse.Content.ReadAsStringAsync());

                string response = dynamicOpenAIResponse.choices[0].message.content.ToString();

                // Assert
                var softAssert = new SoftAssertHelper();

                softAssert.False(string.IsNullOrEmpty(response), "Response is empty");

                softAssert.True(char.IsUpper(response[0]), "Sentance should start with a capital letter");

                softAssert.True(response.EndsWith(".", StringComparison.Ordinal) || response.EndsWith("?", StringComparison.Ordinal) || response.EndsWith("!", StringComparison.Ordinal), "Sentence should end with punctuation.");

                // Keyword match
                bool allKeywordsPresent = requiredKeywords.All(k => response.Has(k));

                //Score Range Interpretation Use Case Recommendation
                //90 – 100  Very close match, likely nearly identical Accept as a full match in most cases
                //80 – 89   Good match, minor variation or extra words  Acceptable for AI response matching with context
                //70 – 79   Moderate match, structural or lexical differences   Use cautiously; combine with other checks(e.g., keywords)
                //< 70      Weak match, likely different content    Usually reject or flag for review
                
                // Fuzzy match
                int fuzzyScore = Fuzz.PartialRatio(expectedFuzzy, response);

                softAssert.True(allKeywordsPresent || fuzzyScore > 85, $"Failed for prompt: {prompt}");

                // Arrange
                string nliPrompt = $"""Response: "{response}"Claim: "{prompt}"Is the claim entailed by the response? Reply only with one word: ENTAILMENT, CONTRADICTION, or NEUTRAL.""";

                payload = RequestHelper.OpenAIRequest(nliPrompt, _systemPromptFactChecking);

                // Act
                var verdictResponse = OpenAIEndpoint.OpenAIPost(payload, _apiKey).EnsureIsSuccessStatusCode();

                var dynamicVerdictResponse = JsonConvert.DeserializeObject<dynamic>(await verdictResponse.Content.ReadAsStringAsync());

                string verdict = dynamicVerdictResponse.choices[0].message.content.ToString();

                string normalizedVerdict = verdict.ToUpperInvariant();

                // Natural Language Inference (NLI) Explanation:
                // ENTAILMENT - Response supports claim.
                //   e.g. Response: "Virgin Atlantic flies direct to Atlanta from London." ENTAILMENT
                //
                // CONTRADICTION - Response disproves claim.
                //   e.g. Response: "Virgin Atlantic does not currently fly to Atlanta." CONTRADICTION
                // Fix for CS1503: Argument 3: cannot convert from 'string' to 'System.Collections.Generic.IEqualityComparer<char>'
                // The issue arises because the Assert.Equal method is being used incorrectly. The third argument is expected to be an IEqualityComparer<char>, but a string is being passed instead.
                // To fix this, we need to remove the third argument or ensure the correct type is passed.
                //
                // NEUTRAL - Response does not clearly support or contradict.
                //   e.g. Response: "Check the website for destinations."  NEUTRAL

                softAssert.True(normalizedVerdict == "ENTAILMENT", $"NLI validation failed. Expected: ENTAILMENT but got: {normalizedVerdict}. Response was: {verdictResponse.ReadResponseAsString()}");

                softAssert.AssertAll();
            }
        }
    }
}