namespace VAA.EAPI.Microservices.Retail.Basket.Constants
{
    public static class Application
    {
        public static readonly string InvalidStatus = "INVALID";
        public static readonly string ValidStatus = "VALID";
        public static readonly string AdultPtc = "ADULT";
        public static readonly string ChildPtc = "CHILD";
        public static readonly string InfantPtc = "INFANT";
        public static readonly string TeenPtc = "TEEN";
        public static readonly string FirstNameRegex = "^(?!.*?\\s{2})[a-zA-Z-' ]{1,30}$";
        public static readonly string LastNameRegex = "^(?!.*?\\s{2})[a-zA-Z-' ]{2,35}$";
        public static readonly string PhoneRegex = @"^(\d{9,11}$)";
        public static readonly string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public static readonly string RemoveSpecialCharacter = @"[!@#$%\^&*\(\)+=\/\\\{\}\[\]\|:;\<>,.\?\~`;]";
    }
}
