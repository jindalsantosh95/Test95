using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Customer : Party
    {
        public string IndividualID { get; set; }
        public string ContactTypeText { get; set; }
        public string CustID { get; set; }
        public string PTC { get; set; }
        public string FirstName { get; set; }
        public string FirstNameAlias { get; set; }
        public string LastName { get; set; }
        public string LastNameAlias { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string AuthenticationNonce { get; set; }
        public LifeTimeValue ValueMeasures { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        
    }
}
