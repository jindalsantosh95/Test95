using System;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;

namespace VAA.EAPI.Web.Book.Domain.Basket.Requests
{
    public class People
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? ProfilePassenger { get; set; } = null;
        public string Validation { get; set; }
        public string Type { get; set; }
        public FQTMembership MembershipDetails { get; set; }
    }
}