using VAA.Facade.AzureB2C.Infrastructure.Services;
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
using System.Threading;
using VAA.Facade.AzureB2C.Infrastructure.Proxy;
using System.Collections.Generic;
using System.Net.Http;

namespace VAA.Facade.AzureB2C.Infrastructure.Services.Tests
{
    [TestClass()]
    public class EnrolmentServiceQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<EnrolmentServiceQueryHandler>> _mockLoggerController;
        private readonly Mock<FunctionContext> _mockExecutionContext;
        private readonly Mock<IHttpClientFactory> _mockhttpClientFactory;
        private EnrolmentRequest enrolmentRequest;

       

        public EnrolmentServiceQueryHandlerTests()
        {

            _mockMediator = new Mock<IMediator>();
            _mockLoggerController = new Mock<ILogger<EnrolmentServiceQueryHandler>>();
            _mockExecutionContext = new Mock<FunctionContext>();
            _mockhttpClientFactory = new Mock<IHttpClientFactory>();

        }

        public EnrolmentRequest EnrolmentRequestSetup()
        {

            CustomerDetails customerDetails = new CustomerDetails();
            customerDetails.FirstName = "TestFirstName";
            customerDetails.LastName = "TestLastName";
            customerDetails.CustomerType = "Basic";          
            customerDetails.Preferences = new List<Preference>() { new Preference() { PreferenceType = "VAMarketing", PreferenceValue = "Y" } };

           
            ContactDetails contactDetails = new ContactDetails();
            contactDetails.EmailDetails = new System.Collections.Generic.List<EmailDetail> {
                    new EmailDetail() {  EmailId = "TestFristName.TestLastName@noreply.com" , PreferredFlag = "Y" } };
           
            customerDetails.ContactDetails = contactDetails;

           


            return new EnrolmentRequest
            {
                CustomerDetails = customerDetails,                              
            };

        }

        [TestMethod()]
        public async Task EnrolmentServiceHandlerTest_With_Success()
        {
            //Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddHttpClient();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new Mock<FunctionContext>();
            context.SetupProperty(c => c.InstanceServices, serviceProvider); 
           // var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockhttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
            _mockMediator.Setup(m => m.Send(It.IsAny<EnrolmentServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KeyValuePair<bool, EnrolmentServiceResponse>());

            enrolmentRequest = EnrolmentRequestSetup();
            EnrolmentServiceQuery serviceQuery = new EnrolmentServiceQuery(enrolmentRequest);
            EnrolmentServiceQueryHandler enrolmentHandler = new EnrolmentServiceQueryHandler(_mockhttpClientFactory.Object, _mockLoggerController.Object);


            //Act
            var responseData = await enrolmentHandler.Handle(serviceQuery, new System.Threading.CancellationToken());
            //var reader = new StreamReader(httpResponseData.Body);
            //var responseBody = await reader.ReadToEndAsync();
            //var b2cResponse = JsonConvert.DeserializeObject<B2CResponse>(responseBody);

            //Assert
            Assert.IsNotNull(responseData);
            // Assert.IsNotNull(b2cResponse.extension_customerId);
        }

       
    }
}
