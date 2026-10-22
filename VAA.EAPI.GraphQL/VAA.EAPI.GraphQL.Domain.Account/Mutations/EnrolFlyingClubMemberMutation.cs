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
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class EnrolFlyingClubMemberMutation
    {

        private readonly IEnrolmentProvider _enrolmentProvider;

        public EnrolFlyingClubMemberMutation(IEnrolmentProvider enrolmentProvider)
        {
            _enrolmentProvider = enrolmentProvider ?? throw new ArgumentNullException(nameof(enrolmentProvider));
        }

        [GraphQLName("accountEnrolMember")]
        [GraphQLDescription("Enrols a Flying club member into loyalty program.")]
        public async Task<FlyingClubSignUpResponse> EnrolFcMember(CreateCustomerAccountRequest request)
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
                return await _enrolmentProvider.EnrolFcMember(request);
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Account Creation failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
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
                            .Build()); ;
            }

        }
    }

}
