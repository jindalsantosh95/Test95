using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Book.Requests;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Mapper;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Domain.Book.Handler
{
    public class OrderConfirmationRequestHandler : IRequestHandler<OrderConfirmationRequest, Response<ConfirmationResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderConfirmationRequestHandler> _logger;
        public OrderConfirmationRequestHandler(IMediator mediator, ILogger<OrderConfirmationRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Response<ConfirmationResponse>> Handle(OrderConfirmationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.BookCreateOrderPqId),
                    Variables = JObject.Parse(Json.Serialize(new { request = request.Request}, false)),
                    Headers = Helper.Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                var mapperResponse = CreateOrderMapper.MapOrderCreateResponse(Json.DeserializeObject<ConfirmationResponse>(graphQLResponse.Data["bookCreateOrder"]));
     
                if (mapperResponse.Errors.IsNullOrEmpty())
                {
                    return new Response<ConfirmationResponse>()
                    {
                        Status = Status.OK,
                        Value = mapperResponse,
                    };
                }
                else
                {
                    var confirmationError = mapperResponse.Errors.FirstOrDefault();

                    if (confirmationError.Code == Constants.Application.FraudCheckErrorCode)
                    {
                        return new Response<ConfirmationResponse>()
                        {
                            Status = Status.PreConditionFailure,
                            Errors = ErrorMapper.MapDeltaExceptionToEAPIException(confirmationError.Code, AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath, confirmationError.Message),
                            IgnoreNullValues = true
                        };
                    }
                    else
                    {
                        return new Response<ConfirmationResponse>()
                        {
                            Status = Status.BusinessException,
                            Errors = ErrorMapper.MapDeltaExceptionToEAPIException(confirmationError.Code, AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath, confirmationError.Message),
                            IgnoreNullValues = true
                        };
                    }

                }

            }
            catch (GraphQLException gqlException)
            {
                _logger.LogError(gqlException, $"Error in OrderConfirmationHandler: {Json.Serialize(gqlException.Errors)},BasketId: {request.Request.BasketId}, CorrelationId: {AsyncContext.Current.CorrelationId}");
                
                var status = GraphQLExceptionStatus.GetStatusCode(gqlException, true);

                var errorResponse = gqlException.Errors.FirstOrDefault().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(gqlException.Errors.FirstOrDefault().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(gqlException.Errors?.FirstOrDefault()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return new Response<ConfirmationResponse>()
                {
                    Status = status,
                    Errors = errorResponse.ToList(),
                    IgnoreNullValues = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderConfirmationHandler: {ex.Message},BasketId: {request.Request.BasketId}, CorrelationId: {AsyncContext.Current.CorrelationId}");
                throw;
            }
        }
    }
}
