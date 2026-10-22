using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ContactTracing
{
    public class ContactTracing
    {
        public string? PassengerId { get; set; }
        public string? StreetAddress { get; set; }
        public string? CountryCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public bool? InTransit { get; set; }
        public string? Email { get; set; }
        public bool? NoEmail { get; set; }
        public string? PrimaryPhoneNumber { get; set; }
        public string? PrimaryPhoneNumberCountryCode { get; set; }
        public string? SecondaryPhoneNumber { get; set; }
        public string? SecondaryPhoneNumberCountryCode { get; set; }
        public bool? NoSecondaryPhoneNumber { get; set; }

        public static explicit operator ContactTracing(DeltaTrm.ContactInformation source)
        {
            var primaryPhone = source.Phone.FirstOrDefault(p => p.Preference == DeltaTrm.PhoneInfoPreference.PRIMARY);
            var secondaryPhone = source.Phone.FirstOrDefault(p => p.Preference == DeltaTrm.PhoneInfoPreference.SECONDARY);

            var contactTracing = new ContactTracing
            {
                Email = source.Email?.FirstOrDefault()?.Value,
                NoEmail = source.Email?.FirstOrDefault()?.Declined == true,
                PrimaryPhoneNumber = primaryPhone?.PhoneNumber,
                PrimaryPhoneNumberCountryCode = primaryPhone?.CountryCode,
                SecondaryPhoneNumber = secondaryPhone?.PhoneNumber,
                SecondaryPhoneNumberCountryCode = secondaryPhone?.CountryCode,
                NoSecondaryPhoneNumber = secondaryPhone?.Declined == true,
            };
            if (source.Address.StreetAddress == Constants.Application.ContactTracingInTransit)
            {
                contactTracing.InTransit = true;
            }
            else
            {
                contactTracing.InTransit = false;
                contactTracing.StreetAddress = source.Address.StreetAddress;
                contactTracing.City = source.Address.CityCode;
                contactTracing.State = source.Address.StateCode;
                contactTracing.Zip = source.Address.ZipCode;
            }
            return contactTracing;
        }

        public static bool operator ==(ContactTracing a, ContactTracing b)
        {
            return a.StreetAddress == b.StreetAddress
                && a.City == b.City
                && a.State == b.State
                && a.Zip == b.Zip
                && a.Email == b.Email
                && a.NoEmail == b.NoEmail
                && a.PrimaryPhoneNumber == b.PrimaryPhoneNumber
                && a.PrimaryPhoneNumberCountryCode == b.PrimaryPhoneNumberCountryCode
                && a.SecondaryPhoneNumber == b.SecondaryPhoneNumber
                && a.SecondaryPhoneNumberCountryCode == b.SecondaryPhoneNumberCountryCode
                && a.NoSecondaryPhoneNumber == b.NoSecondaryPhoneNumber;
        }

        public static bool operator !=(ContactTracing a, ContactTracing b)
        {
            return (a == b) == false;
        }

        public static bool Equals(ContactTracing a, ContactTracing b)
        {
            return a == b;
        }

        public static explicit operator DeltaTrm.ContactInformation(ContactTracing request)
        {
            return new DeltaTrm.ContactInformation
            {
                Address = request.InTransit == true
                            ? new DeltaTrm.AddressInfo
                            {
                                StreetAddress = Constants.Application.ContactTracingInTransit
                            }
                            : new DeltaTrm.AddressInfo
                            {
                                StreetAddress = request.StreetAddress,
                                CountryCode = request.CountryCode ?? Constants.Application.ContactTracingCountry,
                                StateCode = request.State,
                                CityCode = request.City,
                                ZipCode = request.Zip,
                                Declined = false,
                            },

                Email = new List<DeltaTrm.EmailInfo> { new()
                {
                    Value = request.Email,
                    Declined = request.Email.IsNullOrEmpty()
                }},

                Phone = new List<DeltaTrm.PhoneInfo> { new()
                    {
                        Type = DeltaTrm.PhoneInfoType.MOBILE,
                        Preference = DeltaTrm.PhoneInfoPreference.PRIMARY,
                        CountryCode = request.PrimaryPhoneNumberCountryCode,
                        PhoneNumber = request.PrimaryPhoneNumber,
                        Declined = false
                    },
                    new()
                    {
                        Type = DeltaTrm.PhoneInfoType.MOBILE,
                        Preference = DeltaTrm.PhoneInfoPreference.SECONDARY,
                        CountryCode = request.SecondaryPhoneNumber.IsNullOrEmpty() ? null : request?.SecondaryPhoneNumberCountryCode,
                        PhoneNumber = request?.SecondaryPhoneNumber,
                        Declined = request.SecondaryPhoneNumber.IsNullOrEmpty() 
                    }
                }
            };
        }
    }
}
