namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface ITokenValidatorExtension
    {
        Task<string> ValidateTokenExtractFCNumber(string token);
    }
}
