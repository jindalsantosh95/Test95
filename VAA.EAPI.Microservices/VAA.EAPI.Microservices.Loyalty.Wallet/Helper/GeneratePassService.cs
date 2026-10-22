using Passbook.Generator;
using Passbook.Generator.Fields;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Helper
{
    public class GeneratePassService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static X509Certificate2 GetCertificate(string secret) => new(Convert.FromBase64String(EAPI.Infrastructure.Services.KeyVault.Default.GetSecretValue(ConfigReader.Get(secret))), string.Empty, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

        public static async Task<byte[]> GeneratePass(string token, string Name, string fcNumber, string tierCode,string validUntil)
        {

            PassGenerator generator = new PassGenerator();
            PassGeneratorRequest passRequest = new PassGeneratorRequest();
            passRequest.PassTypeIdentifier = ConfigReader.Get(Constants.Environment.PassTypeIdentifier);
            passRequest.TeamIdentifier = ConfigReader.Get(Constants.Environment.TeamIdentifier);
            passRequest.SerialNumber = fcNumber;
            passRequest.Description = Constants.Application.passDescription;
            passRequest.OrganizationName = Constants.Application.passOrganization;

            passRequest.PassbookCertificate = GetCertificate(ConfigReader.Get(Constants.Environment.PassbookCertificate));
            passRequest.AppleWWDRCACertificate = GetCertificate(ConfigReader.Get(Constants.Environment.AppleWalletCertificate));

            passRequest.LabelColor = Constants.Application.LableColor;
            passRequest.ForegroundColor = Constants.Application.ForegroundColor;


            passRequest.Images.Add(PassbookImage.Icon, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.IconImage}"));
            passRequest.Images.Add(PassbookImage.Icon2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.Icon2XImage}"));
            passRequest.Images.Add(PassbookImage.Icon3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.Icon3XImage}"));
            

            switch (tierCode)
            {
                case "Red":

                    passRequest.BackgroundColor = ConfigReader.Get(Constants.Environment.Red_Background_RGB);

                    passRequest.Images.Add(PassbookImage.Logo, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.RedLogoImage}"));
                    passRequest.Images.Add(PassbookImage.Logo2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.RedLogo2XImage}"));
                    passRequest.Images.Add(PassbookImage.Logo3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.RedLogo3XImage}"));

                    passRequest.Images.Add(PassbookImage.Strip, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.RedTierImage}"));
                    passRequest.Images.Add(PassbookImage.Strip2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.RedTier2XImage}"));
                    passRequest.Images.Add(PassbookImage.Strip3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.RedTier3XImage}"));                    

                    break;

                case "Gold":
                   
                    passRequest.BackgroundColor = ConfigReader.Get(Constants.Environment.Gold_Background_RGB);

                    passRequest.Images.Add(PassbookImage.Logo, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.GoldLogoImage}"));
                    passRequest.Images.Add(PassbookImage.Logo2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.GoldLogo2XImage}"));
                    passRequest.Images.Add(PassbookImage.Logo3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.GoldLogo3XImage}"));
                    
                    passRequest.Images.Add(PassbookImage.Strip, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.GoldTierImage}"));
                    passRequest.Images.Add(PassbookImage.Strip2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.GoldTier2XImage}"));
                    passRequest.Images.Add(PassbookImage.Strip3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.GoldTier3XImage}"));

                    break;

                case "Silver":
                    
                    passRequest.BackgroundColor = ConfigReader.Get(Constants.Environment.Silver_Background_RGB);

                    passRequest.Images.Add(PassbookImage.Logo, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.SilverLogoImage}"));
                    passRequest.Images.Add(PassbookImage.Logo2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.SilverLogo2XImage}"));
                    passRequest.Images.Add(PassbookImage.Logo3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.SilverLogo3XImage}"));

                    passRequest.Images.Add(PassbookImage.Strip, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.SilverTierImage}"));
                    passRequest.Images.Add(PassbookImage.Strip2X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.SilverTier2XImage}"));
                    passRequest.Images.Add(PassbookImage.Strip3X, await DownloadImageAsync($"{ConfigReader.Get(Constants.Environment.WalletBlobUrl)}{Constants.Application.SilverTier3XImage}"));

                    break;
            }

            passRequest.Style = PassStyle.StoreCard;

            var nameField = new StandardField
            {
                Key = Constants.Application.Name,
                Label = Constants.Application.Name,
                Value = Name,
                TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
            };
            var membershipField = new StandardField
            {
                Key = "MemberShipNumber",
                Label = Constants.Application.MemberShipNumber,
                Value = fcNumber,
                TextAlignment = FieldTextAlignment.PKTextAlignmentRight
            };          
            passRequest.AddSecondaryField(nameField);
            passRequest.AddSecondaryField(membershipField);

            if (!string.IsNullOrEmpty(validUntil))
            {
                var validUntilField = new StandardField
                {
                    Key = "ValidUntil",
                    Label = Constants.Application.ValidUntil,
                    Value = validUntil,
                    TextAlignment = FieldTextAlignment.PKTextAlignmentLeft
                };
                passRequest.AddHeaderField(validUntilField);
            }

            passRequest.WebServiceUrl = ConfigReader.Get(Constants.Environment.WalletGeneratePassUrl);
            passRequest.AuthenticationToken = token;

            return generator.Generate(passRequest);
        }

        private static async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            return imageBytes;
        }
    }
}
