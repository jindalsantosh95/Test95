using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class AccountSignInMutation
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly IAccountCustomerService _customerService;
        private readonly ILogger<AccountSignInMutation> _logger;
        private readonly IReferenceDataProvider _referenceDataProvider;

        public AccountSignInMutation(IAccountCustomerService customerService, IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<AccountSignInMutation> logger, IReferenceDataProvider referenceDataProvider)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _referenceDataProvider = referenceDataProvider ?? throw new ArgumentNullException(nameof(referenceDataProvider));
        }

        [GraphQLName("accountMemberSignIn")]
        [GraphQLDescription("Sign In a user with email/Fc Number.")]
        public async Task<SignInResponse> SignInFcMember(SignInRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.FlyingClubNumber))
                    throw InvalidCredentialsExceptionResponse();

                List<CustomerProfileDetails>? customerProfiles;

                try
                {
                    customerProfiles = await _customerService.GetCustomerDetailsByEmail(
                        new CustomerProfileRequest
                        {
                            MemberNumber = string.IsNullOrWhiteSpace(request.FlyingClubNumber)
                                ? null
                                : request.FlyingClubNumber,

                            Email = string.IsNullOrWhiteSpace(request.Email)
                                ? null
                                : request.Email
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Customer lookup returned an invalid sign-in request result");
                    throw InvalidCredentialsExceptionResponse();
                }

                var customer = customerProfiles?.FirstOrDefault();

                if (customer == null || string.IsNullOrWhiteSpace(customer.CustomerId))
                    throw InvalidCredentialsExceptionResponse();

                if (!Helper.CheckMemberStatus(customer.Status))
                    throw InactiveMemberExceptionResponse();

                var preferredEmail = customer?.ContactDetails?
                    .FirstOrDefault(x =>
                        x.Primary == "Y" &&
                        string.Equals(x.ContactType, "Email", StringComparison.OrdinalIgnoreCase)
                    )?.ContactValue;

                var countries = await _referenceDataProvider.RetrieveCountriesAsync();

                foreach (var address in customer.AddressDetails.Where(a => a.Primary == "Y"))
                    address.Country = countries.FirstOrDefault(x => x.CountryName.Is(address.Country))?.IATACountryCode
                        ?? throw SignInExceptionResponse(HttpStatusCode.InternalServerError.ToString(), "Invalid country reference", "Invalid country reference in address details");

                request.FlyingClubNumber = customer.LoyaltyMemberNumber;
                request.Email = preferredEmail;
                var customerSignInProfile = new AccountSignInCustomerProfile
                {
                    CustomerId = customer.CustomerId,
                    MemberNumber = customer.LoyaltyMemberNumber,
                    Email = preferredEmail,
                    MemberStatus = customer.Status,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    DateOfBirth = customer.DateOfBirth == default
                        ? string.Empty
                        : customer.DateOfBirth?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    EnrollmentChannel = customer.CreatedBy,
                    CreatedOn = customer.CreatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    AddressDetails = customer.AddressDetails?
                        .Where(address => address.Primary == "Y")
                        .Select(address => new AccountAddressDetail
                        {
                            AddressLine1 = address.AddressLine1,
                            AddressLine2 = string.Empty,
                            AddressLine3 = string.Empty,
                            AddressType = address.AddressType,
                            PreferredFlag = address.Primary,
                            City = address.City,
                            Country = address.Country,
                            Region = address.Province,
                            PostalCode = address.PostCode
                        })
                        .ToList(),
                    ContactDetails = new AccountContactDetails
                    {
                        PhoneDetails = customer.ContactDetails?
                            .Where(contact => contact.Primary == "Y" &&
                                              string.Equals(contact.ContactType, "Phone", StringComparison.OrdinalIgnoreCase))
                            .Select(contact => new AccountPhoneDetail
                            {
                                PhoneNumber = contact.ContactValue,
                                CountryCode = contact.CountryCode,
                                PhoneType = string.Equals(contact.ContactSubType, "Landline", StringComparison.OrdinalIgnoreCase) ? "Landline" : "Mobile",
                                PreferredFlag = contact.Primary
                            })
                            .ToList(),
                        EmailDetails = customer.ContactDetails?
                            .Where(contact => contact.Primary == "Y" &&
                                              string.Equals(contact.ContactType, "Email", StringComparison.OrdinalIgnoreCase))
                            .Select(contact => new AccountEmailDetail
                            {
                                EmailId = contact.ContactValue,
                                Usage = string.Equals(contact.Usage, "Business", StringComparison.OrdinalIgnoreCase) ? "Business" : "Personal",
                                PreferredFlag = contact.Primary
                            })
                            .ToList()
                    }
                };

                return (await _connector.SendAsync<SignInResponse>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.SignInEndpoint}", GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Post, Json.Serialize((EntraSignInRequest)(request.Email, request.FlyingClubNumber, request.Password, customerSignInProfile)))).Response;
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception in account member signIn");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Account member SignIn failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }
                    ;

                    throw new GraphQLException(builder.Build());
                }

                throw SignInExceptionResponse(HttpStatusCode.InternalServerError.ToString(), $"Error While account member signIn\r\n{ex.Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in account member signIn - Exception - {ex.Message}", ex);
                throw SignInExceptionResponse(HttpStatusCode.InternalServerError.ToString(), "Error While account member signIn", ex.Message);
            }
        }

        private static GraphQLException SignInExceptionResponse(string code, string message, string exception)
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
                            .SetMessage("Error during account member Sign In, invalid username or password")
                            .SetExtension("Exception", "Invalid username or password")
                            .SetExtension("httpStatusCode", (int)HttpStatusCode.Unauthorized)
                            .SetCode(HttpStatusCode.Unauthorized.ToString())
                            .Build());
        }

        private static GraphQLException InactiveMemberExceptionResponse()
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Account member SignIn failed")
                            .SetExtension("code", "601")
                            .SetExtension("message", "Account did not meet the required status to log in")
                            .Build());
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.CommonEAPIOcpAPIMKey) }
            };
        }
    }
}
