using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using MediatR;
using Moq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.Facade.AzureB2C.Domain.Model;

namespace VAA.Facade.AzureB2C.Triggers.Http.Tests
{
    [TestClass()]
    public class CustomerEnrolmentControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CustomerEnrolmentController>> _mockLoggerController;
        private readonly Mock<FunctionContext> _mockExecutionContext;

        string b2cSuccessResponse = "{\"version\":\"1.0.0\",\"action\":\"Continue\",\"userMessage\":null,\"status\":null,\"extension_customerId\":\"9b02149d-ddb9-47c8-ac95-a84736c20d8f\"}";
        string b2cRequest = "{\"step\":\"PostAttributeCollection\",\"client_id\":\"c4f2d199-1149-4ad0-b973-c616a718c513\",\"extension_f144285dc2c14758b7eaa9753ca9df60_marketpreferences\":true,\"extension_f144285dc2c14758b7eaa9753ca9df60_customerId\":null,\"ui_locales\":\"en-US\",\"email\":\"Retailtherapyuser@noreply.com\",\"surname\":\"therapyuser\",\"givenName\":\"Retail\",\"identities\":[{\"signInType\":\"emailAddress\",\"issuer\":\"avadeunloyaltyfcb2c.onmicrosoft.com\",\"issuerAssignedId\":\"Retailtherapyuser@noreply.com\"}]}";


        public CustomerEnrolmentControllerTests()
        {
           
            _mockMediator = new Mock<IMediator>();
            _mockLoggerController = new Mock<ILogger<CustomerEnrolmentController>>();
            _mockExecutionContext = new Mock<FunctionContext>();
           

        }

        

        public Mock<HttpRequestData> HttpRequestSetup(FunctionContext executionContext, string body)
        {
            Mock<HttpRequestData> reqMock = new Mock<HttpRequestData>(new object[] { executionContext });
          
            // reqMock.Setup(req => req.FunctionContext).Returns(executionContext);
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(body);
            writer.Flush();
            stream.Position = 0;
            reqMock.Setup(req => req.Body).Returns(stream);
            var resBody = new MemoryStream(Encoding.ASCII.GetBytes(b2cSuccessResponse));
            reqMock.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(executionContext);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, resBody);
                return response.Object;
            });
            return reqMock;
        }      
    }
}