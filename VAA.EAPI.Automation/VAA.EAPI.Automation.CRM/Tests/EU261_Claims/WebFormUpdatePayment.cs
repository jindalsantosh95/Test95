using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.CRM.Constants;
using VAA.EAPI.Automation.CRM.Requests.Endpoint;

namespace VAA.EAPI.Automation.CRM.Tests.EU261_Claims
{
    public class WebFormUpdatePayment : IClassFixture<TokenFixture>, IClassFixture<MemoryStoreFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly MemoryStoreFixture memoryStore;

        public WebFormUpdatePayment(TokenFixture tokenFixture, MemoryStoreFixture memoryStore)
        {
            this.tokenFixture = tokenFixture;
            this.memoryStore = memoryStore;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
            this.tokenFixture.AddTokenForDynamicsFromConfig("crm");
        }


        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task PaymentNumber_Verification()
        {
            string title = $"Automation DummyCase_{DataGenerator.RandomString(5)}";
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";

            //Sending the create case request to CRM
            HttpResponseMessage createCRMCaseResponse = Crm.CrmCreateCase(tokenFixture.GetToken("crm"), title).EnsureIsSuccessStatusCode();
            var locationUrl = createCRMCaseResponse.Headers.GetValues("Location").First();

            //Getting the Case Details by ID from the location header response
            HttpResponseMessage getCrmCaseDetailsByIdResponse = Crm.CrmGetCaseDetailsById(tokenFixture.GetToken("crm"), locationUrl).EnsureIsSuccessStatusCode();
            dynamic getCrmCaseDetailsByIdResult = JsonConvert.DeserializeObject<dynamic>(await getCrmCaseDetailsByIdResponse.Content.ReadAsStringAsync())!;

            //Extracting the details required for the next request
            string caseNumber = getCrmCaseDetailsByIdResult.vaa_casenumber.ToString();
            string incidentId = $"/vaa_payments({getCrmCaseDetailsByIdResult.incidentid.ToString()})";

            //Creating the payment record against the case
            HttpResponseMessage createPaymentRecordAgainstCaseResponse = Crm.CrmCreatePaymentRecord(tokenFixture.GetToken("crm"), caseNumber, incidentId).EnsureIsSuccessStatusCode();
            locationUrl = createPaymentRecordAgainstCaseResponse.Headers.GetValues("Location").First();

            //Getting the payment details by id from the location URL returned in the header
            HttpResponseMessage getCrmPaymentDetailsByIdResponse = Crm.CrmGetPaymentDetailsById(tokenFixture.GetToken("crm"), locationUrl).EnsureIsSuccessStatusCode();
            dynamic getCrmPaymentDetailsByIdResult = JsonConvert.DeserializeObject<dynamic>(await getCrmPaymentDetailsByIdResponse.Content.ReadAsStringAsync())!;

            //Extracting the details required
            memoryStore.Set("paymentNumberVerified", getCrmPaymentDetailsByIdResult.vaa_paymentnumber.ToString());

            //Hitting the CRM API to get the payment details for the Payment Number 
            HttpResponseMessage paymentUsingPaymentNumberResponse = Crm.CrmReturnPaymentUsingPaymentNumber(tokenFixture.GetToken("crm"), memoryStore.Get<string>("paymentNumberVerified")).EnsureSuccessStatusCode();

            dynamic paymentUsingPaymentNumberResult = JsonConvert.DeserializeObject<dynamic>(await paymentUsingPaymentNumberResponse.Content.ReadAsStringAsync())!;

            Assert.True(paymentUsingPaymentNumberResult.value[0].vaa_paymentnumber.ToString() == memoryStore.Get<string>("paymentNumberVerified"));
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task WebFormPayment_CreateNew()
        {
            string title = $"Automation DummyCase_{DataGenerator.RandomString(5)}";
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";
            string bankCountryName = "United Kingdom";

            InvokeCRMApi(title);

            GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", memoryStore.Get<string>("invokedPaymentNumber"), bankCountryName, "", bankName, "", "", "", "", "", accountHolderName, "", "", country: "").EnsureGQLResponseSuccess();

            //Hitting the CRM API to get the CountryCodeId for the BankCountryCode passed in
            HttpResponseMessage countryFilterOnNameResponse = Crm.CrmCountryFilterOnName(tokenFixture.GetToken("crm"), bankCountryName).EnsureIsSuccessStatusCode();

            dynamic countryFilterOnNameResult = JsonConvert.DeserializeObject<dynamic>(await countryFilterOnNameResponse.Content.ReadAsStringAsync())!;

            string countryId = countryFilterOnNameResult.value[0].vaa_countryid.ToString();

            Assert.True(countryFilterOnNameResult.value[0].vaa_name.ToString() == bankCountryName);

            //Hitting the CRM API to get the payment details for the Payment Number 
            HttpResponseMessage paymentUsingPaymentNumberResponse = Crm.CrmReturnPaymentUsingPaymentNumber(tokenFixture.GetToken("crm"), memoryStore.Get<string>("invokedPaymentNumber")).EnsureIsSuccessStatusCode();

            dynamic paymentUsingPaymentNumberResult = JsonConvert.DeserializeObject<dynamic>(await paymentUsingPaymentNumberResponse.Content.ReadAsStringAsync())!;

            Assert.True(paymentUsingPaymentNumberResult.value[0].vaa_beneficiaryname.ToString() == accountHolderName, $"Beneficiary Name in the response {paymentUsingPaymentNumberResult.value[0].vaa_beneficiaryname.ToString()} did not match expected being {accountHolderName}");
            Assert.True(paymentUsingPaymentNumberResult.value[0].vaa_bankname.ToString() == bankName, $"Bank Name in the response being {paymentUsingPaymentNumberResult.value[0].vaa_bankname.ToString()} did not match the expected Bank Name being {bankName}");
            Assert.True(paymentUsingPaymentNumberResult.value[0]._vaa_countrycode_value.ToString() == countryId, $"Country ode Id in the response being {paymentUsingPaymentNumberResult.value[0]._vaa_countrycode_value.ToString()} did not match the exected Country code Id being {countryId} ");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Theory]
        [InlineData("United Kingdom", "")]
        [InlineData("United Kingdom", "UK")]
        [InlineData("Canada", "United States")]
        [InlineData("Canada", "Canada")]
        public async Task WebFormUpdatePayment_CountryValidation(string bankCountryName, string country)
        {
            string title = $"Automation DummyCase_{DataGenerator.RandomString(5)}";
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";

            InvokeCRMApi(title);//invoke Crm payment to create duplicate payment no scenario.

            //Invalid country check
            if(country == "UK")
            {
                var webFormUpdatePaymentErrorResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", memoryStore.Get<string>("invokedPaymentNumber"), bankCountryName, "", bankName, "", "", "", "", "", accountHolderName, "", "", country: country);

                JObject webFormUpdatePaymentResult = JsonConvert.DeserializeObject<JObject>(await webFormUpdatePaymentErrorResponse.Content.ReadAsStringAsync())!;

                Assert.True(webFormUpdatePaymentResult.ToString().Has("Customer country is invalid"), "Error message did not contain - Customer country is invalid as expected");

                return;
            }

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", memoryStore.Get<string>("invokedPaymentNumber"), bankCountryName, "", bankName, "", "", "", "", "", accountHolderName, "", "", country: country).EnsureGQLResponseSuccess();

            var response = webFormUpdatePaymentResponse.ReadResponseAsString();

            Assert.True(response.Has("Success"), $"Response did not contain Success. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public void WebFormUpdatePayment_DuplicateBankDetails()
        {
            string title = $"Automation DummyCase_{DataGenerator.RandomString(5)}";
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";

            InvokeCRMApi(title);//invoke Crm payment to create duplicate payment no scenario.

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", memoryStore.Get<string>("invokedPaymentNumber"), "United Kingdom", "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureGQLResponseSuccess();

            var response = webFormUpdatePaymentResponse.ReadResponseAsString();

            Assert.True(response.Has("Success"), $"Response did not contain Success. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task WebFormUpdatePayment_IncorrectPaymentNumber()
        {
            string paymentNumber = "1231231231";
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";
            string validationMessage = "One of the Mandatory field is incorrect/Missing in the request";
            string validationCode = "10104";

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", paymentNumber, "United Kingdom", "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureIsSuccessStatusCode();

            JObject webFormUpdatePaymentResult = JsonConvert.DeserializeObject<JObject>(await webFormUpdatePaymentResponse.Content.ReadAsStringAsync())!;

            var response = webFormUpdatePaymentResult.SelectToken("$.errors[0].extensions")?.ToString();

            Assert.True(response.Has(validationMessage), $"Response did not contain {validationMessage}. Returned {response}");
            Assert.True(response.Has(validationCode), $"Response did not contain {validationCode}. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task WebFormUpdatePayment_BlankPaymentNumber()
        {
            string paymentNumber = string.Empty;
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";
            string validationMessage = "One of the Mandatory field is incorrect/Missing in the request";
            string validationCode = "10104";

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", paymentNumber, "United Kingdom", "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureIsSuccessStatusCode();

            JObject webFormUpdatePaymentResult = JsonConvert.DeserializeObject<JObject>(await webFormUpdatePaymentResponse.Content.ReadAsStringAsync())!;

            var response = webFormUpdatePaymentResult.SelectToken("$.errors[0].extensions")?.ToString();

            Assert.True(response.Has(validationMessage), $"Response did not contain {validationMessage}. Returned {response}");
            Assert.True(response.Has(validationCode), $"Response did not contain {validationCode}. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task WebFormUpdatePayment_MissingBankName()
        {
            string paymentNumber = memoryStore.Get<string>("paymentNumber");
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = string.Empty;
            string validationMessage = "One of the Mandatory field is incorrect/Missing in the request";
            string validationCode = "10104";

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", paymentNumber, "United Kingdom", "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureIsSuccessStatusCode();

            JObject webFormUpdatePaymentResult = JsonConvert.DeserializeObject<JObject>(await webFormUpdatePaymentResponse.Content.ReadAsStringAsync())!;

            var response = webFormUpdatePaymentResult.SelectToken("$.errors[0].extensions")?.ToString();

            Assert.True(response.Has(validationMessage), $"Response did not contain {validationMessage}. Returned {response}");
            Assert.True(response.Has(validationCode), $"Response did not contain {validationCode}. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task WebFormUpdatePayment_MissingAccountHolderName()
        {
            string paymentNumber = memoryStore.Get<string>("paymentNumber");
            string accountHolderName = string.Empty;
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";
            string validationMessage = "One of the Mandatory field is incorrect/Missing in the request";
            string validationCode = "10104";

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", paymentNumber, "United Kingdom", "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureIsSuccessStatusCode();

            JObject webFormUpdatePaymentResult = JsonConvert.DeserializeObject<JObject>(await webFormUpdatePaymentResponse.Content.ReadAsStringAsync())!;

            var response = webFormUpdatePaymentResult.SelectToken("$.errors[0].extensions")?.ToString();

            Assert.True(response.Has(validationMessage), $"Response did not contain {validationMessage}. Returned {response}");
            Assert.True(response.Has(validationCode), $"Response did not contain {validationCode}. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Fact]
        public async Task WebFormUpdatePayment_MissingBankCountryName()
        {
            string paymentNumber = memoryStore.Get<string>("paymentNumber");
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";
            string bankCountryName = string.Empty;
            string validationMessage = "One of the mandatory field is missing/invalid";
            string validationCode = "1081";

            //Sending the payment information via the GraphQl endpoint
            var webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", paymentNumber, bankCountryName, "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureIsSuccessStatusCode();

            JObject webFormUpdatePaymentResult = JsonConvert.DeserializeObject<JObject>(await webFormUpdatePaymentResponse.Content.ReadAsStringAsync())!;

            var response = webFormUpdatePaymentResult.SelectToken("$.errors[0].extensions")?.ToString();

            Assert.True(response.Has(validationMessage), $"Response did not contain {validationMessage}. Returned {response}");
            Assert.True(response.Has(validationCode), $"Response did not contain {validationCode}. Returned {response}");
        }

        [Trait("Category", Traits.EuClaimForm)]
        [Theory]
        [InlineData("India")]
        [InlineData("Korea (the Democratic People's Republic of)")]
        [InlineData("Lao People's Democratic Republic")]
        [InlineData("Côte d'Ivoire")]
        [InlineData("Rubbish Name")]
        public async Task WebFormUpdatePayment_ValidateCountryName(string countryName)
        {
            string title = $"Automation DummyCase_{DataGenerator.RandomString(5)}";
            string accountHolderName = $"AutoName_{DataGenerator.RandomString(5)}";
            string bankName = $"AutoBank_{DataGenerator.RandomString(5)}";
            string bankCountryName = countryName;

            //Sending the create case request to CRM -s1
            HttpResponseMessage createCRMCaseResponse = Crm.CrmCreateCase(tokenFixture.GetToken("crm"), title).EnsureIsSuccessStatusCode(); ;
            var locationUrl = createCRMCaseResponse.Headers.GetValues("Location").First();

            //Getting the Case Details by ID from the location header response -s2
            HttpResponseMessage getCrmCaseDetailsByIdResponse = Crm.CrmGetCaseDetailsById(tokenFixture.GetToken("crm"), locationUrl).EnsureIsSuccessStatusCode(); ;
            dynamic getCrmCaseDetailsByIdResult = JsonConvert.DeserializeObject<dynamic>(await getCrmCaseDetailsByIdResponse.Content.ReadAsStringAsync())!;

            //Extracting the details required for the next request
            string caseNumber = getCrmCaseDetailsByIdResult.vaa_casenumber.ToString();
            string incidentId = $"/vaa_payments({getCrmCaseDetailsByIdResult.incidentid.ToString()})";

            //Creating the payment record against the case -s3
            HttpResponseMessage createPaymentRecordAgainstCaseResponse = Crm.CrmCreatePaymentRecord(tokenFixture.GetToken("crm"), caseNumber, incidentId).EnsureIsSuccessStatusCode();
            locationUrl = createPaymentRecordAgainstCaseResponse.Headers.GetValues("Location").First();

            //Getting the payment details by id from the location URL returned in the header -s4
            HttpResponseMessage getCrmPaymentDetailsByIdResponse = Crm.CrmGetPaymentDetailsById(tokenFixture.GetToken("crm"), locationUrl).EnsureIsSuccessStatusCode();
            dynamic getCrmPaymentDetailsByIdResult = JsonConvert.DeserializeObject<dynamic>(await getCrmPaymentDetailsByIdResponse.Content.ReadAsStringAsync())!;

            //Extracting the details required
            memoryStore.Set($"paymentNumber_{countryName}", getCrmPaymentDetailsByIdResult.vaa_paymentnumber.ToString());

            //Hitting the CRM API to get the CountryCodeId for the BankCountryCode passed in
            HttpResponseMessage countryFilterOnNameResponse = Crm.CrmCountryFilterOnName(tokenFixture.GetToken("crm"), bankCountryName).EnsureIsSuccessStatusCode();
            dynamic countryFilterOnNameResult = JsonConvert.DeserializeObject<dynamic>(await countryFilterOnNameResponse.Content.ReadAsStringAsync())!;

            //Sending the payment information via the GraphQl endpoint
            HttpResponseMessage webFormUpdatePaymentResponse = GraphQl.WebFormUpdatePayment(tokenFixture.GetToken("graphqlv2"), "", memoryStore.Get<string>($"paymentNumber_{countryName}"), bankCountryName, "", bankName, "", "", "", "", "", accountHolderName, "", "").EnsureIsSuccessStatusCode();

            if(bankCountryName.Is("Rubbish Name"))
            {
                var responseError = webFormUpdatePaymentResponse.ReadResponseAsString();

                Assert.True(responseError.Has("Request failed."), "Request failed was not returned in the response as expected");
                Assert.True(responseError.Has("One of the mandatory field is missing/invalid"), "One of the mandatory field is missing/invalid was not returned in the response as expected");
                Assert.True(responseError.Has("1081"), "Code 1081 was not returned in the response as expected");
                Assert.True(responseError.Has("Internal Server Error"), $"Internal Server Error was not returned in the response as expected");

                return;
            }

            dynamic webFormUpdatePaymentResult = JsonConvert.DeserializeObject<dynamic>(await webFormUpdatePaymentResponse.Content.ReadAsStringAsync())!;

            Assert.True(webFormUpdatePaymentResult.data.webformUpdatePayment.status.ToString() == "Success", $"Success was not returned. Returned {webFormUpdatePaymentResult.ToString()}");

            //Hitting the CRM API to get the payment details for the Payment Number 
            HttpResponseMessage paymentUsingPaymentNumberResponse = Crm.CrmReturnPaymentUsingPaymentNumber(tokenFixture.GetToken("crm"), memoryStore.Get<string>($"paymentNumber_{countryName}")).EnsureIsSuccessStatusCode();

            dynamic paymentUsingPaymentNumberResult = JsonConvert.DeserializeObject<dynamic>(await paymentUsingPaymentNumberResponse.Content.ReadAsStringAsync())!;

            Assert.True(paymentUsingPaymentNumberResult.value[0]._vaa_countrycode_value.ToString() == countryFilterOnNameResult.value[0].vaa_countryid.ToString());
        }

        private void InvokeCRMApi(string title)
        {
            //Sending the create case request to CRM -s1
            HttpResponseMessage createCRMCaseResponse = Crm.CrmCreateCase(tokenFixture.GetToken("crm"), title).EnsureIsSuccessStatusCode();
            var locationUrl = createCRMCaseResponse.Headers.GetValues("Location").First();

            //Getting the Case Details by ID from the location header response -s2
            HttpResponseMessage getCrmCaseDetailsByIdResponse = Crm.CrmGetCaseDetailsById(tokenFixture.GetToken("crm"), locationUrl).EnsureIsSuccessStatusCode();
            dynamic getCrmCaseDetailsByIdResult = JsonConvert.DeserializeObject<dynamic>(getCrmCaseDetailsByIdResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult())!;

            //Extracting the details required for the next request
            string caseNumber = getCrmCaseDetailsByIdResult.vaa_casenumber.ToString();
            string incidentId = $"/vaa_payments({getCrmCaseDetailsByIdResult.incidentid.ToString()})";

            //Creating the payment record against the case -s3
            HttpResponseMessage createPaymentRecordAgainstCaseResponse = Crm.CrmCreatePaymentRecord(tokenFixture.GetToken("crm"), caseNumber, incidentId).EnsureIsSuccessStatusCode();
            locationUrl = createPaymentRecordAgainstCaseResponse.Headers.GetValues("Location").First();

            //Getting the payment details by id from the location URL returned in the header -s4
            HttpResponseMessage getCrmPaymentDetailsByIdResponse = Crm.CrmGetPaymentDetailsById(tokenFixture.GetToken("crm"), locationUrl).EnsureIsSuccessStatusCode();
            dynamic getCrmPaymentDetailsByIdResult = JsonConvert.DeserializeObject<dynamic>(getCrmPaymentDetailsByIdResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult())!;

            //Extracting the details required
            memoryStore.Set("invokedPaymentNumber", getCrmPaymentDetailsByIdResult.vaa_paymentnumber.ToString());
        }
    }
}