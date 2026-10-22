using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PasswordResetStartMutation
    {
        private readonly IEntraServicesProvider<string> _entraServicesPasswordReset;
        private readonly IAccountCustomerService _customerService;
        private readonly ILogger<PasswordResetStartMutation> _logger;

        public PasswordResetStartMutation(IEntraServicesProvider<string> entraServicesPasswordReset, IAccountCustomerService customerService, ILogger<PasswordResetStartMutation> logger)
        {
            _entraServicesPasswordReset = entraServicesPasswordReset ?? throw new ArgumentNullException(nameof(entraServicesPasswordReset));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountPasswordResetStart")]
        [GraphQLDescription("Initiating Password Reset for Account Member.")]
        public async Task<InitiatePasswordResetResponse> ResetPasswordInitiate(PasswordResetStartRequest request)
        {
            try
            {
                if (request.FcNumberOrEmail.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.FcNumberOrEmail));

                var customerProfiles = await _customerService.GetCustomerDetailsByEmail(new CustomerProfileRequest
                {
                    MemberNumber = request.FcNumberOrEmail.Contains("@") ? null : request.FcNumberOrEmail,
                    Email = request.FcNumberOrEmail.Contains("@") ? request.FcNumberOrEmail : null
                });

                var customer = customerProfiles?.FirstOrDefault();
                if (customer == null || string.IsNullOrWhiteSpace(customer.CustomerId))
                    throw InvalidCredentialsExceptionResponse();

                if (!Helper.CheckMemberStatus(customer.Status))
                    throw InactiveMemberExceptionResponse();

                var preferredEmail = customer.ContactDetails?
                    .FirstOrDefault(x => x.Primary == "Y" &&
                                         string.Equals(x.ContactType, "Email", StringComparison.OrdinalIgnoreCase))
                    ?.ContactValue;

                if (preferredEmail.IsNullOrEmpty())
                    throw InvalidCredentialsExceptionResponse();

                request.FcNumberOrEmail = preferredEmail;

                return new InitiatePasswordResetResponse
                {
                    ContinuationToken = await _entraServicesPasswordReset.PasswordResetStart(request)
                };
            }

            catch (GraphQLException)
            {
                throw;
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception in account member password reset");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Account member password reset failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }
                    ;

                    throw new GraphQLException(builder.Build());
                }

                throw ResetPasswordStartExceptionResponse(HttpStatusCode.InternalServerError.ToString(), $"Error While resetting Password\r\n{ex.Message}", ex.Message);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception While resetting Password - Exception - {ex.Message}", ex);
                throw ResetPasswordStartExceptionResponse(HttpStatusCode.InternalServerError.ToString(), "Error While resetting Password", ex.Message);
            }
        }

        private static GraphQLException ResetPasswordStartExceptionResponse(string code, string message, string exception)
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage(message)
                            .SetExtension("Exception", exception)
                            .SetCode(code)
                            .Build());
        }

        private static GraphQLException InvalidCredentialsExceptionResponse()
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Error during account member password reset, invalid username")
                            .SetExtension("Exception", "Invalid username")
                            .SetExtension("httpStatusCode", (int)HttpStatusCode.Unauthorized)
                            .SetCode(HttpStatusCode.Unauthorized.ToString())
                            .Build());
        }

        private static GraphQLException InactiveMemberExceptionResponse()
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Account member password reset failed")
                            .SetExtension("code", "601")
                            .SetExtension("message", "Account did not meet the required status to reset password")
                            .Build());
        }
    }
}
