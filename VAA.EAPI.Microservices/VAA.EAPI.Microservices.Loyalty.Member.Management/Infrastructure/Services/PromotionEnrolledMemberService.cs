using Azure.Storage.Blobs;
using ClosedXML.Excel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class PromotionEnrolledMemberService : IPromotionEnrolledMemberService
    {
        private readonly ILogger<PromotionEnrolledMemberService> _logger;

        public PromotionEnrolledMemberService(ILogger<PromotionEnrolledMemberService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> CreateXlStatementAndUploadToBlob(IEnumerable<PromotionEnrolledMembers> memberList)
        {
            Stream stream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(Constants.Application.EnrolledMemberWorksheetName);
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = Constants.Application.EnrolledMemberWorksheetColumnName;

                foreach (var member in memberList)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = SanitizeInput(member.MemberNumber);
                }

                workbook.SaveAs(stream);
            }

            var promotionName = memberList.First().Name.Replace(" ", "");
            var response = await FileUploadToBlob(stream, promotionName);

            var result = new Response<string>
            {
                Status = Status.OK,
                Value = response
            };

            return result;
        }

        private async Task<string> FileUploadToBlob(Stream stream, string promotionName)
        {
            var fileName = $"{promotionName}_{DateTime.Now.ToString("ddMMyyyy")}.xlsx";

            var blobClient = new BlobContainerClient(ConfigReader.Get(Constants.Environment.BlobConnStringForEnrolledMemberToPromotion), ConfigReader.Get(Constants.Environment.ContainerNameForEnrolledMemberToPromotion));
            _ = blobClient.CreateIfNotExistsAsync();

            var blob = blobClient.GetBlobClient(fileName);
            stream.Position = 0;

            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.AbsoluteUri;
        }

        private static string SanitizeInput(string value)
        {
            return Regex.Replace(value, @"^([-+=@])", "'");
        }
    }
}
