using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;
using Constant = VAA.EAPI.Microservices.Flight.Services.Constants;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta
{
    public class DeltaSSRDetailsService : IDeltaSSRDetailsService
    {
        private readonly IHttpConnector _apiConnector;

        public DeltaSSRDetailsService(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<DeltaSSRResponse> ManageSSRDetailsAsync(SSRUpsertRequest ssrUpsertRequest, string tripResevationID)
        {
            var dlSSRUri = $"{ConfigReader.Get<string>(Constant.Environment.DlMySSRUri)}/{tripResevationID}";

            var headers = new Dictionary<string, string>
            {
                {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                { Constant.Application.ChannelId, Constant.Application.DFMobileChannelId },
                { Constant.Application.AppId, Constant.Application.DFAppId },
                { Constant.Environment.DlMyTripsID, ConfigReader.Try(Constant.Environment.DlMyTripsID) }
            };

            return await _apiConnector.SendAsync<DeltaSSRResponse>(dlSSRUri, headers: headers, bearerToken: null, verb: HttpMethod.Patch, content: Json.Serialize(ssrUpsertRequest));
        }

        public async Task<DeltaSSRResponse> RetrieveSSRDetails(string deltaSSRDetailsRequest)
        {
            var dlSSRUri = $"{ConfigReader.Get<string>(Constant.Environment.DlMySSRUri)}/{deltaSSRDetailsRequest}";

            var headers = new Dictionary<string, string>
            {
                {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                { Constant.Application.ChannelId, Constant.Application.DFMobileChannelId },
                { Constant.Application.AppId, Constant.Application.DFAppId },
                { Constant.Environment.DlMyTripsID, ConfigReader.Try(Constant.Environment.DlMyTripsID) }
            };

            return await _apiConnector.SendAsync<DeltaSSRResponse>(dlSSRUri, headers: headers, bearerToken: null, verb: HttpMethod.Get, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}
