using HotChocolate;
using HotChocolate.Types;
using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class EntraEnrolFlyingClubMemberMutation
    {
        private readonly IEnrolmentProvider _enrolmentProvider;

        public EntraEnrolFlyingClubMemberMutation(IEnrolmentProvider enrolmentProvider)
        {
            _enrolmentProvider = enrolmentProvider ?? throw new ArgumentNullException(nameof(enrolmentProvider));
        }

        [GraphQLName("accountEntraEnrolMember")]
        [GraphQLDescription("Enrols a Flying club member into loyalty program.")]
        public async Task<EntraEnrolTokenResponse> EnrolFcMember(EnrolMemberAccountRequest request)
        {
            if (request == null)
            {
                var exception = new ArgumentNullException(nameof(request));
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage(exception.Message)
                    .SetExtension("Exception", "Error occurred while creating account")
                    .SetCode(HttpStatusCode.InternalServerError.ToString())
                    .Build());
            }

            StandardEnrolmentAgePolicy.EnsureEligible(request.CustomerDetails);

            try
            {
                return await _enrolmentProvider.EntraEnrolFcMember(request);
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Count != 0)
                {
                    var primaryError = errorDetails[0];
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("Account Creation failed")
                        .SetExtension("code", primaryError.Code)
                        .SetExtension("message", primaryError.Message)
                        .SetExtension("source", primaryError.ErrorSource)
                        .SetExtension("errors", errorDetails.Select(e => new { code = e.Code, message = e.Message, source = e.ErrorSource }).ToList())
                        .Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Account Creation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                            .Build());
            }
            catch (Exception ex)
            {
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage(ex.Message)
                            .SetExtension("Exception", "Error occurred while creating account")
                            .SetCode(HttpStatusCode.InternalServerError.ToString())
                            .Build());
            }
        }
    }

}
