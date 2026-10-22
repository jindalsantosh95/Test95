﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.IdentityMicroservice
{
    public class IdentityMicroServiceTests : IClassFixture<TokenFixture>, IClassFixture<TableStorageService<OtpRequestTableEntity>>
    {
        private readonly ITestOutputHelper output;
        private readonly TokenFixture tokenFixture;
        private readonly TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService;

        public static string env = Config.Environment;

        public IdentityMicroServiceTests(ITestOutputHelper output, TokenFixture tokenFixture, TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService)
        {
            this.output = output;
            this.tokenFixture = tokenFixture;
            this.otpRequestTableEntityTableStorageService = otpRequestTableEntityTableStorageService;

            this.tokenFixture.AddTokenForDynamicsIdentityFromKeyVault("identity");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateOtpGenerationFromIdentityMs()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";

            System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

            Assert.True(otpGenerateResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {otpGenerateResponse.ReadResponseAsString()}");

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {otpGenerateResponse.ReadResponseAsString()}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(OTP != 0, "OTP should not be zero");
            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");
        }

        [Theory]
        [InlineData("testing")]
        [InlineData("1234324@")]
        [InlineData("")]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateValidEmailRequestInOtpGeneration(string emailId)
        {
            System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            Assert.True(otpGenerateResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from identity ms, returned {otpGenerateResponse.ReadResponseAsString()}");

            string errorMessage = "Email id in the request is Invalid or Missing";

            JArray emailOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {emailOtpResult}");

            Assert.True(emailOtpResult[0]["code"].ToString().Equals("G013"), $"Error code is not correct, response is {emailOtpResult}");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateErrorMessageWhenEmailIdIsAleradyExists()
        {
            string emailId = "georgia.schimmel.637958257426649049@asdf.com";

            System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            JArray emailOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult.ToString().Equals("OTP generated successfully"), $"Error message is wrong in response, response is {emailOtpResult}");

        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateReGenerateOtpFromIdentityMs()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            int otpGenerationCount = 4;

            foreach (int count in Enumerable.Range(1, otpGenerationCount))
            {
                System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

                Assert.True(otpGenerateResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {otpGenerateResponse.ReadResponseAsString()}");

                JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

                Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {otpGenerateResponse.ReadResponseAsString()}");

                int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

                int otpGenerationCountFromStorageTable = await EnrolmentHelper.GetOTPGenerationCountByEmailId(otpRequestTableEntityTableStorageService, emailId);

                Assert.True(OTP != 0, "OTP should not be zero");

                Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

                Assert.True(otpGenerationCountFromStorageTable == count, $"OTP Generation count doesn't match expected {count} & actual from storage table {otpGenerationCountFromStorageTable}");
            }
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateErrorMessageAfterMaximumRegeneratingOtpCount()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            int otpGenerationCount = 5;

            foreach (int count in Enumerable.Range(1, otpGenerationCount))
            {
                System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

                Assert.True(otpGenerateResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {otpGenerateResponse.ReadResponseAsString()}");

                JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

                Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {otpGenerateResponse.ReadResponseAsString()}");

                int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

                int otpGenerationCountFromStorageTable = await EnrolmentHelper.GetOTPGenerationCountByEmailId(otpRequestTableEntityTableStorageService, emailId);

                Assert.True(OTP != 0, "OTP should not be zero");

                Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

                Assert.True(otpGenerationCountFromStorageTable == count, $"OTP Generation count doesn't match expected {count} & actual from storage table {otpGenerationCountFromStorageTable}");
            }

            System.Net.Http.HttpResponseMessage otpGenerateResponseAfterMaximumRegenerateCount = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            Assert.True(otpGenerateResponseAfterMaximumRegenerateCount.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from identity ms, returned {otpGenerateResponseAfterMaximumRegenerateCount.ReadResponseAsString()}");

            string errorMessage = "Maximum attempt reached. Please request OTP again after sometime";

            JArray emailOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponseAfterMaximumRegenerateCount.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {emailOtpResult}");

            Assert.True(emailOtpResult[0]["code"].ToString().Equals("10156"), $"Error code is not correct, response is {emailOtpResult}");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateGeneratedOtpFromIdentityMs()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";

            System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            Assert.True(otpGenerateResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {otpGenerateResponse.ReadResponseAsString()}");

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {otpGenerateResponse.ReadResponseAsString()}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(OTP != 0, "OTP should not be zero");
            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, OTP);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {validateOtpResponse.ReadResponseAsString()}");

            JValue emailOtpResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult.Value.ToString().IsJwtTokenValid(), $"OTP is not validated the response is {emailOtpResult}");

            int afterValidatedOtp = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(afterValidatedOtp == 0, $"OTP not deleted from Storage table, otp present in table {afterValidatedOtp}");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateErrorMessageWhenEmailIdNotPresentInStorageValidateOtpMS()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            int OTP = 123445;

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, OTP);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from identity ms, returned {validateOtpResponse.ReadResponseAsString()}");

            string errorMessage = "Data is unavailable for the set of input provided";

            JArray emailOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {emailOtpResult}");

            Assert.True(emailOtpResult[0]["code"].ToString().Equals("G007"), $"Error code is not correct, response is {emailOtpResult}");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateErrorMessageWhenEmailIdIsBlankInValidateOtpMS()
        {
            string emailId = "";
            int OTP = 123445;

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, OTP);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from identity ms, returned {validateOtpResponse.ReadResponseAsString()}");

            string errorMessage = "One of the mandatory field is missing in the request";

            JArray emailOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {emailOtpResult}");

            Assert.True(emailOtpResult[0]["code"].ToString().Equals("G050"), $"Error code is not correct, response is {emailOtpResult}");
        }

        [Theory]
        [InlineData(123)]
        [InlineData(123123123)]
        [InlineData(0)]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateOtpIdentityMSErrorMessageWhenOtpIsWrong(int OTP)
        {
            string emailId = "automation@fly.virgin.com";

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, OTP);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, "422 was not returned from validate OTP Identity MS");

            string errorMessage = "One of the mandatory field is missing or invalid in the request";

            JArray validateOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            Assert.True(validateOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {validateOtpResult}");

            Assert.True(validateOtpResult[0]["code"].ToString().Equals("10152"), $"Error code is not correct, response is {validateOtpResult}");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateOtpGraphQlErrorMessageWhenOtpIsInvalid()
        {
            string firstName = $"First_{DataGenerator.RandomString(5)}";
            string lastName = $"Last_{DataGenerator.RandomString(5)}";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";

            System.Net.Http.HttpResponseMessage otpGenerateResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await otpGenerateResponse.Content.ReadAsStringAsync());

            Assert.True(otpGenerateResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {otpGenerateResponse.ReadResponseAsString()}");

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {otpGenerateResponse.ReadResponseAsString()}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(OTP != 0, "OTP should not be zero");
            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, 123456);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, "422 was not returned from validate OTP Identity MS");

            string errorMessage = "Invalid OTP. Please try again with correct otp";

            JArray validateOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            Assert.True(validateOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {validateOtpResult}");

            Assert.True(validateOtpResult[0]["code"].ToString().Equals("10153"), $"Error code is not correct, response is {validateOtpResult}");
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateOtpGraphQlErrorMessageWhenOtpIsExpired()
        {
            string firstName = $"First_{DataGenerator.RandomString(5)}";
            string lastName = $"Last_{DataGenerator.RandomString(5)}";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@expiredtest.com";

            System.Net.Http.HttpResponseMessage emailOtpResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await emailOtpResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {emailOtpResponse.ReadResponseAsString()}");

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponse.ReadResponseAsString()}");

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponse.ReadResponseAsString()}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(OTP != 0, "OTP should not be zero");
            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

            string expiryTime = await EnrolmentHelper.GetOTPExpiryTimeByEmailId(otpRequestTableEntityTableStorageService, emailId);

            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(expiryTime);

            DateTimeOffset expiryMinusHour = dateTimeOffset.AddHours(-1);

            string resultsDateTime = expiryMinusHour.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            await EnrolmentHelper.UpdateOtpExpiryTimeAsync(emailId, resultsDateTime);

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, OTP);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, "422 was not returned from OTP Validation end point");

            JArray validateOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            string errorMessage = "Provided OTP is expired. Please request OTP again.";

            Assert.True(validateOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {validateOtpResult}");

            Assert.True(validateOtpResult[0]["code"].ToString().Equals("10154"), $"Error code is not correct, response is {validateOtpResult}");

            int afterValidatingOTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(afterValidatingOTP == 0, $"Data is not deleted from Storage table, OTP value is {afterValidatingOTP}");

        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateUnblockAccount()
        {
            string emailId = "unblockaccount_donotuseidenitityms@mail.com";
            int otpGenerationCount = 4;

            System.Net.Http.HttpResponseMessage emailOtpResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            Assert.True(emailOtpResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {emailOtpResponse.ReadResponseAsString()}");

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await emailOtpResponse.Content.ReadAsStringAsync());

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponse.ReadResponseAsString()}");

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponse.ReadResponseAsString()}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            int otpGenerationCountFromStorageTable = await EnrolmentHelper.GetOTPGenerationCountByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(OTP != 0, "OTP should not be zero");

            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

            Assert.True(otpGenerationCountFromStorageTable == 1, $"OTP generation count is not 1, count from DB is {otpGenerationCountFromStorageTable}");

            //Generating data for another 4 times so that data already presents in DB 
            foreach (int count in Enumerable.Range(1, otpGenerationCount))
            {
                System.Net.Http.HttpResponseMessage emailOtpResponseAfterValidation = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

                Assert.True(emailOtpResponseAfterValidation.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {emailOtpResponseAfterValidation.ReadResponseAsString()}");

                JValue emailOtpResultAfterValidation = (JValue)JsonConvert.DeserializeObject<dynamic>(await emailOtpResponseAfterValidation.Content.ReadAsStringAsync());

                Assert.True(emailOtpResultAfterValidation.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponseAfterValidation.ReadResponseAsString()}");

                int OTPAfterValidation = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

                int otpGenerationCountFromStorageTableAfterValidation = await EnrolmentHelper.GetOTPGenerationCountByEmailId(otpRequestTableEntityTableStorageService, emailId);

                Assert.True(OTPAfterValidation != 0, "OTP should not be zero");

                Assert.True(OTPAfterValidation.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

                Assert.True(otpGenerationCountFromStorageTableAfterValidation == count + 1, $"OTP Generation count doesn't match expected {count} & actual from storage table {otpGenerationCountFromStorageTableAfterValidation}");
            }
        }

        [Fact]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateErrorMessageWhenValidationCountIsMaximum()
        {
            string firstName = $"First_{DataGenerator.RandomString(5)}";
            string lastName = $"Last_{DataGenerator.RandomString(5)}";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            int otpValidationCount = 5;

            System.Net.Http.HttpResponseMessage emailOtpResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            Assert.True(emailOtpResponse.StatusCode == System.Net.HttpStatusCode.OK, "200 OK was not returned from OTP generation end point");

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await emailOtpResponse.Content.ReadAsStringAsync());

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponse.ReadResponseAsString()}");

            foreach (int count in Enumerable.Range(1, otpValidationCount))
            {
                System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, 123456);

                Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, "422 was not returned from OTP Validation MS end point");

                JArray validateOtpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

                string errorMessage = "Invalid OTP. Please try again with correct otp";

                Assert.True(validateOtpResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {validateOtpResult}");

                Assert.True(validateOtpResult[0]["code"].ToString().Equals("10153"), $"Error code is not correct, response is {validateOtpResult}");

                int otpValidationCountFromStorageTable = await EnrolmentHelper.GetOTPValidationCountByEmailId(otpRequestTableEntityTableStorageService, emailId);

                Assert.True(otpValidationCountFromStorageTable == count, $"OTP Validation count doesn't match expected {count} & actual from storage table {otpValidationCountFromStorageTable}");
            }

            System.Net.Http.HttpResponseMessage validateOtpResponseAfterMaxValidationCount = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, 123456);

            Assert.True(validateOtpResponseAfterMaxValidationCount.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, "422 was not returned from OTP Validation MS end point");

            JArray validateOtpResultAfterMaxValidationCount = (JArray)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponseAfterMaxValidationCount.Content.ReadAsStringAsync());

            string errorMessageAfterMaxValidationCount = "Maximum attempt reached for unsuccessful verification. Please request OTP";

            Assert.True(validateOtpResultAfterMaxValidationCount[0]["message"].ToString().Equals(errorMessageAfterMaxValidationCount), $"Error message is wrong in response, response is {validateOtpResultAfterMaxValidationCount}");

            Assert.True(validateOtpResultAfterMaxValidationCount[0]["code"].ToString().Equals("10158"), $"Error code is not correct, response is {validateOtpResultAfterMaxValidationCount}");

            int afterValidatingOTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId);

            Assert.True(afterValidatingOTP == 0, $"Data is not deleted from Storage table, OTP value is {afterValidatingOTP}");
        }

        [Theory]
        [InlineData("ALLUPPERCASE@MAIL.COM")]
        [InlineData("MIXEDcase@Mail.com")]
        [InlineData("alllowercase@mail.com")]
        [Trait("Category", "IdentityMS-Regression")]
        public async Task ValidateStandardiseCreationAndValidationOtpForEmailId(string emailId)
        {
            System.Net.Http.HttpResponseMessage emailOtpResponse = Microservice.GenerateOtp(env, tokenFixture.GetToken("identity"), emailId);

            Assert.True(emailOtpResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from identity ms, returned {emailOtpResponse.ReadResponseAsString()}");

            JValue otpGenerateResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await emailOtpResponse.Content.ReadAsStringAsync());

            Assert.True(otpGenerateResult.Value.ToString().Equals("OTP generated successfully"), $"OTP is not generated the response is {emailOtpResponse.ReadResponseAsString()}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId.ToLower());

            Assert.True(OTP != 0, "OTP should not be zero");

            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

            System.Net.Http.HttpResponseMessage validateOtpResponse = Microservice.ValidateOtp(env, tokenFixture.GetToken("identity"), emailId, OTP);

            Assert.True(validateOtpResponse.StatusCode == System.Net.HttpStatusCode.OK, "200 OK was not returned from OTP Validation MS end point");

            JValue emailOtpResult = (JValue)JsonConvert.DeserializeObject<dynamic>(await validateOtpResponse.Content.ReadAsStringAsync());

            Assert.True(emailOtpResult.Value.ToString().IsJwtTokenValid(), $"OTP is not validated the response is {emailOtpResult}");

            int afterValidatedOtp = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailId.ToLower());

            Assert.True(afterValidatedOtp == 0, $"OTP not deleted from Storage table, otp present in table {afterValidatedOtp}");
        }
    }
}
