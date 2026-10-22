using VAA.EAPI.GraphQL.Domain.Trips.Interface;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public new DateTime Now => DateTime.Now;
        public new DateTime UtcNow => DateTime.UtcNow;

    }
}
