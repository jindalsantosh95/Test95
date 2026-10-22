using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Services
{
    public sealed class AutoAccrualReferenceValidationService : IAutoAccrualReferenceValidationService
    {
        private readonly ICommonRefDBServices _commonReferenceDataService;

        public AutoAccrualReferenceValidationService(ICommonRefDBServices commonReferenceDataService)
        {
            _commonReferenceDataService = commonReferenceDataService ?? throw new ArgumentNullException(nameof(commonReferenceDataService));
        }

        public async Task ValidateAsync(AutoAccrualRequest request, CancellationToken cancellationToken)
        {
            if (await _commonReferenceDataService.GetAutoAccrualValidationCount(Constants.Application.GetIneligibleClassValidity, request, cancellationToken) == 0)
                throw new AutoAccrualValidationException("Ineligible booking class");

            if (await _commonReferenceDataService.GetAutoAccrualValidationCount(Constants.Application.GetNonAccrualClassValidity, request, cancellationToken) > 0)
                throw new AutoAccrualValidationException("Non-accrual booking class");

            if (await _commonReferenceDataService.GetAutoAccrualValidationCount(Constants.Application.GetMktClassValidity, request, cancellationToken) == 0)
                throw new AutoAccrualValidationException("Invalid marketing booking class");

            if (await _commonReferenceDataService.GetAutoAccrualValidationCount(Constants.Application.GetAirlineActivityDateValidity, request, cancellationToken) == 0)
                throw new AutoAccrualValidationException("Invalid airline activity date", Constants.Application.AutoAccrualInvalidScheduledDepartureDate);

            var terms = await _commonReferenceDataService.GetAutoAccrualPartnerTerms(request.Producer, cancellationToken);
            var activityDate = DateTime.ParseExact(request.ScheduledDepartureDate, Constants.Application.AutoAccrualActivityDateFormat, CultureInfo.InvariantCulture).Date;
            if (terms is null || !terms.AgreementStartDate.HasValue || activityDate < terms.AgreementStartDate.Value.Date || (terms.AgreementEndDate.HasValue && activityDate > terms.AgreementEndDate.Value.Date))
                throw new AutoAccrualValidationException("Invalid agreement date.", Constants.Application.AutoAccrualInvalidScheduledDepartureDate);
            if (terms.RetroClaimPeriod.HasValue && terms.RetroClaimPeriod.Value < (DateTime.Today - activityDate).Days)
                throw new AutoAccrualValidationException("Retro claim period exceeded.", Constants.Application.AutoAccrualInvalidScheduledDepartureDate);
        }
    }
}
