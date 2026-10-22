using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayCarInformation")]
    public class CarInformation
    {
        public string? CarProductId { get; set; }
        public string? Name { get; set; }
        public bool? VipBooking { get; set; }
        public LocalDateTime? StartDateLocal { get; set; }
        public LocalDateTime? EndDateLocal { get; set; }
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
        public TravellersComposition? TravellersComposition { get; set; }
        public SupplierInformation? SupplierInformation { get; set; }
        public string? SippCode { get; set; }
        public string? ManageBookingUrl { get; set; }

        public static explicit operator CarInformation((CarDetails? car, ProductSummaryDetails summary, SupplierDetails? supplierInfo, TravellerInfo travellersComposition) source)
        {
            var (car, summary, supplierInfo, travellersComposition) = source;
            if (car == null || summary == null) return default;

            return new()
            {
                Name = car.Name,
                VipBooking = car.VipBooking,
                StartDateLocal = DateTimeHelper.ToLocal(summary.StartTime),
                EndDateLocal = DateTimeHelper.ToLocal(summary.EndTime),
                Model = car.Model,
                Make = car.Make,
                Class = car.Class,
                ClassName = car.ClassName,
                CarProduct = (CarProductDetails)car.CarProduct,
                MaxBaggage = car.MaxBaggage,
                NumberOfCars = car.NumberOfCars,
                PickUpLocations = car.PickUpLocations?.Select(x => (Location)x),
                ReturnLocations = car.ReturnLocations?.Select(x => (Location)x),
                Destination = (Location)car.Destination,
                SelectedPickupLocation = (Location)car.SelectedPickupLocation,
                SelectedReturnLocation = (Location)car.SelectedReturnLocation,
                TransmissionType = car.TransmissionType,
                DoorCount = car.DoorCount,
                AirConditioned = car.AirConditioned,
                Type = car.Type,
                SupplierInformation = (SupplierInformation)supplierInfo,
                TravellersComposition = (TravellersComposition)travellersComposition,
                SippCode = car.SippCode,
                ManageBookingUrl = supplierInfo?.SupplierPaymentInfo?.SupplierCode?.Trim().Is("HERTZ") == true
                                   ? Common.Helper.ConfigReader.Get(Constants.Environment.HertzManageBookingUrl) : null,
            };
        }

        internal CarInformation Clone()
        {
            return new CarInformation
            {
                Name = this.Name,
                CarProductId = this.CarProductId,
                VipBooking = this.VipBooking,
                StartDateLocal = this.StartDateLocal,
                EndDateLocal = this.EndDateLocal,
                Model = this.Model,
                Make = this.Make,
                Class = this.Class,
                ClassName = this.ClassName,
                Type = this.Type,
                TypeName = this.TypeName,
                NumberOfCars = this.NumberOfCars,
                MaxBaggage = this.MaxBaggage,
                TransmissionType = this.TransmissionType,
                PickUpLocations = this.PickUpLocations?.Select(x => x.Clone()).ToList(),
                ReturnLocations = this.ReturnLocations?.Select(x => x.Clone()).ToList(),
                Destination = this.Destination?.Clone(),
                SelectedPickupLocation = this.SelectedPickupLocation?.Clone(),
                SelectedReturnLocation = this.SelectedReturnLocation?.Clone(),
                ContractType = this.ContractType,
                CarProduct = this.CarProduct?.Clone(),
                TravellersComposition = this.TravellersComposition?.Clone(),
                SupplierInformation = this.SupplierInformation?.Clone(),
                AirConditioned = this.AirConditioned,
                DoorCount = this.DoorCount,
                SippCode = this.SippCode,
                ManageBookingUrl = this.ManageBookingUrl
            };

        }
    }
}





