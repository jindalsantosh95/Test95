using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.ServiceList;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.ServiceList
{
    public class ValidateServiceListRequestTests
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IServiceListRequestValidations<IATA_ServiceListRS>> _requestValidations;
        private readonly Mock<IGenericDataMapper> _genericDataMapper;
        private readonly Mock<IServiceListResponseValidation> _responseValidations;
        private readonly ServiceListQueryHandler _handler;
        private readonly Mock<ILogger<ServiceListQueryHandler>> _logger;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;

        public ValidateServiceListRequestTests()
        {
            _mediator = new Mock<IMediator>();
            _requestValidations = new Mock<IServiceListRequestValidations<IATA_ServiceListRS>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _genericDataMapper = new Mock<IGenericDataMapper>();
            _responseValidations = new Mock<IServiceListResponseValidation>();
            _logger = new Mock<ILogger<ServiceListQueryHandler>>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _handler = new ServiceListQueryHandler(_mediator.Object, _hostEnvironment.Object, _logger.Object, _requestValidations.Object, _genericDataMapper.Object, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceListQueryHandler(null, _hostEnvironment.Object, _logger.Object, _requestValidations.Object, _genericDataMapper.Object, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void When_HostEnvironmentObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceListQueryHandler(_mediator.Object, null, _logger.Object, _requestValidations.Object, _genericDataMapper.Object, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void When_GenericDataMapperObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceListQueryHandler(_mediator.Object, _hostEnvironment.Object, _logger.Object, _requestValidations.Object, null, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void Validate_ErrorFor_Null_ParticipatingOrg()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].ParticipatingOrg = null;
            bool isOfferServiceRq = true;

            var expectedRS = new Response<IATA_ServiceListRS>() { ResponseMessages = new ResponseMessage { Errors = new List<ErrorDetails>() { new() { Code = "912_1" } } } };
            _requestValidations.Setup(x => x.ValidateServiceListRequest(request, isOfferServiceRq)).Returns(expectedRS);

            var actualRS = _requestValidations.Object.ValidateServiceListRequest(request, isOfferServiceRq);

            Assert.NotNull(actualRS);
            Assert.Equal("912_1", actualRS.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void Validate_ErrorFor_Empty_ParticipatingOrgId()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].ParticipatingOrg.OrgID = string.Empty;
            bool isOfferServiceRq = true;

            var ExpectedRs = new Response<IATA_ServiceListRS>
            {
                ResponseMessages = new ResponseMessage()
                {
                    Errors = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "912_1"
                        }
                    }
                }
            };
            _requestValidations.Setup(x => x.ValidateServiceListRequest(request, isOfferServiceRq)).Returns(ExpectedRs);

            var actualRS = _requestValidations.Object.ValidateServiceListRequest(request, isOfferServiceRq);

            Assert.NotNull(actualRS);
            Assert.Equal("912_1", actualRS.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void Validate_ErrorFor_EmptySalesAgentIdOfSalesAgent()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].SalesAgent = new() { SalesAgentID = string.Empty };

            var validationErrors = ValidateServiceList.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptySalesBranchIdOfSalesBranch()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].SalesBranch = new() { SalesBranchID = string.Empty };

            var validationErrors = ValidateServiceList.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptyOrdinal()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].Ordinal = string.Empty;

            var validationErrors = ValidateServiceList.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_7", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_ZeroOrdinal()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].Ordinal = "0";

            var validationErrors = ValidateServiceList.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_SellerOrdinalNotStartingWithOne()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].Ordinal = "2";

            var validationErrors = ValidateServiceList.ValidateAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_NoErrorFor_SellerOrdinalStartingWithOne()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            request.DistributionChain[0].Ordinal = "123";

            var validationErrors = ValidateServiceList.ValidateAgentDetails(request.DistributionChain);

            Assert.Empty(validationErrors);

        }

        [Fact]
        public void Validate_ErrorFor_NullOffer()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            ServiceListCoreRequestType serviceListCoreRequest = request.Request.ServiceListCoreRequest;

            foreach (var offer in ((ServiceListOfferRequestType)serviceListCoreRequest.Item).Offer)
            {
                offer.OfferID = null;
                offer.OwnerCode = null;
            }

            var validationErrors = ValidateServiceList.ValidateServiceOfferDetails(request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptyOfferId()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            ((ServiceListOfferRequestType)request.Request.ServiceListCoreRequest.Item).Offer.First().OfferID = string.Empty;

            var validationErrors = ValidateServiceList.ValidateServiceOfferDetails(request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptyOwnerCode()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            ((ServiceListOfferRequestType)request.Request.ServiceListCoreRequest.Item).Offer.First().OwnerCode = string.Empty;

            var validationErrors = ValidateServiceList.ValidateServiceOfferDetails(request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_VS_OwnerCode()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            ((ServiceListOfferRequestType)request.Request.ServiceListCoreRequest.Item).Offer.First().OwnerCode = "VS";

            var validationErrors = ValidateServiceList.ValidateServiceOfferDetails(request);

            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void Validate_NonVs_OwnerCode()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOfferRQ");
            ((ServiceListOfferRequestType)request.Request.ServiceListCoreRequest.Item).Offer.First().OwnerCode = "NonVS";

            var validationErrors = ValidateServiceList.ValidateServiceOfferDetails(request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_NullOfferResponse()
        {
            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceListOfferResponse");
            OfferDoc = null;

            var validationErrors = ValidateServiceList.ValidateOfferDetails(OfferDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_ErrorOfferResponse()
        {
            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceListOfferResponse");
            OfferDoc.Error = new List<ErrorDetails> { new() { Message = "Failed" } };

            var validationErrors = ValidateServiceList.ValidateOfferDetails(OfferDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_NonVsSegment_OfferResponse()
        {
            var OfferDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceListOfferResponse");
            OfferDoc.DataList.FlightSegments.FirstOrDefault().OperatingCarrier = "DL";

            var validationErrors = ValidateServiceList.ValidateOfferDetails(OfferDoc);

            Assert.True(validationErrors.Any());
            Assert.Equal("76H_3", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ServiceList_ValidateOfferDetails__if_Respornse_Null()
        {
            var OfferResp = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferResponse");
            OfferResp = null;
            var validationErrors = ValidateServiceList.ValidateOfferDetails(OfferResp);
            
            Assert.NotNull(validationErrors);
            Assert.Equal("913_1",validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ServiceList_ValidateOfferDetails_for_NonVsSegment()
        {
            var OfferResp = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferResponse");
            OfferResp?.DataList?.FlightSegments?.ToList().ForEach(fs =>
            {
                fs.OperatingCarrier = "KL";
            });
            
            var validationErrors = ValidateServiceList.ValidateOfferDetails(OfferResp);

            Assert.NotNull(validationErrors);
            Assert.Equal("76H_3", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateAgentID_OfferServiceList()
        {
            var OfferResp = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferResponse");
            var agencyValidateResponse = TestDataProvider.GetTestData<AgencyValidateResponse>("NDCOffer\\ServiceList\\AgencyValidationResponse");

            agencyValidateResponse.AgencyDetails.RequestAgencyId = "666666";
            agencyValidateResponse.AgencyDetails.RequestOrgName = "AAAAAAA";

            var validationErrors = RequestValidations.ValidateAgentID_OfferServiceList(agencyValidateResponse, OfferResp);

            Assert.NotNull(validationErrors);
            Assert.Equal("368_2", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidateAgentID_OfferServiceList_if_AgencyValidateResponse_Null_and_OfferResponse_Null()
        {
            var OfferResp = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\ServiceList\\OfferResponse");
            var agencyValidateResponse = TestDataProvider.GetTestData<AgencyValidateResponse>("NDCOffer\\ServiceList\\AgencyValidationResponse");
            
            OfferResp = null;
            agencyValidateResponse = null;

            var validationErrors = RequestValidations.ValidateAgentID_OfferServiceList(agencyValidateResponse, OfferResp);

            Assert.NotNull(validationErrors);
            Assert.Equal("0", validationErrors.Count().ToString());
        }
    }
}
