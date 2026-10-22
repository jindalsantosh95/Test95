namespace VAA.EAPI.Microservices.Retail.Trip.Mapper
{
    public static class EnumMapper
    {
        public static Infrastructure.Proxy.DeltaMyTrips.RequestOptionCodeType MapToDLRequestOptionCodeType(Domain.TripDetails.RequestOptionCodeType source) =>
             source switch
             {
                 Domain.TripDetails.RequestOptionCodeType.PNR => Infrastructure.Proxy.DeltaMyTrips.RequestOptionCodeType.CONFIRMATION,
                 Domain.TripDetails.RequestOptionCodeType.TICKET => Infrastructure.Proxy.DeltaMyTrips.RequestOptionCodeType.TICKET,
                 Domain.TripDetails.RequestOptionCodeType.CREDITCARD => Infrastructure.Proxy.DeltaMyTrips.RequestOptionCodeType.CREDITCARD,
                 Domain.TripDetails.RequestOptionCodeType.LOYALTYNUM => Infrastructure.Proxy.DeltaMyTrips.RequestOptionCodeType.LOYALTYNUM,
                 _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
             };
    }
}