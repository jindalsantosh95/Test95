using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class TransactionMutationProvider : ITransactionMutationProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;

        public TransactionMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector, IRefDataService<ProductDetails> productRefService, IRefDataService<PartnerProfileDetails> partnerProfileService)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _productRefService = productRefService ?? throw new ArgumentNullException(nameof(productRefService));
            _partnerProfileService = partnerProfileService ?? throw new ArgumentNullException(nameof(partnerProfileService));
        }

        public async Task<VredTransactionResponse> CreateEarnTransaction(CreateTransactionRequest earnTransactionRequest)
        {
            string transactionUri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            //Invoke Vred MS
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.SendAsync<VredTransactionResponse>($"{transactionUri}{Common.Constants.Application.VredTransactionEndPoint}", headers, token, HttpMethod.Post, Json.Serialize(earnTransactionRequest))).Response;
        }

        public async Task<VredTransactionResponse> CreateBurnTransaction(CreateTransactionRequest burnTransactionRequest)
        {
            string transactionUri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            //Invoke Vred MS
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.SendAsync<VredTransactionResponse>($"{transactionUri}{Common.Constants.Application.VredTransactionEndPoint}", headers, token, HttpMethod.Post, Json.Serialize(burnTransactionRequest))).Response;
        }


        public async Task<VredTransactionResponse> CancelTransaction(CancellationRequest cancelTransactionRequest)
        {
            string transactionUri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            //Invoke FE API
            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.SendAsync<VredTransactionResponse>($"{transactionUri}{Common.Constants.Application.VredCancelTransactionEndPoint}", headers, token, HttpMethod.Post, Json.Serialize(cancelTransactionRequest))).Response;
        }

        public async Task<VredTransactionResponse> CreateVredFlightTransaction(CreateTransactionRequest vredFlightTransactionRequest)
        {
            string transactionUri = ConfigReader.Get(Common.Constants.Environment.VredMsUrl);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.VRedAppId));

            Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

            return (await _connector.SendAsync<VredTransactionResponse>($"{transactionUri}{Common.Constants.Application.VredTransactionEndPoint}", headers, token, HttpMethod.Post, Json.Serialize(vredFlightTransactionRequest))).Response;
        }
    }
}
