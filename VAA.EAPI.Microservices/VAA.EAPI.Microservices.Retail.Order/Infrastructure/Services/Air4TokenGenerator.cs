using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Constants;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services
{
    public sealed class Air4TokenGenerator : IAir4TokenGenerator
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static readonly ConcurrentDictionary<string, TokenResponse> cache = new();

        private readonly ILogger<Air4TokenGenerator> _logger;
        private readonly IHttpConnector _apiConnector;

        public Air4TokenGenerator(ILogger<Air4TokenGenerator> logger, IHttpConnector apiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<Result<Response<string>>> GetToken(string clientId, string clientSecret)
        {
            var switchToAws = ConfigReader.Try<bool>(Constants.Environment.SwitchToAws, false);

            if (cache.ContainsKey(clientId) && (switchToAws ? cache[clientId].Access_token.IsJwtTokenValid() : IsAir4TokenValid(cache[clientId]))) return Result.Success(new Response<string>
            {
                Value = cache[clientId].Access_token,
                Status = Status.OK
            });

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (cache.ContainsKey(clientId) && (switchToAws ? cache[clientId].Access_token.IsJwtTokenValid() : IsAir4TokenValid(cache[clientId]))) return Result.Success(new Response<string>
                {
                    Value = cache[clientId].Access_token,
                    Status = Status.OK
                });                

                var tokenRequest = new Dictionary<string, string>
                {
                    { Constants.Application.ClientId, clientId },
                    { Constants.Application.ClientSecret, clientSecret },
                    { Constants.Application.GrantType, Constants.Application.ClientCredentials }
                };

                if(switchToAws)
                {
                    tokenRequest.Add(Constants.Application.Scope, Constants.Application.Read);
                }

                var httpResponseMessage = await _apiConnector.PostAsync(switchToAws ? ConfigReader.Try(Constants.Environment.DlAwsOrderAuthenticationUrl) : ConfigReader.Get(Constants.Environment.DlOfferAuthenticationUrl), new FormUrlEncodedContent(tokenRequest));

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var content = await httpResponseMessage.Content.ReadAsStringAsync();
                    var tokenResponse = switchToAws ? Json.Deserialize<TokenResponse>(content) : Xml.Deserialize<TokenResponse>(content);

                    cache.AddOrUpdate(clientId, tokenResponse, (k, v) => tokenResponse);
                }
                else if(!switchToAws)
                {                    
                    var error = Xml.Deserialize<DeltaErrorResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, string.Concat(error.Code, error.Message, error.DeveloperMessage))));
                }
                else
                {
                    var error = Json.Deserialize<DeltaAWSTokenErrorResponse>(await httpResponseMessage.Content.ReadAsStringAsync());

                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, string.Concat(error.error_description, error.error))));
                }

                var hasToken = cache.ContainsKey(clientId) && (switchToAws ? cache[clientId].Access_token.IsJwtTokenValid() : IsAir4TokenValid(cache[clientId]));

                return Result.Success(new Response<string>
                {
                    Value = hasToken ? cache[clientId].Access_token : string.Empty,
                    Status = hasToken ? Status.OK : Status.Error
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id: {AsyncContext.Current.CorrelationId}. " +
                            $"Error Occured while receiving token for DeltaService. Error Response: " +
                            $"{Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message))}");

                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            
        }

        private static bool IsAir4TokenValid(TokenResponse source) => DateTime.UtcNow < DateTimeOffset.FromUnixTimeMilliseconds(source.Issued_at.Parse<long>()).AddSeconds(source.Expires_in.Parse<double>());
    }
}
