namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class EntraSignInRequest
    {
        public string Email { get; set; }
        public string FlyingClubNumber { get; set; }
        public string Password { get; set; }
        public AccountSignInCustomerProfile CustomerProfile { get; set; }

        public static explicit operator EntraSignInRequest((string email, string flyingClubNumber, string password, AccountSignInCustomerProfile customerProfile) input)
        {
            var email = input.email;
            var flyingClubNumber = input.flyingClubNumber;
            var password = input.password;
            var customerProfile = input.customerProfile;

            return new EntraSignInRequest { Email = email, FlyingClubNumber = flyingClubNumber, Password = password, CustomerProfile = customerProfile };
        }
    }
}
