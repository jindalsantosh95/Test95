using VAA.EAPI.Automation.NDC.Requests;

namespace VAA.EAPI.Automation.NDC.Tests
{
    public class OrderCreateXsdValidation
    {
        /// <summary>
        /// Example test to show how to use the schema validator and asserting the outcome.
        /// </summary>
        [Trait("Category", "Example")]
        [Fact]
        public void OrderCreateXsdValidatorExample()
        {
            //Calling the XsdValidator for Order Create passing in the request. This should be the request which has been built during the test 
            //but this example is just passing in a hardcoded example for now. 
            bool isValid = XsdValidator.ValidateOrderCreateRequest(File.ReadAllText(@".\Requests\Body\OrderCreateExample.xml"), out string[] exceptions);

            Assert.True(isValid, $"Schema validation failed with the following exceptions:\r\n{string.Join("\r\n", exceptions)}");
        }
    }
}
