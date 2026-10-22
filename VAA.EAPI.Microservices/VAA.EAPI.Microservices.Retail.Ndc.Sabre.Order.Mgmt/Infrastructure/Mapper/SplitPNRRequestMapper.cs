using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TravelItineraryDivideLLS;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class SplitPNRRequestMapper
    {
        public static TravelItineraryDivideRQRequest DivideRQRequest(string token, IEnumerable<Customer> customers)
        {
            return new()
            {
                MessageHeader = new()
                {
                    From = new From
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },

                    To = new To
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },

                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.SplitPNRAction,
                },
                Security = new() { BinarySecurityToken = token },
                TravelItineraryDivideRQ = new()
                {
                    Version = Constants.Application.SplitPNRVersion,
                    ReturnHostCommand = Constants.Application.ReturnHostCommandvalue,
                    ReturnHostCommandSpecified = true,
                    Divide = new()
                    {
                        CustomerInfo = customers.Select(c => new TravelItineraryDivideRQDividePersonName
                         {
                             NameNumber = $"{c.LastNameNumber.TrimStart('0')}.{c.FirstNameNumber.TrimStart('0')}"

                         }).ToArray()

                    }
                }
            };
        }

        public static TravelItineraryDivideRQRequest FileSplitPNRRQ(string token)
        {
            return new()
            {
                MessageHeader = new()
                {
                    From = new From
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },

                    To = new To
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },

                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.SplitPNRAction,
                },
                Security = new()
                {
                    BinarySecurityToken = token
                },
                TravelItineraryDivideRQ = new()
                {
                    Version = Constants.Application.SplitPNRVersion,
                    ReturnHostCommand = Constants.Application.ReturnHostCommandvalue,
                    File = new()
                }
            };
        }
    }
}
