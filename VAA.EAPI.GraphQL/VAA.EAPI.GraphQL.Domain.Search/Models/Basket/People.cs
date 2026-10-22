namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public sealed class People
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
        public int Age { get; set; }

        public static explicit operator People(FlightOffer.Customer source)
        {
            if (source == null)
                return null;

            return new()
            {
                Type = source.Ptc
            };
        }
    }
}