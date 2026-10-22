using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Infrastructure.Repositories.VSRetro.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service.VSRetro.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.VSRetro.Interface
{
    public class VSRetroFlownDBRepository : IVSRetroFlownDBRepository
    {
        private readonly IVSRetroFlownDBService _flownDBService;
        private readonly ILogger<VSRetroFlownDBRepository> _logger;

        public VSRetroFlownDBRepository(IVSRetroFlownDBService flownDBService, ILogger<VSRetroFlownDBRepository> logger)
        {
            _flownDBService = flownDBService ?? throw new ArgumentNullException(nameof(flownDBService)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<VSFlownDetailsResponse> GetVSFlownDetailsByTicketNumber(string ticketNumber)
        {
            var flownData = await _flownDBService.GetVSFlownDetailsAsync(ticketNumber, "VS", "Y");

            if (flownData.Rows.Count == 0)
            {
                _logger.LogError("Invalid Ticket Number");
                throw new ApplicationException("Invalid Ticket Number");
            }

            return FlownDataResponseMapper.GetFlownDataFromDataTableResponse(flownData);
        }

        public async Task<VSFlownDetailsResponse> GetVSRetroFlownDetails(string ticketNumber, string pnr, string flightNumber)
        {
            var flownData = await _flownDBService.GetVSRetroFlownDetailsAsync(ticketNumber, pnr, flightNumber);

            if (flownData.Rows.Count == 0)
            {
                _logger.LogError("No VS Retro flown details found for the supplied search criteria");
                throw new ApplicationException("No VS Retro flown details found for the supplied search criteria");
            }

            return FlownDataResponseMapper.GetRetroFlownDataFromDataTableResponse(flownData);
        }

        public Task<bool> UpdateVSRetroFlownDetailsAsync(UpdateVSFlownDetailsRequest request, CancellationToken cancellationToken)
        {
            return _flownDBService.UpdateVSRetroFlownDetailsAsync(request, cancellationToken);
        }
    }
}
