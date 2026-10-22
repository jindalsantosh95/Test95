using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;


namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class OTPGenerationMutation
    {
        private readonly ILogger<OTPGenerationMutation> _logger;
        private readonly IOTPMutationProvider<string> _otpMutationProvider;

        public OTPGenerationMutation(IOTPMutationProvider<string> otpMutationProvider, ILogger<OTPGenerationMutation> logger)
        {
            _otpMutationProvider = otpMutationProvider ?? throw new ArgumentNullException(nameof(otpMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [GraphQLName("accountOTPGenerate")]
        [GraphQLDescription("Generate OTP into account program.")]
        public async Task<string> generateOTP(string emailId)
        {
            _logger.LogInformation($"OTP Generate Mutation. Request: {emailId}");

            try
            {
                if (string.IsNullOrEmpty(emailId))
                {
                    var builder = ErrorBuilder.New().SetMessage("OTP Generation failed").SetExtension("code", Application.InvalidorMissingEmailIDErrorCode).SetExtension("message", Application.InvalidorMissingEmailIDErrorMessage).SetExtension("source", Application.EmailErrorsource);
                    throw new GraphQLException(builder.Build());
                }

                var oTPGenerateResponse = await _otpMutationProvider.GenerateOtp(emailId.ToLower());
                return oTPGenerateResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OTP Generation Mutation failed");
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("OTP Generation failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in OTP Generation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled Exception in accountOTPGenerate mutation");
                throw;
            }

        }

    }
}
