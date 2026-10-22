using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.FlightOperations.Crew.Application.Models
{
    public sealed class CrewMember
    {
        public static explicit operator CrewMember(string[] source)
        {
            return new()
            {
                Id = source[0],
                FirstName = source[1],
                LastName = source[2],
                Email = source[3],
                Base = source[4],
            };
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Base { get; set; }
    }
}
