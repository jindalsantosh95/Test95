using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.InflightProfile
{
    public class ContactDetailsMapper
    {
        public static Domain.InflightProfile.Contact MapContactDetails(Proxy.SCVPlusProfile.Customer customerDetails)
        {
            if (customerDetails.ContactDetails != null)
            {
                return new Domain.InflightProfile.Contact()
                {
                    EmailDetails = MapEmailDetails(customerDetails.ContactDetails),
                    PhoneDetails = MapPhoneDetails(customerDetails.ContactDetails)
                };
            }
            return null;
        }

        public static List<Domain.InflightProfile.Phone> MapPhoneDetails(Proxy.SCVPlusProfile.Contact contactDetails)
        {
            if (contactDetails.PhoneDetails != null)
            {
                Proxy.SCVPlusProfile.Phone scvPlusPhone = contactDetails.PhoneDetails.FirstOrDefault(p => p.Primary == "Y");
                if (scvPlusPhone != null)
                {
                    return new List<Domain.InflightProfile.Phone>()
                    {
                        new Domain.InflightProfile.Phone()
                        {
                            CountryCode = scvPlusPhone.CountryCode,
                            Description = scvPlusPhone.Description,
                            PhoneNumber = scvPlusPhone.PhoneNumber,
                            PhoneType = Enum.TryParse(Convert.ToString(scvPlusPhone.ContactSubType), out Domain.InflightProfile.Phone.PhoneTypeEnum phoneType)? phoneType : null,
                            PreferredFlag = scvPlusPhone.Primary,
                            Usage = Enum.TryParse(Convert.ToString(scvPlusPhone.Usage), out Domain.InflightProfile.Phone.UsageEnum usage) ? usage : null
                        }
                    };
                }
            }
            return null;
        }

        public static List<Domain.InflightProfile.Email> MapEmailDetails(Proxy.SCVPlusProfile.Contact contactDetails)
        {
            if (contactDetails.EmailDetails != null)
            {
                Proxy.SCVPlusProfile.Email scvPlusEmail = contactDetails.EmailDetails.FirstOrDefault(e => e.Primary == "Y");
                if (scvPlusEmail != null)
                {
                    return new List<Domain.InflightProfile.Email>()
                    {
                         new Domain.InflightProfile.Email()
                        {
                            Description = scvPlusEmail.Description,
                            EmailId = scvPlusEmail.EmailAddress,
                            PreferredFlag = scvPlusEmail.Primary,
                            Valid=scvPlusEmail.Valid,
                            Usage = Enum.TryParse(Convert.ToString(scvPlusEmail.Usage), out Domain.InflightProfile.Email.UsageEnum usage) ? usage : null
                        }
                    };
                }
            }
            return null;
        }
    }
}