using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Holidays;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class CarDetails
    {
        public string? Name { get; set; }
        public bool? VipBooking { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Model { get; set; }
        public string? Make { get; set; }
        public string? Class { get; set; }
        public string? ClassName { get; set; }
        public string? Type { get; set; }
        public string? TypeName { get; set; }
        public int? NumberOfCars { get; set; }
        public int? MaxBaggage { get; set; }
        public string? TransmissionType { get; set; }
        public int? DoorCount { get; set; }
        public bool? AirConditioned { get; set; }
        public IEnumerable<Location>? PickUpLocations { get; set; }
        public IEnumerable<Location>? ReturnLocations { get; set; }
        public Location? Destination { get; set; }
        public Location? SelectedPickupLocation { get; set; }
        public Location? SelectedReturnLocation { get; set; }
        public string? ContractType { get; set; }
        public CarProductDetails? CarProduct { get; set; }
        public string? SippCode { get; set; }
    }
}