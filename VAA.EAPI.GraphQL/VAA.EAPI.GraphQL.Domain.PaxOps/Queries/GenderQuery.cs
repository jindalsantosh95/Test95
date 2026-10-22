using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class GenderQuery
    {
        private IReferenceServiceProvider _referenceServiceProvider;

        public GenderQuery(IReferenceServiceProvider referenceServiceProvider)
        {
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [GraphQLName("GetGenderDetails")]
        public async Task<IEnumerable<Gender>> GetGenderDetails()
        {
            return await _referenceServiceProvider.GetGenderDetails();
        }
    }
}