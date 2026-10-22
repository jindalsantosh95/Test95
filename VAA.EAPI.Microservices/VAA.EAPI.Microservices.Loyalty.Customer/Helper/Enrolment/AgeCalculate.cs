using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class AgeCalculate
    {

        public static int CalculateAge(DateTime dateofBirth)
        {

            int age = 0, dayofyear = 0;
            if (DateTime.IsLeapYear(dateofBirth.Year) && dateofBirth.DayOfYear >= 60)
            {
                dayofyear = dateofBirth.DayOfYear - 1;
            }
            else
            {
                dayofyear = dateofBirth.DayOfYear;
            }
            age = DateTime.UtcNow.Year - dateofBirth.Year;
            if (DateTime.UtcNow.DayOfYear < dayofyear)
            {
                age--;
            }
            return age;
        }
    }
}
