using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Enrolment
{
    public class DeDupCheckCommandHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly TestMediaCodeValidator _mediaValidator = new();
        private readonly TestDeDupValidation _deDupValidation = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        public DeDupCheckCommandHandlerTests()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
            _serviceProvider = new ServiceCollection()
               .AddSingleton<ILogger<DeDupCheckCommandHandler>>(NullLogger<DeDupCheckCommandHandler>.Instance)
               .AddSingleton<IValidateMediaCodeReferenceDB>(_mediaValidator)
               .AddSingleton<IDeDupValidationEnrolment>(_deDupValidation)
               .AddSingleton(_mediatorMock.Object)
               .AddSingleton<IFCNumberGenerationRepository, TestFcRepository>()
               .AddTransient<DeDupCheckCommandHandler>()
               .BuildServiceProvider();
        }
        private DeDupCheckCommandHandler Handler => _serviceProvider.GetRequiredService<DeDupCheckCommandHandler>();

        private DeDupCheckCommand BuildRequest(string preferenceValue = null, bool includePreference = true)
        {
            var request = TestDataProvider.GetTestData<DeDupCheckCommand>("CustomerMS\\Enrolment\\EnrolmentRequest");

            var preferences = new List<VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.Preferences>();

            if (includePreference)
            {
                preferences.Add(new VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.Preferences
                {
                    PreferenceType = VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.Preferences.PreferenceTypes.PreferredPartnerFFP,
                    PreferenceValue = preferenceValue
                });
            }

            request.DeDupRequest.Data.CustomerDetails.Preferences = preferences;

            return request;
        }

        [Fact]
        public void Handler_ShouldThrowMediaCodeValidationError_ForInvalidMediaCode()
        {
            var request = BuildRequest(includePreference: false);             
            request.DeDupRequest.Data.CustomerDetails.LoyaltyMember.MediaCode = "TestCode";

            _mediaValidator.Errors = TestDataProvider.GetTestData<List<ErrorDetails>>("CustomerMS\\Enrolment\\MediaCodeValidationError");
            
            //Act
            var result = Handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Contains("1024", result.Result.Value.Errors.First().Code);
            Assert.Contains("The media code in the request is invalid.", result.Result.Value.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_Should_Return_BusinessException_When_PreferredPartner_Length_Greater_Than_3()
        {           
            var request = BuildRequest("Delta Airlines");
            SetupHappyPath();

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.Contains(result.Value.Errors, e => e.Code == Constants.ErrorCodes.InvalidPreferredPartnerFFP);
        }

        [Fact]
        public async Task Handler_Should_Return_OK_When_PreferredPartner_Length_Lesser_Than_3()
        {
            // Arrange            
            var request = BuildRequest("DL");

            SetupHappyPath();

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Value.Status);
        }

        [Fact]
        public async Task Handler_Should_Return_OK_When_PreferredPartner_Is_Missing()
        {
            var request = BuildRequest(includePreference: false);

            SetupHappyPath();

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
        }

        [Fact]
        public async Task Should_Return_OK_When_PreferredPartner_Is_Empty()
        {           
            var request = BuildRequest("");

            SetupHappyPath();

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
        }

        private void SetupHappyPath()
        {
            _mediaValidator.Errors = new List<ErrorDetails>();
            _deDupValidation.Errors = new List<ErrorDetails>();

            _mediatorMock.Setup(x => x.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        }

        private class TestMediaCodeValidator : IValidateMediaCodeReferenceDB
        {
            public List<ErrorDetails> Errors { get; set; } = new();

            public Task<List<ErrorDetails>> ValidateMediaCodeFromReferenceDB(string mediaCode)
                => Task.FromResult(Errors);
        }

        private class TestDeDupValidation : IDeDupValidationEnrolment
        {
            public List<ErrorDetails> Errors { get; set; } = new();

            public Task<List<ErrorDetails>> DeDupValidate(EnrolmentRequestCommand request)
                => Task.FromResult(Errors);
        }

        private class TestFcRepository : IFCNumberGenerationRepository
        {
            public Task<Result<Response<EnrolmentResponse>>> GenerateFlyingClubNumber()
            {
                return Task.FromResult(Result.Success(new Response<EnrolmentResponse>
                {
                    Value = new EnrolmentResponse
                    {
                        Memberdetails = new Domain.Enrolment.CustomerDetails.MemberStatus
                        {
                            MemberNumber = "123456"
                        }
                    },
                    Status = Status.OK
                }));
            }
        }
    }
}