using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;
using static VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange.OrderAmendment;
using VAA.EAPI.Infrastructure.Helper;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{
    public class ManageSpecialServiceEventProcessCommandHandler : IRequestHandler<ManageSpecialServiceEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly TableStorageService<ReferenceDataEntity> _tableStorageService;
        private readonly ICachedChannel _cachedChannel;
        private readonly ILogger<ManageSpecialServiceEventProcessCommandHandler> _logger;


        public ManageSpecialServiceEventProcessCommandHandler(IMediator mediator, ITableStorageService<ReferenceDataEntity> tableStorageService, ICachedChannel cachedChannel, IOfferSearchItems offerSearchItems, ILogger<ManageSpecialServiceEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tableStorageService = (TableStorageService<ReferenceDataEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ReferenceDataTableName);
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(ManageSpecialServiceEventProcessCommand request, CancellationToken cancellationToken)
        {
            Response<string> manageSpecialServiceResponse = new();
            try
            {
                var manageSpecialServiceRequestData = Json.Deserialize<OrderChangeRequestData>(Convert.ToString(request.Data));
                var channelInfo = await _cachedChannel.GetChannelEntity(manageSpecialServiceRequestData.ChannelId);
                var locationCode = manageSpecialServiceRequestData.Pos.CityCode;

                await AmendBooking(request, manageSpecialServiceRequestData, channelInfo, locationCode);

                var result = ManageSpecialServiceResponseMapper.ResponseMapper(manageSpecialServiceRequestData);
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.ManageSpecialServiceEventSubject });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");

                if (ex.Message == ErrorCodes.DeltaErrorCode || ex.Message == ErrorCodes.DeltaSetupContextErrorCode || ex.Message == ErrorCodes.DeltaTearDownErrorCode)
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ex.Message, request.Id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex?.InnerException.Message)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageSpecialServiceEventSubject });
                else
                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, request.Id, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.ManageSpecialServiceEventSubject });

                return Result.Failure<Response<string>>(ex.Message);
            }

            return manageSpecialServiceResponse;
        }

        private async Task<PnrSessionDetails> AmendBooking(ManageSpecialServiceEventProcessCommand request, OrderChangeRequestData orderChangeRequestData, Channel channelInfo, string locationCode)
        {
            var pnrSession = await SetupContext(orderChangeRequestData, request, locationCode, channelInfo);

            await AddAdditionalSpecialServiceRequest(request, orderChangeRequestData, pnrSession, locationCode, channelInfo);

            await ContextTearDown(request, pnrSession, locationCode, channelInfo);

            return pnrSession;
        }

        private async Task<PnrSessionDetails> SetupContext(OrderChangeRequestData orderChangeRequestData, ManageSpecialServiceEventProcessCommand request, string locationCode, Channel channelInfo)
        {
            return await _mediator.Send(new Air4CreateSessionCommand { pnrNumber = orderChangeRequestData.Rloc, transactionId = request.Id, location = locationCode, ChannelInfo = channelInfo });
        }

        private async Task AddAdditionalSpecialServiceRequest(ManageSpecialServiceEventProcessCommand request, OrderChangeRequestData orderChangeRequestData, PnrSessionDetails sessionDetails, string locationCode, Channel channelInfo)
        {
            var specialServiceReqInfo = orderChangeRequestData.OrderAmendments.Where(x => x.ProductType == ProductTypes.SSREnum).SelectMany(y => y.SpecialServiceRequestInfo);

            if (specialServiceReqInfo is not null && specialServiceReqInfo.Any())
            {
                foreach (var item in specialServiceReqInfo)
                {
                    var response = await _mediator.Send(new SpecialServiceRequestV4Command() { ChannelInfo = channelInfo, SessionId = sessionDetails.SessionId, Location = locationCode, TransactionId = request.Id, specialServiceRequestInfo = item });
                }
            }
        }

        private async Task ContextTearDown(ManageSpecialServiceEventProcessCommand request, PnrSessionDetails sessionDetails, string locationCode, Channel channelInfo)
        {

            var tearDownResponse = await _mediator.Send(new PNRCommonV6TearDownCommand { SessionId = sessionDetails.SessionId, TransactionId = request.Id, Location = locationCode, ChannelInfo = channelInfo });

        }
    }
}
