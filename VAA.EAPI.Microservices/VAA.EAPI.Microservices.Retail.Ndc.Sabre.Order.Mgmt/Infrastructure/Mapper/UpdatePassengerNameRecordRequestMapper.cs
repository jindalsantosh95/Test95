using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Models;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class UpdatePassengerNameRecordRequestMapper
    {
        public static UpdatePassengerNameRecordRQRequest MapUpdatePassengerNameRecordRequest(OrderChangeRequestData requestData, string token)
        {
            var customer = requestData?.DataList?.CustomerDetails?.FirstOrDefault();
            var contact = customer?.Contacts;
            var nameNumber = customer is null ? null : $"{customer.LastNameNumber}.{customer.FirstNameNumber}";

            return new UpdatePassengerNameRecordRQRequest
            {
                UpdatePassengerNameRecordRQ = new()
                {
                    version = Constants.Application.UpdatePNRVersion,
                    Itinerary = new() { id = requestData?.Rloc },
                    TravelItineraryAddInfo = new()
                    {
                        AgencyInfo = new()
                        {
                            Ticketing = new()
                            {
                                TicketType = "7TAW"
                            }
                        },
                        CustomerInfo = new()
                        {
                            ContactNumbers = string.IsNullOrWhiteSpace(contact?.Phone)
                                ? null
                                : [new()
                                {
                                    Phone = contact.Phone,
                                    NameNumber = nameNumber,
                                    PhoneUseType = string.IsNullOrWhiteSpace(contact.PhoneType) ? "H" : contact.PhoneType
                                }],
                            Email = string.IsNullOrWhiteSpace(contact?.Email)
                                ? null
                                : [new()
                                {
                                    Address = contact.Email,
                                    NameNumber = nameNumber
                                }]
                        }
                    },
                    PostProcessing = new()
                    {
                        EndTransaction = new()
                        {
                            Source = new()
                            {
                                ReceivedFrom = Constants.Application.SabreSource
                            }
                        }
                    }
                },
                MessageHeader = new()
                {
                    Action = Constants.Application.UpdatePNRAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }
            };
        }
    }
}
