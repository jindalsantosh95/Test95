using Microsoft.AspNetCore.Mvc.ApplicationModels;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Mapper
{
    public static class EnhancedEndTransactionRQMapper
    {
        public static EnhancedEndTransactionRQRequest EnhancedEndTransactionRQ(string token, bool actionCode)
        {
            if (actionCode)
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

                        ConversationId = Constants.Application.EnhancedEndTransactionAction,
                        Action = Constants.Application.EnhancedEndTransactionAction,
                    },
                    EnhancedEndTransactionRQ = new()
                    {
                        version = Constants.Application.EnhancedEndTransactionRQVersion,
                        EndTransaction = new EnhancedEndTransactionRQEndTransaction()
                        {
                            Ind = true,
                            ScheduleChange = new EnhancedEndTransactionRQEndTransactionScheduleChange() { Ind = true }
                        },
                        Source = new EnhancedEndTransactionRQSource() { ReceivedFrom = Constants.Application.ReceivedFrom },

                    },
                    Security = new() { BinarySecurityToken = token }
                };
            }
            else
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

                        ConversationId = Constants.Application.EnhancedEndTransactionAction,
                        Action = Constants.Application.EnhancedEndTransactionAction,
                    },
                    EnhancedEndTransactionRQ = new()
                    {
                        version = Constants.Application.EnhancedEndTransactionRQVersion,
                        EndTransaction = new EnhancedEndTransactionRQEndTransaction()
                        {
                            Ind = true
                        },
                        Source = new EnhancedEndTransactionRQSource() { ReceivedFrom = Constants.Application.ReceivedFrom },

                    },
                    Security = new() { BinarySecurityToken = token }
                };

            }

        }
    }
}

