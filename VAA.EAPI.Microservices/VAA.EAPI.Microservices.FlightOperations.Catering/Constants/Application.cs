namespace VAA.EAPI.Microservices.FlightOperations.Catering.Constants
{
    public static class Application
    {       
        public static readonly string[] DateOnlyIncomingFormats = ["dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy", "d/M/yyyy"];
        public static readonly string Success = "Success";
        public static readonly string MealOrderCreateSuccessMessage = "Meal Order created successfully";
        public static readonly string MealOrderCreateFailureMessage  = "Create Meal Order failed";
        public static readonly string MealOrderUpdateSuccessMessage = "Meal Order updated successfully";
        public static readonly string MealOrderUpdateFailureMessage = "Update Meal Order failed";
        public static readonly string ValidateSegmentErrorMessage = "Error while validating segments meal eligibility ";
        public static readonly string ToEarlyOrderMessage = "Too early to order meals; Eligible for DF only";
        public static readonly string ToLateOrderMessage = "Too late to order meals; Eligible for DF only";
        public static readonly string FltNotFoundMessage = "Flight not eligible to order meals; Eligible for DF only";
        public static readonly string DataNotFoundMessage = "Data not found";
        public static readonly string CabinNonEligibleMessage = "Cabin not eligible to order meals; Eligible for DF only";




    }
}
