namespace VAA.EAPI.GraphQL.Domain.Trips.Constants
{
    public enum AccountBannerCode
    {
        VAC_TO_FC_UPGRADE
    }

    public enum ProductTypes
    {
        Bag,
        Seat, 
        Flight
    }

    public enum ActionContexts
    {
        Delete,
        Add,       
        Retain
    }

    public enum SeatResponseWarnings
    {
        PartialSeatConfirmation,
        SeatConfirmationSuccessWithWarnings
    }
}
