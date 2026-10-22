using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    internal class PassportCountryQuery
    {
        private IReferenceServiceProvider _referenceServiceProvider;

        public PassportCountryQuery(IReferenceServiceProvider referenceServiceProvider)
        {
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [GraphQLName("GetPassportCountries")]
        public async Task<IEnumerable<Models.ReferenceData.PassportCountry>> GetPassportCountries()
        {
            return await _referenceServiceProvider.GetPassportCountries();
        }
    }
}