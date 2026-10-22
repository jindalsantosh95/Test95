using InfrastructureStoreWorkHours = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreWorkHours;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreWorkHours
    {
        public string Day { get; set; } = string.Empty;
        public string OpeningTime { get; set; } = string.Empty;
        public string ClosingTime { get; set; } = string.Empty;
        public bool IsClosed { get; set; }

        public static explicit operator StoreWorkHours(InfrastructureStoreWorkHours storeWorkHours)
        {
            ArgumentNullException.ThrowIfNull(storeWorkHours);

            return new StoreWorkHours
            {
                Day = storeWorkHours.Day,
                OpeningTime = storeWorkHours.OpeningTime,
                ClosingTime = storeWorkHours.ClosingTime,
                IsClosed = storeWorkHours.IsClosed
            };
        }
    }
}
