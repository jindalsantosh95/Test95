using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories;
using Xunit;
using CreateSessionProxy = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using GetReservationProxy = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using IgnoreTransactionProxy = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;
using QueueAccessProxy = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;
using SessionCloseProxy = VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Tests
{
    public sealed class RemoveDuplicatePnrRepoCommandHandlerTests
    {
        public RemoveDuplicatePnrRepoCommandHandlerTests()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Constants.Environment.ConversationId, "ConversationId");
            ConfigReader.Set(Constants.Environment.FromPartyId, "FromPartyId");
            ConfigReader.Set(Constants.Environment.ToPartyId, "ToPartyId");
        }

        [Fact]
        public async Task Handle_WhenGetReservationDoesNotReturnReservation_DoesNotRemoveOrIgnoreAndClosesSession()
        {
            var actions = new List<string>();
            var queueAccess = new FakeQueueAccessCommand(actions);
            var ignoreTransaction = new FakeIgnoreTransaction(actions);
            var handler = new RemoveDuplicatePnrRepoCommandHandler(
                NullLogger<RemoveDuplicatePnrRepoCommandHandler>.Instance,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new GetReservationQueryHandler(new FakeGetReservationWithoutReservation(actions)),
                new IgnoreTransactionCommandHandler(ignoreTransaction),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                queueAccess);

            var result = await handler.Handle(
                new RemoveDuplicatePnrRequest { PCC = "6UOL", Pnr = "PNSJVQ" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(new[] { "CreateSession", "GetReservation", "SessionClose" }, actions);
            Assert.Equal(0, queueAccess.CallCount);
            Assert.Equal(0, ignoreTransaction.CallCount);
        }

        [Fact]
        public async Task Handle_WhenTargetPnrLoads_RemovesItAndClosesTheSession()
        {
            var actions = new List<string>();
            var getReservation = new FakeGetReservation(actions);
            var queueAccess = new FakeQueueAccessCommand(actions, returnsSuccess: true);
            var ignoreTransaction = new FakeIgnoreTransaction(actions, returnsSuccess: true);
            var sessionClose = new FakeSessionClose(actions);
            var logger = new RecordingLogger<RemoveDuplicatePnrRepoCommandHandler>();
            var handler = new RemoveDuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new GetReservationQueryHandler(getReservation),
                new IgnoreTransactionCommandHandler(ignoreTransaction),
                new SessionCloseCommandHandler(sessionClose),
                queueAccess);

            var result = await handler.Handle(
                new RemoveDuplicatePnrRequest { PCC = "6UOL", Pnr = "PNSJVQ" },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Value.Removed);
            Assert.Equal(new[] { "CreateSession", "GetReservation", "QueueAccess", "IgnoreTransaction", "SessionClose" }, actions);
            Assert.Equal("PNSJVQ", getReservation.Request.GetReservationRQ.Locator);
            Assert.Equal("session-token", getReservation.Request.Security.BinarySecurityToken);
            Assert.Equal("6UOL", queueAccess.Request.QueueAccessRQ.QueueIdentifier[0].PseudoCityCode);
            Assert.Equal(QueueAccessProxy.QueueAccessRQNavigationAction.QR, queueAccess.Request.QueueAccessRQ.Navigation.Action);
            Assert.Equal("session-token", queueAccess.Request.Security.BinarySecurityToken);
            Assert.Equal("6UOL", ignoreTransaction.Request.MessageHeader.CPAId);
            Assert.Equal("session-token", ignoreTransaction.Request.Security.BinarySecurityToken);
            Assert.Equal("6UOL", sessionClose.Request.MessageHeader.CPAId);
            Assert.Equal("session-token", sessionClose.Request.Security.BinarySecurityToken);
            Assert.Contains(logger.Messages, message => message == "DuplicatePNR QueueRemoval completed. PNR=PNSJVQ");
        }

        [Fact]
        public async Task Handle_WhenRemovalSessionCloseThrows_LogsErrorWithException()
        {
            var actions = new List<string>();
            var sessionCloseException = new Exception("Session close failed");
            var logger = new RecordingLogger<RemoveDuplicatePnrRepoCommandHandler>();
            var handler = new RemoveDuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new GetReservationQueryHandler(new FakeGetReservation(actions)),
                new IgnoreTransactionCommandHandler(new FakeIgnoreTransaction(actions, returnsSuccess: true)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions, exception: sessionCloseException)),
                new FakeQueueAccessCommand(actions, returnsSuccess: true));

            var result = await handler.Handle(
                new RemoveDuplicatePnrRequest { PCC = "6UOL", Pnr = "PNSJVQ" },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR session close failed. PCC=6UOL, PNR=PNSJVQ");
            Assert.Contains(logger.Exceptions, exception => ReferenceEquals(sessionCloseException, exception));
        }

        [Fact]
        public async Task Handle_WhenQueueContainsPnrs_ReturnsPnrsAndLogsQueueFetchSummary()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                new FakeQueueAccessCommand(actions, rlocs: ["PNSJVQ", "PVXKZK"]));

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "PNSJVQ", "PVXKZK" }, result.Value.Value.Rlocs);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch completed. PCC=6UOL, Queue=20, PnrCount=2");
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch item. PCC=6UOL, Queue=20, PNR=PNSJVQ");
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch item. PCC=6UOL, Queue=20, PNR=PVXKZK");
        }

        [Fact]
        public async Task Handle_WhenQueueAccessReturnsApplicationFailure_DoesNotLogSuccessAndClosesSession()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                new FakeQueueAccessCommand(actions, applicationStatus: QueueAccessProxy.CompletionCodes.Incomplete));

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(new[] { "CreateSession", "QueueAccess", "SessionClose" }, actions);
            Assert.DoesNotContain(logger.Messages, message => message.StartsWith("DuplicatePNR QueueFetch completed."));
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch failed. PCC=6UOL, Queue=20, Reason=QueueAccess failed. Status: Incomplete.");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenSessionCloseFailsAfterQueueFetch_LogsFetchedPnrsAndReturnsFailure()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions, exception: new Exception("Session close failed"))),
                new FakeQueueAccessCommand(actions, rlocs: ["PNSJVQ", "PVXKZK"]));

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch completed. PCC=6UOL, Queue=20, PnrCount=2");
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch item. PCC=6UOL, Queue=20, PNR=PNSJVQ");
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch session close failed. PCC=6UOL, Queue=20, Reason=SessionCloseFailed, ExceptionType=Exception");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenSessionCloseIsNotApprovedAfterQueueFetch_LogsFetchedPnrsAndReturnsFailure()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions, status: "Incomplete")),
                new FakeQueueAccessCommand(actions, rlocs: ["PNSJVQ"]));

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(new[] { "CreateSession", "QueueAccess", "SessionClose" }, actions);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch item. PCC=6UOL, Queue=20, PNR=PNSJVQ");
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch session close failed. PCC=6UOL, Queue=20, Reason=Session close was not approved.");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenSessionCreationIsNotApproved_ReturnsFailureWithoutLoggingQueueFetchSuccess()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var queueAccess = new FakeQueueAccessCommand(actions, rlocs: ["PNSJVQ"]);
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions, status: "Incomplete")),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                queueAccess);

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(new[] { "CreateSession" }, actions);
            Assert.Equal(0, queueAccess.CallCount);
            Assert.DoesNotContain(logger.Messages, message => message.StartsWith("DuplicatePNR QueueFetch completed."));
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch failed. PCC=6UOL, Queue=20, Reason=QueueFetchFailed, ExceptionType=Exception");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenSessionCloseIsCancelled_PropagatesCancellation()
        {
            var actions = new List<string>();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var handler = new DuplicatePnrRepoCommandHandler(
                NullLogger<DuplicatePnrRepoCommandHandler>.Instance,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions, exception: new OperationCanceledException(cancellationTokenSource.Token))),
                new FakeQueueAccessCommand(actions, rlocs: ["PNSJVQ"]));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                cancellationTokenSource.Token));
        }

        [Fact]
        public async Task Handle_WhenQueueFetchHasGeneralFailure_LogsSanitisedStructuredFailureWithoutException()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                new FakeQueueAccessCommand(actions, exception: new Exception("Queue fetch failed\r\nraw detail")));

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(new[] { "CreateSession", "QueueAccess", "SessionClose" }, actions);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch failed. PCC=6UOL, Queue=20, Reason=QueueFetchFailed, ExceptionType=Exception");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenQueueRemovalHasExternalIntegrationFailure_LogsStructuredFailure()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<RemoveDuplicatePnrRepoCommandHandler>();
            var handler = new RemoveDuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new GetReservationQueryHandler(new FakeGetReservation(actions)),
                new IgnoreTransactionCommandHandler(new FakeIgnoreTransaction(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                new FakeQueueAccessCommand(
                    actions,
                    exception: new VAA.EAPI.Domain.Exceptions.ExternalServiceIntegrationException("G009", "Sabre unavailable")));

            var result = await handler.Handle(
                new RemoveDuplicatePnrRequest { PCC = "6UOL", Pnr = "PNSJVQ" },
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueRemoval failed. PCC=6UOL, Queue=20, PNR=PNSJVQ, Reason=G009");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenQueueRemovalHasGeneralFailure_LogsSanitisedStructuredFailureWithoutException()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<RemoveDuplicatePnrRepoCommandHandler>();
            var handler = new RemoveDuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new GetReservationQueryHandler(new FakeGetReservation(actions)),
                new IgnoreTransactionCommandHandler(new FakeIgnoreTransaction(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                new FakeQueueAccessCommand(actions, exception: new Exception("Queue removal failed\r\nraw detail")));

            var result = await handler.Handle(
                new RemoveDuplicatePnrRequest { PCC = "6UOL", Pnr = "PNSJVQ" },
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueRemoval failed. PCC=6UOL, Queue=20, PNR=PNSJVQ, Reason=QueueRemovalFailed, ExceptionType=Exception");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        [Fact]
        public async Task Handle_WhenQueueFetchHasExternalIntegrationFailure_LogsStructuredFailureWithoutException()
        {
            var actions = new List<string>();
            var logger = new RecordingLogger<DuplicatePnrRepoCommandHandler>();
            var handler = new DuplicatePnrRepoCommandHandler(
                logger,
                new CreateSessionCommandHandler(new FakeCreateSession(actions)),
                new SessionCloseCommandHandler(new FakeSessionClose(actions)),
                new FakeQueueAccessCommand(
                    actions,
                    exception: new VAA.EAPI.Domain.Exceptions.ExternalServiceIntegrationException("G009", "Sabre unavailable")));

            var result = await handler.Handle(
                new DuplicatePnrRequest { PCC = "6UOL", Queue = "20" },
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Contains(logger.Messages, message => message ==
                "DuplicatePNR QueueFetch failed. PCC=6UOL, Queue=20, Reason=G009");
            Assert.All(logger.Exceptions, exception => Assert.Null(exception));
        }

        private sealed class FakeCreateSession(List<string> actions, string status = "Approved") : ICreateSession
        {
            public Task<CreateSessionProxy.SessionCreateRQResponse> CreateSessionAsync(CreateSessionProxy.SessionCreateRQRequest sessionCreateRQ)
            {
                actions.Add("CreateSession");
                return Task.FromResult(new CreateSessionProxy.SessionCreateRQResponse
                {
                    Security = new CreateSessionProxy.Security
                    {
                        BinarySecurityToken = new CreateSessionProxy.SecurityBinarySecurityToken { Value = "session-token" }
                    },
                    SessionCreateRS = new CreateSessionProxy.SessionCreateRS { status = status }
                });
            }
        }

        private sealed class FakeGetReservationWithoutReservation(List<string> actions) : IGetReservation
        {
            public Task<GetReservationProxy.GetReservationRS> GetReservationAsync(GetReservationProxy.GetReservationOperationRequest getReservationRQ)
            {
                actions.Add("GetReservation");
                return Task.FromResult(new GetReservationProxy.GetReservationRS
                {
                    Item = new GetReservationProxy.GetReservationRSTypeContent()
                });
            }
        }

        private sealed class FakeGetReservation(List<string> actions) : IGetReservation
        {
            public GetReservationProxy.GetReservationOperationRequest Request { get; private set; } = null!;

            public Task<GetReservationProxy.GetReservationRS> GetReservationAsync(GetReservationProxy.GetReservationOperationRequest getReservationRQ)
            {
                Request = getReservationRQ;
                actions.Add("GetReservation");
                return Task.FromResult(new GetReservationProxy.GetReservationRS
                {
                    Item = new GetReservationProxy.ReservationPNRB()
                });
            }
        }

        private sealed class FakeQueueAccessCommand(List<string> actions, bool returnsSuccess = false, string[] rlocs = null, Exception? exception = null, QueueAccessProxy.CompletionCodes applicationStatus = QueueAccessProxy.CompletionCodes.Complete) : IQueueAccessCommand
        {
            public int CallCount { get; private set; }
            public QueueAccessProxy.QueueAccessRQRequest Request { get; private set; } = null!;

            public Task<QueueAccessProxy.QueueAccessRQResponse> QueueAccess(QueueAccessProxy.QueueAccessRQRequest queueAccessRQRequest)
            {
                CallCount++;
                Request = queueAccessRQRequest;
                actions.Add("QueueAccess");

                if (exception != null)
                    throw exception;

                return Task.FromResult(returnsSuccess || rlocs != null || applicationStatus != QueueAccessProxy.CompletionCodes.Complete
                    ? new QueueAccessProxy.QueueAccessRQResponse
                    {
                        QueueAccessRS = new QueueAccessProxy.QueueAccessRS
                        {
                            Line = rlocs?.Select(rloc => new QueueAccessProxy.QueueAccessRSLine
                            {
                                UniqueID = new QueueAccessProxy.QueueAccessRSLineUniqueID { ID = rloc }
                            }).ToArray(),
                            ApplicationResults = new QueueAccessProxy.ApplicationResults
                            {
                                status = applicationStatus
                            }
                        }
                    }
                    : null!);
            }
        }

        private sealed class FakeIgnoreTransaction(List<string> actions, bool returnsSuccess = false) : IIgnoreTransaction
        {
            public int CallCount { get; private set; }
            public IgnoreTransactionProxy.IgnoreTransactionRQRequest Request { get; private set; } = null!;

            public Task<IgnoreTransactionProxy.IgnoreTransactionRQResponse> IgnoreTransactionAsync(IgnoreTransactionProxy.IgnoreTransactionRQRequest ignoreTransactionRQ)
            {
                CallCount++;
                Request = ignoreTransactionRQ;
                actions.Add("IgnoreTransaction");
                return Task.FromResult(returnsSuccess
                    ? new IgnoreTransactionProxy.IgnoreTransactionRQResponse
                    {
                        IgnoreTransactionRS = new IgnoreTransactionProxy.IgnoreTransactionRS
                        {
                            ApplicationResults = new IgnoreTransactionProxy.ApplicationResults
                            {
                                status = IgnoreTransactionProxy.CompletionCodes.Complete
                            }
                        }
                    }
                    : null!);
            }
        }

        private sealed class FakeSessionClose(List<string> actions, Exception? exception = null, string status = "Approved") : ISessionClose
        {
            public SessionCloseProxy.SessionCloseRQRequest Request { get; private set; } = null!;

            public Task<SessionCloseProxy.SessionCloseRQResponse> SessionCloseAsync(SessionCloseProxy.SessionCloseRQRequest sessionCloseRQ)
            {
                Request = sessionCloseRQ;
                actions.Add("SessionClose");

                if (exception != null)
                    throw exception;

                return Task.FromResult(new SessionCloseProxy.SessionCloseRQResponse
                {
                    SessionCloseRS = new SessionCloseProxy.SessionCloseRS { status = status }
                });
            }
        }

        private sealed class RecordingLogger<T> : ILogger<T>
        {
            public List<string> Messages { get; } = new();
            public List<Exception?> Exceptions { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => EmptyScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Messages.Add(formatter(state, exception));
                Exceptions.Add(exception);
            }

            private sealed class EmptyScope : IDisposable
            {
                public static EmptyScope Instance { get; } = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
