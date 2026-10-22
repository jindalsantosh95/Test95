namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Mapper
{
    public sealed class MissingRequiredFlownPassengerFieldException : Exception
    {
        public MissingRequiredFlownPassengerFieldException(string columnName)
            : base($"Required flown passenger column {columnName} is missing.") => ColumnName = columnName;

        public string ColumnName { get; }
    }
}
