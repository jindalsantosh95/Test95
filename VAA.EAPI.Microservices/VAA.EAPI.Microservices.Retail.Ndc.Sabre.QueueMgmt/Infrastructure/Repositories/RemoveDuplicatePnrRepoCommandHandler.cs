using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories
{
    public class RemoveDuplicatePnrRepoCommandHandler : IRemoveDuplicatePnrRepoCommand
    {
        private readonly ILogger<RemoveDuplicatePnrRepoCommandHandler> _logger;
        private readonly CreateSessionCommandHandler _createSessionCommandHandler;
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly IgnoreTransactionCommandHandler _ignoreTransactionCommandHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        private readonly IQueueAccessCommand _queueAccessCommand;

        public RemoveDuplicatePnrRepoCommandHandler(
            ILogger<RemoveDuplicatePnrRepoCommandHandler> logger,
            CreateSessionCommandHandler createSessionCommandHandler,
            GetReservationQueryHandler getReservationQueryHandler,
            IgnoreTransactionCommandHandler ignoreTransactionCommandHandler,
            SessionCloseCommandHandler sessionCloseCommandHandler,
            IQueueAccessCommand queueAccessCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createSessionCommandHandler = createSessionCommandHandler ?? throw new ArgumentNullException(nameof(createSessionCommandHandler));
            _getReservationQueryHandler = getReservationQueryHandler ?? throw new ArgumentNullException(nameof(getReservationQueryHandler));
            _ignoreTransactionCommandHandler = ignoreTransactionCommandHandler ?? throw new ArgumentNullException(nameof(ignoreTransactionCommandHandler));
            _sessionCloseCommandHandler = sessionCloseCommandHandler ?? throw new ArgumentNullException(nameof(sessionCloseCommandHandler));
            _queueAccessCommand = queueAccessCommand ?? throw new ArgumentNullException(nameof(queueAccessCommand));
        }

        public async Task<Result<Response<RemoveDuplicatePnrResponse>>> Handle(RemoveDuplicatePnrRequest request, CancellationToken cancellationToken)
        {
            string token = null;
            try
            {
                if (request == null) { throw new ArgumentNullException(nameof(request)); }

                var pcc = request.PCC;
                var pnr = request.Pnr;

                token = await _createSessionCommandHandler.Handle(new CreateSessionCommand { PCC = pcc }, cancellationToken);
                if (token == null)
                    throw new Exception("Sabre session token was null.");

                var getReservationResponse = await _getReservationQueryHandler.Handle(
                    new GetReservationQuery(GetReservationRequestMapper.Map(token, pnr)),
                    cancellationToken);

                if (getReservationResponse?.Errors?.Length > 0 ||
                    getReservationResponse?.Item is not VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.ReservationPNRB)
                    throw new Exception($"Sabre GetReservation did not load target PNR {pnr}.");

                var queueRemoveResponse = await _queueAccessCommand.QueueAccess(
                    QueueAccessRequestMapper.Map(
                        token,
                        pcc,
                        listInd: false,
                        navigationAction: QueueAccessRQNavigationAction.QR,
                        returnHostCommand: true));

                EnsureQueueAccessSucceeded(queueRemoveResponse);

                var ignored = await _ignoreTransactionCommandHandler.Handle(
                    new IgnoreTransactionCommand { BinarySecurityToken = token, PCC = pcc },
                    cancellationToken);

                if (!ignored)
                    throw new Exception("Sabre IgnoreTransaction did not complete.");

                _logger.LogInformation("DuplicatePNR QueueRemoval completed. PNR={PNR}", pnr);

                return Result.Success(new Response<RemoveDuplicatePnrResponse>
                {
                    Status = Status.OK,
                    Value = new RemoveDuplicatePnrResponse { Removed = true }
                });
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(
                    "DuplicatePNR QueueRemoval failed. PCC={PCC}, Queue={Queue}, PNR={PNR}, Reason={Reason}",
                    request?.PCC ?? string.Empty,
                    Constants.Application.QueueNumber,
                    request?.Pnr ?? string.Empty,
                    SanitiseSabreError(ex.Message));
                return Result.Failure<Response<RemoveDuplicatePnrResponse>>(
                    Json.Serialize(VAA.EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(
                        VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError,
                        Guid.NewGuid().ToString(),
                        VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "DuplicatePNR QueueRemoval failed. PCC={PCC}, Queue={Queue}, PNR={PNR}, Reason={Reason}, ExceptionType={ExceptionType}",
                    request?.PCC ?? string.Empty,
                    Constants.Application.QueueNumber,
                    request?.Pnr ?? string.Empty,
                    "QueueRemovalFailed",
                    ex.GetType().Name);
                return Result.Failure<Response<RemoveDuplicatePnrResponse>>(
                    Json.Serialize(VAA.EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(
                        VAA.EAPI.Infrastructure.Constants.Constants.TechnicalError,
                        Guid.NewGuid().ToString(),
                        VAA.EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            finally
            {
                if (!token.IsNullOrWhiteSpace())
                {
                    try
                    {
                        await _sessionCloseCommandHandler.Handle(
                            new SessionCloseCommand { BinaryToken = token, PCC = request?.PCC },
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "DuplicatePNR session close failed. PCC={PCC}, PNR={PNR}",
                            request?.PCC ?? string.Empty,
                            request?.Pnr ?? string.Empty);
                    }
                }
            }
        }

        private static string SanitiseSabreError(string? error)
        {
            var singleLineError = (error ?? string.Empty).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
            return singleLineError.Length <= Constants.Application.MaxSabreErrorMessageLength
                ? singleLineError
                : $"{singleLineError[..Constants.Application.MaxSabreErrorMessageLength]}...";
        }

        private static void EnsureQueueAccessSucceeded(QueueAccessRQResponse queueAccessResponse)
        {
            var applicationResults = queueAccessResponse?.QueueAccessRS?.ApplicationResults;
            if (applicationResults == null)
                throw new Exception("QueueAccess did not return ApplicationResults.");

            var sabreError = string.Join(" | ", applicationResults.Error
                ?.SelectMany(error => error?.SystemSpecificResults ?? Array.Empty<SystemSpecificResults>())
                ?.SelectMany(result => result?.Message ?? Array.Empty<MessageCondition>())
                ?.Select(message => message?.Value)
                ?.Where(message => message?.IsNullOrWhiteSpace() == false)
                ?.Select(SanitiseSabreError)
                ?.Take(Constants.Application.MaxSabreErrorMessages)
                ?? Enumerable.Empty<string>());

            var sabreErrorCount = applicationResults.Error?.Length ?? 0;
            var sabreErrorSuffix = !sabreError.IsNullOrWhiteSpace()
                ? $" Sabre error: {sabreError}"
                : sabreErrorCount > 0
                    ? " Sabre error details were not provided."
                    : " No Sabre errors were returned.";

            if (applicationResults.status != CompletionCodes.Complete)
                throw new Exception($"QueueAccess failed. Status: {applicationResults.status}. Error count: {sabreErrorCount}.{sabreErrorSuffix}");

            if (sabreErrorCount > 0)
                throw new Exception($"QueueAccess returned {sabreErrorCount} error(s).{sabreErrorSuffix}");
        }
    }
}
