using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse;

namespace VAA.EAPI.GraphQL.Domain.VHols.Application
{
    public class HolidayComponentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(IHolidayComponent));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string compType = (string)jo["typeName"];
            IHolidayComponent item = null;
            switch (compType)
            {
                case "Extra":
                    item = new Extra();
                    break;
                case "Car":
                    item = new Car();
                    break;
                case "Flight":
                    item = new Flight();
                    break;
                case "Transfer":
                    item = new Transfer();
                    break;
                case "Hotel":
                    item = new Hotel();
                    break;
                case "Tour":
                    item = new Tour();
                    break;
                case "Excursion":
                    item = new Excursion();
                    break;
                default:
                    break;
            }

            serializer.Populate(jo.CreateReader(), item);
            return item;
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}