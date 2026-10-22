#nullable enable

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;

public sealed class TitleMaster
{
    public string? EnvelopeSalutationTemplate { get; set; }
    public string? FormalEnvelopeSalutationTemplate { get; set; }
    public string? Gender { get; set; }
    public string? InformalSalutationTemplate { get; set; }
    public decimal? Priority { get; set; }
    public string? Title { get; set; }
}
