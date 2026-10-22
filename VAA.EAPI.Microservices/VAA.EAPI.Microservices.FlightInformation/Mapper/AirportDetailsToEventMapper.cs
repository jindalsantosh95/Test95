using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.FlightInformation.Mapper
{
    public class AirportDetailsToEventMapper<T> where T : Domain.Airportdetails.Model.Airportdetail
    {
        public static CloudEvent CommandToEventMapper(T flightInfoCommand, string originator)
        {
            return new CloudEvent()
            {
                Data = flightInfoCommand,
                Type = flightInfoCommand.OpType,
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Originator = ConfigReader.Get(Constants.Environment.Originator),
                Subject = ConfigReader.Get(Constants.Environment.EventSubject),
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = ConfigReader.Get(Constants.Environment.DataVersion)
            };

        }
    }
}
