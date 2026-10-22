namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreMasterRecord
    {
        public string StoreId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string StoreWorkHoursJson { get; set; } = string.Empty;
        public string StoreSpecialHoursJson { get; set; } = string.Empty;

        // Parsed objects
        public List<DbWeeklyHour> WeeklyHours { get; set; } = new();
        public List<DbSpecialHour> SpecialHours { get; set; } = new();

        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }

        public bool HasKidsArea { get; set; }
        public bool HasWelcomeDrinksAndSnacks { get; set; }
        public bool HasAircraftSeats { get; set; }
        public bool HasSecretCinema { get; set; }

        public string PlaceId { get; set; } = string.Empty;
        public DateTime LastOpeningHoursSyncUtc { get; set; }
        public DateTime LastPlaceIdSyncUtc { get; set; }

        public string StoreImageSrc { get; set; } = string.Empty;
        public string StoreImageAlt { get; set; } = string.Empty;
        public bool MarkedAsClosed { get; set; }
        public List<string> Services { get; set; } = new();
    }

    public class DbWeeklyHour
    {
        public string day { get; set; }
        public string openingTime { get; set; }
        public string closingTime { get; set; }
        public bool isClosed { get; set; }
    }

    public class DbSpecialHour
    {
        public string Label { get; set; }
        public string Date { get; set; }   // keep as string because API returns string
        public string OpeningTime { get; set; }
        public string ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }

    public class DbSpecialHoursWrapper
    {
        public List<DbSpecialHour> specialHours { get; set; } = new();
    }
}
