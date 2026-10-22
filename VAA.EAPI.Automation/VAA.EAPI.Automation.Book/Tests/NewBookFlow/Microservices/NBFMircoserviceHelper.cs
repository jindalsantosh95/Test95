using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Models.BasketMS.ContactBasket;
using VAA.EAPI.Automation.Book.Models.BasketMS.PeopleBasket;
using VAA.EAPI.Automation.Book.Models.BasketMS.TermsAndConditionsBasket;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.TestDataConfig;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.Microservices
{
    public class NBFMircoserviceHelper
    {
        private static Random rnd = new Random();

        public static void FindEquipmentCode(string fleetTypeCode, out string equipmentCode)
        {
            equipmentCode = null;

            switch (fleetTypeCode)
            {
                case "333":
                    equipmentCode = "L3D";
                    break;
                case "339":
                    equipmentCode = "L39";
                    break;
                case "789":
                    equipmentCode = "L9E";
                    break;
                case "351":
                    equipmentCode = "L5H";
                    break;
            }
        }

        public static void UpdateAllPassengerDetails(dynamic basket, string basketId)
        {
            HttpResponseMessage updatePassengerResponse_MS = null;

            foreach (var item in (JArray)basket["people"])
            {
                string id = item["id"].ToString();
                string type = item["type"].ToString();

                //update basket contact details BFF request
                string firstName = $"First{DataGenerator.RandomString(5)}";
                string lastName = $"Last{DataGenerator.RandomString(5)}";
                int genderType = rnd.Next(0, 1);
                string dob = string.Empty;
                string gender = genderType.Equals(1) ? "MALE" : "FEMALE";

                switch (type)
                {
                    case "ADULT":
                        dob = DateTime.Now.AddYears(-rnd.Next(16, 100)).ToString("yyyy-MM-dd");

                        break;

                    case "YOUNG":
                        dob = DateTime.Now.AddYears(-rnd.Next(12, 15)).ToString("yyyy-MM-dd");

                        break;

                    case "CHILD":
                        dob = DateTime.Now.AddYears(-rnd.Next(2, 11)).ToString("yyyy-MM-dd");

                        break;

                    case "INFANT":
                        dob = DateTime.Now.AddYears(-rnd.Next(0, 1)).ToString("yyyy-MM-dd");

                        break;

                    default: throw new ArgumentException(nameof(type));
                }

                var updatePassengerRequest = UpdatePassengerRequest(firstName, lastName, dob, gender, type, id, null, null);

                updatePassengerResponse_MS = MicroserviceEndpoint.UpdatePeopleBasket(basketId, id, updatePassengerRequest).EnsureIsSuccessStatusCode();
            }
            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponse_MS.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JArray)passengerBasket.people).All(x => x["validation"].ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase)), "VALID was not returned as expected for People");
        }

        public static string UpdatePassengerRequest(string firstName, string lastName, string dob, string gender, string type, string id, string flyProgram, string flyNumber)
        {
            Person updatePassengerReq = new Person();
            {
                updatePassengerReq.FirstName = firstName;
                updatePassengerReq.LastName = lastName;
                updatePassengerReq.DateOfBirth = dob;
                updatePassengerReq.Gender = gender;
                updatePassengerReq.Type = type;
                updatePassengerReq.Id = id;
                updatePassengerReq.Validation = "VALID";
            }
            return JsonConvert.SerializeObject(updatePassengerReq);
        }

        public static string ContactBasketRequstMS(TestConfig config)
        {
            var contactDetails = new ContactDetails
            {
                EmailAddress = config.Book.Contact.EmailId,
                PhoneNumber = config.Book.Contact.PhoneNumber,
                AlternativePhoneNumber = config.Book.Contact.AltPhoneNumber,
                AcceptEmail = config.Book.Contact.AcceptEmail,
                Validation = "VALID",
                CountryCode = config.Book.Contact.CountryCode
            };
            return JsonConvert.SerializeObject(contactDetails);
        }

        public static string TermsAndConditionsBasketRequstMS()
        {
            var acceptanceDetails = new TermsAndConditionsBasket
            {
                Accepted = true,
                Validation = "VALID"
            };

            return JsonConvert.SerializeObject(acceptanceDetails);
        }
    }
}