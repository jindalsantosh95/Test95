using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Passenger.Constants;
//using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper
{
    public class PassengerMapperConfiguration
    {
        public static AutoMapper.Mapper MapperConfig()
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Domain.Entities.HostContextParamsType, HostContextParamsType>();
                cfg.CreateMap<Domain.Entities.DBConnectionInfoType, DBConnectionInfoType>();
                cfg.CreateMap<Domain.Entities.RequestInfoType, RequestInfoType>().ForMember(
                 dest => dest.TestLab, opt => opt.MapFrom(o => ConfigReader.Get(Environment.DlTestLab)));
                cfg.CreateMap<Domain.Entities.PnrRequestParamsType, PnrRequestParamsType>();
                cfg.CreateMap<Domain.Entities.PTCCodeType, PTCCodeType>();
                cfg.CreateMap<Domain.Entities.RequestOptionType, RequestOptionType>();
                cfg.CreateMap<Domain.Entities.AddPTCCodesRequestType, AddPTCCodesRequestType>();
                cfg.CreateMap<Domain.Entities.AddPTCCodesResponse, AddPTCCodesResponse>();
                cfg.CreateMap<Domain.Entities.AddRequestType, AddRequestType>();
                cfg.CreateMap<Domain.Entities.AddResponseType, AddResponseType>();
                cfg.CreateMap<Domain.Entities.UserAuthorizationDataType, UserAuthorizationDataType>();
                cfg.CreateMap<Domain.Entities.PrinterAuthorizationDataType, PrinterAuthorizationDataType>();
                cfg.CreateMap<Domain.Entities.StampingInfoType, StampingInfoType>();
                cfg.CreateMap<Domain.Entities.PassengerInfoType, PassengerInfoType>();
                cfg.CreateMap<Domain.Entities.PhoneInfoType, PhoneInfoType>();
                cfg.CreateMap<Domain.Entities.DocumentDetailDataType, DocumentDetailDataType>();
                cfg.CreateMap<Domain.Entities.GovtIssuedNumberType, GovtIssuedNumberType>();
                cfg.CreateMap<Domain.Entities.FlightInfoType, FlightInfoType>();
                cfg.CreateMap<Domain.Entities.PassengerContactDataType, PassengerContactDataType>();
                cfg.CreateMap<Domain.Entities.PassengerNameItemType, PassengerNameItemType>();
                cfg.CreateMap<Domain.Entities.PnrRequestParamsType, PnrRequestParamsType>();
                cfg.CreateMap<Domain.Entities.SecureFlightPassengerDataType, SecureFlightPassengerDataType>();
                cfg.CreateMap<Domain.Entities.SetupContextRequestType1, SetupContextRequestType1>();
                cfg.CreateMap<Domain.Entities.TeardownContextRequestType1, TeardownContextRequestType1>();
                cfg.CreateMap<Domain.Entities.EndRequestType1, EndRequestType1>();
                cfg.CreateMap<Domain.Entities.PnrOptionsType, PnrOptionsType>();
                cfg.CreateMap<Domain.Entities.IgnoreRequestType1, IgnoreRequestType1>();
                cfg.CreateMap<Domain.Entities.AuditFlagType, AuditFlagType>();
                cfg.CreateMap<Domain.Entities.RefreshRequestType1, RefreshRequestType1>();
                cfg.CreateMap<Domain.Entities.InfantProcessRequestOptionsType, InfantProcessRequestOptionsType>();
                cfg.CreateMap<Domain.Entities.InfantPassengerInfoType, InfantPassengerInfoType>();
                cfg.CreateMap<Domain.Entities.SeatEligiblePassengerInfoType, SeatEligiblePassengerInfoType>();
                cfg.CreateMap<Domain.Entities.GovtIssuedNumberType1, GovtIssuedNumberType1>();
                cfg.CreateMap<Domain.Entities.SecureFlightPassengerDataType1, SecureFlightPassengerDataType1>();
                cfg.CreateMap<Domain.Entities.InfantNameManagementType, InfantNameManagementType>();
                cfg.CreateMap<Domain.Entities.InfantSSRFlightDataType, InfantSSRFlightDataType>();
                cfg.CreateMap<Domain.Entities.InfantV2InfoType, InfantV2InfoType>();


                cfg.CreateMap<ErrorType, Domain.Entities.ErrorType>();
                cfg.CreateMap<ResponseInfoType, Domain.Entities.ResponseInfoType>();
                cfg.CreateMap<PhoneInfoType, Domain.Entities.PhoneInfoType>();
                cfg.CreateMap<PassengerInfoType, Domain.Entities.PassengerInfoType>();
                cfg.CreateMap<AddPhoneErrorListType, Domain.Entities.AddPhoneErrorListType>();
                cfg.CreateMap<ErrorType, Domain.Entities.ErrorType>();
                cfg.CreateMap<AddErrorListType, Domain.Entities.AddErrorListType>();
                cfg.CreateMap<StoredFareData, Domain.Entities.StoredFareData>();
                cfg.CreateMap<HostContextParamsType, Domain.Entities.HostContextParamsType>();
                cfg.CreateMap<DBConnectionInfoType, Domain.Entities.DBConnectionInfoType>();
                cfg.CreateMap<PnrType, Domain.Entities.PnrType>();
                cfg.CreateMap<RequestInfoType, Domain.Entities.RequestInfoType>();
                cfg.CreateMap<DebugInfoType, Domain.Entities.DebugInfoType>();
                cfg.CreateMap<AdvisoryMessageType, Domain.Entities.AdvisoryMessageType>();
                cfg.CreateMap<Ptc, Domain.Entities.Ptc>();
                cfg.CreateMap<SeatOa, Domain.Entities.SeatOa>();
                cfg.CreateMap<RecordLocator, Domain.Entities.RecordLocator>();
                cfg.CreateMap<HistoryCreditOrSignature, Domain.Entities.HistoryCreditOrSignature>();
                cfg.CreateMap<TicketNumberData3, Domain.Entities.TicketNumberData3>();
                cfg.CreateMap<ApprovalCode, Domain.Entities.ApprovalCode>();
                cfg.CreateMap<OtherServiceInformation, Domain.Entities.OtherServiceInformation>();
                cfg.CreateMap<Dti, Domain.Entities.Dti>();
                cfg.CreateMap<SegmentDl, Domain.Entities.SegmentDl>();
                cfg.CreateMap<TicketNumberData1, Domain.Entities.TicketNumberData1>();
                cfg.CreateMap<HistoryPnrCreation, Domain.Entities.HistoryPnrCreation>();
                cfg.CreateMap<TicketPassengerInfo, Domain.Entities.TicketPassengerInfo>();
                cfg.CreateMap<TicketingTimeLimit, Domain.Entities.TicketingTimeLimit>();
                cfg.CreateMap<Phone, Domain.Entities.Phone>();
                cfg.CreateMap<PassengerSpecial, Domain.Entities.PassengerSpecial>();
                cfg.CreateMap<TicketInformation, Domain.Entities.TicketInformation>();
                cfg.CreateMap<SegmentOa, Domain.Entities.SegmentOa>();
                cfg.CreateMap<TicketingField, Domain.Entities.TicketingField>();
                cfg.CreateMap<TicketCouponInfo, Domain.Entities.TicketCouponInfo>();
                cfg.CreateMap<TicketingInstruction, Domain.Entities.TicketingInstruction>();
                cfg.CreateMap<DtiSegmentInfo, Domain.Entities.DtiSegmentInfo>();
                cfg.CreateMap<FareCalc, Domain.Entities.FareCalc>();
                cfg.CreateMap<SeatDl, Domain.Entities.SeatDl>();
                cfg.CreateMap<Remark, Domain.Entities.Remark>();
                cfg.CreateMap<SpecialServiceRequest, Domain.Entities.SpecialServiceRequest>();
                cfg.CreateMap<Passenger1, Domain.Entities.Passenger>();
                cfg.CreateMap<FormOfPayment, Domain.Entities.FormOfPayment>();
                cfg.CreateMap<SegmentAdditionalInfo, Domain.Entities.SegmentAdditionalInfo>();
                cfg.CreateMap<TicketData, Domain.Entities.TicketData>();
                cfg.CreateMap<TicketNumberData2, Domain.Entities.TicketNumberData2>();
                cfg.CreateMap<DtiTerminatingCity, Domain.Entities.DtiTerminatingCity>();
                cfg.CreateMap<FrequentFlyer, Domain.Entities.FrequentFlyer>();
                cfg.CreateMap<HistoryActionItem, Domain.Entities.HistoryActionItem>();
                cfg.CreateMap<ElectronicTicketingData, Domain.Entities.ElectronicTicketingData>();
                cfg.CreateMap<StandbyListItin, Domain.Entities.StandbyListItin>();
                cfg.CreateMap<DocumentDetailDataType, Domain.Entities.DocumentDetailDataType>();
                cfg.CreateMap<GovtIssuedNumberType, Domain.Entities.GovtIssuedNumberType>();
                cfg.CreateMap<PassengerAddressInfoType, Domain.Entities.PassengerAddressInfoType>();
                cfg.CreateMap<PassengerNameItemType, Domain.Entities.PassengerNameItemType>();
                cfg.CreateMap<SecureFlightPassengerDataType, Domain.Entities.SecureFlightPassengerDataType>();
                cfg.CreateMap<TeardownContextResponseType1, Domain.Entities.TeardownContextResponseType1>();
                cfg.CreateMap<SeatEligiblePassengerInfoType, Domain.Entities.SeatEligiblePassengerInfoType>();
                cfg.CreateMap<InfantPassengerInfoType, Domain.Entities.InfantPassengerInfoType>();
                cfg.CreateMap<GovtIssuedNumberType1, Domain.Entities.GovtIssuedNumberType1>();
                cfg.CreateMap<SecureFlightPassengerDataType1, Domain.Entities.SecureFlightPassengerDataType1>();
                cfg.CreateMap<InfantNameManagementType, Domain.Entities.InfantNameManagementType>();
                cfg.CreateMap<InfantSSRFlightDataType, Domain.Entities.InfantSSRFlightDataType>();
                cfg.CreateMap<AdvisoryMessageType, Domain.Entities.AdvisoryMessageType>();
                cfg.CreateMap<InfantSSRFlightDataType, Domain.Entities.InfantSSRFlightDataType>();
                cfg.CreateMap<InfantV2ResponseInfoType, Domain.Entities.InfantV2ResponseInfoType>();
            });
            var mapper = new AutoMapper.Mapper(config);
            return mapper;
        }
    }
}