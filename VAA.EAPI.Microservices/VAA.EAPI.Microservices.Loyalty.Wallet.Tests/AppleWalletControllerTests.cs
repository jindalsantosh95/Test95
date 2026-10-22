using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Tests
{
    public class AppleWalletControllerTests
    {
        private readonly Mock<IMediator> _mediator;

        public AppleWalletControllerTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AppleWalletController(null));
        }

        [Fact]
        public async Task ShouldReturnSuccessResponse_ForGeneratePassValidRequest()
        {
            //Arrange
            var generateResponse = TestDataProvider.GetTestData<byte[]>("WalletMS//GeneratePassResponse");
            var response = new Response<byte[]>() { Status = Status.OK, Value = generateResponse };

            _mediator.Setup(m => m.Send(It.IsAny<GeneratePassCommand>(), CancellationToken.None)).ReturnsAsync(response);

            Mock<FunctionContext> context = new();
            var request = new Mock<HttpRequestData>(context.Object);

            string reqBody = "{\"FcNumber\":\"9999998835\",\"MemberName\":\"Testing\",\"TierCode\":\"Red\"}";
            var byteArray = Encoding.ASCII.GetBytes(reqBody);
            var bodyStream = new MemoryStream(byteArray);


            var req = new Mock<HttpRequestData>(context.Object);
            req.Setup(m => m.Body).Returns(bodyStream);
            req.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });          

            //Act
            var handler = new AppleWalletController(_mediator.Object);

            var result = await handler.GeneratePass(req.Object);

            result.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new(result.Body);
            var body = rdr.ReadToEnd();
            result.Body.Close();

            var output = Json.Deserialize<byte[]>(body);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            using (var memoryStream = new MemoryStream(output))
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
        }
    }
}
