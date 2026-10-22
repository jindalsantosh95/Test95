using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using VAA.Facade.AzureB2C.Infrastructure.Proxy;
using VAA.Facade.AzureB2C.Shared;
using System.Net;
using VAA.EAPI.Common.Serialisation;

namespace VAA.Facade.AzureB2C.Infrastructure.Services
{
    public class EnrolmentServiceQueryHandler : IRequestHandler<EnrolmentServiceQuery, KeyValuePair<bool, EnrolmentServiceResponse>>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EnrolmentServiceQueryHandler> _logger;
        private int numberOfAttempts;

        public EnrolmentServiceQueryHandler(IHttpClientFactory httpClientFactory, ILogger<EnrolmentServiceQueryHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<KeyValuePair<bool, EnrolmentServiceResponse>> Handle(EnrolmentServiceQuery enrolment, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Customer Enrolment Service handler");

            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage httpResponse;

            try
            {
                //Prepare httpRequest for enrolment service.
                var jsonContent = Json.Serialize(enrolment.EnrolmentServiceRequest);
                var buffer = Encoding.UTF8.GetBytes(jsonContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                byteContent.Headers.Add("channelId", Constants.EnrolmentChannelId);
                bool tokenResetRequired = true;
                
                string baseURL = ReadConfig.GetConfigurationSettings(Constants.EnrolmentApiBaseURL);
                do
                {
                    var token = await AzureADHelper.GetBearerToken(tokenResetRequired);
                    if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    if (!httpClient.DefaultRequestHeaders.Contains(Constants.apimSubscription))
                    {
                        if (Constants.EnrolmentSubscriptionNameValueCached == null)
                        {
                            Constants.EnrolmentSubscriptionNameValueCached = ReadConfig.GetConfigurationSettings(Constants.EnrolmentSubscriptionNameValue);
                        }
                        httpClient.DefaultRequestHeaders.Add(Constants.apimSubscription, Constants.EnrolmentSubscriptionNameValueCached);
                    }
                    httpClient.BaseAddress = new Uri(baseURL);
                    _logger.LogInformation("Customer Enrolment Service API Http Client Created", null);


                    httpResponse = await httpClient.PostAsync(GetRequestUri(), byteContent);

                    _logger.LogInformation("Customer Enrolment Service API successfully response received", null);
                    string responseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                    numberOfAttempts++;
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        tokenResetRequired = true;
                        httpClient.DefaultRequestHeaders.Remove(Constants.authorizationHeader);
                    }
                    if (!httpResponse.IsSuccessStatusCode || responseMessage.Contains("Error", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Exception exResponse = new Exception(responseMessage);
                        _logger.LogInformation(exResponse, "Customer Enrolment Service API Error Response", null);
                        return new KeyValuePair<bool, EnrolmentServiceResponse>(false, new EnrolmentServiceResponse() { EnrolmentFailedData = Json.DeserializeObject<List<EnrolmentFailureData>>(responseMessage) });
                    }
                    else
                    {
                        _logger.LogInformation("Customer Enrolment Service API Success Response");
                        return new KeyValuePair<bool, EnrolmentServiceResponse>(true, new EnrolmentServiceResponse() { EnrolmentData = Json.DeserializeObject<EnrolmentSuccessData>(responseMessage) });
                    }

                } while (numberOfAttempts < 3 && httpResponse.StatusCode == HttpStatusCode.Unauthorized);

            }
            catch (Exception execption)
            {
                _logger.LogError(execption, "Enrolment Service Query Handler Failed", null);                
                return new KeyValuePair<bool, EnrolmentServiceResponse>(false, new EnrolmentServiceResponse()
                { 
                    EnrolmentFailedData = new List<EnrolmentFailureData>() 
                    { 
                        new EnrolmentFailureData() 
                        { 
                            Code = "E001", ErrorSource = execption.Source, Message = execption.Message, DeveloperMessage = execption.StackTrace
                        }
                    }
                });

            }
        }

        private string GetRequestUri()
        {
            var enrolmentApiuri = ReadConfig.GetConfigurationSettings(Constants.EnrolmentApiURI);
            return enrolmentApiuri;
        }
    }
}
