using System;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserResponse
{
    public class UserProperties
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string State { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
