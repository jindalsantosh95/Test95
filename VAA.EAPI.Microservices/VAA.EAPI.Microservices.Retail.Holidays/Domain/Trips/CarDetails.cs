using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class CarDetails
    {
        public string? Name { get; set; }
        public bool? VipBooking { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Model { get; set; }
        public string? Make { get; set; }
        public string? Class { get; set; }
        public string? ClassName { get; set; }
        public string? Type { get; set; }
        public string? TypeName { get; set; }
        public int? NumberOfCars { get; set; }
        public int? MaxBaggage { get; set; }
        public string? TransmissionType { get; set; }
        public int? DoorCount { get; set; }
        public bool? AirConditioned { get; set; }
        public IEnumerable<Location>? PickUpLocations { get; set; }
        public IEnumerable<Location>? ReturnLocations { get; set; }
        public Location? Destination { get; set; }
        public Location? SelectedPickupLocation { get; set; }
        public Location? SelectedReturnLocation { get; set; }
        public string? ContractType { get; set; }
        public CarProductDetails? CarProduct { get; set; }
        public string? SippCode { get; set; }

        public static explicit operator CarDetails?(JToken? source)
        {
            if (source == null) return null;

            var summary = source["summary"];
            var detail = source["detail"];

            return new CarDetails()
            {
                Name = $"{summary?["name"]?.Value<string>()} - {summary?["highlight"]?.Value<string>()}",
                VipBooking = summary?["vipBooking"]?.Value<bool>(),
                StartDate = summary?["startTime"]?.Value<DateTime>(),
                EndDate = summary?["endTime"]?.Value<DateTime>(),
                Model = detail?["model"]?.Value<string>(),
                Make = detail?["make"]?.Value<string>(),
                Class = detail?["class"]?.Value<string>(),
                ClassName = detail?["className"]?.Value<string>(),
                Type = detail?["type"]?.Value<string>(),
                TypeName = detail?["typeName"]?.Value<string>(),
                NumberOfCars = detail?["numberOfCars"]?.Value<int>(),
                MaxBaggage = detail?["maxBaggage"]?.Value<int>(),
                TransmissionType = detail?["transmissionType"]?.Value<string>(),
                DoorCount = detail?["doorCount"]?.Value<int>(),
                AirConditioned = detail?["airConditioned"]?.Value<bool>(),
                PickUpLocations = detail?["pickupLocations"]?.Select(pl => (Location)pl),
                ReturnLocations = detail?["returnLocations"]?.Select(pl => (Location)pl),
                Destination = (Location)detail?["destination"],
                SelectedPickupLocation = (Location)detail?["selectedPickupLocation"],
                SelectedReturnLocation = (Location)detail?["selectedReturnLocation"],
                ContractType = detail?["contractType"]?.Value<string>(),
                CarProduct = (CarProductDetails?)source["carProduct"],
                SippCode = detail?["sippCode"]?.Value<string>()
            };
        }
    }
}