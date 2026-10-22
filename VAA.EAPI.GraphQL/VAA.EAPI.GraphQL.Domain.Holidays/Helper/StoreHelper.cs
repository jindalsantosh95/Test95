using System.Globalization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;
using InfrastructureImage = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreImage;
using InfrastructureStoreRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreRecord;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Helper;

public static class StoreHelper
{
    internal static StoreImage BuildImage(InfrastructureImage? image)
    {
        if (image == null)
        {
            return new StoreImage();
        }

        return new StoreImage
        {
            Src = image.Src,
            Alt = image.Alt
        };
    }

    internal static List<StoreImage> BuildCardImages(InfrastructureStoreRecord storeRecord)
    {
        if (storeRecord.CardImages == null || storeRecord.CardImages.Count() == 0)
        {
            return new List<StoreImage>();
        }

        return storeRecord.CardImages
            .Select(BuildImage)
            .ToList();
    }

    internal static StoreInformation? BuildFullDetails(InfrastructureStoreRecord storeRecord)
    {
        if (storeRecord.FullDetails == null)
        {
            return null;
        }

        return new StoreInformation
        {
            Details = new StoreContactDetails
            {
                Phone = storeRecord.FullDetails.Details?.Phone ?? string.Empty,
                EmailAddress = storeRecord.FullDetails.Details?.EmailAddress ?? string.Empty,
                Address = new StoreAddress
                {
                    AddressLine1 = storeRecord.FullDetails.Details?.Address.AddressLine1 ?? string.Empty,
                    AddressLine2 = storeRecord.FullDetails.Details?.Address.AddressLine2 ?? string.Empty,
                    City = storeRecord.FullDetails.Details?.Address.City ?? string.Empty,
                    County = storeRecord.FullDetails.Details?.Address.County ?? string.Empty,
                    CountryCode = storeRecord.FullDetails.Details?.Address.CountryCode ?? string.Empty,
                    PostalCode = storeRecord.FullDetails.Details?.Address.PostalCode ?? string.Empty
                }
            },
            OpeningHours = new StoreOpeningHours
            {
                IsOpenNow = storeRecord.FullDetails.OpeningHours?.IsOpenNow ?? false,
                WeeklyHours = storeRecord.FullDetails.OpeningHours?.WeeklyHours
                    .Select(wh => new StoreOpeningWindow { Day = wh.Day, Hours = wh.Hours })
                    .ToList() ?? new List<StoreOpeningWindow>(),
                SpecialHours = storeRecord.FullDetails.OpeningHours?.SpecialHours
                    .Select(sh => new StoreExceptionalOpening
                    {
                        Label = sh.Label,
                        Date = FormatExceptionalDate(sh.Date),
                        OpeningTime = sh.OpeningTime,
                        ClosingTime = sh.ClosingTime,
                        IsClosed = sh.IsClosed
                    })
                    .ToList() ?? new List<StoreExceptionalOpening>()
            },
            Services = new StoreServiceList
            {
                InStoreFacilities = storeRecord.FullDetails.Services?.InStoreFacilities ?? new List<string>(),
                AccessibilityFeatures = storeRecord.FullDetails.Services?.AccessibilityFeatures
                    .Select(a => new StoreAccessibilityFeature { Name = a.Name, Value = a.Value })
                    .ToList() ?? new List<StoreAccessibilityFeature>()
            }
        };
    }

    internal static List<StoreWorkHours> BuildStoreWorkHours(InfrastructureStoreRecord storeRecord)
    {
        if (storeRecord.StoreWorkHours.Count() > 0)
        {
            return storeRecord.StoreWorkHours
                .Select(workHour => (StoreWorkHours)workHour)
                .ToList();
        }

        return storeRecord.FullDetails?.OpeningHours?.WeeklyHours
            .Select(workHour => new StoreWorkHours
            {
                Day = workHour.Day,
                OpeningTime = ParseOpeningTime(workHour.Hours),
                ClosingTime = ParseClosingTime(workHour.Hours),
                IsClosed = IsClosedHours(workHour.Hours)
            })
            .ToList() ?? new List<StoreWorkHours>();
    }

    private static string ParseOpeningTime(string hours)
    {
        if (string.IsNullOrWhiteSpace(hours) || !hours.Contains('-', StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return hours.Split('-', 2)[0].Trim();
    }

    private static string ParseClosingTime(string hours)
    {
        if (string.IsNullOrWhiteSpace(hours) || !hours.Contains('-', StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return hours.Split('-', 2)[1].Trim();
    }

    private static bool IsClosedHours(string hours)
    {
        return !string.IsNullOrWhiteSpace(hours)
            && hours.Contains("closed", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatExceptionalDate(DateOnly date)
    {
        var suffix = GetOrdinalSuffix(date.Day);
        return $"{date.Day}{suffix} {date.ToString("MMMM", CultureInfo.InvariantCulture)}";
    }

    private static string GetOrdinalSuffix(int day)
    {
        if (day % 100 is 11 or 12 or 13)
        {
            return "th";
        }

        return (day % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}
