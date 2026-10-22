using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class NationalityQuery
    {
        private IReferenceServiceProvider _referenceServiceProvider;

        public NationalityQuery(IReferenceServiceProvider referenceServiceProvider)
        {
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [GraphQLName("GetNationalityDetails")]
        public async Task<IEnumerable<Models.ReferenceData.Nationality>> GetNationalityDetails()
        {
            return await _referenceServiceProvider.GetNationalityDetails();
        }
    }
}