using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta
{
    public class DeltaOfferService : IDeltaOfferService
    {
        private readonly ILogger<DeltaOfferService> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private readonly bool _switchToAws = ConfigReader.Get<bool>(Constants.Environment.SwitchToAws);

        public DeltaOfferService(ILogger<DeltaOfferService> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<OffersSetList> GetOffers(OfferSearchCriteria offerRequest)
        {
            try
            {
                if (offerRequest == null) throw new ArgumentNullException(nameof(offerRequest));

                var offerUri = _switchToAws ? ConfigReader.Get(Constants.Environment.DlAwsOfferUri) : ConfigReader.Get(Constants.Environment.DlOfferUri);
                var token = _switchToAws
                    ? await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlAwsOfferClientId), ConfigReader.Get(Constants.Environment.DlAwsOfferClientSecret), _switchToAws)
                    : await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlOfferClientId), ConfigReader.Get(Constants.Environment.DlOfferClientSecret), _switchToAws);


                _logger.LogInformation($"CorrelationId: {AsyncContext.Current.CorrelationId}, Delta request \n {Json.Serialize(offerRequest)}");

                var headers = new Dictionary<string, string>
                {
                    {Constants.Application.ApplicationId, ConfigReader.Get(Constants.Environment.DlApiApplicationId)},
                    {Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.DlApiChannelId)},
                    {"transactionId", AsyncContext.Current.CorrelationId.ToString()}
                };

                return await _apiConnector.SendAsync<OffersSetList>(offerUri, headers: headers, bearerToken: token, verb: HttpMethod.Post, content: Json.Serialize(offerRequest));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} request terminated while calling DeltaOfferService. Response: {((HttpResponseException)ex.InnerException).Response}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} request terminated while calling DeltaOfferService. Exception Message: {ex.Message}");
                throw;
            }
        }

        public async Task<Result<Response<OfferFareRules>>> GetOfferFareRules(string offerID)
        {
            try
            {
                var token = _switchToAws
                    ? await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlAwsOfferClientId), ConfigReader.Get(Constants.Environment.DlAwsOfferClientSecret), _switchToAws)
                    : await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlOfferClientId), ConfigReader.Get(Constants.Environment.DlOfferClientSecret), _switchToAws);

                var headers = new Dictionary<string, string>
                {
                    {Constants.Application.ApplicationId, ConfigReader.Get(Constants.Environment.DlApiApplicationId)},
                    {Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.DlApiChannelId)},
                    {"transactionId", AsyncContext.Current.CorrelationId.ToString()}

                };

                var fareRuleURL = ConfigReader.Get(_switchToAws ? Constants.Environment.DlAwsOfferFareRulesUri : Constants.Environment.DlOfferFareRulesUri).Replace("{offerid}", offerID);
                // Work around Delta bug that requires Content-Type even for a GET request (even though Content-Type has no meaning for HTTP GET).
                var fareRules = await _apiConnector.SendAsync<OfferRuleList>(fareRuleURL, headers: headers, token, verb: HttpMethod.Get, content: new StringContent("", Encoding.UTF8, "application/json"));
                
                return Result.Success(new Response<OfferFareRules>
                {
                    Status = Status.OK,
                    Value = (OfferFareRules)fareRules
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} request terminated while calling DeltaOfferService");

                var errorResponse = Json.Deserialize<DeltaErrorResponse>(await ((HttpResponseException)ex.InnerException).Response.Content.ReadAsStringAsync(CancellationToken.None));
                return Result.Failure<Response<OfferFareRules>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException("ErrorCodes.DeltaServicesErrorCode", AsyncContext.Current.CorrelationId.ToString(), "ErrorCodes.ErrorCodePath", Json.Serialize(errorResponse))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} request terminated while calling DeltaOfferService");
                return Result.Failure<Response<OfferFareRules>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}