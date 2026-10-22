using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command
{
    public class AutoAccrualFallbackHandler : IAutoAccrualFallbackHandler
    {
        private readonly IFlownDbRepository _flownDbRepository;
        private readonly IPartnerDataService _partnerDataService;
        private readonly ISkyTeamAutoAccrualKafkaPublisher _kafkaPublisher;
        public AutoAccrualFallbackHandler(IFlownDbRepository flownDbRepository, IPartnerDataService partnerDataService, ISkyTeamAutoAccrualKafkaPublisher kafkaPublisher)
        {
            _flownDbRepository = flownDbRepository ?? throw new ArgumentNullException(nameof(flownDbRepository));
            _partnerDataService = partnerDataService ?? throw new ArgumentNullException(nameof(partnerDataService));
            _kafkaPublisher = kafkaPublisher ?? throw new ArgumentNullException(nameof(kafkaPublisher));
        }
        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            var skyTeamPartners = await _partnerDataService.GetSkyTeamPartnersAsync(cancellationToken);
            var skyTeamPartnerCodes = skyTeamPartners.Select(partner => partner.PartnerCode).Where(partnerCode => !partnerCode.IsNullOrWhiteSpace()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (skyTeamPartnerCodes.Count == 0)
            {
                return;
            }
            var criteria = new AutoAccrualFallbackCriteria
            {
                ProcessingStatus = Constants.Application.ClaimStatus,
                NoShowIndicator = Constants.Application.NoShowNo,
                SentToPartnerIndicator = Constants.Application.SentToPartnerNo,
                DepartureDate = DateTime.UtcNow.Date.AddDays(-int.Parse(ConfigReader.Get(Constants.Environment.AutoAccrualFallbackDepartureThresholdDays))),
                FfpCarrierCodes = string.Join(',', skyTeamPartnerCodes)
            };
            var flownRecords = await _flownDbRepository.GetFlownDetailsForAutoAccrualFallbackAsync(criteria, cancellationToken) ?? [];
            foreach (var flownRecord in flownRecords.Where(record => !record.FFPCarrierCode.IsNullOrWhiteSpace() && FlownDataValidator.ValidateMandatoryFields(record)))
            {
                var accrualRequest = DbaAccrualMapper.Map(flownRecord);
                await _kafkaPublisher.PublishAsync(accrualRequest, cancellationToken);
            }
        }
    }
}