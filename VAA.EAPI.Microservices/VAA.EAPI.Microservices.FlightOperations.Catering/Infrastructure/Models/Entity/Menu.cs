using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity
{
    public class Menu
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public DateTime? Effective_From { get; set; }

        public DateTime? Effective_To { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        public string User { get; set; }

        public string Nutrition { get; set; }

        public string Allergies { get; set; }

        public string Categories { get; set; } 

        public string Tags { get; set; }       

        public string Sign_Postings { get; set; } 

        public string Dish_Id { get; set; }    

        public string Meal_Short_Name { get; set; }

        public string Meal_Type { get; set; }

        public string Colour { get; set; }
    }

}