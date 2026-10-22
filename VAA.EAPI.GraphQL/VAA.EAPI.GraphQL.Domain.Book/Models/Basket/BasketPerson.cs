namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class BasketPerson
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? ProfilePassenger { get; set; } = null;
        public string Validation { get; set; }

        private string type;
        public string Type
        {
            get => type;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    type = "";
                }
                else
                {
                    switch (value.ToUpper())
                    {
                        case "ADT":
                            type = Constants.Application.AdultPtc;
                            break;
                        case "CNN":
                            type = Constants.Application.ChildPtc;
                            break;
                        case "GBE":
                            type = Constants.Application.TeenPtc;
                            break;
                        case "INF":
                            type = Constants.Application.InfantPtc;
                            break;
                        default:
                            type = value;
                            break;
                    }
                }
            }
        }

        public FQTMembership? MembershipDetails { get; set; }
    }
}