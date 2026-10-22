using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{

    [ExtendObjectType("Mutation")]
    public class OTPValidationMutation
    {
        private readonly ILogger<OTPValidationMutation> _logger;
        private readonly IOTPMutationProvider<string> _otpMutationProvider;
        private readonly ICustomerMutationProvider<TierDetail> _customerTierProvider;

        public OTPValidationMutation(ILogger<OTPValidationMutation> logger, IOTPMutationProvider<string> otpMutationProvider, ICustomerMutationProvider<TierDetail> customerTierProvider)
        {
            _otpMutationProvider = otpMutationProvider ?? throw new ArgumentNullException(nameof(otpMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerTierProvider = customerTierProvider ?? throw new ArgumentNullException(nameof(customerTierProvider));

        }


        [GraphQLName("accountOTPValidate")]
        [GraphQLDescription("Verify OTP against an email.")]
        public async Task<OtpValidationResponse> validateOTP(string emailId, int otp)
        {
            OtpValidationResponse otpValidationResponse = new();
            _logger.LogInformation($"OTP Validate Mutation. Request: {emailId} and {otp}");
            try
            {
                var errors = new List<ErrorDetails>();

                var oTPValidateResponse = await _otpMutationProvider.ValidateOtp(emailId.ToLower(), otp);
                var customer = await _customerTierProvider.GetTierResponse(emailId);

                otpValidationResponse.EnrolmentToken = oTPValidateResponse;
                otpValidationResponse.IsEnrolledMember = !(string.IsNullOrEmpty(customer.Tier) || customer.Tier == "Glass");

                return otpValidationResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OTP Validation Mutation failed");
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("OTP validation failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in OTP Validation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled Exception in accountOTPValidation mutation");
                throw;
            }
        }

    }
}
