using VAA.EAPI.GraphQL.Domain.Holidays.Helper;
using InfrastructureImage = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreImage;
using InfrastructureStoreRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreRecord;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreRecord
    {
        public string StoreId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationLabel { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public IEnumerable<StoreWorkHours> StoreWorkHours { get; set; } = Enumerable.Empty<StoreWorkHours>();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public StoreImage FeaturedImage { get; set; } = new();
        public IEnumerable<StoreImage> CardImages { get; set; } = Enumerable.Empty<StoreImage>();
        public string Distance { get; set; } = string.Empty;
        public string DrivingTime { get; set; } = string.Empty;
        public bool MarkedAsClosed { get; set; }
        public bool IsNew { get; set; }
        public bool IsClosest { get; set; }
        public StoreInformation? FullDetails { get; set; }

        public static explicit operator StoreRecord(InfrastructureStoreRecord storeRecord)
        {
            ArgumentNullException.ThrowIfNull(storeRecord);

            return new StoreRecord
            {
                StoreId = storeRecord.StoreId,
                Name = storeRecord.Name,
                LocationLabel = storeRecord.Label,
                AddressLine1 = storeRecord.AddressLine1,
                AddressLine2 = storeRecord.AddressLine2,
                City = storeRecord.City,
                County = storeRecord.County,
                CountryCode = storeRecord.CountryCode,
                PostalCode = storeRecord.PostalCode,
                StoreWorkHours = StoreHelper.BuildStoreWorkHours(storeRecord),
                Latitude = storeRecord.Latitude,
                Longitude = storeRecord.Longitude,
                FeaturedImage = StoreHelper.BuildImage(storeRecord.FeaturedImage),
                CardImages = StoreHelper.BuildCardImages(storeRecord),
                Distance = storeRecord.Distance,
                DrivingTime = storeRecord.DrivingTime,
                MarkedAsClosed = storeRecord.MarkedAsClosed,
                IsNew = storeRecord.IsNew,
                IsClosest = storeRecord.IsClosest,
                FullDetails = StoreHelper.BuildFullDetails(storeRecord)
            };
        }
    }
}
