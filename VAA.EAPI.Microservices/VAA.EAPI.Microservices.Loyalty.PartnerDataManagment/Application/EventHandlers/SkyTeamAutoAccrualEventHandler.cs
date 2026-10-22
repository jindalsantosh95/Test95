using Microsoft.Extensions.Logging;
using System.Text.Json;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class SkyTeamAutoAccrualEventHandler : ISkyTeamAutoAccrualEventHandler
    {
        private readonly ILogger<SkyTeamAutoAccrualEventHandler> _logger;
        private readonly IFlownDbServices _flownDbServices;
        private readonly IPartnerDataService _partnerDataService;
        private readonly ISkyTeamAutoAccrualKafkaPublisher _kafkaPublisher;

        public SkyTeamAutoAccrualEventHandler(ILogger<SkyTeamAutoAccrualEventHandler> logger, IFlownDbServices flownDbServices, IPartnerDataService partnerDataService, ISkyTeamAutoAccrualKafkaPublisher kafkaPublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _flownDbServices = flownDbServices ?? throw new ArgumentNullException(nameof(flownDbServices));
            _partnerDataService = partnerDataService ?? throw new ArgumentNullException(nameof(partnerDataService));
            _kafkaPublisher = kafkaPublisher ?? throw new ArgumentNullException(nameof(kafkaPublisher));
        }
        public async Task<bool> Handle(AutoAccrualPayload message, CancellationToken cancellationToken)
        {
            try
            {
                var skyTeamPartners = await _partnerDataService.GetSkyTeamPartnersAsync(cancellationToken);

                foreach (var partner in skyTeamPartners)
                {
                    var flownRecords = await _flownDbServices.GetFlownDetailsForAutoAccrualAsync(message, partner.PartnerCode, cancellationToken);

                    if (flownRecords == null || !flownRecords.Any())
                    {
                        continue;
                    }
                    foreach (var flownRecord in flownRecords)
                    {
                        if (flownRecord.ProcessingStatus == null || !Constants.Application.ValidProcessingStatuses.Contains(flownRecord.ProcessingStatus) || !flownRecord.FdNoShow.Is(Constants.Application.NoShowNo) || !flownRecord.SentToPartner.Is(Constants.Application.SentToPartnerNo))
                        {
                            continue;
                        }

                        if (!FlownDataValidator.ValidateMandatoryFields(flownRecord))
                        {
                            continue;
                        }

                        var accrualRequest = DbaAccrualMapper.Map(flownRecord);

                        await _kafkaPublisher.PublishAsync(accrualRequest, cancellationToken);
                    }

                }

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing SkyTeam Auto Accrual request.");

                return false;
            }
        }
    }
}
