namespace VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper
{
    public interface IGenericDataMapper
    {
        string GetITABrandName(string vaaFareFamily);
        string GetOperatingCarrierNameByCode(string carrierCode);
        string ConvertNdcPtcToItaPtc(string ndcptc);
    }
}
