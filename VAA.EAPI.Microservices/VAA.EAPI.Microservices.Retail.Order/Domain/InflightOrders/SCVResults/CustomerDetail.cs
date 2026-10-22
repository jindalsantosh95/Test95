using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults
{
    public class CustomerDetail
    {
        public string FirstNameNumber { get; set; }
        public string CustomerSubType { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string GenderOnDocument { get; set; }
        public FrequentFlyer FrequentFlyer { get; set; }
        public string CustomerRiskValue { get; set; }
        public string CustomerValue { get; set; }
        public string CustomerType { get; set; }
        public string Title { get; set; }
        public string CustID { get; set; }
        public string FirstName { get; set; }
        public string CountryOfIssue { get; set; }
        public string DocumentFirstName { get; set; }
        public string DocmumentLastName { get; set; }
        public string Bookingtype { get; set; }
        public string Nationality { get; set; }
        public string DocumentType { get; set; }
        public string VAASCVID { get; set; }
        public string DocumentNumber { get; set; }
        public string DOBOnDocument { get; set; }
        public string ExpiryDate { get; set; }
        public CustomerContacts CustomerContacts { get; set; }
        public string ContactTypeText { get; set; }
        public string DocumentMiddleName { get; set; }
        public string LastNameNumber { get; set; }
        public string DocumentTitle { get; set; }
        public string PTC { get; set; }
        public string CustomerLifeTimeValue { get; set; }
        public string VirginHolidayBookingFlag { get; set; }
        public string CorporateIndicator { get; set; }
        public string CorporateAccountName { get; set; }
        public string StaffTravellerIndicator { get; set; }
        public string StaffTravellerType { get; set; }
        public List<CustomerService> CustomerService { get; set; }
    }
}
