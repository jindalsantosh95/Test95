using InfraStoreRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreRecord;
using DomainStoreRecord = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator.StoreRecord;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreResponse
    {
        public string StoreId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationLabel { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public StoreImage FeaturedImage { get; set; } = new();
        public IEnumerable<StoreImage> CardImages { get; set; } = Enumerable.Empty<StoreImage>();
        public string DrivingTime { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public bool MarkedAsClosed { get; set; }
        public bool IsNew { get; set; }
        public bool IsClosest { get; set; }
        public StoreInformation? FullDetails { get; set; }

        public static explicit operator StoreResponse(InfraStoreRecord infraRecord)
        {
            ArgumentNullException.ThrowIfNull(infraRecord);

            var domainRecord = (DomainStoreRecord)infraRecord;

            var response = new StoreResponse
            {
                StoreId = domainRecord.StoreId,
                Name = domainRecord.Name,
                LocationLabel = domainRecord.LocationLabel,
                Latitude = domainRecord.Latitude,
                Longitude = domainRecord.Longitude,
                FeaturedImage = domainRecord.FeaturedImage,
                CardImages = domainRecord.CardImages,
                DrivingTime = domainRecord.DrivingTime,
                Distance = domainRecord.Distance,
                MarkedAsClosed = domainRecord.MarkedAsClosed,
                IsNew = domainRecord.IsNew,
                IsClosest = domainRecord.IsClosest,
                FullDetails = domainRecord.FullDetails
            };

            return response;
        }
    }
}
