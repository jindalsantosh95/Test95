namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Constants
{
    public static class ErrorCodes
    {
        public static readonly string DBConnectionError = "DBConnectionError";
        public static readonly string NoRecordToMerge = "No Record found to be merged to storage table";
        public static readonly string ErrorWhileProcessingMerging = "MergingErrorInfo";
    }
}

