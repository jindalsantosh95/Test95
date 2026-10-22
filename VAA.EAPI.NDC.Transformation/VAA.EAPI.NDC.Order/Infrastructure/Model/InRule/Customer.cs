using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using SourceCustomer = VAA.EAPI.Domain.Virgin.Models.Customer.Customer;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class Customer
    {
        public string CustId { get; set; }
        public string Ptc { get; set; }
        public string ItaPtc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Contacts Contacts { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public bool? IsNameChanged { get; set; }
        public bool? IsApisAdded { get; set; }
        public bool? IsEmailUpdates { get; set; }

        public static explicit operator Customer(SourceCustomer source) =>
            source is null
                ? null
                : new Customer
                {
                    CustId = source.CustId,
                    Ptc = source.Ptc,
                    ItaPtc = source.ItaPtc,
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    Contacts = source.Contacts,
                    FirstNameNumber = source.FirstNameNumber,
                    LastNameNumber = source.LastNameNumber,
                    IsNameChanged = source.IsNameChanged,
                    IsApisAdded = source.IsApisAdded,
                    IsEmailUpdates = source.IsEmailUpdates
                };
    }
}
