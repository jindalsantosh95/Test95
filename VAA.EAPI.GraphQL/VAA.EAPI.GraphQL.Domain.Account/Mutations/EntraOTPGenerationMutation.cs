using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class EntraOTPGenerationMutation(IAccountCustomerService customerService, ILogger<EntraOTPGenerationMutation> logger, IOTPMutationProvider<string> otpMutationProvider)
    {
        private readonly IAccountCustomerService _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        private readonly ILogger<EntraOTPGenerationMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOTPMutationProvider<string> _otpMutationProvider = otpMutationProvider ?? throw new ArgumentNullException(nameof(otpMutationProvider));

        [GraphQLName("accountEntraOTPGenerate")]
        [GraphQLDescription("Generates an OTP for Entra sign-up")]
        public async Task<string> SignUp(string emailId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailId))
                    throw InvalidCredentialsExceptionResponse();

                List<CustomerProfileDetails> customerProfiles;

                try
                {
                    customerProfiles = await _customerService.GetCustomerDetailsByEmail(
                        new CustomerProfileRequest
                        {
                            Email = emailId
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Customer lookup returned an exception while looking for user existance");
                    throw InvalidCredentialsExceptionResponse();
                }

                var customer = customerProfiles?.FirstOrDefault();
                var normalizedEmail = emailId.ToLowerInvariant();

                if (customer == null || string.IsNullOrWhiteSpace(customer.CustomerId))
                    return await _otpMutationProvider.GenerateEntraOtp(normalizedEmail);

                //account already exists
                return await _otpMutationProvider.GenerateOtpContinuationToken(normalizedEmail);
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "OTP Generation Mutation failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Count != 0)
                {
                    throw new GraphQLException(errorDetails.Aggregate(ErrorBuilder.New()
                        .SetMessage("OTP Generation failed"), (builder, error) => builder
                        .SetExtension("code", error.Code)
                        .SetExtension("message", error.Message)
                        .SetExtension("source", error.ErrorSource))
                        .Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in OTP Generation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled Exception in accountEntraOTPGenerate mutation");
                throw;
            }
        }

        private static GraphQLException InvalidCredentialsExceptionResponse()
        {
            throw new GraphQLException(ErrorBuilder.New().SetMessage("OTP Generation failed").SetExtension("code", Application.InvalidorMissingEmailIDErrorCode).SetExtension("message", Application.InvalidorMissingEmailIDErrorMessage).SetExtension("source", Application.EmailErrorsource).Build());
        }
    }
}
