namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers
{
    public sealed class TestDateTimeProvider : Interface.IDateTimeProvider
    { 
            public new DateTime Now { get; set; }
            public new DateTime UtcNow { get; set; }

            public TestDateTimeProvider(DateTime? utcNow = null)
            {
                UtcNow = utcNow ?? DateTime.UtcNow;
                Now = UtcNow.ToLocalTime();
            }        
    }
}
