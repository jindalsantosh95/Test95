namespace VAA.EAPI.Microservices.Loyalty.Identity.Validation
{
    public class ValidateEmail
    {
        public static bool Validate(string email)
        {
            try
            {
                System.Net.Mail.MailAddress emailaddr = new System.Net.Mail.MailAddress(email);
                return emailaddr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
