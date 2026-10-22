using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service.VSRetro.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service.VSRetro
{
    public class VSRetroFlownDBService : IVSRetroFlownDBService
    {
        private readonly FlownDbContext _flownDataContext;
        private readonly ILogger<FlownDBServices> _logger;
        private readonly IDbConnection _dbConnection;

        public VSRetroFlownDBService(FlownDbContext flownDataContext, ILogger<FlownDBServices> logger)
        {
            _flownDataContext = flownDataContext ?? throw new ArgumentNullException(nameof(flownDataContext));
            _dbConnection = _flownDataContext.CreateConnection();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DataTable> GetVSFlownDetailsAsync(string ticketNumber, string operatingCarrier, string noShowIndicator)
        {
            try
            {
                DataTable dtFlownDetails = new();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@TicketNumber", ticketNumber, DbType.String);
                parameters.Add("@OperatingAirlineCode", operatingCarrier, DbType.String);
                parameters.Add("@NoShowIndicator", noShowIndicator, DbType.String);

                var flownData = await _dbConnection.ExecuteReaderAsync(Constants.Application.GetVSFlownDetails, parameters, null, null, CommandType.StoredProcedure);

                dtFlownDetails.Load(flownData);

                return dtFlownDetails;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while fetching VS Flown Details from Database:  {ex.Message} {ex?.InnerException}");
                throw;
            }
        }

        public async Task<DataTable> GetVSRetroFlownDetailsAsync(string ticketNumber, string pnr, string flightNumber)
        {
            try
            {
                DataTable dtFlownDetails = new();

                DynamicParameters parameters = new();
                parameters.Add("@TicketNumber", ticketNumber.IsNullOrWhiteSpace() ? null : ticketNumber, DbType.String);
                parameters.Add("@PNR", pnr.IsNullOrWhiteSpace() ? null : pnr, DbType.String);
                parameters.Add("@FlightNumber", flightNumber.IsNullOrWhiteSpace() ? null : flightNumber, DbType.String);
                parameters.Add("@ActivityDate", null, DbType.String);
                parameters.Add("@LastName", null, DbType.String);

                var flownData = await _dbConnection.ExecuteReaderAsync(Constants.Application.GetVSRetroFlownDetails, parameters, null, null, CommandType.StoredProcedure);

                dtFlownDetails.Load(flownData);

                return dtFlownDetails;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while fetching VS Retro Flown Details from Database:  {ex.Message} {ex?.InnerException}");
                throw;
            }
        }

        public async Task<bool> UpdateVSRetroFlownDetailsAsync(UpdateVSFlownDetailsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                DynamicParameters parameters = new();
                parameters.Add("@OriginAirport", request.OriginAirport, DbType.String);
                parameters.Add("@DestinationAirport", request.DestinationAirport, DbType.String);
                parameters.Add("@TicketNumber", request.TicketNumber, DbType.String);
                parameters.Add("@Updateflag", "U", DbType.String);
                parameters.Add("@status", request.Status, DbType.String);
                parameters.Add("@VALoyaltyProgramCode", request.VALoyaltyProgramCode.IsNullOrWhiteSpace() ? null : request.VALoyaltyProgramCode, DbType.String);
                parameters.Add("@MemberNumber", request.MemberNumber.IsNullOrWhiteSpace() ? null : request.MemberNumber, DbType.String);

                var command = new CommandDefinition(
                    Constants.Application.UpdateVSRetroFlownDetails,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);
                var impactedRows = await _dbConnection.ExecuteAsync(command);

                return impactedRows > 0;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating VS Retro flown details in the database");
                throw;
            }
        }
    }
}
