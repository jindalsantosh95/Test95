using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.NDC.Sabre.Models;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class SplitPNRUpdatePassengerNameRecordMapper
    {
        public static UpdatePassengerNameRecordRQRequest UpdatePnrRequestMapper(string token, IEnumerable<Customer> customers, GetReservationRS getReservationRS, string rloc)
        {
            return new()
            {
                MessageHeader = new()
                {
                    From = new VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord.From
                    {
                        PartyId = [new VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord.PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },

                    To = new VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord.To
                    {
                        PartyId = [new VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord.PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },

                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.UpdatePNRAction,
                },
                Security = new() { BinarySecurityToken = token },
                UpdatePassengerNameRecordRQ = new()
                {
                    version = Constants.Application.UpdatePNRVersion,
                    Itinerary = new()
                    {
                        id = rloc,
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
                    },
                    SpecialReqDetails = new()
                    {
                        SpecialService = new()
                        {
                            SpecialServiceInfo = new()
                            {
                                Service = GetService(customers, getReservationRS, rloc)
                            }
                        }
                    }
                },
            };


        }

        private static UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService[] GetService(IEnumerable<Customer> customers, GetReservationRS getReservationRS, string rloc)
        {
            var customerInfo = customers.Where(x => x.IsEmailUpdates);
            var passenger = (getReservationRS.Item as ReservationPNRB).PassengerReservation.Passengers.Passenger;
            var servicesInfo = new List<UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService>();
            foreach (var pax in passenger)
            {
                var matchingCustomer = customerInfo.FirstOrDefault(t => pax.FirstName == t.FirstName && pax.LastName == t.LastName);

                if (matchingCustomer != null)
                {
                    var nameId = pax.nameId;
                    var service = new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoService
                    {
                        SSR_Code = "CTCE",
                        SegmentNumber = "A",
                        PersonName = new UpdatePassengerNameRecordRQSpecialReqDetailsSpecialServiceSpecialServiceInfoServicePersonName()
                        {
                            NameNumber = nameId
                        },
                        Text = matchingCustomer.Contacts.Email.Replace("@", "//")
                    };
                    servicesInfo.Add(service);
                }
            }
            return servicesInfo.ToArray();
        }
    }
}
