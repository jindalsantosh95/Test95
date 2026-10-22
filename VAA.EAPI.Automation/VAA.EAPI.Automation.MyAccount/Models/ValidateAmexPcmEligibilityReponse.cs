
public class ValidateAmexPcmEligibilityReponse
{
    public Data? data { get; set; }
}

public class Data
{
    public ValidatePcmEligibility validatePcmEligibility { get; set; }
}

public class ValidatePcmEligibility
{
    public bool valid { get; set; }
    public bool claimed { get; set; }
    public string? fcNumber { get; set; }
    public DateTime? claimedDateTime { get; set; }
}


