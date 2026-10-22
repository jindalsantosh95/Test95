using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class ContactNoteQueryProvider : CustomerTransactionsProviderBase, IContactNoteQueryProvider
    {
        public ContactNoteQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector) : base(tokenProvider, connector)
        { }

        public async Task<IEnumerable<ContactNote>> Query(string customerId)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl).TrimEnd('/')}/{customerId}/contactnotes";
            var contactNoteResponse = await _connector.GetAsync<ContactNoteResponse>(uri, GetHeaders(), await GetToken());
            return contactNoteResponse.Response.ContactNotes;
        }
    }
}
