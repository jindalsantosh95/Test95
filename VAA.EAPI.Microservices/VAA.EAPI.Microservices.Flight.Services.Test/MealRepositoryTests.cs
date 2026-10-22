using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services.Interfaces;
using Xunit;
using SpecialService = VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.SpecialService;
using PnrCommon = VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.PNRCommon;
using Passenger = VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal.Passenger;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.PNRCommon;

namespace VAA.EAPI.Microservices.Flight.Services.Tests
{
    public class MealRepositoryTests
    {
        private readonly Mock<ISpecialServiceRequestV4> _specialServiceRequestV4;
        private readonly Mock<IPnrCommonV6> _iAir4PnrCommonV6;
        private readonly Mock<ILogger<MealRepository>> _logger;

        public MealRepositoryTests()
        {
            _iAir4PnrCommonV6 = new Mock<IPnrCommonV6>();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _specialServiceRequestV4 = new Mock<ISpecialServiceRequestV4>();
            _logger = new Mock<ILogger<MealRepository>>();
            FlightServiceMsEnvironmentSetup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void MealOperationResponse_ExplicitOperator_MapsFromRequest()
        {
        
            var request = CreateSampleMealOrderRequest();     
            var response = (MealOperationResponse)request;

            Assert.Equal(request.RecordLocator, response.RecordLocator);
            Assert.Equal(request.Segments, response.Segments);
        }

        [Fact]
        public void MealOrderRequest_ValidData_CreatesSuccessfully()
        {
            var request = CreateSampleMealOrderRequest();
                     
            Assert.NotNull(request);
            Assert.Equal("DJ4GWA", request.RecordLocator);
            Assert.NotNull(request.Segments);
            Assert.Single(request.Segments);
        }


        [Fact]
        public void MealRepository_NullValidation_Works()
        {
            var request = CreateSampleMealOrderRequest();
            Assert.NotNull(request);
            Assert.Equal("DJ4GWA", request.RecordLocator);
        }


        [Fact]
        public void MealRequestMapper_MapToAddRequest_MapsCorrectly()
        {
          
            var segment = new FlightSegment { SegmentId = "01" };
            var passengers = new List<Passenger> { new Passenger { PassengerId = "01.02" } };
            var mealCode = "VGML";
            var session = "23231213123-32123132123";
                      
            var result = MealRequestMapper.AddMealRequest(segment, mealCode, passengers, session);
                  
            Assert.NotNull(result);
            Assert.NotNull(result.RequestInfo);
            Assert.Equal(session, result.RequestInfo.SessionId);
            Assert.NotNull(result.SpecialServiceRequestInfo);
            Assert.Equal(mealCode, result.SpecialServiceRequestInfo.RequestCode);
            Assert.Equal("HK", result.SpecialServiceRequestInfo.StatusCode);
            Assert.Single(result.SpecialServiceRequestInfo.SegmentNumberInfo);
            Assert.Equal(segment.SegmentId, result.SpecialServiceRequestInfo.SegmentNumberInfo[0].SegmentNumber);
            Assert.Single(result.SpecialServiceRequestInfo.NameNumberInfo);
            Assert.Equal("01", result.SpecialServiceRequestInfo.NameNumberInfo[0].FirstNameNumber);
            Assert.Equal("02", result.SpecialServiceRequestInfo.NameNumberInfo[0].LastNameNumber);
        }

        [Fact]
        public void MealRequestMapper_MapToAddRequest_HandlesMultiplePassengers()
        {
          
            var segment = new FlightSegment { SegmentId = "02" };
            var passengers = new List<Passenger>
            {
                new Passenger { PassengerId = "01.01" },
                new Passenger { PassengerId = "02.03" }
            };
            var mealCode = "KSML";
            var session = "32424334234234";
                   
            var result = MealRequestMapper.AddMealRequest(segment, mealCode, passengers, session);
                     
            Assert.Equal(2, result.SpecialServiceRequestInfo.NameNumberInfo.Length);
            Assert.Equal("01", result.SpecialServiceRequestInfo.NameNumberInfo[0].FirstNameNumber);
            Assert.Equal("01", result.SpecialServiceRequestInfo.NameNumberInfo[0].LastNameNumber);
            Assert.Equal("02", result.SpecialServiceRequestInfo.NameNumberInfo[1].FirstNameNumber);
            Assert.Equal("03", result.SpecialServiceRequestInfo.NameNumberInfo[1].LastNameNumber);
        }

        #region MealRepository Unit Tests

        [Fact]
        public async Task AddMealAsync_NullRequest_ReturnsFailure()
        {

            var repository = new MealRepository(_specialServiceRequestV4.Object, _iAir4PnrCommonV6.Object, _logger.Object);

            var result = await repository.AddMealAsync(null);

            Assert.True(result.IsFailure);
            Assert.Equal("Request cannot be null", result.Error);
        }


        [Fact]
        public async Task AddMealAsync_ValidRequestWithSegments_CallsTeardownContext()
        {

            var repository = new MealRepository(_specialServiceRequestV4.Object, _iAir4PnrCommonV6.Object, _logger.Object);
            var request = CreateSampleMealOrderRequest();

            var responseTR = new PnrCommon.TeardownContextResponseType()
            {
                Pnr = new PnrCommon.PnrType() { SegmentDl = Array.Empty<PnrCommon.SegmentDl>(), Passenger = Array.Empty<PnrCommon.Passenger>() },
                DebugInfo = Array.Empty<PnrCommon.DebugInfoType>(),
                ResponseInfo = Array.Empty<PnrCommon.ResponseInfoType>()
            };

            _specialServiceRequestV4.Setup(r => r.AddSSRsRequest(It.IsAny<SpecialService.AddRequestType>())).Returns(SsrResponse());


            _iAir4PnrCommonV6.Setup(x => x.Air4SetupContext(It.IsAny<PnrCommon.SetupContextRequestType>()))
                   .Returns(GetResponse());

      
            _iAir4PnrCommonV6.Setup(r => r.Air4TeardownContext(It.IsAny<PnrCommon.TeardownContextRequestType>())).Returns(responseTR);
      
            var result = await repository.AddMealAsync(request);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(request.RecordLocator, result.Value.Value.RecordLocator);

        }


        [Fact]
        public async Task AddMealAsync_ExceptionThrown_ReturnsFailure()
        {

            var repository = new MealRepository(_specialServiceRequestV4.Object, _iAir4PnrCommonV6.Object, _logger.Object);
            var request = CreateSampleMealOrderRequest();

            _iAir4PnrCommonV6.Setup(x => x.Air4SetupContext(It.IsAny<PnrCommon.SetupContextRequestType>()))
                   .Throws(new Exception("exception"));
                   
            var result = await repository.AddMealAsync(request);
                     
            Assert.True(result.IsFailure);
            Assert.Equal("exception", result.Error);
        }

        [Fact]
        public async Task AddMealAsync_RequestWithoutSegments_DoesNotCallTeardown()
        {
     
            var repository = new MealRepository(_specialServiceRequestV4.Object, _iAir4PnrCommonV6.Object, _logger.Object);
            var request = new MealOrderRequest { RecordLocator = "TEST123", Segments = null };
            var setupResponse = new PnrCommon.SetupContextResponseType { SessionId = "34234324324342423" };
            _iAir4PnrCommonV6.Setup(x => x.Air4SetupContext(It.IsAny<PnrCommon.SetupContextRequestType>()))
                   .Returns(setupResponse);
    
            var result = await repository.AddMealAsync(request);
       
            _iAir4PnrCommonV6.Verify(x => x.Air4TeardownContext(It.IsAny<PnrCommon.TeardownContextRequestType>()), Times.Never);
        }


        [Fact]
        public void MealRepository_Constructor_WithValidDependencies_CreatesInstance()
        {
            var repository = new MealRepository(_specialServiceRequestV4.Object, _iAir4PnrCommonV6.Object, _logger.Object);

            Assert.NotNull(repository);
        }

        [Fact]
        public void MealRepository_Constructor_WithNullSSR_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MealRepository(null, _iAir4PnrCommonV6.Object, _logger.Object));
        }

