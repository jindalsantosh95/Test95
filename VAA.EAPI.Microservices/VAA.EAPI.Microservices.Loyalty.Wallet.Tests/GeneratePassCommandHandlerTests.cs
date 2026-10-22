using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Tests
{
    public class GeneratePassCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;        
        private readonly GeneratePassCommandHandler _handler;
        private readonly Mock<ILogger<GeneratePassCommandHandler>> _logger;

        public GeneratePassCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GeneratePassCommandHandler>>();
            _handler = new GeneratePassCommandHandler(_mockMediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new GeneratePassCommandHandler(null, _logger.Object));
        }

        [Fact]
        public async Task Handler__ForSuccessResponse()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<GeneratePassCommand>("WalletMS//GeneratePassRequest");
            var generateResponse = TestDataProvider.GetTestData<byte[]>("WalletMS//GeneratePassResponse");

            //Act            
            _mockMediator.Setup(x => x.Send(It.IsAny<GeneratePassRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(generateResponse);
          
            var result = await _handler.Handle(request, CancellationToken.None);

                        
            //Assert
            Assert.IsType<byte[]>(result.Value);
            Assert.Equal(Status.OK, result.Status);
            using (var memoryStream = new MemoryStream(result.Value))
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream))
                {
                    var manifestEntry = archive.GetEntry("pass.json");
                    Assert.NotNull(manifestEntry);

                    using (var reader = new StreamReader(manifestEntry.Open()))
                    {
                        var content = reader.ReadToEnd();
                        Assert.Contains("\"passTypeIdentifier\"", content);
                        Assert.Contains("\"teamIdentifier\"", content);
                        Assert.Contains("\"organizationName\"", content);
                        Assert.Contains("\"VirginAtlantic\"", content);
                        Assert.Contains("\"TestName\"", content);
                        Assert.Contains("\"12345\"", content);
                    }
                }
            }

           
            Assert.Equal(generateResponse, result.Value);
            Assert.Equal(80, result.Value.First());
            Assert.Equal(0,result.Value.Last());
            Assert.Null(result.Errors);
            
        }
    }
}
