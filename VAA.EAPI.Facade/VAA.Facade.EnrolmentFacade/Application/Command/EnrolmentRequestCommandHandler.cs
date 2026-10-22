using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Domain;
using VAA.Facade.EnrolmentFacade.Helper;
using VAA.Facade.EnrolmentFacade.Helper.Serialisation;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;
using VAA.Facade.EnrolmentFacade.Infrastructure.Services;
using VAA.Facade.EnrolmentFacade.Domain.CustomerDetails;
using VAA.Facade.EnrolmentFacade.Domain.Events;
using VAA.Facade.EnrolmentFacade.Mapper;

namespace VAA.Facade.EnrolmentFacade.Application.Command
{
    public class EnrolmentRequestCommandHandler : IEnrolmentRequestCommandHandler
    {
        private readonly ILogger<EnrolmentRequestCommandHandler> _logger;
        private readonly IHttpServiceHandler _httpServiceHandler;
        private readonly IPnrLinkServiceHandler _pnrLinkServiceHandler;
        private readonly IOAuthTokenProvider _tokenProvider;


        public EnrolmentRequestCommandHandler(
            IHttpServiceHandler httpServiceHandler,
            IPnrLinkServiceHandler pnrLinkServiceHandler,
            ILogger<EnrolmentRequestCommandHandler> logger,
            IOAuthTokenProvider tokenProvider)
        {
            _httpServiceHandler = httpServiceHandler ?? throw new ArgumentNullException(nameof(httpServiceHandler));
            _pnrLinkServiceHandler = pnrLinkServiceHandler ?? throw new ArgumentNullException(nameof(pnrLinkServiceHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public async Task<Result<Response<EnrolmentResponse>>> Handle(EnrolmentRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Facade Handler");
                var enrolmentContent = RequestMapper.RequestDataMapper(request.EnrolmentRequest);

                Response<EnrolmentResponse> enrolResponse = new Response<EnrolmentResponse>();
                string ocpApimSubscriptionKey = ConfigReader.GetConfigurationSettings(Constants.Constants.RetrieveEnrolmentOcpKey);
                var retryCounter = int.Parse(ConfigReader.GetConfigurationSettings(Constants.Environment.RetryAttempt));


                string url = ConfigReader.GetConfigurationSettings(Constants.Constants.RetrieveCustomerEndpoint);
                string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.GetConfigurationSettings(Constants.Constants.RetrieveCustomerApiResource));

                List<KeyValuePair<string, string>> responseHeaders = new()
                    {
                        new KeyValuePair<string, string>("Authorization", $"Bearer {token}"),
                        new KeyValuePair<string, string>("Ocp-Apim-Subscription-Key", ocpApimSubscriptionKey),
                        new KeyValuePair<string, string>("channelId", request.ChannelId),
                        new KeyValuePair<string, string>("channelUserId", request.ChannelUserId),
                    };

                bool enrolmentSucceeded;

                using (var responsePost = await SendEnrolmentRequestAsync(request, enrolmentContent, url, responseHeaders, retryCounter, cancellationToken))
                {
                    enrolmentSucceeded = responsePost.IsSuccessStatusCode;
                    string returnVal = await responsePost.Content.ReadAsStringAsync(cancellationToken);

                    MapEnrolmentResponse(enrolResponse, responsePost, returnVal);
                }

                var preferences = request.EnrolmentRequest.CustomerDetails.Preferences ?? Enumerable.Empty<Preferences>();
                bool hasFfpPreference = preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypeEnum.PreferredPartnerFFP && !string.IsNullOrEmpty(x.PreferenceValue));

                if (enrolmentSucceeded && enrolResponse?.Value != null && !hasFfpPreference)
                    await _pnrLinkServiceHandler.Handle(new PnrLinkService((CustomerEnrolmentDetails)(request, enrolResponse?.Value?.Memberdetails?.MemberNumber)), cancellationToken);

                return enrolResponse;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception - {ex}");

                return new Response<EnrolmentResponse>()
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>() { new() { Code = "E001", DeveloperMessage = ex.StackTrace, ErrorSource = ex.Source, Message = ex.Message } }

                };
            }
        }

        private async Task<HttpResponseMessage> SendEnrolmentRequestAsync(
            EnrolmentRequestCommand request,
            EventGridTopicEvent enrolmentContent,
            string url,
            List<KeyValuePair<string, string>> responseHeaders,
            int retryCounter,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                var responsePost = await _httpServiceHandler.Handle(new HttpServiceCommand(request, enrolmentContent, url, responseHeaders, HttpMethod.Post), cancellationToken);
                retryCounter--;

                if (retryCounter <= 0 || !ShouldRetry(responsePost))
                {
                    return responsePost;
                }

                responsePost.Dispose();
            }
        }

        private static bool ShouldRetry(HttpResponseMessage responsePost)
        {
            return responsePost.StatusCode == HttpStatusCode.UnprocessableEntity
                || responsePost.StatusCode == HttpStatusCode.RequestTimeout
                || responsePost.StatusCode == HttpStatusCode.TooManyRequests
                || (int)responsePost.StatusCode >= 500;
        }

        private static void MapEnrolmentResponse(Response<EnrolmentResponse> enrolResponse, HttpResponseMessage response, string returnVal)
        {
            if (response.IsSuccessStatusCode)
            {
                enrolResponse.Value = Json.DeserializeObject<EnrolmentResponse>(returnVal);
            }
            else if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                enrolResponse.Errors = Json.DeserializeObject<List<ErrorDetails>>(returnVal);
                enrolResponse.Status = Status.BusinessException;
            }
            else
            {
                enrolResponse.Errors = Json.DeserializeObject<List<ErrorDetails>>(returnVal);
                enrolResponse.Status = Status.Error;
            }
        }
    }
}