        [Fact]
        public void MealRepository_Constructor_WithNullPNR_ThrowsArgumentNullException()
        {                   
            Assert.Throws<ArgumentNullException>(() => new MealRepository(_specialServiceRequestV4.Object, null, _logger.Object));
        }

        #endregion


        private MealOrderRequest CreateSampleMealOrderRequest()
        {
            return new MealOrderRequest
            {
                RecordLocator = "DJ4GWA",
                Segments = new[]
                {
                    new FlightSegment
                    {
                        SegmentId = "01",
                        FlightNumber = new FlightNumber
                        {
                            AirlineCode = "VS",
                            Number = "01"
                        },
                        Passengers = new[]
                        {
                            new Passenger
                            {
                                PassengerId = "01.01",
                                Meals = new[]
                                {
                                    new Meal
                                    {
                                        Code = "VGML"
                                    }
                                }
                            },
                             new Passenger
                            {
                                PassengerId = "01.01",
                                Meals = new[]
                                {
                                    new Meal
                                    {
                                        Code = "KSML"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private SetupContextResponseType GetResponse()
        {
            return new SetupContextResponseType
            {
                SessionId = "121212121212",
                Pnr = new PnrType
                {
                    SpecialServiceRequest = [new SpecialServiceRequest
                    {
                        LastNameNumber="01",
                        FirstNameNumber="01",
                        RequestCode="VGML",
                        FlightNumber="013"
                    }]
                }
            };
        }

        private SpecialService.AddResponseType SsrResponse()
        {
            return new SpecialService.AddResponseType()
            {
                Pnr = new SpecialService.PnrType()
                {
                    SpecialServiceRequest = new SpecialService.SpecialServiceRequest1[]
        {
            new SpecialService.SpecialServiceRequest1
            {
                AirlineCode = "VS",
                FirstNameNumber = "01",
                FlightDate = "01FEB",
                FlightNumber = "0103",
                LastNameNumber = "02",
                NumberOfSsrsRequestedFor = "01",
                RequestCode = "VGML",
                SsrNumber = "006",        
            },
            new SpecialService.SpecialServiceRequest1
            {
                AirlineCode = "VS",
                FirstNameNumber = "01",
                FlightDate = "28FEB",
                FlightNumber = "0104",
                LastNameNumber = "02",
                NumberOfSsrsRequestedFor = "01",
                RequestCode = "KSML",
                SsrNumber = "007",          
            }
        }

                }
            };


        }
    }
}