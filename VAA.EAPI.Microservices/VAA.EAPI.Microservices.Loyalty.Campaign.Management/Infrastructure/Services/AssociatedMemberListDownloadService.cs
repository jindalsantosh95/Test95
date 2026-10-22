using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class AssociatedMemberListDownloadService : IAssociatedMemberListDownloadService
    {
        public async Task<string> CreateXlStatementAndUploadToBlob(string campaignCode, List<AssociatedMemberData> associatedMemberList)
        {
            Stream stream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(Constants.Application.AssocMemsWorksheetName);
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = Constants.Application.AssocMemsWorksheetColumns[0];
                worksheet.Cell(currentRow, 2).Value = Constants.Application.AssocMemsWorksheetColumns[1];

                foreach (var members in associatedMemberList)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = SanitizeInput(members.MemberNumber);
                    worksheet.Cell(currentRow, 2).Value = SanitizeInput(campaignCode);
                }

                workbook.SaveAs(stream);
            }

            return await BlobStorageHelper.FileUploadToBlob(stream, $"{campaignCode}_{DateTime.Now:MMddyyyy}", ConfigReader.Get(Constants.Environment.ContainerNameForCampAssocMembers));
        }

        private static string SanitizeInput(string value)
        {
            return Regex.Replace(value, @"^([-+=@])", "'");
        }
    }
}
