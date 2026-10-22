using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public static class ValidateAge
    {
        public static bool Validate(string dateofBirth, int ageLimit)
        {
            DateTime customerDob = Convert.ToDateTime(dateofBirth).Date;
            int customerAge = Helper.AgeCalculate.CalculateAge(customerDob);
            if (customerAge < ageLimit)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
