using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.IgnoreTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Mapper;
using Xunit;
using QueueMgmtApplicationConstants = VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Constants.Application;
using QueueMgmtEnvironmentConstants = VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Tests
{
    public sealed class QueueAccessRequestMapperTests
    {
        public QueueAccessRequestMapperTests()
        {
            Setup.CreateTestEnvironmentSettings();

            ConfigReader.Set(QueueMgmtEnvironmentConstants.ConversationId, "ConversationId");
            ConfigReader.Set(QueueMgmtEnvironmentConstants.FromPartyId, "FromPartyId");
            ConfigReader.Set(QueueMgmtEnvironmentConstants.ToPartyId, "ToPartyId");
        }

        [Fact]
        public void Map_ReturnsExpectedRequest()
        {
            var request = QueueAccessRequestMapper.Map("dummyToken", "1AB");

            Assert.NotNull(request);
            Assert.NotNull(request.MessageHeader);
            Assert.NotNull(request.MessageHeader.From);
            Assert.NotNull(request.MessageHeader.From.PartyId);
            Assert.NotNull(request.MessageHeader.To);
            Assert.NotNull(request.MessageHeader.To.PartyId);

            Assert.Equal(QueueMgmtApplicationConstants.QueueAccessAction, request.MessageHeader.Action);
            Assert.Equal("ConversationId", request.MessageHeader.ConversationId);
            Assert.Equal("FromPartyId", request.MessageHeader.From.PartyId[0].Value);
            Assert.Equal("ToPartyId", request.MessageHeader.To.PartyId[0].Value);

            Assert.NotNull(request.Security);
            Assert.Equal("dummyToken", request.Security.BinarySecurityToken);

            Assert.NotNull(request.QueueAccessRQ);
            Assert.NotNull(request.QueueAccessRQ.QueueIdentifier);
            Assert.Single(request.QueueAccessRQ.QueueIdentifier);
            Assert.NotNull(request.QueueAccessRQ.QueueIdentifier.First().List);
            Assert.True(request.QueueAccessRQ.QueueIdentifier.First().List.Ind);
            Assert.Equal(QueueMgmtApplicationConstants.QueueNumber, request.QueueAccessRQ.QueueIdentifier[0].Number);
            Assert.Equal("1AB", request.QueueAccessRQ.QueueIdentifier[0].PseudoCityCode);
        }

        [Fact]
        public void Map_UsesSuppliedTokenAndPcc()
        {
            var request = QueueAccessRequestMapper.Map("another-token", "2CD");

            Assert.Equal("another-token", request.Security.BinarySecurityToken);
            Assert.Equal("2CD", Assert.Single(request.QueueAccessRQ.QueueIdentifier).PseudoCityCode);
        }

        [Fact]
        public void Map_WhenRemoveCurrentAndExitRequested_ReturnsExpectedRequest()
        {
            var request = QueueAccessRequestMapper.Map(
                "remove-token",
                "6UOL",
                listInd: false,
                navigationAction: QueueAccessRQNavigationAction.QR,
                returnHostCommand: true);

            Assert.NotNull(request.QueueAccessRQ);
            Assert.NotNull(request.QueueAccessRQ.QueueIdentifier);
            Assert.Single(request.QueueAccessRQ.QueueIdentifier);

            Assert.Equal(QueueMgmtApplicationConstants.QueueNumber, request.QueueAccessRQ.QueueIdentifier[0].Number);
            Assert.Equal("6UOL", request.QueueAccessRQ.QueueIdentifier[0].PseudoCityCode);
            Assert.Null(request.QueueAccessRQ.QueueIdentifier[0].List);

            Assert.True(request.QueueAccessRQ.ReturnHostCommand);
            Assert.True(request.QueueAccessRQ.ReturnHostCommandSpecified);

            Assert.NotNull(request.QueueAccessRQ.Navigation);
            Assert.Equal(QueueAccessRQNavigationAction.QR, request.QueueAccessRQ.Navigation.Action);
            Assert.True(request.QueueAccessRQ.Navigation.ActionSpecified);
        }

        [Fact]
        public void GetReservationMap_ReturnsStatefulRequestForPnr()
        {
            var request = GetReservationRequestMapper.Map("session-token", "PNSJVQ");

            Assert.NotNull(request);
            Assert.Equal("session-token", request.Security.BinarySecurityToken);
            Assert.Equal("PNSJVQ", request.GetReservationRQ.Locator);
            Assert.Equal("Stateful", request.GetReservationRQ.RequestType);
        }

        [Fact]
        public void IgnoreTransactionMap_UsesRequestPcc()
        {
            var request = IgnoreTransactionRQMapper.IgnoreTransactionRequestMapper(
                new IgnoreTransactionCommand { BinarySecurityToken = "session-token", PCC = "6UOL" });

            Assert.Equal("6UOL", request.MessageHeader.CPAId);
        }
    }
}
