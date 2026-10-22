using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Models;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Mapper.Airportdetails
{
    public class AirportDataMapper
    {
        public static List<Domain.Airportdetails.Model.Airportdetail> MapAirportData(List<DimAirport> airports)
        {
            if (airports == null) throw new ArgumentNullException(nameof(airports));

            List<Domain.Airportdetails.Model.Airportdetail> airportDetailsList = new List<Domain.Airportdetails.Model.Airportdetail>();
            airports.ForEach(m =>
            airportDetailsList.Add(new Domain.Airportdetails.Model.Airportdetail()
            {
                AirportID = m.AirportID.ToString(),
                AirportName = m.Name ?? string.Empty,
                IataAirportCode = m.IataAirportCode,
                CityIataCode = m.CityIataCode ?? string.Empty,
                CountryName = m.Country ?? string.Empty,
                CountryCode = m.CountryCode ?? string.Empty,
                EffectiveEndDate = m.EffectiveEndDate.ToString(Constants.Application.DateTimeFormat),
                EffectiveStartDate = m.EffectiveStartDate.ToString(Constants.Application.DateTimeFormat)
            }));
            return airportDetailsList;
        }
    }
}
