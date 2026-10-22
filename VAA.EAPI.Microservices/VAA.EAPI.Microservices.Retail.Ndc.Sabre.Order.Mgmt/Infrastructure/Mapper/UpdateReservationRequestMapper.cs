using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class UpdateReservationRequestMapper
    {
        public static UpdateReservationOperationRequest GetUpdateReservationRequest(string token, ManageCustomerRequest request, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            return new()
            {
                UpdateReservationRQ = UpdateReservationReqMapper(request, passengers),
                MessageHeader = new()
                {
                    Action = Constants.Application.UpdateReservationAction,
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

        private static UpdateReservationRQ UpdateReservationReqMapper(ManageCustomerRequest request, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            return new()
            {
                Version = "1.19.8",
                RequestType = new() { commitTransaction = true, commitTransactionSpecified = true, initialIgnore = false, initialIgnoreSpecified = true, Value = RequestEnumerationType.Stateful },
                ReturnOptions = new() { RetrievePNR = true },
                ReservationUpdateList = GetReservationUpdateList(request, passengers)
            };
        }

        private static ReservationUpdateListType GetReservationUpdateList(ManageCustomerRequest request, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            var reservationUpdateItem = new List<ReservationUpdateItemType>();
            int updateId = 1;
            IEnumerable<EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.APISRequestPNRB> getReservationApisDocs = null;
            string matchingAmendApisDocId = null;
            var customerDetails = request.CustomerDetails.First();
            var paxName = passengers.First(x => x.nameId.Split(".").First() == customerDetails.LastNameNumber);
            var associatedAdult = customerDetails.Ptc == Constants.Application.Infant ? passengers.FirstOrDefault(x => x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[2] == paxName.FirstName && x.SpecialRequests?.GenericSpecialRequest?.FirstOrDefault(c => c.Code == "INFT")?.FreeText?.Split("/")?[1] == paxName.LastName) : null;
            var infantApisDocs = associatedAdult == null ? null : associatedAdult.SpecialRequests.APISRequest.Where(x => x.DOCSEntry != null && x.DOCSEntry.Forename == paxName.FirstName && x.DOCSEntry.Surname == paxName.LastName || x.DOCOEntry != null && x.DOCOEntry.FreeText.Contains("/I"));
            var infantSsrs = associatedAdult == null ? null : associatedAdult.SpecialRequests?.GenericSpecialRequest?.Where(x => x.Code == "INFT");

            getReservationApisDocs = customerDetails.Ptc == Constants.Application.Infant ? infantApisDocs : passengers.First(x => x.passengerType == (customerDetails.Ptc == "CHD" ? "CNN" : customerDetails.Ptc == "GBE" ? "YTH" : customerDetails.Ptc) && x.nameId.Split(".").First() == customerDetails.LastNameNumber).SpecialRequests?.APISRequest.Where(x => (x.DOCSEntry?.Forename == paxName.FirstName && x.DOCSEntry?.Surname == paxName.LastName) || x.DOCOEntry != null && !x.DOCOEntry.FreeText.Contains("/I"));

            foreach (var reservationApisDoc in getReservationApisDocs)
            {
                matchingAmendApisDocId = request.TicketedNameCorrection || request.UnTicketedNameCorrection ? ((reservationApisDoc.DOCSEntry?.Forename != null && reservationApisDoc.DOCSEntry?.Surname != null) ? reservationApisDoc.DOCSEntry?.id : null) :
                                                  (from changeApisDoc in customerDetails.TravelDocuments
                                                   where (changeApisDoc.DocumentType == "PT" && reservationApisDoc.DOCSEntry?.DocumentType == "P") ||
                                                         (changeApisDoc.DocumentType == "GC" && reservationApisDoc.DOCSEntry?.DocumentType == "C") ||
                                                         (changeApisDoc.DocumentType == "VI" && reservationApisDoc.DOCOEntry != null && reservationApisDoc.DOCOEntry.FreeText.Contains("/V/")) ||
                                                         (changeApisDoc.DocumentType == "CR" && reservationApisDoc.DOCOEntry != null && reservationApisDoc.DOCOEntry.FreeText.Contains("/K/")) ||
                                                         (changeApisDoc.DocumentType == "AEA" && reservationApisDoc.DOCOEntry != null && reservationApisDoc.DOCOEntry.FreeText.Contains("/R/"))
                                                   select (changeApisDoc.DocumentType == "PT" || changeApisDoc.DocumentType == "GC") ? reservationApisDoc.DOCSEntry?.id : reservationApisDoc.DOCOEntry?.id).FirstOrDefault();

                if (matchingAmendApisDocId != null)
                {
                    reservationUpdateItem.Add(new ReservationUpdateItemType
                    {
                        UpdateId = updateId.ToString(),
                        Items = new SpecialServiceRequestUpdatePNRB[]
                        {
                            deleteApisDocs(matchingAmendApisDocId)
                        }
                    });
                    updateId++;
                }
            }

            if (customerDetails.Ptc == Constants.Application.Infant && (request.TicketedNameCorrection || request.UnTicketedNameCorrection))
            {
                foreach(var ssrDocs in infantSsrs)
                {
                    reservationUpdateItem.Add(new ReservationUpdateItemType
                    {
                        UpdateId = updateId.ToString(),
                        Items = new SpecialServiceRequestUpdatePNRB[]
                        {
                            deleteApisDocs(ssrDocs.id)
                        }
                    });
                    updateId++;
                }
            }

            if (request.TicketedNameCorrection || request.UnTicketedNameCorrection)
            {
                var matchingNameDocId = passengers.First(x => x.passengerType == (customerDetails.Ptc == "CHD" ? "CNN" : customerDetails.Ptc == "GBE" ? "YTH" : customerDetails.Ptc) && x.nameId.Split(".").First() == customerDetails.LastNameNumber).id;

                var travelerName = new TravelerName()
                {
                    Given = customerDetails.FirstName,
                    Surname = customerDetails.LastName
                };

                if (customerDetails.Ptc == Constants.Application.Infant)
                {
                    travelerName.type = TravelerType.INF;
                }
                reservationUpdateItem.Add(new ReservationUpdateItemType
                {
                    UpdateId = updateId.ToString(),
                    Items = new PassengerNameUpdatePNRB[]
                    {
                        new()
                        {
                            id = matchingNameDocId,
                            op = OperationTypePNRB.U,
                            opSpecified = true,
                            TravelerName = travelerName,
                            NameAssociationTag = new()
                            {
                                NameRefNumber = customerDetails.LastNameNumber
                            }
                        }
                    }
                });
                updateId++;
            }

            return new ReservationUpdateListType
            {
                Items = reservationUpdateItem.ToArray(),
                ReceivedFrom = new() { Name = Constants.Application.ReceivedFromName },
            };
        }

        private static SpecialServiceRequestUpdatePNRB deleteApisDocs(string id)
        {
            return new SpecialServiceRequestUpdatePNRB
            {
                id = id,
                op = OperationTypePNRB.D,
                opSpecified = true,
                type = FactTypePNRB.G
            };
        }
    }
}
