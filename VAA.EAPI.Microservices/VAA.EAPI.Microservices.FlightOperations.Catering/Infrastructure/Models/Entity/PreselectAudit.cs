using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity
{
    public class PreselectAudit
    {
        // Composite PK parts
        public string FlightCodesAirline { get; set; }      
        public string FlightCodesSuffix { get; set; }        
        public string FlightCodesNumber { get; set; }        
        public string BookingReference { get; set; }         

        // Other columns
        public string TransactionId { get; set; }            
        public string Status { get; set; }                   
        public DateTime? OfferDateTime { get; set; }         
        public DateTime? ReminderDateTime { get; set; }      
        public DateTime? OrderDateTime { get; set; }         
        public DateTime? Created { get; set; }              
        public DateTime? Modified { get; set; }              
        public string User { get; set; }                     
        public string Referrer { get; set; }                 
        public DateTime? FlightDate { get; set; }            
        public DateTime? PromptedDateTime { get; set; }      
    }
}

