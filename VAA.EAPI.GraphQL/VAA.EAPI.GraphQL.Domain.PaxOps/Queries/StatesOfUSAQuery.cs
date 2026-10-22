using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class StatesOfUSAQuery
    {
        private IReferenceServiceProvider _referenceServiceProvider;

        public StatesOfUSAQuery(IReferenceServiceProvider referenceServiceProvider)
        {
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [GraphQLName("GetStatesOfUSA")]
        public async Task<IEnumerable<Models.ReferenceData.StatesOfUSA>> GetStatesOfUSA()
        {
            return await _referenceServiceProvider.GetStatesOfUSA();
        }
    }
}