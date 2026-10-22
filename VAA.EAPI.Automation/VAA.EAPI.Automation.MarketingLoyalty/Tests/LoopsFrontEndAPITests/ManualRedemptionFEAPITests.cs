using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsFrontEndAPITests
{
    [Collection("ManualRedemptionFEAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class ManualRedemptionFEAPITests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public ManualRedemptionFEAPITests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
        }

        [Trait("Category", Traits.LOOPSTransactionFEAPIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("VS", "Standard Companion Reward")]
        [InlineData("DF", "VAA Duty Free")]
        public async Task TC01_AirlineAndNonAirlineManualRedemptionSuccessScenario(string partnerCode, string productName)
        {

            #region Test Data

            PreRequisiteTestData.TestDataForMemberEnrolment(out string title, out string firstName, out string lastName, out string emailId, out string postCode, out string country, out string channelId, out string enroledBy, out string dateOfBirth, out _, out _);

            HttpResponseMessage enrolmentResponse = (await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelId, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy)).EnsureGQLResponseSuccess();

            string enrolmentContent = await enrolmentResponse.Content.ReadAsStringAsync();
            JObject enrolmentResult = JObject.Parse(enrolmentContent);
            string customerId = enrolmentResult.SelectToken("data.accountEnrolMember.customerId")?.ToString();
            string memberNumber = enrolmentResult.SelectToken("data.accountEnrolMember.memberDetails.memberNumber")?.ToString();

            Assert.False(customerId.IsNullOrWhiteSpace(), "Enrolment response did not contain a customer ID.");
            Assert.False(memberNumber.IsNullOrWhiteSpace(), "Enrolment response did not contain a member number.");

            string activityDate = DataGenerator.RandomFutureDate(+365).ToString("dd-MMM-yyyy");

            string comments = "Testing Only";

            #endregion

            #region Creating a sample accrual txns to award pts to the member before doing redemptions

            DateTime currentDate = DateTime.UtcNow.Date;

            DateTime ticketIssuePastDate = DataGenerator.RandomPastDate(150);

            string ticketIssueDate = ticketIssuePastDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string activityDt = ticketIssuePastDate == currentDate ? ticketIssueDate : ticketIssuePastDate.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string createdOn = currentDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            string pnr = DataGenerator.RandomString(6, true).ToUpper();

            HttpResponseMessage txnresponse = LoopsFrontEndAPIEndpoints.CreateFlightAccuralTransaction(pnr, "J", activityDt, "VS",
                                                                              ticketIssueDate, "LHR", "301", "JFK",
                                                                              memberNumber, customerId, createdOn, comments, "99999990000").EnsureIsSuccessStatusCode();

            string txnResult = await txnresponse.Content.ReadAsStringAsync();

            FlightAccrualResponse flightAccrualResponse = JsonConvert.DeserializeObject<FlightAccrualResponse>(txnResult);

            Assert.True(flightAccrualResponse is not null, $"Failed to meet the Test Pre-requisite criteria to recieve flightAccrualResponse from the API endpoint.");

            Assert.True(flightAccrualResponse.transactionId is not null, $"Pre-requsite Transaction for awarding pts to the member not processed with message {flightAccrualResponse.message}");


            #endregion


            #region VRed Balance API Call after FA Transaction

            HttpResponseMessage vRedResponseForRecipient = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

            dynamic recipientResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponseForRecipient.Content.ReadAsStringAsync());

            int recipientUsableBalance = Convert.ToInt32(recipientResult.usable.ToString());

            int retry = 5;

            while (recipientUsableBalance <= 10 && retry > 0)
            {
                await Task.Delay(3000);

                vRedResponseForRecipient = LoopsFrontEndAPIEndpoints.VRedBalanceAPI(tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

                recipientResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponseForRecipient.Content.ReadAsStringAsync());

                recipientUsableBalance = Convert.ToInt32(recipientResult.usable.ToString());

                retry--;
            }

            Assert.True(recipientUsableBalance is > 10, $"The Member Usable balance is not sufficient for the Redemption transfer. The usable balance is {recipientUsableBalance}");

            string points = DataGenerator.RandomNumber(10, recipientUsableBalance, true).ToString();

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.ProcessManualRedemption(partnerCode, productName, activityDate, points, comments, customerId, memberNumber).EnsureIsSuccessStatusCode();


            #endregion

            #region Validations with VRED API call if the request is present in VRED system

            await Task.Delay(5000);

            HttpResponseMessage vredResponse = LoopsFrontEndAPIEndpoints.VRedStatementAPI(tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

            string result = await vredResponse.Content.ReadAsStringAsync();

            VredStatement vredStatement = JsonConvert.DeserializeObject<VredStatement>(result);

            IEnumerable<Statement> statements = vredStatement.statements;

            retry = 2;

            while (retry > 0 && !statements.Any(x => x.details.Has($"\"productName\": \"{productName}\"") && x.points == int.Parse(points)))
            {
                await Task.Delay(5000);

                vredResponse = LoopsFrontEndAPIEndpoints.VRedStatementAPI(tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();

                result = await vredResponse.Content.ReadAsStringAsync();

                vredStatement = JsonConvert.DeserializeObject<VredStatement>(result);

                statements = vredStatement.statements;

                retry--;
            }

            Assert.True(statements.Any(x => x.details.Has($"\"productName\": \"{productName}\"")),
                 $" Txn at VRED side doesnot match with the redemption product {productName} . The product name is not found at VRED statement API response");

            Statement statement = statements.FirstOrDefault(x => x.details.Has($"\"productName\": \"{productName}\""));

            Assert.True(statement?.type == "DEBIT",
                 $" Txn at VRED side doesnot match with the redemption type - DEBIT  . The product found at VRED end is {statement?.type}");

            Assert.True(statement?.points == Int32.Parse(points),
                 $" Txn debit with points at VRED side {statement?.points} doesnot match the the points debited for the product at the VAA end {points}");

            #endregion
        }
    }
}
