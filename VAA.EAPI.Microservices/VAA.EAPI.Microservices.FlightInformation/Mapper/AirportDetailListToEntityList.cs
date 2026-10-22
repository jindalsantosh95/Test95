using System.Collections.Generic;
using VAA.EAPI.Microservices.FlightInformation.Domain.Airportdetails.Model;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Entities;

namespace VAA.EAPI.Microservices.FlightInformation.Mapper
{
    public class AirportDetailListToEntityList
    {
        public static List<AirportDatabaseEntity> MapToDBEntityList(IEnumerable<Airportdetail> airportDetailsList)
        {
            List<AirportDatabaseEntity> dbEntities = new List<AirportDatabaseEntity>();
            foreach (Airportdetail airportDetail in airportDetailsList)
            {
                dbEntities.Add(new AirportDatabaseEntity()
                {
                    RowKey = airportDetail.AirportID,
                    PartitionKey = airportDetail.IataAirportCode,
                    CityIataCode = airportDetail.CityIataCode,
                    CountryName = airportDetail.CountryName,
                    AirportName = airportDetail.AirportName,
                    CountryCode = airportDetail.CountryCode,
                    EffectiveEndDate = airportDetail.EffectiveEndDate,
                    EffectiveStartDate = airportDetail.EffectiveStartDate
                });

            }
            return dbEntities;
        }
    }
}
