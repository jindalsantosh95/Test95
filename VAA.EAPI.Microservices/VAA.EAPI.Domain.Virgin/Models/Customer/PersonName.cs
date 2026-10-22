using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Customer
{
    public class PersonName
    {
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public static explicit operator PersonName(JObject source)
        {
            return new PersonName()
            {
                Title = source["Title"]?.ToString(),
                FirstName = source["FirstName"]?.ToString(),
                MiddleName = source["MiddleName"]?.ToString(),
                LastName = source["LastName"]?.ToString()
            };
        }
    }
}
