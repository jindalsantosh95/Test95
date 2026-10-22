using VAA.EAPI.GraphQL.Domain.Loyalty.Models.AddressSearch;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IAddressSearchLookupQueryProvider
    {
        Task<List<AddressSearchbyPostcodeResponse>> GetAddressSearchbyPostcode(AddressSearchbyPostcodeRequest request);
        Task<List<AddressSearchbyRetrieveResponse>> GetAddressSearchbyRetrieve(string addressId);

    }
}
