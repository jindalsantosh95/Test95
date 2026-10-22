using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PartnerProfile
    {
        [GraphQLDescription("The unique ID of Partner")]
        public string PartnerId { get; set; }
        [GraphQLDescription("The Partner Name")]
        public string Name { get; set; }
        [GraphQLDescription("The unique Partner Code")]
        public string PartnerCode { get; set; }
        [GraphQLDescription("The type of Partner")]
        public string PartnerType { get; set; }
        [GraphQLDescription("The subtype of Partner")]
        public string PartnerSubtype { get; set; }
        [GraphQLDescription("The Partner active status")]
        public bool Active { get; set; }
        [GraphQLDescription("The Partner Codeshare flag")]
        public string CodeShareFlag { get; set; }
        [GraphQLDescription("The Partner Exception Booking Class flag")]
        public string ExceptionBookingClassFlag { get; set; }
        [GraphQLDescription("The Partner auto retro flag")]
        public string AutomatedRetroPartner { get; set; }
        [GraphQLDescription("Maximum transaction limit allowed for the Partner")]
        public long? MaxTxnLimitForPoints { get; set; }
        [GraphQLDescription("Start Date of the Partner")]
        public DateTime? PartnerStartDate { get; set; }
        [GraphQLDescription("End Date of the Partner")]
        public DateTime? PartnerEndDate { get; set; }
    }
}