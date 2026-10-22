using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Interface;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Models;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb
{
    public class CommonRefDBService : ICommonRefDBService
    {
        private readonly CommonRefDbContext _commonRefDbContext;
        private readonly ILogger<CommonRefDBService> _logger;

        public CommonRefDBService(CommonRefDbContext commonRefDbContext, ILogger<CommonRefDBService> logger)
        {
            _commonRefDbContext = commonRefDbContext;
            _logger = logger;
        }
        public async Task<List<DimAirport>> GetAllAirportsAsync()
        {
            try
            {
                return await _commonRefDbContext.DimAirports.ToListAsync();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "CommonRefDBService.GetAllAirportsAsync failed.");
                throw;
            }

        }
    }
}
