using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Tests
{
    public class EnrolmentControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<EnrolmentController>> _mockLoggerEnrolmentController;
        private readonly Mock<FunctionContext> _mockFunctionContext;

        public EnrolmentControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLoggerEnrolmentController = new Mock<ILogger<EnrolmentController>>();
            _mockFunctionContext = new Mock<FunctionContext>();
        }

        [Fact]
        public async Task TestEnrolmentMicroservice()
        {
            ConfigReader.Set("APPINSIGHTS_INSTRUMENTATIONKEY", "8bcd02b5-45f8-431c-8263-998548b786e0");
            string body = "{\"type\":null,\"id\":\"a25fb75a-4626-4464-bf9b-d8bea9634219\",\"source\":\"\",\"subject\":\"\",\"time\":\"2021-12-13T12:53:13.4770788Z\",\"dataContentType\":\"\",\"data\":{\"customerDetails\":{\"firstName\":\"Scuhbchfl\",\"lastName\":\"Ighjkbchfl\",\"Middlename\":\"\",\"preferredFirstName\":\"Shelly\",\"preferredLastName\":\"Izz\",\"preferredGender\":\"M\",\"preferredName\":\"Shelly\",\"preferredSalutation\":\"DearMr\",\"preferredPronouns\":\"He\",\"gender\":\"M\",\"title\":\"Mr\",\"salutation\":\"DearMr\",\"nationality\":\"British\",\"customerType\":\"Loyalty\",\"dateOfBirth\":\"2015-08-10\",\"password\":\"password123!xyzLvv\",\"addressDetails\":[{\"addressType\":\"Home\",\"addressLine1\":\"11FriendsClose23\",\"addressLine2\":\"HomeLocality\",\"addressLine3\":\"\",\"companyName\":\"VAA\",\"city\":\"Colchester\",\"postalCode\":\"RH117TY\",\"region\":\"Essex\",\"country\":\"GB\",\"preferredFlag\":\"Y\"},{\"addressType\":\"Work\",\"addressLine1\":\"8DencharPlat\",\"addressLine2\":\"HomeLocality\",\"addressLine3\":\"\",\"companyName\":\"VAA\",\"city\":\"Colchester\",\"postalCode\":\"RH117TY\",\"region\":\"Essex\",\"country\":\"GB\",\"preferredFlag\":\"\"}],\"contactDetails\":{\"emailDetails\":[{\"emailId\":\"abyhtyfvclh12@gmail.com\",\"description\":\"PersonalEmail\",\"usage\":\"\",\"preferredFlag\":\"Y\"}],\"phoneDetails\":[{\"phoneType\":\"Mobile\",\"description\":\"PersonalPhone\",\"countryCode\":\"44\",\"phoneNumber\":\"8677899888\",\"usage\":\"\",\"preferredFlag\":\"Y\"},{\"phoneType\":\"Mobile\",\"description\":\"PersonalPhone\",\"countryCode\":\"44\",\"phoneNumber\":\"9687699799\",\"usage\":\"\"}]},\"loyaltyMember\":{\"memberType\":\"Individual\",\"programName\":\"VirginAtlanticFlyingClub\",\"memberNumber\":\"1111113334440\",\"flyingClubCardNumber\":\"1111113334440\",\"status\":\"Active\",\"campaignCode\":\"\",\"testMember\":\"false\",\"mediaCode\":\"A50\",\"enrollmentChannel\":\"\",\"termsAndConditionsAccepted\":\"false\",\"guardianDetails\":{\"name\":\"Alexander\",\"dateOfBirth\":\"1975-01-27\",\"cardNumber\":\"1075810877\",\"phoneDetails\":{\"phoneType\":\"Landline\",\"usage\":\"Business\",\"countryCode\":\"44\",\"phoneNumber\":\"9898987787\",\"preferredFlag\":\"Y\",\"description\":\"PersonalPhone\"},\"emailDetails\":{\"usage\":\"Business\",\"emailId\":\"alex.123@noreply.com\",\"preferredFlag\":\"Y\",\"description\":\"PersonalEmail\"},\"guardianTermsAndConditionsAccepted\":\"false\"},\"partnerMember\":{\"partnerCode\":\"\",\"startDate\":\"\",\"statusEnhancementCode\":\"\"}},\"travelProfile\":{\"reasonForTravel\":\"Business\"},\"preferences\":[{\"preferenceType\":\"VAAMarketingPermissions\",\"preferenceValue\":\"\"},{\"preferenceType\":\"DoNotCall\",\"preferenceValue\":\"\"}],\"customerAttribute\":[{\"type\":\"IphoneApp\",\"value\":\"ABC\"},{\"type\":\"TwitterHandle\",\"value\":\"XYZ\"}]},\"securityQuestions\":[{\"question\":1,\"answer\":\"11\"}]}}";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var response = new Response<string>()
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterlocation, Value = "http://localhost:7071/enrolment/response/a25fb75a-4626-4464-bf9b-d8bea9634210" } },
            };

            Mock<FunctionContext> context = new Mock<FunctionContext>();
            context.SetupProperty(c => c.InstanceServices, serviceProvider);
            context.Setup(c => c.FunctionDefinition.PathToAssembly);

            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            {
                headers.Add(new KeyValuePair<string, string>("channelId", "Delta.com"));
                headers.Add(new KeyValuePair<string, string>("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Imwzc1EtNTBjQ0g0eEJWWkxIVEd3blNSNzY4MCIsImtpZCI6Imwzc1EtNTBjQ0g0eEJWWkxIVEd3blNSNzY4MCJ9.eyJhdWQiOiI3NjU5YWNiNS0yOTg2LTQ2ZGItYWU1Ny05ODUwODhkMDc0MzMiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC82YmRmZDVhOC01MzFmLTRmMTQtOWFmMS1kYzdiNDQ0YzNhZDgvIiwiaWF0IjoxNjM4ODc4ODQwLCJuYmYiOjE2Mzg4Nzg4NDAsImV4cCI6MTYzODg4Mjc0MCwiYWlvIjoiRTJaZ1lQaXJ4N2xVNE11bEU5ZVlqMjY0VlBsME5nQT0iLCJhcHBpZCI6ImJjNTIwNjI3LTVjNDctNGE1MS05MzIwLTAwY2QzNGM4MjIyMyIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0LzZiZGZkNWE4LTUzMWYtNGYxNC05YWYxLWRjN2I0NDRjM2FkOC8iLCJvaWQiOiJhYjRiMmE0My00ZjU3LTRkODAtYjQ0MS0wMDRlZGY2ZTA5MWMiLCJyaCI6IjAuQVFrQXFOWGZheDlURkUtYThkeDdSRXc2MkNjR1VyeEhYRkZLa3lBQXpUVElJaU1KQUFBLiIsInN1YiI6ImFiNGIyYTQzLTRmNTctNGQ4MC1iNDQxLTAwNGVkZjZlMDkxYyIsInRpZCI6IjZiZGZkNWE4LTUzMWYtNGYxNC05YWYxLWRjN2I0NDRjM2FkOCIsInV0aSI6IkR6X2JfUHVrYTBXazk1eW8wSGdMQUEiLCJ2ZXIiOiIxLjAifQ.FNU7XYtg5GRsulHYaDxYItjTZnhmXzodGV-CK6LOQ6wAMoJhpPurc6YgWmV2uBZCIv95IPj40nJrm9JRFgg7rk2rkcM-HAr-ktbKFLYpyB5KF1W74BsNjadAgtmDCUQIW1AUQB85UPRAylf5O2hKUUoEkiHPQM6fiu-Bl9pgedUheLHtsUsvNBVBL8HnNQ4pu5HrVOvr6SEbOWD7-fkwAR_FacYNkTMXOOHZnZ8LRQDbmipffeilmk3ybiAisn_e2_mTtSzo__u5JpVag4UiPGs4LU5ddFfvOt0wIMJiS_FspTZk5vJCP3eWs0P3leliP8R4WrIVjsD3zRmijvLiiw"));
            };

            var byteArray = Encoding.ASCII.GetBytes(body);
            var bodyStream = new MemoryStream(byteArray);

            var request = new Mock<HttpRequestData>(context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.Headers).Returns(new HttpHeadersCollection(headers));
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<EnrolmentEventRequest>(), System.Threading.CancellationToken.None)).ReturnsAsync(response);
            EnrolmentController enrolmentController = new EnrolmentController(_mockMediator.Object, _mockLoggerEnrolmentController.Object);
            HttpResponseData result = await enrolmentController.Enrolment(request.Object, context.Object);
            Assert.True(result.Headers.Contains(Constants.Application.HeaderParameterlocation));
            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);

        }

        [Fact]
        public async Task TestEnrolmentResponseSubscriber()
        {
            //Arrange

            EnrolmentController enrolmentController = new EnrolmentController(_mockMediator.Object, _mockLoggerEnrolmentController.Object);
            Mock<FunctionContext> context = new Mock<FunctionContext>();
            context.Setup(c => c.FunctionDefinition.PathToAssembly);

            var request = new Mock<HttpRequestData>(context.Object);

            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                return response.Object;
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<EnrolmentResponseQuery>(), new System.Threading.CancellationToken()).Result).Returns(new Response<EnrolmentResponse>() { Status = Status.Accepted });

            //Act

            HttpResponseData result = await enrolmentController.EnrolmentResponse(request.Object, "123", context.Object);

            //Assert

            Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        }
    }
}
