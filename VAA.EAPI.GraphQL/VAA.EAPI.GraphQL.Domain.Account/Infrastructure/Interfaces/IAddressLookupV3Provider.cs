using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3;

namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces
{
    public interface IAddressLookupV3Provider
    {
        Task<AddressLookupV3SuggestionResponse> GetSuggestionsAsync(AddressLookupV3SuggestionInput request, CancellationToken cancellationToken);
        Task<AddressLookupV3SelectionResponse> GetSelectionAsync(AddressLookupV3SelectionInput request, CancellationToken cancellationToken);
    }
}
