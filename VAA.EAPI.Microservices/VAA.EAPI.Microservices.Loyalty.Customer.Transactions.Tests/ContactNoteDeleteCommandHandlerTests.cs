using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class ContactNoteDeleteCommandHandlerTests
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<TransactionBaseCommandHandler<ContactNoteCommandResponse>>> _mockLogger = new();

        public ContactNoteDeleteCommandHandlerTests()
        {
            ContactNoteTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactNoteDeleteCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactNoteDeleteCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handle_ShouldReturnAccepted_GoodDeleteRequest()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            List<ContactNote> notes = TestDataProvider.GetTestData<List<ContactNote>>("ListOfContactNotes");

            var notesResponse = Result.Success(new Response<ContactNotesResponse>()
            {
                Value = new ContactNotesResponse()
                {
                    CustomerId = customerID,
                    ContactNotes = notes
                }
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));
            _mockMediator.Setup(m => m.Send(It.IsAny<ContactNotesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(notesResponse);

            var command = TestDataProvider.GetTestData<ContactNoteDeleteCommand>("ContactNote-DELETE");
            command.CustomerId = customerID;

            var handler = new ContactNoteDeleteCommandHandler(_mockMediator.Object, _mockLogger.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Status.Accepted, result.Value.Status);
        }

        /// <summary>
        /// Even with a Bad request ID, the request should still be accepted as the actual validation is done asynchronously 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Handle_ShouldReturnAccepted_BadDeleteRequestID()
        {
            // Arrange
            string customerID = TestDataProvider.GetTestData<string>("CustID");
            List<ContactNote> notes = TestDataProvider.GetTestData<List<ContactNote>>("ListOfContactNotes");

            var notesResponse = Result.Success(new Response<ContactNotesResponse>()
            {
                Value = new ContactNotesResponse()
                {
                    CustomerId = customerID,
                    ContactNotes = notes
                }
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()));
            _mockMediator.Setup(m => m.Send(It.IsAny<ContactNotesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(notesResponse);

            var command = TestDataProvider.GetTestData<ContactNoteDeleteCommand>("ContactNote-DELETE-BadID");
            command.CustomerId = customerID;

            var handler = new ContactNoteDeleteCommandHandler(_mockMediator.Object, _mockLogger.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Status.Accepted, result.Value.Status);
        }

    }
}
