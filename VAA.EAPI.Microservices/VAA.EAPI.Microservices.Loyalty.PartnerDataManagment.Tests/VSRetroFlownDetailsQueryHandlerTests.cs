using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Application.Query.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class VSRetroFlownDetailsQueryHandlerTests
    {
        private readonly FakeVSRetroFlownDBRepository _repository;
        private readonly VSRetroFlownDetailsQueryHandler _handler;

        public VSRetroFlownDetailsQueryHandlerTests()
        {
            _repository = new FakeVSRetroFlownDBRepository();
            _handler = new VSRetroFlownDetailsQueryHandler(new NoOpLogger<VSRetroFlownDetailsQueryHandler>(), _repository);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenDependenciesNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VSRetroFlownDetailsQueryHandler(null!, _repository));
            Assert.Throws<ArgumentNullException>(() => new VSRetroFlownDetailsQueryHandler(new NoOpLogger<VSRetroFlownDetailsQueryHandler>(), null!));
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRequestNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnBusinessException_WhenNeitherTicketNorPnrFlightProvided()
        {
            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = string.Empty,
                    Pnr = string.Empty,
                    FlightNumber = string.Empty
                }),
                CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Empty(_repository.GetVSRetroFlownDetailsCalls);
        }

        [Fact]
        public async Task Handle_ShouldCallRepository_WhenTicketNumberProvided()
        {
            var expected = new VSFlownDetailsResponse(Array.Empty<VSRetroFlownDetail>());
            _repository.NextVSRetroFlownDetailsResponse = expected;

            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = "123",
                    Pnr = string.Empty,
                    FlightNumber = string.Empty
                }),
                CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Same(expected, result.Value.Value);

            var call = Assert.Single(_repository.GetVSRetroFlownDetailsCalls);
            Assert.Equal("123", call.TicketNumber);
            Assert.Equal(string.Empty, call.Pnr);
            Assert.Equal(string.Empty, call.FlightNumber);
        }

        [Fact]
        public async Task Handle_ShouldCallRepository_WhenPnrAndFlightNumberProvided()
        {
            var expected = new VSFlownDetailsResponse(Array.Empty<VSRetroFlownDetail>());
            _repository.NextVSRetroFlownDetailsResponse = expected;

            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = string.Empty,
                    Pnr = "PNR1",
                    FlightNumber = "1234"
                }),
                CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Same(expected, result.Value.Value);

            var call = Assert.Single(_repository.GetVSRetroFlownDetailsCalls);
            Assert.Equal(string.Empty, call.TicketNumber);
            Assert.Equal("PNR1", call.Pnr);
            Assert.Equal("1234", call.FlightNumber);
        }

        [Fact]
        public async Task Handle_ShouldReturnBusinessException_WhenAllThreeProvided()
        {
            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = "123",
                    Pnr = "PNR1",
                    FlightNumber = "1234"
                }),
                CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Empty(_repository.GetVSRetroFlownDetailsCalls);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRepositoryThrows_ForTicketNumberMode()
        {
            _repository.NextVSRetroFlownDetailsException = new Exception("db down");

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(
                    new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                    {
                        TicketNumber = "123",
                        Pnr = string.Empty,
                        FlightNumber = string.Empty
                    }),
                    CancellationToken.None));

            Assert.Equal("db down", ex.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnBusinessException_WhenOnlyPnrProvided()
        {
            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = string.Empty,
                    Pnr = "PNR1",
                    FlightNumber = string.Empty
                }),
                CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Empty(_repository.GetVSRetroFlownDetailsCalls);
        }

        [Fact]
        public async Task Handle_ShouldReturnBusinessException_WhenOnlyFlightNumberProvided()
        {
            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = string.Empty,
                    Pnr = string.Empty,
                    FlightNumber = "1234"
                }),
                CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Empty(_repository.GetVSRetroFlownDetailsCalls);
        }

        [Fact]
        public async Task Handle_ShouldReturnBusinessException_WhenRepositoryThrowsApplicationException()
        {
            _repository.NextVSRetroFlownDetailsException = new ApplicationException("Invalid Ticket Number");

            var result = await _handler.Handle(
                new VSRetroFlownDetailsQuery(new VSRetroSearchRequest
                {
                    TicketNumber = "123",
                    Pnr = string.Empty,
                    FlightNumber = string.Empty
                }),
                CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Contains(result.Value.Errors, e => e.Code == "10181");

            var call = Assert.Single(_repository.GetVSRetroFlownDetailsCalls);
            Assert.Equal("123", call.TicketNumber);
            Assert.Equal(string.Empty, call.Pnr);
            Assert.Equal(string.Empty, call.FlightNumber);
        }

        private class FakeVSRetroFlownDBRepository : IVSRetroFlownDBRepository
        {
            public List<(string TicketNumber, string Pnr, string FlightNumber)> GetVSRetroFlownDetailsCalls { get; } = new();

            public VSFlownDetailsResponse? NextVSRetroFlownDetailsResponse { get; set; }
            public Exception? NextVSRetroFlownDetailsException { get; set; }

            public Task<VSFlownDetailsResponse> GetVSFlownDetailsByTicketNumber(string ticketNumber)
            {
                throw new NotSupportedException("Not used by VSRetroFlownDetailsQueryHandler.");
            }

            public Task<VSFlownDetailsResponse> GetVSRetroFlownDetails(string ticketNumber, string pnr, string flightNumber)
            {
                GetVSRetroFlownDetailsCalls.Add((ticketNumber, pnr, flightNumber));

                if (NextVSRetroFlownDetailsException is not null)
                {
                    return Task.FromException<VSFlownDetailsResponse>(NextVSRetroFlownDetailsException);
                }

                if (NextVSRetroFlownDetailsResponse is null)
                {
                    throw new InvalidOperationException("No fake response configured.");
                }

                return Task.FromResult(NextVSRetroFlownDetailsResponse);
            }

            public Task<bool> UpdateVSRetroFlownDetailsAsync(UpdateVSFlownDetailsRequest request, CancellationToken cancellationToken)
            {
                throw new NotSupportedException("Not used by VSRetroFlownDetailsQueryHandler.");
            }
        }

        private class NoOpLogger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoOpDisposable.Instance;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

            private class NoOpDisposable : IDisposable
            {
                public static readonly NoOpDisposable Instance = new();
                public void Dispose() { }
            }
        }
    }
}
