using System.Collections.Generic;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Entities;

namespace VAA.EAPI.Microservices.FlightInformation.Mapper
{
    public class AirportDetailsToDBEnitytMapper
    {
        public static AirportDatabaseEntity MaptoDBEntity(Domain.Airportdetails.Model.Airportdetail item)
        {
            return new AirportDatabaseEntity()
            {
                RowKey = item.AirportID.ToString(),
                PartitionKey = item.IataAirportCode.ToString(),
                AirportName = item.AirportName,
                CountryName = item.CountryName,
                CountryCode = item.CountryCode,
                CityIataCode = item.CityIataCode,
                EffectiveEndDate = item.EffectiveEndDate,
                EffectiveStartDate = item.EffectiveStartDate

            };
        }
        public static List<Domain.Airportdetails.Model.Airportdetail> MaptoAirportDetailsList(List<AirportDatabaseEntity> dbEntities)
        {
            List<Domain.Airportdetails.Model.Airportdetail> airportDetails = new List<Domain.Airportdetails.Model.Airportdetail>();
            dbEntities.ForEach(item =>
            {
                airportDetails.Add(
                    new Domain.Airportdetails.Model.Airportdetail()
                    {
                        AirportID = item.RowKey,
                        IataAirportCode = item.PartitionKey,
                        AirportName = item.AirportName,
                        CountryName = item.CountryName,
                        CountryCode = item.CountryCode,
                        CityIataCode = item.CityIataCode,
                        EffectiveEndDate = item.EffectiveEndDate,
                        EffectiveStartDate = item.EffectiveStartDate,
                        OpType = item.OpType
                    });
            });
            return airportDetails;
        }

    }
}


