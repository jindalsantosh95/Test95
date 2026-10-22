using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Mapper;
using QueueAccessProxy = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories
{
    public class DuplicatePnrRepoCommandHandler : IDuplicatePnrRepoCommand
    {
        private readonly ILogger<DuplicatePnrRepoCommandHandler> _logger;
        private readonly CreateSessionCommandHandler _createSessionCommandHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        private readonly IQueueAccessCommand _queueAccessCommand;

        public DuplicatePnrRepoCommandHandler(ILogger<DuplicatePnrRepoCommandHandler> logger, CreateSessionCommandHandler createSessionCommandHandler, SessionCloseCommandHandler sessionCloseCommandHandler, IQueueAccessCommand queueAccessCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createSessionCommandHandler = createSessionCommandHandler ?? throw new ArgumentNullException(nameof(createSessionCommandHandler));
            _sessionCloseCommandHandler = sessionCloseCommandHandler ?? throw new ArgumentNullException(nameof(sessionCloseCommandHandler));
            _queueAccessCommand = queueAccessCommand ?? throw new ArgumentNullException(nameof(queueAccessCommand));
        }

        public async Task<Result<Response<DuplicatePnrResponse>>> Handle(DuplicatePnrRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null) { throw new ArgumentNullException(nameof(request)); }
                var pcc = request.PCC;

                var token = await _createSessionCommandHandler.Handle(new CreateSessionCommand { PCC = pcc }, cancellationToken);
                if (token == null)
                    throw new Exception("Session creation failed.");

                var queueAccessResponse = new EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess.QueueAccessRQResponse();
                var rlocs = Array.Empty<string>();
                string? queueAccessFailureReason = null;
                var sessionCloseFailed = false;
                try
                {
                    var queueAccessRequest = QueueAccessRequestMapper.Map(token, pcc);
                    queueAccessResponse = await _queueAccessCommand.QueueAccess(queueAccessRequest);
                    queueAccessFailureReason = GetQueueAccessFailureReason(queueAccessResponse);

                    if (queueAccessFailureReason == null)
                    {
                        rlocs = queueAccessResponse.QueueAccessRS?.Line?.Select(a => a.UniqueID.ID).ToArray() ?? Array.Empty<string>();
                        LogQueueFetch(pcc, rlocs);
                    }
                }
                finally
                {
                    try
                    {
                        var sessionClosed = await _sessionCloseCommandHandler.Handle(new SessionCloseCommand { BinaryToken = token, PCC = request.PCC }, cancellationToken);
                        if (!sessionClosed)
                        {
                            _logger.LogError(
                                "DuplicatePNR QueueFetch session close failed. PCC={PCC}, Queue={Queue}, Reason={Reason}",
                                pcc,
                                Constants.Application.QueueNumber,
                                "Session close was not approved.");
                            sessionCloseFailed = true;
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            "DuplicatePNR QueueFetch session close failed. PCC={PCC}, Queue={Queue}, Reason={Reason}, ExceptionType={ExceptionType}",
                            pcc,
                            Constants.Application.QueueNumber,
                            "SessionCloseFailed",
                            ex.GetType().Name);
                        sessionCloseFailed = true;
                    }
                }

                if (queueAccessFailureReason != null)
                {
                    _logger.LogError(
                        "DuplicatePNR QueueFetch failed. PCC={PCC}, Queue={Queue}, Reason={Reason}",
                        pcc,
                        Constants.Application.QueueNumber,
                        queueAccessFailureReason);
                    return Result.Failure<Response<DuplicatePnrResponse>>(Json.Serialize(VAA.EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }

                if (sessionCloseFailed)
                    return Result.Failure<Response<DuplicatePnrResponse>>(Json.Serialize(VAA.EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));

                var response = new Response<DuplicatePnrResponse>()
                {
                    Status = Status.OK,
                    Value = new DuplicatePnrResponse()
                    {
                        Rlocs = rlocs
                    }
                };

                return Result.Success(response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(
                    "DuplicatePNR QueueFetch failed. PCC={PCC}, Queue={Queue}, Reason={Reason}",
                    request?.PCC ?? string.Empty,
                    Constants.Application.QueueNumber,
                    SanitiseSabreError(ex.Message));
                return Result.Failure<Response<DuplicatePnrResponse>>(Json.Serialize(VAA.EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "DuplicatePNR QueueFetch failed. PCC={PCC}, Queue={Queue}, Reason={Reason}, ExceptionType={ExceptionType}",
                    request?.PCC ?? string.Empty,
                    Constants.Application.QueueNumber,
                    "QueueFetchFailed",
                    ex.GetType().Name);
                return Result.Failure<Response<DuplicatePnrResponse>>(Json.Serialize(VAA.EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        private static string SanitiseSabreError(string? error)
        {
            var singleLineError = (error ?? string.Empty).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
            return singleLineError.Length <= Constants.Application.MaxSabreErrorMessageLength
                ? singleLineError
                : $"{singleLineError[..Constants.Application.MaxSabreErrorMessageLength]}...";
        }

        private static string? GetQueueAccessFailureReason(QueueAccessProxy.QueueAccessRQResponse queueAccessResponse)
        {
            var applicationResults = queueAccessResponse?.QueueAccessRS?.ApplicationResults;
            if (applicationResults == null)
                return "QueueAccess did not return ApplicationResults.";

            if (applicationResults.status != QueueAccessProxy.CompletionCodes.Complete)
                return $"QueueAccess failed. Status: {applicationResults.status}.";

            var sabreErrorCount = applicationResults.Error?.Length ?? 0;
            return sabreErrorCount > 0
                ? $"QueueAccess returned {sabreErrorCount} error(s)."
                : null;
        }

        private void LogQueueFetch(string pcc, string[] rlocs)
        {
            _logger.LogInformation(
                "DuplicatePNR QueueFetch completed. PCC={PCC}, Queue={Queue}, PnrCount={PnrCount}",
                pcc,
                Constants.Application.QueueNumber,
                rlocs.Length);

            foreach (var rloc in rlocs)
            {
                _logger.LogInformation(
                    "DuplicatePNR QueueFetch item. PCC={PCC}, Queue={Queue}, PNR={PNR}",
                    pcc,
                    Constants.Application.QueueNumber,
                    rloc);
            }
        }
    }
}
