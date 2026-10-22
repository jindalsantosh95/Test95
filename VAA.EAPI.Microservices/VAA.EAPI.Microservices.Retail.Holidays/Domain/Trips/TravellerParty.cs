using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TravellerParty
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? LeadTraveller { get; set; }
        public bool? LeadDriver { get; set; }
        public string Title { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string TravellerType { get; set; }
        public IEnumerable<Address> Address { get; set; }

        public static explicit operator TravellerParty(JToken source)
        {
            if (source == null || source["profile"] == null)
                return null;

            var profile = source["profile"];

            return new()
            {
                DateOfBirth = profile["dob"]?.Value<DateTime>(),
                FirstName = profile["firstName"]?.Value<string>(),
                LastName = profile["lastName"]?.Value<string>(),
                LeadTraveller = source["lead"]?.Value<bool>(),
                LeadDriver = source["driver"]?.Value<bool>(),
                Gender = profile["gender"]?.Value<string>(),
                Title = profile["title"]?.Value<string>()
            };
        }
    }
}
