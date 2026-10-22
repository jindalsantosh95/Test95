using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service
{
    public sealed class FlightsService(IHttpConnector httpConnector, ILogger<FlightsService> logger) : IFlightsService
    {
        private const string SOAPActionHeader = "SOAPAction";
        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
        private const string OcpApimSubscriptionKeyHeaderValue = "Aims:APIM:OcpSubscriptionKey";
        private const string SOAPActionHeaderValueFlightDetailsX1 = "http://tempuri.org/IAIMSWebService/FlightDetailsX1";
        private const string SOAPActionHeaderValueFlightDetailsForPeriod = "http://tempuri.org/IAIMSWebService/FlightDetailsForPeriod";
        private const string SOAPActionHeaderValueFetchLegMembers = "http://tempuri.org/IAIMSWebService/FetchLegMembers";
        private const string ContentTypeHeaderValue = "text/xml";

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ILogger<FlightsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly string _serviceUrl = ConfigReader.Get("Aims:APIM:Url");
        private readonly string _username = ConfigReader.Get("Aims:Username");
        private readonly string _password = ConfigReader.Get("Aims:Password");

        public async Task<Result<Response<IEnumerable<FlightDetail>>>> DetailAsync(FlightDetailRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl)
            {
                Content = new StringContent(Common.Serialisation.Xml.Serialize(new Infrastructure.Aims.Request.Envelope()
                {
                    Body = new()
                    {
                        FlightDetailsX1 = new()
                        {
                            UN = _username,
                            PSW = _password,
                            PM = new Infrastructure.Aims.Request.FlightDetailsX1PM()
                            {
                                DepArrMode = 0,

                                FromDD = request.From.Day,
                                FromMonth = request.From.Month,
                                FromYYYY = request.From.Year,
                                FromHH = request.From.Hour,
                                FromMin = request.From.Minute,

                                ToDD = request.To.Day,
                                ToMonth = request.To.Month,
                                ToYYYY = request.To.Year,
                                ToHH = request.To.Hour,
                                ToMin = request.To.Minute,
                            }
                        }
                    }
                }), System.Text.Encoding.UTF8, ContentTypeHeaderValue)
            };

            httpRequestMessage.Headers.Add(SOAPActionHeader, SOAPActionHeaderValueFlightDetailsX1);
            httpRequestMessage.Headers.Add(OcpApimSubscriptionKeyHeader, ConfigReader.Get(OcpApimSubscriptionKeyHeaderValue));

            var response = new Response<IEnumerable<FlightDetail>>() { Status = Status.OK };

            var isAimsResponseError = false;

            try
            {
                var envelopeResponse = await _httpConnector.SendAsync<Envelope>(httpRequestMessage, CancellationToken.None);

                if (envelopeResponse?.Body?.FlightDetailsX1Response?.FlightDetailsX1Result is null)
                    throw new InvalidOperationException("Invalid response.");

                if (!envelopeResponse.Body.FlightDetailsX1Response.FlightDetailsX1Result.ErrorExplanation.IsNullOrWhiteSpace())
                {
                    isAimsResponseError = true;
                    throw new InvalidOperationException(envelopeResponse.Body.FlightDetailsX1Response.FlightDetailsX1Result.ErrorExplanation);
                }

                response.Value = [.. ((FlightDetailResponse)envelopeResponse.Body.FlightDetailsX1Response).Flights.Where(x => x.Cancelled.Is("0"))];
            }
            catch (Exception ex)
            {
                RemoveSensitiveHttpConnectorData(ex);
                _logger.LogError(GetSafeLoggingException(ex, isAimsResponseError), "Flight detail lookup failed from {From} to {To}.", request.From, request.To);
                response.Status = Status.Error;
                response.ErrorMessage = ex.Message;
                response.Errors = [
                    new ErrorDetails()
                    {
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message                         ,
                        DeveloperMessage =  (ex is HttpResponseException exception) ? await exception.Response.Content.ReadAsStringAsync(CancellationToken.None) : null
                    }
                ];
            }

            return Result.Success(response);
        }

        public async Task<Result<Response<IEnumerable<FlightDetail>>>> ScheduleAsync(FlightScheduleRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl)
            {
                Content = new StringContent(Common.Serialisation.Xml.Serialize(new Infrastructure.Aims.Request.Envelope()
                {
                    Body = new()
                    {
                        FlightDetailsForPeriod = new()
                        {
                            UN = _username,
                            PSW = _password,

                            FromDD = request.From.Day.ToString(),
                            FromMMonth = request.From.Month.ToString(),
                            FromYYYY = request.From.Year.ToString(),
                            FromHH = request.From.Hour.ToString(),
                            FromMMin = request.From.Minute.ToString(),

                            ToDD = request.To.Day.ToString(),
                            ToMMonth = request.To.Month.ToString(),
                            ToYYYY = request.To.Year.ToString(),
                            ToHH = request.To.Hour.ToString(),
                            ToMMin = request.To.Minute.ToString(),
                        }
                    }
                }), System.Text.Encoding.UTF8, ContentTypeHeaderValue)
            };

            httpRequestMessage.Headers.Add(SOAPActionHeader, SOAPActionHeaderValueFlightDetailsForPeriod);
            httpRequestMessage.Headers.Add(OcpApimSubscriptionKeyHeader, ConfigReader.Get(OcpApimSubscriptionKeyHeaderValue));

            var response = new Response<IEnumerable<FlightDetail>>() { Status = Status.OK };

            var isAimsResponseError = false;

            try
            {
                var envelopeResponse = await _httpConnector.SendAsync<Envelope>(httpRequestMessage, CancellationToken.None);

                if (envelopeResponse?.Body?.FlightDetailsForPeriodResponse?.FlightDetailsForPeriodResult is null)
                    throw new InvalidOperationException("Invalid response.");

                if (!envelopeResponse.Body.FlightDetailsForPeriodResponse.FlightDetailsForPeriodResult.ErrorExplanation.IsNullOrWhiteSpace())
                {
                    isAimsResponseError = true;
                    throw new InvalidOperationException(envelopeResponse.Body.FlightDetailsForPeriodResponse.FlightDetailsForPeriodResult.ErrorExplanation);
                }

                response.Value = [.. ((FlightScheduleResponse)envelopeResponse.Body.FlightDetailsForPeriodResponse).Flights];
            }
            catch (Exception ex)
            {
                RemoveSensitiveHttpConnectorData(ex);
                _logger.LogError(GetSafeLoggingException(ex, isAimsResponseError), "Flight schedule lookup failed from {From} to {To}.", request.From, request.To);
                response.Status = Status.Error;
                response.ErrorMessage = ex.Message;
                response.Errors = [
                    new ErrorDetails()
                    {
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message                         ,
                        DeveloperMessage =  (ex is HttpResponseException exception) ? await exception.Response.Content.ReadAsStringAsync(CancellationToken.None) : null
                    }
                ];
            }

            return Result.Success(response);
        }

        public async Task<Result<Response<IEnumerable<FlightCrewMember>>>> RosterAsync(FlightCrewRosterRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _serviceUrl)
            {
                Content = new StringContent(Common.Serialisation.Xml.Serialize(new Infrastructure.Aims.Request.Envelope()
                {
                    Body = new()
                    {
                        FetchLegMembers = new()
                        {
                            UN = _username,
                            PSW = _password,
                            DD = request.FlightDate.Day,
                            MM = request.FlightDate.Month,
                            YY = request.FlightDate.Year,
                            DEP = request.Departure,
                            Flight = request.FlightNo.ToString()
                        }
                    }
                }), System.Text.Encoding.UTF8, ContentTypeHeaderValue)
            };

            httpRequestMessage.Headers.Add(SOAPActionHeader, SOAPActionHeaderValueFetchLegMembers);
            httpRequestMessage.Headers.Add(OcpApimSubscriptionKeyHeader, ConfigReader.Get(OcpApimSubscriptionKeyHeaderValue));

            var response = new Response<IEnumerable<FlightCrewMember>>() { Status = Status.OK };

            var isAimsResponseError = false;

            try
            {
                var envelopeResponse = await _httpConnector.SendAsync<Envelope>(httpRequestMessage, CancellationToken.None);

                if (envelopeResponse?.Body?.FetchLegMembersResponse?.FetchLegMembersResult is null)
                    throw new InvalidOperationException("Invalid response.");

                if (!envelopeResponse.Body.FetchLegMembersResponse.FetchLegMembersResult.ErrorExplanation.IsNullOrWhiteSpace())
                {
                    isAimsResponseError = true;
                    throw new InvalidOperationException(envelopeResponse.Body.FetchLegMembersResponse.FetchLegMembersResult.ErrorExplanation);
                }

                response.Value = [.. ((FlightCrewRosterResponse)envelopeResponse.Body.FetchLegMembersResponse).Crew];
            }
            catch (Exception ex)
            {
                RemoveSensitiveHttpConnectorData(ex);
                _logger.LogError(GetSafeLoggingException(ex, isAimsResponseError), "Flight crew roster lookup failed for {FlightNumber} on {FlightDate} from {DepartureAirportCode}.", request.FlightNo, request.FlightDate, request.Departure);
                response.Status = Status.Error;
                response.ErrorMessage = ex.Message;
                response.Errors = [
                    new ErrorDetails()
                    {
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message                         ,
                        DeveloperMessage =  (ex is HttpResponseException exception) ? await exception.Response.Content.ReadAsStringAsync(CancellationToken.None) : null
                    }
                ];
            }

            return Result.Success(response);
        }

        private static void RemoveSensitiveHttpConnectorData(Exception exception)
        {
            exception.Data.Remove("Request");
            exception.Data.Remove("Response");
        }

        private static Exception GetSafeLoggingException(Exception exception, bool isAimsResponseError)
        {
            return isAimsResponseError
                ? new InvalidOperationException("AIMS returned an error response.")
                : exception;
        }
    }
}
