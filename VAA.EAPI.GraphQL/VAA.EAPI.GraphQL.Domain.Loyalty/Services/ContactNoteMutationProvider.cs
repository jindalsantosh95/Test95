using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class ContactNoteMutationProvider<T> : CustomerTransactionsProviderBase, IContactNoteMutationProvider<T>
    {
        public ContactNoteMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        { }

        public async Task<T> Mutate(ContactNoteMutationRequest request)
        {
            string baseUri = ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/');
            string uri = request.RequestType == HttpMethod.Delete ? $"{baseUri}/{request.CustomerId}/contactnotes/{request.ContactNote.Id}" : $"{baseUri}/{request.CustomerId}/contactnotes";
            var contactNote = await _connector.SendAsync<T>(uri, GetHeaders(), await GetToken(), request.RequestType, Common.Serialisation.Json.Serialize(request));

            return contactNote.Response;
        }
    }
}
