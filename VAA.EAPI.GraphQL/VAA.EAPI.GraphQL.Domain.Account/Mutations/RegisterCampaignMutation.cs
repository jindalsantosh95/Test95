using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class RegisterCampaignMutation
    {
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICampaignProvider _campaignProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<RegisterCampaignMutation> _logger;


        public RegisterCampaignMutation(IAccountCustomerService customerProvider, ICampaignProvider campaignProvider, ITokenB2CProvider<string> tokenB2CProvider, ILogger<RegisterCampaignMutation> logger)
        {
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _campaignProvider = campaignProvider ?? throw new ArgumentNullException(nameof(campaignProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        [GraphQLName("amexRegisterCampaign")]
        [GraphQLDescription("Register Campaign and update unique id")]
        public async Task<string> AmexRegisterCampaign(RegisterCampaignRequest registerCampaignRequest)
        {
            if (string.IsNullOrWhiteSpace(registerCampaignRequest.UniqueId))
                throw new ArgumentNullException(nameof(registerCampaignRequest.UniqueId));

            if (string.IsNullOrWhiteSpace(registerCampaignRequest.CreatedBy))
                throw new ArgumentNullException(nameof(registerCampaignRequest.CreatedBy));


            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            else
            {
                try
                {
                    var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                    var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);
                    string memberNumber = claim?.FcNumber;

                    var customerResponse = await _customerProvider.GetCustomer(null, memberNumber);

                    await _campaignProvider.AssignCampaign(CampaignMapper.MapToCampaignAssign(registerCampaignRequest, customerResponse));

                    return await _campaignProvider.UpdateCampaign(CampaignMapper.MapToCampaignUpdate(registerCampaignRequest.UniqueId, memberNumber));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled Exception in RegisterCampaign mutation");
                    throw;
                }
            }
        }
    }
}
