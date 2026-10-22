using System.Collections.Generic;
namespace VAA.EAPI.Microservices.Retail.Passenger.Tests
{
    public class PnrHelper
    {
        public static Domain.Entities.PnrType GetPnrObjct()
        {
            var pnrObject = new Domain.Entities.PnrType();
            pnrObject.DebugInfo = new Domain.Entities.DebugInfoType()
            {
                AdditionalInfo = "test",
                DataSourceName = "delta",
                DataSourceRequest = "delta",
                DataSourceResponse = "success",
                ConfigurationSettings = "configure",
                Timers = "timer",
                TransportInfo = "transport",
                RequestInfo = new Domain.Entities.RequestInfoType()
            };
            pnrObject.AdvisoryMessage = new List<Domain.Entities.AdvisoryMessageType>()
            {
                new Domain.Entities.AdvisoryMessageType() {MessageNumber="test", FreeformText="free",Process="proces",
                Spare="spare",SscRequest="sscRequest"
            } }.ToArray();
            pnrObject.ApprovalCode = new List<Domain.Entities.ApprovalCode>()
            {
                new Domain.Entities.ApprovalCode() {
                    AmericanExpressFlag="y",ApUsedFlag="y",CreditCardApprovalCode="code",CreditCardExpDate="12061990",
                    CreditCardNumber="#4567",FirstNameNumber="fname",LastNameNumber="lname",FreeformText="free",
                    RecordName="record",RemarkNumber="1234",SegmentNumber="1234"
                }
             }.ToArray();
            pnrObject.Dti = new List<Domain.Entities.Dti>()
            {
                new  Domain.Entities.Dti(){
                     AdvisoryMessage ="test message",
                     BaseAmount ="20",
                     BaseCurrency ="USD",
                     BooksCount ="56",
                     DtiCount ="67",
                     EquivalentFarePaidAmount ="34",
                     EquivalentFarePaidCurrency ="usd",
                     EquivalentFarePaidMinimumFlag ="test",
                     ExpandedPassengerTypeCode ="test",
                     FarecalcType ="test",
                     FirstNameNumber ="test",
                     InternationalFlag ="test",
                     InternationalSalesCode ="test",
                     LastNameNumber ="test",
                     PassengerTypeCode ="test",
                     RecordName ="test",
                     SegmentNumber ="test",
                     Tax1Amount ="test",
                     Tax1Type ="test",
                     Tax2Amount ="test",
                     Tax2Type ="test",
                     Tax3Amount ="test",
                     Tax3Type ="test",
                     TaxItemCount ="test",
                     TotalFareAmount ="test",
                     TotalFareCurrency ="test",
                     TotalFareType ="test",
                     TotalTax ="test"
                }
                }.ToArray();
            pnrObject.Ptc = new List<Domain.Entities.Ptc>()
            {
                new Domain.Entities.Ptc()
                {
                    ApplyToAllFlag ="test",
                    FirstNameNumber ="test",
                    LastNameNumber ="test",
                    PassengerTypeCode ="test",
                    RecordName ="test",
                    SegmentNumber ="test",
                }
            }.ToArray();
            pnrObject.SeatOa = new List<Domain.Entities.SeatOa>()
            {
                new Domain.Entities.SeatOa()
                {
                     ActiveFlag ="test",
                     AirlineCode ="test",
                     ArrivalPoint ="test",
                     ChangeStatusFlag ="test",
                     CompartmentNumber ="test",
                     CurrentAssignment ="test",
                     CurrentStatus ="test",
                     DeleteNameFlag ="test",
                     DeletedDueToNameChangeFlag ="test",
                     DepartureDate ="test",
                     DeparturePoint ="test",
                     DepartureYear ="test",
                     DeplanedShortFlag ="test",
                     Description ="test",
                     FirstNameNumber ="test",
                     FlightNumber ="test",
                     HistoryOnlyFlag ="test",
                     LastNameNumber ="test",
                     LegNumber ="test",
                     NewFlag ="test",
                     NumberInParty ="test",
                     PartialSmokingFlag ="test",
                     PassengerName ="test",
                     PassengerRequestedFlag ="test",
                     PassengerTicketingStatus ="test",
                     PreferredFlag ="test",
                     PreviousAssignment ="test",
                     PreviousStatus ="test",
                     ReaccomodationFlag ="test",
                     RecordName ="test",
                     SaleOfDxaSpaceFlag ="test",
                     SeatDeparturePoint ="test",
                     SegmentNumber ="test",
                     SegmentType ="test",
                     StandbyFlag ="test",
                     ThroughCheckinFlag ="test",
                     TypeOfRequestFlag ="test",
                     UnclaimedFlag ="test",
    }
            }.ToArray();
            pnrObject.SeatDl = new List<Domain.Entities.SeatDl>()
            {
                new Domain.Entities.SeatDl()
                {
                     ActiveSegmentFlag ="test",
                     ArrivalPoint ="test",
                     ChangeStatusFlag ="test",
                     CompartmentNumber ="test",
                     CompositeFlag ="test",
                     CurrentSeatAssignment ="test",
                     CurrentStatus ="test",
                     DeleteNameFlag ="test",
                     DeletedDueToNameChangeFlag ="test",
                     DeltaIssueBoardingPassOnlyFlag ="test",
                     DepartureDate ="test",
                     DeparturePoint ="test",
                     DepartureYear ="test",
                     DeplanedShortFlag ="test",
                     Description ="test",
                     FirstNameNumber ="test",
                     FlightNumber ="test",
                     HistoryOnlyFlag ="test",
                     InternalFlagForCassReaccomodation ="test",
                     ItineraryCanceledFlag ="test",
                     LastNameNumber ="test",
                     LegNumber ="test",
                     NewSegmentFlag ="test",
                     NumberInParty ="test",
                     OaOrdinalNumber ="test",
                     OaSeatEndTransactionFlag ="test",
                     PartialSmokingFlag ="test",
                     PassengerName ="test",
                     PassengerRequestedFlag ="test",
                     PassengerTicketingStatus ="test",
                     PreviousStatus ="test",
                     PreviouslyCanceledFlag ="test",
                     ReaccomodationFlag ="test",
                     RecordName ="test",
                     SaleOfDxaSpaceFlag ="test",
                     SeatDeparturePoint ="test",
                     SegmentNumber ="test",
                     SegmentType ="test",
                     StandByFlag ="test",
                     UnaccomodatedOrChangedFlag ="test",
                     UnclaimedFlag ="test",
    }
            }.ToArray();
            pnrObject.DtiSegmentInfo = new List<Domain.Entities.DtiSegmentInfo>()
            {
                new Domain.Entities.DtiSegmentInfo()
                {
                     BookNumber ="test",
                     Carrier ="test",
                     ClassOfService ="test",
                     DepartureDate ="test",
                     DeparturePoint ="test",
                     DepartureTime ="test",
                     ExpandedPassengerTypeCode ="test",
                     FarebasisFareclassCode ="test",
                     FirstNameNumber ="test",
                     FlightNumber ="test",
                     ItinerarySegmentNumber ="test",
                     LastNameNumber ="test",
                     NotValidAfterDate ="test",
                     NotValidBeforeDate ="test",
                     OpenFlag ="test",
                     PassengerTypeCode ="test",
                     RecordName ="test",
                     SegmentAmount ="test",
                     SegmentNumber ="test",
                     SegmentStatus ="test",
                     StopFlag ="test",
                     SurchargeAmount ="test",
                     SurchargeIdentifier ="test",
                     VoidFlag ="test",
            }}.ToArray();
            pnrObject.DtiTerminatingCity = new List<Domain.Entities.DtiTerminatingCity>()
            {
                new Domain.Entities.DtiTerminatingCity()
                {
                   BookNumber ="test",
                   FirstNameNumber ="test",
                   LastNameNumber ="test",
                   PassengerTypeCode ="test",
                   RecordName ="test",
                   SegmentNumber ="test",
                   StopIndicator ="test",
                   TerminatingCity ="test",
          }}.ToArray();
            pnrObject.ElectronicTicketingData = new List<Domain.Entities.ElectronicTicketingData>()
            {
                new Domain.Entities.ElectronicTicketingData()
                {
                      FirstNameNumber ="test",
                      LastNameNumber ="test",
                      RecordName ="test",
                      SegmentNumber ="test",
                      TicketStatus ="test"
                 }
            }.ToArray();
            pnrObject.FareCalc = new List<Domain.Entities.FareCalc>()
            {
                new Domain.Entities.FareCalc()
                {
                      FirstNameNumber ="test",
                      LastNameNumber ="test",
                      RecordName ="test",
                      SegmentNumber ="test",
                      FreeformText="test"
                 }
            }.ToArray();
            pnrObject.FormOfPayment = new List<Domain.Entities.FormOfPayment>()
            {
                new Domain.Entities.FormOfPayment()
                {
                     AdditionalCollectionFlag ="test",
                     AmericanExpressPaymentFlag ="test",
                     CardHolderNameOrOther ="test",
                     CreditCardApprovalCode ="test",
                     CreditCardExpiration ="test",
                     CreditCardNumber ="test",
                     FirstNameNumber ="test",
                     FormOfPayment1 ="test",
                     FreeformText ="test",
                     LastNameNumber ="test",
                     NonRefundableFlag ="test",
                     PaymentDueFlag ="test",
                     PrepaidTicketAdviceFlag ="test",
                     RecordName ="test",
                     RefundFlag ="test",
                     RemarkNumber ="test",
                     SegmentNumber ="test",
               }
            }.ToArray();
            pnrObject.FrequentFlyer = new List<Domain.Entities.FrequentFlyer>()
            {
                new Domain.Entities.FrequentFlyer()
                {
                 AccountNumber ="test",
                 AccountTypeCode ="test",
                 AirlineCode ="test",
                 FirstName ="test",
                 FirstNameNumber ="test",
                 LastName ="test",
                 LastNameNumber ="test",
                 OsiNumber ="test",
                 RecordName ="test",
                 SegmentNumber ="test",
                 Status ="test",
    }
            }.ToArray();
            pnrObject.HistoryActionItem = new List<Domain.Entities.HistoryActionItem>()
            {
                new Domain.Entities.HistoryActionItem()
                {
                 ActionCode ="test",
                 FreeformText ="test",
                 HavData ="test",
                 OrdinalNumber ="test",
                 RecordName ="test",
    }
            }.ToArray();
            pnrObject.HistoryCreditOrSignature = new List<Domain.Entities.HistoryCreditOrSignature>()
            {
                new Domain.Entities.HistoryCreditOrSignature()
                {
                     ActionReceivedFromInfo ="test",
                     CreditText ="test",
                     OrdinalNumber ="test",
                     RecordName ="test",
    }
            }.ToArray();
            pnrObject.HistoryPnrCreation = new List<Domain.Entities.HistoryPnrCreation>()
            {
                new Domain.Entities.HistoryPnrCreation()
                { CreditText ="test",
                 OrdinalNumber ="test",
                 ReceivedFromInfo ="test",
                 RecordName ="test",
            }
        }.ToArray();
            pnrObject.OtherServiceInformation = new List<Domain.Entities.OtherServiceInformation>()
            {
                new Domain.Entities.OtherServiceInformation()
                {
                     AirlineCode ="test",
                     FirstNameNumber ="test",
                     FreeformText ="test",
                     LastNameNumber ="test",
                     OaCarrierCodeRequestFlag ="test",
                     OsiNumber ="test",
                     RecordName ="test",
                     SegmentNumber ="test",
                }
            }.ToArray();
            pnrObject.Passenger = new List<Domain.Entities.Passenger>()
            {
                new Domain.Entities.Passenger()
                {
                     CustomerId ="test",
                     FirstName ="test",
                     FirstNameNumber ="test",
                     LastName ="test",
                     LastNameNumber ="test",
                     NonRevenueStandbyFlag ="test",
                     RecordName ="test",
                     Remarks ="test",
                     RevenueStandbyFlag ="test",
                     SegmentNumber ="test",
                     Suffix ="test",
                     Title ="test",
                }
            }.ToArray();
            pnrObject.PassengerSpecial = new List<Domain.Entities.PassengerSpecial>()
            {
                new Domain.Entities.PassengerSpecial()
                {
                     FirstNameNumber ="test",
                     GroupId ="test",
                     GroupName ="test",
                     LastNameNumber ="test",
                     NonRevenueStandbyFlag ="test",
                     NumberInGroup ="test",
                     RecordName ="test",
                     RevenueStandbyFlag ="test",
                     SegmentNumber ="test"}}.ToArray();
            pnrObject.Phone = new List<Domain.Entities.Phone>()
            {
                new Domain.Entities.Phone()
                {
                     AreaCode ="test",
                    CityCode ="test",
                    CountryCode ="test",
                    Extension ="test",
                    FreeformText ="test",
                    LastNameNumber ="test",
                    MarkedForDeletionFlag ="test",
                    Number ="test",
                    PhoneFieldNumber ="test",
                    PhoneRecordNumber ="test",
                    RecordName ="test",
                    SegmentNumber ="test",
                    Type ="test",
    }
}.ToArray();
            pnrObject.RecordLocator = new List<Domain.Entities.RecordLocator>()
            {
                new Domain.Entities.RecordLocator()
                {
                     ActiveLevel ="test",
                     AnyActionBitFlag ="test",
                     AwardPricingPerformedFlag ="test",
                     FilePendingModeFlag ="test",
                     NewBagDataPresent ="test",
                     NewLevelIdForDividedPnr ="test",
                     OutsideSystemRecordLocator ="test",
                     PnrCreationDate ="test",
                     PnrCreationDateWithYear ="test",
                     RecordLocator1 ="test",
                     RecordName ="test",
    }
}.ToArray();
            pnrObject.TicketData = new List<Domain.Entities.TicketData>()
            {
                new Domain.Entities.TicketData()
                {
                     AccountControlNumberFlag ="test",
                     AutoReissueFlag ="test",
                     BoardingPassControlNumberFlag ="test",
                     CityCodeForAccountingPrinter ="test",
                     CityCodeForTicketPrinter ="test",
                     ConnTicketNumber ="test",
                     ControlNumberExhausedFlag2 ="test",
                     ControlNumberExhaustedFlag ="test",
                     DateOfBoardingPassIssue ="test",
                     DateOfInvoice ="test",
                     DeleteItemFlag ="test",
                     DigitCheck ="test",
                     ElectronicTicketNumberFlag ="test",
                     ExchangedTicketFlag ="test",
                     FirstNameNumber ="test",
                     FirstScnAcctDocRangeA ="test",
                     FirstScnAcctDocRangeB ="test",
                     FirstScnBoardRangeA ="test",
                     FirstScnBoardRangeB ="test",
                     FirstScnTicketRangeA ="test",
                     FirstScnTicketRangeB ="test",
                     InvoiceNumber ="test",
                     InvoiceNumberFlag ="test",
                     LastNameNumber ="test",
                     LastScnAcctDocRangeA ="test",
                     LastScnAcctDocRangeB ="test",
                     LastScnBoardRangeA ="test",
                     LastScnBoardRangeB ="test",
                     LastScnTicketRangeA ="test",
                     LastScnTicketRangeB ="test",
                     ManualAccountControlNumberFlag ="test",
                     ManualBoardingPassControlNumberFlag ="test",
                     ManualReissueFlag ="test",
                     ManualTicketControlNumberFlag ="test",
                     ManualTicketFlag ="test",
                     NoBoardingPassFlag ="test",
                     PassengerName ="test",
                     RecordName ="test",
                     SegmentNumber ="test",
                     TicketControlNumberFlag ="test",
                     TicketIssueDate ="test",
                     TicketMessage ="test",
                     TicketNumber ="test",
                     TicketNumberFlag ="test",
                     TicketType ="test",
    }
                    }.ToArray();
            pnrObject.TicketInformation = new List<Domain.Entities.TicketInformation>()
            {
                new Domain.Entities.TicketInformation()
                {
                     ConnectionTicketNumber ="test",
                     DigitCheck ="test",
                     FirstNameNumber ="test",
                     IssueDate ="test",
                     LastNameNumber ="test",
                     RecordName ="test",
                     TicketName ="test",
                     TicketNumber ="test",
                     TicketStatus ="test",
                     TicketType1 ="test",
                     TicketType2 ="test",
                     TypeDescription ="test",
                }
            }.ToArray();
            pnrObject.TicketingField = new List<Domain.Entities.TicketingField>()
            {
                new Domain.Entities.TicketingField()
                {
                     FirstNameNumber ="test",
                     FreeformText ="test",
                     LastNameNumber ="test",
                     RecordName ="test",
                     SegmentNumber ="test",
                     TicketingTypeCode ="test",
                }
            }.ToArray();
            pnrObject.TicketingInstruction = new List<Domain.Entities.TicketingInstruction>()
            {
                new Domain.Entities.TicketingInstruction()
                {    FarefaxGeneratedFlag ="test",
                     FirstNameNumber ="test",
                     FreeformText ="test",
                     LastNameNumber ="test",
                     RecordName ="test",
                     SegmentNumber ="test"
                }}.ToArray();
            pnrObject.TicketingTimeLimit = new List<Domain.Entities.TicketingTimeLimit>()
            {
                new Domain.Entities.TicketingTimeLimit()
                {
                 FirstNameNumber ="test",
                 FreeformText ="test",
                 LastNameNumber ="test",
                 RecordName ="test",
                 SegmentNumber ="test",
                }
            }.ToArray();
            pnrObject.TicketNumberData1 = new List<Domain.Entities.TicketNumberData1>()
            {
                new Domain.Entities.TicketNumberData1()
                {
                     AdditionalCollectionAmount ="test",
                     AdditionalCollectionDataFlag ="test",
                     AutoReissueForFormOfPaymentFlag ="test",
                     CommissionRate ="test",
                     CurrencyCode ="test",
                     ExchangedCurrencyAmount ="test",
                     FirstNameNumber ="test",
                     FirstTimeIssueForFormOfPaymentFlag ="test",
                     IataNumber ="test",
                     LastNameNumber ="test",
                     ManualTicketValue ="test",
                     OriginalFormOfPayment ="test",
                     OriginalSalesLocation ="test",
                     OriginalTicketAgent ="test",
                     OriginalTicketCity ="test",
                     OriginalTicketDate ="test",
                     OriginalTicketNumber ="test",
                     RecordName ="test",
                     RefundDataFlag ="test",
                     SegmentNumber ="test",
                     TotalExchangedAmount ="test",
                     UsedYTicketRedemptionFlag ="test",
                     VoluntaryFlag ="test",
            }
                    }.ToArray();
            pnrObject.TicketNumberData2 = new List<Domain.Entities.TicketNumberData2>()
            {
                new Domain.Entities.TicketNumberData2()
                {
                         ExchangeCouponNumber ="test",
                         ExchangeDocumentNumber ="test",
                         ExchangeTicketAmount ="test",
                         FirstNameNumber ="test",
                         LastNameNumber ="test",
                         RecordName ="test",
                         SegmentNumber ="test",
            }
                }.ToArray();
            pnrObject.TicketNumberData3 = new List<Domain.Entities.TicketNumberData3>()
            {
                new Domain.Entities.TicketNumberData3()
                {
                 ApprovalCode ="test",
                 FirstNameNumber ="test",
                 FormOfPayment ="test",
                 LastNameNumber ="test",
                 RecordName ="test",
                 SegmentNumber ="test",
                 Tariff ="test",
                }
            }.ToArray();
            pnrObject.SegmentAdditionalInfo = new List<Domain.Entities.SegmentAdditionalInfo>()
            {
                new Domain.Entities.SegmentAdditionalInfo(){
                     AccumulatedTime ="test",
                     ArrivalPoint ="test",
                     ArrivalTime ="test",
                     CassEquipment ="test",
                     ConnectionCarrierType ="test",
                     DeltaFlightType ="test",
                     DepartureDayOfWeekNumber ="test",
                     DeparturePoint ="test",
                     DepartureTime ="test",
                     ElapsedTime ="test",
                     FirstLegDepartureCity ="test",
                     FirstLegDepartureDate ="test",
                     FirstLegDepartureTime ="test",
                     FirstLegDepartureYear ="test",
                     FlightAdvisoryText ="test",
                     FlightDepartureDate ="test",
                     FlightLegDepartureYear ="test",
                     FlightMileage ="test",
                     InventoryEquipment ="test",
                     LegArrivalDate ="test",
                     LegNumber ="test",
                     MealServiceCode ="test",
                     NoCommsLinkFlag ="test",
                     OfficialAirlineGuideEquipmentCode ="test",
                     OperatingCarrierCode ="test",
                     RecordName ="test",
                     SegmentNumber ="test",
                 }   }.ToArray();
            pnrObject.SegmentDl = new List<Domain.Entities.SegmentDl>()
            {
                new Domain.Entities.SegmentDl(){
                     ActiveFlag ="test",
                     AirlineCode ="test",
                     ArrivalPoint ="test",
                     ArrivalTime ="test",
                     AscMessageFlag ="test",
                     AtlanticExcellenceRevenueValue ="test",
                     BackupCheckinSystemActiveThisFlightFlag ="test",
                     BinaryFareTypeCode ="test",
                     BlockSpaceControlFlag ="test",
                     CancelFlag ="test",
                     ChangeStatusFlag ="test",
                     ClassOfService ="test",
                     ClassOfService2 ="test",
                     ClassOfServiceOverBookedFlag ="test",
                     CodeShareClassOfService ="test",
                     CodeShareParticipantFlag ="test",
                     CompartmentNumber ="test",
                     CompartmentOverBookedFlag ="test",
                     ConditionalCancelAndSellFlag ="test",
                     ConsideredForMscSelectTicketFlag ="test",
                     ContinuingFlag ="test",
                     CurrentActionCode ="test",
                     DeltaConnectionCarrierFlag ="test",
                     DepartureDate ="test",
                     DepartureDateAdjustment ="test",
                     DepartureDateAdjustmentNegativeFlag ="test",
                     DepartureDayOfWeekNumber ="test",
                     DeparturePoint ="test",
                     DepartureTime ="test",
                     DirectConnectReconcilePseudoAaaFlag ="test",
                     DlPassengerOnDlMarketedCodeshareFlag ="test",
                     DynamicAdlInEffectFlag ="test",
                     ElectronicTicketChangedToPaFlag ="test",
                     ElectronicTicketChangedToPbFlag ="test",
                     ElectronicTicketChangedToPcFlag ="test",
                     ElectronicTicketEligibleFlag ="test",
                     EquipmentChangeFlag ="test",
                     ErrorWithDateConversionFlag ="test",
                     FFlyerLevelAtBookingTimeFlag ="test",
                     FirstIndexableFlag ="test",
                     FirstNameNumber ="test",
                     FlightNumber ="test",
                     GrossInventoryFlag ="test",
                     InFlightDateAdjustment ="test",
                     InboundConnectionFlag ="test",
                     InboundOutboundChangeFlag ="test",
                     InflightDateAdjustmentNegativeFlag ="test",
                     IprsBit1 ="test",
                     IprsBit2 ="test",
                     IprsBit3 ="test",
                     LastNameNumber ="test",
                     LegNumber ="test",
                     MarriedControl ="test",
                     MarriedControlAvilBucket ="test",
                     MoreThanOneLegFlag ="test",
                     NWMigrationFlag ="test",
                     NewFlag ="test",
                     NewIropFlag ="test",
                     NoSmokingFlag ="test",
                     NonDemandFlag ="test",
                     NonInventoryFlag ="test",
                     NonRevenueFlag ="test",
                     NumberInParty ="test",
                     OaPassengerOnDlOperatedCodeshareByDlAgentFlag ="test",
                     OaPassengerOnDlOperatedCodeshareFlag ="test",
                     OandDTripNumber ="test",
                     OutboundConnectionFlag ="test",
                     PartOfWorldspanTripFlag ="test",
                     PartialSmokingFlag ="test",
                     PnrNeedsQueuingFlag ="test",
                     PoolAllocationFlag ="test",
                     PreviousActionCode ="test",
                     PseudoTripNumber ="test",
                     RecordName ="test",
                     ReduceNumberInPartyFlag ="test",
                     RmsFareValue ="test",
                     ScheduleChangeFlag ="test",
                     ScheduleChangeRestartFlag ="test",
                     SegmentDepartureYear ="test",
                     SegmentNumber ="test",
                     SegmentType ="test",
                     SpaceSoldFromInventoryThisTransaction ="test",
                     StandbyListFlag ="test",
                     UniqueBasId ="test",
                     VitualInvBuckets ="test",
    } }.ToArray();
            pnrObject.TicketCouponInfo = new List<Domain.Entities.TicketCouponInfo>()
            {
                new Domain.Entities.TicketCouponInfo(){
                      ArrivalPoint ="test",
                     CheckDigit ="test",
                     ConnectionTicketNumber ="test",
                     CouponNumber ="test",
                     CouponStatus ="test",
                     DeparturePoint ="test",
                     FirstNameNumber ="test",
                     LastNameNumber ="test",
                     RecordName ="test",
                     TicketNumber ="test",
                     TravelDate ="test",
    } }.ToArray();
            pnrObject.SeatOa = new List<Domain.Entities.SeatOa>()
            {
                new Domain.Entities.SeatOa()
                {
                     ActiveFlag="test",
                     AirlineCode="test",
                     ArrivalPoint="test",
                     ChangeStatusFlag="test",
                     CompartmentNumber="test",
                     CurrentAssignment="test",
                     CurrentStatus="test",
                     DeleteNameFlag="test",
                     DeletedDueToNameChangeFlag="test",
                     DepartureDate="test",
                     DeparturePoint="test",
                     DepartureYear="test",
                     DeplanedShortFlag="test",
                     Description="test",
                     FirstNameNumber="test",
                     FlightNumber="test",
                     HistoryOnlyFlag="test",
                     LastNameNumber="test",
                     LegNumber="test",
                     NewFlag="test",
                     NumberInParty="test",
                     PartialSmokingFlag="test",
                     PassengerName="test",
                     PassengerRequestedFlag="test",
                     PassengerTicketingStatus="test",
                     PreferredFlag="test",
                     PreviousAssignment="test",
                     PreviousStatus="test",
                     ReaccomodationFlag="test",
                     RecordName="test",
                     SaleOfDxaSpaceFlag="test",
                     SeatDeparturePoint="test",
                     SegmentNumber="test",
                     SegmentType="test",
                     StandbyFlag="test",
                     ThroughCheckinFlag="test",
                     TypeOfRequestFlag="test",
                     UnclaimedFlag="test",
    }
            }.ToArray();
            pnrObject.SpecialServiceRequest = new List<Domain.Entities.SpecialServiceRequest>()
            {
                new Domain.Entities.SpecialServiceRequest()
                {
                     AirlineCode ="test",
                     AllPassengersFlag ="test",
                     AllSegmentsFlag ="test",
                     FirstNameNumber ="test",
                     FlightDate ="test",
                     FlightNumber ="test",
                     IsHAFACT ="test",
                     LastNameNumber ="test",
                     NumberOfSsrsRequestedFor ="test",
                     OaSpecialServiceRequestFlag ="test",
                     RecordName ="test",
                     Remarks ="test",
                     RequestCode ="test",
                     SegmentNumber ="test",
                     SsrNumber ="test",
                     StatusCode ="test",
    }
            }.ToArray();
            pnrObject.TicketPassengerInfo = new List<Domain.Entities.TicketPassengerInfo>()
            {
                new Domain.Entities.TicketPassengerInfo()
                {
                     FirstNameNumber ="test",
                     LastNameNumber ="test",
                     PassengerName ="test",
                     RecordName ="test",
                     TicketMessage ="test",
    }
            }.ToArray();
            pnrObject.StandbyListItin = new List<Domain.Entities.StandbyListItin>()
            {
                new Domain.Entities.StandbyListItin()
                {
                    ClassOfService ="test",
                    CustomerId ="test",
                    FirstName ="test",
                    FirstNameNumber ="test",
                    LastName ="test",
                    LastNameNumber ="test",
                    RecordName ="test",
                    SegmentNumber ="test",
                    StandbyGroupCode ="test",
                    StandbyPriorityCode ="test"
    }
            }.ToArray();
            pnrObject.StoredFareData = new List<Domain.Entities.StoredFareData>()
            {
                new Domain.Entities.StoredFareData()
                {
                    FareRemarks ="test",
                    FirstNameNumber ="test",
                    FreeformText ="test",
                    LastNameNumber ="test",
                    RecordName ="test",
                    SegmentNumber ="test"
                }}.ToArray();
            return pnrObject;
        }
    }
}
