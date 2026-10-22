using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PnrLinkMutation(IStampingPNRsHandler stampingPNRsHandler, ILogger<ManageBannerBannerMutation> logger)
    {
        private readonly IStampingPNRsHandler _stampingPNRsHandler = stampingPNRsHandler ?? throw new ArgumentNullException(nameof(stampingPNRsHandler));
        private readonly ILogger<ManageBannerBannerMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("accountPnrLinkEnrolment")]
        [GraphQLDescription("Links Pnr when Basic member enrols through PowerApp or Delta Channel.")]
        public async Task<string> LinkPnrAsync(CustomerDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                await _stampingPNRsHandler.StampPNRs(request);

                return "Pnr Linked Succesfully with FcNumber.";
            }

            catch (HttpRequestException ex)
            {
                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);
                _logger.LogError(ex, apiResponse);

                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Pnr Linking failed post enrrolment.");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Pnr Link Mutation\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.EmailUpdateError)
                            .Build());
            }
        }
    }
}
