using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class LoyaltyProgramsQuery
    {
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogger<LoyaltyProgramsQuery> _logger;

        public LoyaltyProgramsQuery(IReferenceServiceProvider referenceServiceProvider, ILogger<LoyaltyProgramsQuery> logger)
        {
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("LoyaltyProgramsDetails")]
        [GraphQLDescription("Retrieve Loyalty Programs details")]
        public async Task<LoyaltyProgramDetails> GetLoyaltyProgramsDetails()
        {
            try
            {
                return (LoyaltyProgramDetails)new FQTProgramDetails
                {
                    FQTPrograms = await _referenceServiceProvider.RetrieveLoyaltyProgramsDataAsync()
                };
            }
            catch (Exception ex)
            {
                var msError = await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync();
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Error message - {msError}");

                throw new Exception(Constants.ErrorCodes.RetrieveLoyaltyProgrammesErrorCode, ex);
            }
        }
    }
}