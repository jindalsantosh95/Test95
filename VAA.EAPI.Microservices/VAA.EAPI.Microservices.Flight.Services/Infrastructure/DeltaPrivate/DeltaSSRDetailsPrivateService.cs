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
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.DeltaPrivate.Interfaces;
using Constant = VAA.EAPI.Microservices.Flight.Services.Constants;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.DeltaPrivate
{
    public class DeltaSSRDetailsPrivateService : IDeltaSSRDetailsPrivateService
    {
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;


        public DeltaSSRDetailsPrivateService(IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<DeltaSSRResponse> ManageSSRDetailsAsync(SSRUpsertRequest ssrUpsertRequest, string tripResevationID)
        {
            var token = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constant.Environment.DeltaClientIdValue), ConfigReader.Get(Constant.Environment.DeltaClientSecret));
            var dlSSRUri = $"{ConfigReader.Get<string>(Constant.Environment.DlMySSRPrivateUri)}/{tripResevationID}";
            var headers = new Dictionary<string, string>
            {
                {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                { Constant.Application.ChannelId, Constant.Application.DFMobileChannelId },
                { Constant.Application.AppId, Constant.Application.DFAppId },
                { Constant.Environment.DlMyTripsID, ConfigReader.Try(Constant.Environment.DlMyTripsID) }
            };

            return await _apiConnector.SendAsync<DeltaSSRResponse>(dlSSRUri, headers: headers, bearerToken: token, verb: HttpMethod.Patch, content: Json.Serialize(ssrUpsertRequest));
        }

        public async Task<DeltaSSRResponse> RetrieveSSRDetails(string deltaSSRDetailsRequest)
        {
            var token = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constant.Environment.DeltaClientIdValue), ConfigReader.Get(Constant.Environment.DeltaClientSecret));
            var dlSSRUri = $"{ConfigReader.Get<string>(Constant.Environment.DlMySSRPrivateUri)}/{deltaSSRDetailsRequest}";
            var headers = new Dictionary<string, string>
            {
                {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                { Constant.Application.ChannelId, Constant.Application.DFMobileChannelId },
                { Constant.Application.AppId, Constant.Application.DFAppId },
                { Constant.Environment.DlMyTripsID, ConfigReader.Try(Constant.Environment.DlMyTripsID) }
            };

            return await _apiConnector.SendAsync<DeltaSSRResponse>(dlSSRUri, headers: headers, bearerToken: token, verb: HttpMethod.Get, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}
