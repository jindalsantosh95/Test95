using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class PNRRetrieveHistoryRSMapper
    {
        public static Models.PNRCommonV6RetrieveResponse MapToPnrRetrieveResponse(Proxy.PNRCommonV6Proxy.RetrieveResponseType retrieveResponseType)
        {
            return retrieveResponseType?.TeardownContextResponse?.Pnr != null
                ? new PNRCommonV6RetrieveResponse()
                {
                    PnrType = new Pnr()
                    {
                        HistoryCreditOrSignatures = retrieveResponseType.TeardownContextResponse.Pnr.HistoryCreditOrSignature != null ? GetHistoryCreditOrSignature(retrieveResponseType.TeardownContextResponse.Pnr.HistoryCreditOrSignature) : null,
                        TicketInformation = retrieveResponseType.TeardownContextResponse.Pnr.TicketInformation != null ? GetTicketInformation(retrieveResponseType.TeardownContextResponse.Pnr.TicketInformation) : null,
                        Passengers = retrieveResponseType.TeardownContextResponse.Pnr.Passenger != null ? GetPassengerDetails(retrieveResponseType.TeardownContextResponse.Pnr.Passenger) : null,
                        Ptcs = retrieveResponseType.TeardownContextResponse.Pnr.Ptc != null ? GetPtcInformation(retrieveResponseType.TeardownContextResponse.Pnr.Ptc) : null,
                        InfantInArms = retrieveResponseType.TeardownContextResponse.Pnr.InfantInArms != null ? GetInfantDetails(retrieveResponseType.TeardownContextResponse.Pnr.InfantInArms) : null,
                        FareCalcs = retrieveResponseType.TeardownContextResponse.Pnr.FareCalc != null ? GetFareCalculation(retrieveResponseType.TeardownContextResponse.Pnr.FareCalc) : null,
                        Dtis = retrieveResponseType.TeardownContextResponse.Pnr.Dti != null ? GetDtis(retrieveResponseType.TeardownContextResponse.Pnr.Dti) : null,
                        SegmentDetails = retrieveResponseType.TeardownContextResponse.Pnr.SegmentDl != null ? GetSegmentDetails(retrieveResponseType.TeardownContextResponse.Pnr.SegmentDl) : null,
                        FrequentFlyer = retrieveResponseType.TeardownContextResponse.Pnr.FrequentFlyer != null ? GetFrequentFlyerDetails(retrieveResponseType.TeardownContextResponse.Pnr.FrequentFlyer) : null,
                        DtiSegmentInfos = retrieveResponseType.TeardownContextResponse.Pnr.DtiSegmentInfo != null ? GetDtiSegmentInfoDetails(retrieveResponseType.TeardownContextResponse.Pnr.DtiSegmentInfo) : null,
                    }
                }
                : null;
        }

        private static List<Domain.Virgin.Models.Customer.InfantInArms> GetInfantDetails(IEnumerable<Proxy.PNRCommonV6Proxy.InfantInArms> infantInArms)
        {
            List<Domain.Virgin.Models.Customer.InfantInArms> infantInArmDetails = new();
            foreach (var inf in infantInArms)
            {
                infantInArmDetails.Add(new Domain.Virgin.Models.Customer.InfantInArms()
                {
                    AddedFromMergeFlag = inf.AddedFromMergeFlag,
                    CustomerId = inf.CustomerId,
                    FirstName = inf.FirstName,
                    FirstNameFull = inf.FirstNameFull,
                    FirstNameNumber = inf.FirstNameNumber,
                    LastName = inf.LastName,
                    LastNameNumber = inf.LastNameNumber,
                    MiddleNameFull = inf.MiddleNameFull,
                    NonRevenueStandbyFlag = inf.NonRevenueStandbyFlag,
                    RecordName = inf.RecordName,
                    Remarks = inf.NameRemarks,
                    RevenueStandbyFlag = inf.RevenueStandbyFlag,
                    SegmentNumber = inf.SegmentNumber,
                    Suffix = inf.Suffix,
                    Title = inf.Title
                });
            }
            return infantInArmDetails;
        }

        private static List<PassengerTypeInformation> GetPtcInformation(IEnumerable<Ptc> ptc)
        {
            List<PassengerTypeInformation> passengerTypeInformation = new();
            foreach (var p in ptc)
            {
                passengerTypeInformation.Add(new PassengerTypeInformation()
                {
                    FirstNameNumber = p.FirstNameNumber,
                    LastNameNumber = p.LastNameNumber,
                    ApplyToAllFlag = p.ApplyToAllFlag,
                    PassengerTypeCode = p.PassengerTypeCode,
                    RecordName = p.RecordName,
                    SegmentNumber = p.SegmentNumber
                });
            }
            return passengerTypeInformation;
        }

        private static List<Domain.Virgin.Models.Customer.Passenger> GetPassengerDetails(IEnumerable<Proxy.PNRCommonV6Proxy.Passenger> passengerInfo)
        {
            List<Domain.Virgin.Models.Customer.Passenger> passengers = new();
            foreach (var pi in passengerInfo)
            {
                passengers.Add(new Domain.Virgin.Models.Customer.Passenger()
                {
                    SegmentNumber = pi.SegmentNumber,
                    CustomerId = pi.CustomerId,
                    FirstName = pi.FirstName,
                    FirstNameNumber = pi.FirstNameNumber,
                    NonRevenueStandbyFlag = pi.NonRevenueStandbyFlag,
                    RevenueStandbyFlag = pi.RevenueStandbyFlag,
                    LastName = pi.LastName,
                    LastNameNumber = pi.LastNameNumber,
                    RecordName = pi.RecordName,
                    Remarks = pi.Remarks,
                    Suffix = pi.Suffix,
                    Title = pi.Title
                });
            }
            return passengers;
        }

        private static List<Domain.Virgin.Models.Order.TicketInformation> GetTicketInformation(IEnumerable<Proxy.PNRCommonV6Proxy.TicketInformation> ticketInformation)
        {
            List<Domain.Virgin.Models.Order.TicketInformation> tickets = new();
            foreach (var t in ticketInformation)
            {
                tickets.Add(new Domain.Virgin.Models.Order.TicketInformation()
                {
                    TicketNumber = t.TicketNumber,
                    FirstNameNumber = t.FirstNameNumber,
                    LastNameNumber = t.LastNameNumber,
                    ConnectionTicketNumber = t.ConnectionTicketNumber,
                    DigitCheck = t.DigitCheck,
                    IssueDate = t.IssueDate,
                    RecordName = t.RecordName,
                    TicketName = t.TicketName,
                    TicketStatus = t.TicketStatus,
                    TicketType1 = t.TicketType1,
                    TicketType2 = t.TicketType2,
                    TypeDescription = t.TypeDescription
                }
                );
            }
            return tickets;
        }

        private static List<Domain.Virgin.Models.Order.HistoryCreditOrSignature> GetHistoryCreditOrSignature(IEnumerable<Proxy.PNRCommonV6Proxy.HistoryCreditOrSignature> historyCreditOrSignature)
        {
            List<Domain.Virgin.Models.Order.HistoryCreditOrSignature> historyCredits = new();
            foreach (var h in historyCreditOrSignature)
            {
                historyCredits.Add(new Domain.Virgin.Models.Order.HistoryCreditOrSignature()
                {
                    CreditText = h.CreditText
                });
            }
            return historyCredits;
        }

        private static IEnumerable<Domain.Virgin.Models.Order.FareCalc> GetFareCalculation(IEnumerable<Proxy.PNRCommonV6Proxy.FareCalc> fareCalcs)
        {
            return fareCalcs.Select(fareCalc => new Domain.Virgin.Models.Order.FareCalc
            {
                DTIType = fareCalc.DTIType,
                FirstNameNumber = fareCalc.FirstNameNumber,
                FreeformText = fareCalc.FreeformText,
                LastNameNumber = fareCalc.LastNameNumber,
                RecordName = fareCalc.RecordName,
                SegmentNumber = fareCalc.SegmentNumber,
            });
        }

        private static IEnumerable<Domain.Virgin.Models.Order.Dti> GetDtis(IEnumerable<Proxy.PNRCommonV6Proxy.Dti> dtis)
        {
            return dtis.Select(dti => new Domain.Virgin.Models.Order.Dti
            {
                AdvisoryMessage = dti.AdvisoryMessage,
                BaseAmount = dti.BaseAmount,
                BaseCurrency = dti.BaseCurrency,
                BooksCount = dti.BooksCount,
                DtiCount = dti.DtiCount,
                DTIType = dti.DTIType,
                EquivalentFarePaidAmount = dti.EquivalentFarePaidAmount,
                EquivalentFarePaidCurrency = dti.EquivalentFarePaidCurrency,
                EquivalentFarePaidMinimumFlag = dti.EquivalentFarePaidMinimumFlag,
                ExpandedPassengerTypeCode = dti.ExpandedPassengerTypeCode,
                FarecalcType = dti.FarecalcType,
                FirstNameNumber = dti.FirstNameNumber,
                InternationalFlag = dti.InternationalFlag,
                InternationalSalesCode = dti.InternationalSalesCode,
                LastNameNumber = dti.LastNameNumber,
                OverallMileageTableValue = dti.OverallMileageTableValue,
                PassengerTypeCode = dti.PassengerTypeCode,
                RecordName = dti.RecordName,
                SegmentNumber = dti.SegmentNumber,
                Tax1Amount = dti.Tax1Amount,
                Tax1Type = dti.Tax1Type,
                Tax2Amount = dti.Tax2Amount,
                Tax2Type = dti.Tax2Type,
                Tax3Amount = dti.Tax3Amount,
                Tax3Type = dti.Tax3Type,
                TaxItemCount = dti.TaxItemCount,
                TotalFareAmount = dti.TotalFareAmount,
                TotalFareCurrency = dti.TotalFareCurrency,
                TotalFareType = dti.TotalFareType,
                TotalTax = dti.TotalTax
            });
        }

        private static IEnumerable<Domain.Virgin.Models.Order.SegmentDetail> GetSegmentDetails(IEnumerable<Proxy.PNRCommonV6Proxy.SegmentDl> segmentDetails)
        {
            return segmentDetails.Select(segmentDetail => new Domain.Virgin.Models.Order.SegmentDetail
            {
                ActiveFlag = segmentDetail.ActiveFlag,
                AirlineCode = segmentDetail.AirlineCode,
                ArrivalPoint = segmentDetail.ArrivalPoint,
                ArrivalTime = segmentDetail.ArrivalTime,
                AscMessageFlag = segmentDetail.AscMessageFlag,
                AtlanticExcellenceRevenueValue = segmentDetail.AtlanticExcellenceRevenueValue,
                BackupCheckinSystemActiveThisFlightFlag = segmentDetail.BackupCheckinSystemActiveThisFlightFlag,
                BinaryFareTypeCode = segmentDetail.BinaryFareTypeCode,
                BlockSpaceControlFlag = segmentDetail.BlockSpaceControlFlag,
                ClassOfServiceOverBookedFlag = segmentDetail.ClassOfServiceOverBookedFlag,
                CancelFlag = segmentDetail.CancelFlag,
                ChangeStatusFlag = segmentDetail.ChangeStatusFlag,
                ClassOfService = segmentDetail.ClassOfService,
                ClassOfService2 = segmentDetail.ClassOfService2,
                CodeShareClassOfService = segmentDetail.CodeShareClassOfService,
                CodeShareParticipantFlag = segmentDetail.CodeShareParticipantFlag,
                CompartmentNumber = segmentDetail.CompartmentNumber,
                CompartmentOverBookedFlag = segmentDetail.CompartmentOverBookedFlag,
                ConditionalCancelAndSellFlag = segmentDetail.ConditionalCancelAndSellFlag,
                ConsideredForMscSelectTicketFlag = segmentDetail.ConsideredForMscSelectTicketFlag,
                ContinuingFlag = segmentDetail.ContinuingFlag,
                CurrentActionCode = segmentDetail.CurrentActionCode,
                DeltaConnectionCarrierFlag = segmentDetail.DeltaConnectionCarrierFlag,
                DepartureDate = segmentDetail.DepartureDate,
                DepartureDateAdjustment = segmentDetail.DepartureDateAdjustment,
                DepartureDateAdjustmentNegativeFlag = segmentDetail.DepartureDateAdjustmentNegativeFlag,
                DepartureDayOfWeekNumber = segmentDetail.DepartureDayOfWeekNumber,
                DeparturePoint = segmentDetail.DeparturePoint,
                DepartureTime = segmentDetail.DepartureTime,
                DirectConnectFlag = segmentDetail.DirectConnectFlag,
                DirectConnectReconcilePseudoAaaFlag = segmentDetail.DirectConnectReconcilePseudoAaaFlag,
                DlPassengerOnDlMarketedCodeshareFlag = segmentDetail.DlPassengerOnDlMarketedCodeshareFlag,
                DynamicAdlInEffectFlag = segmentDetail.DynamicAdlInEffectFlag,
                ErrorWithDateConversionFlag = segmentDetail.ErrorWithDateConversionFlag,
                ElectronicTicketChangedToPaFlag = segmentDetail.ElectronicTicketChangedToPaFlag,
                ElectronicTicketChangedToPbFlag = segmentDetail.ElectronicTicketChangedToPbFlag,
                ElectronicTicketChangedToPcFlag = segmentDetail.ElectronicTicketChangedToPcFlag,
                ElectronicTicketEligibleFlag = segmentDetail.ElectronicTicketEligibleFlag,
                EquipmentChangeFlag = segmentDetail.EquipmentChangeFlag,
                FFlyerLevelAtBookingTimeFlag = segmentDetail.FFlyerLevelAtBookingTimeFlag,
                FirstIndexableFlag = segmentDetail.FirstIndexableFlag,
                FirstNameNumber = segmentDetail.FirstNameNumber,
                FlightNumber = segmentDetail.FlightNumber,
                GrossInventoryFlag = segmentDetail.GrossInventoryFlag,
                InboundConnectionFlag = segmentDetail.InboundConnectionFlag,
                InboundOutboundChangeFlag = segmentDetail.InboundOutboundChangeFlag,
                InFlightDateAdjustment = segmentDetail.InFlightDateAdjustment,
                InflightDateAdjustmentNegativeFlag = segmentDetail.InflightDateAdjustmentNegativeFlag,
                IprsBit1 = segmentDetail.IprsBit1,
                IprsBit2 = segmentDetail.IprsBit2,
                IprsBit3 = segmentDetail.IprsBit3,
                LastNameNumber = segmentDetail.LastNameNumber,
                LegNumber = segmentDetail.LegNumber,
                MoreThanOneLegFlag = segmentDetail.MoreThanOneLegFlag,
                MarriedControl = segmentDetail.MarriedControl,
                MarriedControlAvilBucket = segmentDetail.MarriedControlAvilBucket,
                MileageTableValue = segmentDetail.MileageTableValue,
                NewFlag = segmentDetail.NewFlag,
                NewIropFlag = segmentDetail.NewIropFlag,
                NonDemandFlag = segmentDetail.NonDemandFlag,
                NonInventoryFlag = segmentDetail.NonInventoryFlag,
                NonRevenueFlag = segmentDetail.NonRevenueFlag,
                NoSmokingFlag = segmentDetail.NoSmokingFlag,
                NumberInParty = segmentDetail.NumberInParty,
                OandDTripNumber = segmentDetail.OandDTripNumber,
                OaPassengerOnDlOperatedCodeshareByDlAgentFlag = segmentDetail.OaPassengerOnDlOperatedCodeshareByDlAgentFlag,
                OaPassengerOnDlOperatedCodeshareFlag = segmentDetail.OaPassengerOnDlOperatedCodeshareFlag,
                OutboundConnectionFlag = segmentDetail.OutboundConnectionFlag,
                PartOfWorldspanTripFlag = segmentDetail.PartOfWorldspanTripFlag,
                PartialSmokingFlag = segmentDetail?.PartialSmokingFlag,
                PnrNeedsQueuingFlag = segmentDetail?.PnrNeedsQueuingFlag,
                PoolAllocationFlag = segmentDetail.PoolAllocationFlag,
                PreviousActionCode = segmentDetail?.PreviousActionCode,
                PseudoTripNumber = segmentDetail?.PseudoTripNumber,
                ReduceNumberInPartyFlag = segmentDetail?.ReduceNumberInPartyFlag,
                RecordName = segmentDetail?.RecordName,
                RmsFareValue = segmentDetail?.RmsFareValue,
                ScheduleChangeRestartFlag = segmentDetail?.ScheduleChangeRestartFlag,
                ScheduleChangeFlag = segmentDetail?.ScheduleChangeFlag,
                SegmentDepartureYear = segmentDetail?.SegmentDepartureYear,
                SegmentNumber = segmentDetail.SegmentNumber,
                SegmentType = segmentDetail.SegmentType,
                SpaceSoldFromInventoryThisTransaction = segmentDetail.SpaceSoldFromInventoryThisTransaction,
                StandbyListFlag = segmentDetail?.StandbyListFlag,
                UniqueBasId = segmentDetail?.UniqueBasId,
                VitualInvBuckets = segmentDetail?.VitualInvBuckets
            });
        }

        private static IEnumerable<Domain.Virgin.Models.Order.FrequentFlyer> GetFrequentFlyerDetails(IEnumerable<Proxy.PNRCommonV6Proxy.FrequentFlyer> ffDetails)
        {
            return ffDetails.Select(ff => new Domain.Virgin.Models.Order.FrequentFlyer
            {
                FirstName = ff.FirstName,
                LastName = ff.LastName,
                AccountNumber = ff.AccountNumber,
                AirlineCode = ff.AirlineCode,
                AccountTypeCode = ff.AccountTypeCode,
                FirstNameNumber = ff.FirstNameNumber,
                LastNameNumber = ff.LastNameNumber,
                OsiNumber = ff.OsiNumber,
                RecordName = ff.RecordName,
                SegmentNumber = ff.SegmentNumber,
                Status = ff.Status
            });
        }

        private static IEnumerable<Domain.Virgin.Models.Order.DtiSegmentInfo> GetDtiSegmentInfoDetails(IEnumerable<Proxy.PNRCommonV6Proxy.DtiSegmentInfo> dtiSegmentInfos)
        {
            return dtiSegmentInfos.Select(dtiSegInfo => new Domain.Virgin.Models.Order.DtiSegmentInfo
            {
                BookNumber = dtiSegInfo.BookNumber,
                Carrier = dtiSegInfo.Carrier,
                ClassOfService = dtiSegInfo.ClassOfService,
                DTIType = dtiSegInfo.DTIType,
                DepartureDate = dtiSegInfo.DepartureDate,
                DeparturePoint = dtiSegInfo.DeparturePoint,
                DepartureTime = dtiSegInfo.DepartureTime,
                ExpandedPassengerTypeCode = dtiSegInfo.ExpandedPassengerTypeCode,
                FarebasisFareclassCode = dtiSegInfo.FarebasisFareclassCode,
                FirstNameNumber = dtiSegInfo.FirstNameNumber,
                FlightNumber = dtiSegInfo.FlightNumber,
                ItinerarySegmentNumber = dtiSegInfo.ItinerarySegmentNumber,
                LastNameNumber = dtiSegInfo.LastNameNumber,
                NotValidAfterDate = dtiSegInfo.NotValidAfterDate,
                NotValidBeforeDate = dtiSegInfo.NotValidBeforeDate,
                OpenFlag = dtiSegInfo.OpenFlag,
                PassengerTypeCode = dtiSegInfo.PassengerTypeCode,
                RecordName = dtiSegInfo.RecordName,
                SegmentAmount = dtiSegInfo.SegmentAmount,
                SegmentNumber = dtiSegInfo.SegmentNumber,
                SegmentStatus = dtiSegInfo.SegmentStatus,
                StopFlag = dtiSegInfo.StopFlag,
                SurchargeAmount = dtiSegInfo.SurchargeAmount,
                SurchargeIdentifier = dtiSegInfo.SurchargeIdentifier,
                VoidFlag = dtiSegInfo.VoidFlag
            });
        }
    }
}


