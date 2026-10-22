using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public static class ValidateAge
    {
        public static bool Validate(string dateofBirth, int ageLimit)
        {
            DateTime customerDob = Convert.ToDateTime(dateofBirth).Date;
            return AgeCalculate.CalculateAge(customerDob) > ageLimit;
        }
    }
}
