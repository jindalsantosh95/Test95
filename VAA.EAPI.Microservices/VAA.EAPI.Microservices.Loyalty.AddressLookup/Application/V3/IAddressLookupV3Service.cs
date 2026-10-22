using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.V3
{
    public interface IAddressLookupV3Service
    {
        Task<AddressLookupV3SuggestionResponse> GetSuggestionsAsync(AddressLookupV3SuggestionRequest request, CancellationToken cancellationToken);
        Task<AddressLookupV3SelectionResponse> GetSelectionAsync(AddressLookupV3SelectionRequest request, CancellationToken cancellationToken);
    }
}
