using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD
{
    public class AuthorizePaymentCommandHandler : IRequestHandler<AuthorizePaymentCommand, Result<Response<Domain.CPD.PaymentResponse>>>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ILogger<AuthorizePaymentCommandHandler> _logger;
        private readonly IAzureBlobService _azureBlobService;
        private readonly string _correlationId;

        public AuthorizePaymentCommandHandler(IMediator mediator, ILogger<AuthorizePaymentCommandHandler> logger, IHttpConnector httpConnector, IAzureBlobService azureBlobService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<Result<Response<Domain.CPD.PaymentResponse>>> Handle(AuthorizePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string transactionId = string.Empty;

                var countryList = await _azureBlobService.BlobJsonAsAsync<IEnumerable<CountryId>>(ConfigReader.Get(Constants.Environment.CpdCountryBlob), cancellationToken);

                var initializeRQ = Xml.Serialize(InitializePaymentMapper.InitializePaymentRequestMapper(request.AuthorizePaymentRequest, countryList));
                var initializeDocument = new XmlDocument();
                initializeDocument.LoadXml(initializeRQ);

                var cpdResponse = new Domain.CPD.PaymentResponse();

                var initializePaymentResponse = await _mediator.Send(new InitializePaymentCommand() { InitializePaymentXMLRequest = initializeDocument?.InnerXml }, cancellationToken);

                if (initializePaymentResponse.Value.Status == EAPI.Domain.Entities.Common.Status.OK)
                {
                    if (initializePaymentResponse.Value.Value.Transaction != null)
                    {
                        transactionId = initializePaymentResponse.Value.Value.Transaction.Id;
                        var authorizationRequest = AuthorizePaymentMapper.AuthorizePaymentRequest(request.AuthorizePaymentRequest.PaymentDetails.FirstOrDefault(), initializePaymentResponse, countryList, request.AuthorizePaymentRequest.Pos.CountryCode);
                        var authorizeRQ = Xml.Serialize(new Infrastructure.Models.CPD.AuthorizePaymentRequest() { Authorizepayment = authorizationRequest });
                        var authorizeDocument = new XmlDocument();
                        authorizeDocument.LoadXml(authorizeRQ);

                        var authorizePaymentResponse = await _mediator.Send(new Infrastructure.Services.CPD.AuthorizePaymentCommand() { AuthorizePaymentXMLRequest = authorizeDocument?.InnerXml }, cancellationToken);

                        cpdResponse = PaymentResponseMapper.CPDPaymentResponseMapper(initializePaymentResponse.Value.Value, authorizePaymentResponse.Value.Value);

                        #region Add payment document into cosmos db

                        CreatePaymentDocumentRequest paymentDocRequest = PaymentDocumentRequestMapper.CreateRequestMapper(request.AuthorizePaymentRequest, cpdResponse, transactionId);
                        await _mediator.Send(new CreatePaymentDocRepoCommand(paymentDocRequest));

                        #endregion
                    }
                }

                if (initializePaymentResponse.Value.Status == EAPI.Domain.Entities.Common.Status.BadRequest)
                    cpdResponse = PaymentResponseMapper.CPDPaymentResponseMapper(initializePaymentResponse.Value.Value, null);

                if (cpdResponse.Errors == null)
                    return new Response<Domain.CPD.PaymentResponse>() { Status = EAPI.Domain.Entities.Common.Status.OK, Value = cpdResponse };
                else               
                    return new Response<Domain.CPD.PaymentResponse>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest, Errors = cpdResponse.Errors.ToList() };               
            }
            catch (CosmosDBException ex)
            {
                _logger.LogError(ex, $"Error occurred in CPD Authorize PaymentMS call while creating Payment Document - {ex.Message}");
                return Result.Failure<VAA.EAPI.Domain.Entities.Common.Response<Domain.CPD.PaymentResponse>>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in CPD Authorize PaymentMS call - {ex.Message}");

                var errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.DefaultTechErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                return Result.Failure<Response<Domain.CPD.PaymentResponse>>(Json.Serialize(errors));
            }

        }
    }

}
