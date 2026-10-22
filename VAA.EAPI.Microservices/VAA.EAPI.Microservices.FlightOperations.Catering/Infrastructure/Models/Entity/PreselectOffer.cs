using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity
{

    public class PreselectOffer
    {
        public int Menus_Id { get; set; } 

        public string Meal_Code { get; set; }

        public string Flight_Codes_Airline { get; set; }

        public string Flight_Codes_Number { get; set; }

        public string Flight_Codes_Suffix { get; set; }

        public string Cabin_Code { get; set; }

        public int Display_Sequence { get; set; }

        public DateTime Offer_Effective_From { get; set; }

        public DateTime Offer_Effective_To { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Modified { get; set; }

        public string User { get; set; }

        public bool Enabled { get; set; }

        public string Action_By { get; set; }

        public DateTime? Action_DateTime { get; set; }

        public int? Bkg_Win_HWatermark { get; set; }

        public int? Bkg_Win_LWatermark { get; set; }

        public int? Bkg_Win_RemindHWatermark { get; set; }

        public int? Bkg_Win_HD9Mail { get; set; }

        public int? Bkg_Win_LD9Mail { get; set; }

        public Menu Menu { get; set; }
    }
}


