using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;

public sealed class EapiOtpCodeProvider
{
    public async Task<string> GetRegistrationCodeAsync(string email)
    {
        return await GetCodeAsync(email, 8);
    }

    public async Task<DateTimeOffset?> CaptureAccountCodeTimestampAsync(string email)
    {
        return await OtpStorageCodeReader.GetTimestampByEmailIdAsync(email.ToLowerInvariant());
    }

    public async Task<string> GetAccountCodeAsync(string email, DateTimeOffset? newerThan)
    {
        return await GetCodeAsync(email, 6, newerThan);
    }

    private static async Task<string> GetCodeAsync(
        string email,
        int expectedLength,
        DateTimeOffset? newerThan = null)
    {
        int otp = await OtpStorageCodeReader.GetByEmailIdAsync(
            email.ToLowerInvariant(),
            expectedLength,
            newerThan: newerThan);
        if (otp == 0)
        {
            throw new InvalidOperationException("EAPI did not return an OTP for the requested test account.");
        }

        return otp.ToString($"D{expectedLength}");
    }
}
