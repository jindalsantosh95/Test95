namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public static class ValidateEmail
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
