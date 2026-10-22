namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public static class ValidateEmail
    {
        public static bool Validate(string email)
        {
            System.Net.Mail.MailAddress emailaddr = new System.Net.Mail.MailAddress(email);
            return emailaddr.Address == email;

        }
    }
}
