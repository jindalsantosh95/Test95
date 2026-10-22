using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Models;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Interface
{
    public interface ICommonRefDBService
    {
        public Task<List<DimAirport>> GetAllAirportsAsync();

    }
}
