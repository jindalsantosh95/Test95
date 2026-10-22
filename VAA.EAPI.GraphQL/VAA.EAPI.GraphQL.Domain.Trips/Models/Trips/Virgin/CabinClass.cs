using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class CabinClass
    {
        public string? Code { get; set; }
        public string? ProductCode { get; set; }
        public string? SeatAssignmentType { get; set; }
        public string? SeatStatus { get; set; }
        public string? Name { get; set; }

        public static explicit operator CabinClass((Infrastructure.Models.TripMS.CabinClass cabinClass, IEnumerable<CabinRefData> cabinDetails) source)
        {
            var (cabinClass, cabinDetails) = source;

            if (cabinClass == null) return null;

            return new()
            {
                Code = cabinClass.Code,
                Name = cabinDetails.FirstOrDefault(c => c.BrandID == cabinClass.ProductCode)?.BrandName,
                ProductCode = cabinClass.ProductCode,
                SeatAssignmentType = cabinClass.SeatAssignmentType,
                SeatStatus = cabinClass.SeatStatus
            };
        }

        public static explicit operator CabinClass((Infrastructure.Models.HolidaysMS.TripsCore.CabinClass BookedCabinClass, IEnumerable<CabinRefData>) source)
        {
            var (cabinClass, cabinDetails) = source;

            if (cabinClass == null) return null;

            return new()
            {
                Code = cabinClass.Code,
                Name = cabinDetails.FirstOrDefault(c => c.BrandID == cabinClass.ProductCode)?.BrandName,
                ProductCode = cabinClass.ProductCode,
                SeatAssignmentType = cabinClass.SeatAssignmentType,
                SeatStatus = cabinClass.SeatStatus
            };
        }
    }
}
