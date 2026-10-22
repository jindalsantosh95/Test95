using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class CampaignListDownloadService : ICampaignListDownloadService
    {
        private readonly ILogger<CampaignListDownloadService> _logger;

        public CampaignListDownloadService(ILogger<CampaignListDownloadService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CreateXlStatementAndUploadToBlob(CampaignSearchResponse campaignList)
        {
            Stream stream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(Constants.Application.CampaignsWorksheetName);
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = Constants.Application.CampaignstWorksheetColumns[0];
                worksheet.Cell(currentRow, 2).Value = Constants.Application.CampaignstWorksheetColumns[1];
                worksheet.Cell(currentRow, 3).Value = Constants.Application.CampaignstWorksheetColumns[2];
                worksheet.Cell(currentRow, 4).Value = Constants.Application.CampaignstWorksheetColumns[3];
                worksheet.Cell(currentRow, 5).Value = Constants.Application.CampaignstWorksheetColumns[4];
                worksheet.Cell(currentRow, 6).Value = Constants.Application.CampaignstWorksheetColumns[5];
                worksheet.Cell(currentRow, 7).Value = Constants.Application.CampaignstWorksheetColumns[6];
                worksheet.Cell(currentRow, 8).Value = Constants.Application.CampaignstWorksheetColumns[7];
                worksheet.Cell(currentRow, 9).Value = Constants.Application.CampaignstWorksheetColumns[8];

                foreach (var campaign in campaignList.Campaigns)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = campaign.CampaignCode;
                    worksheet.Cell(currentRow, 2).Value = campaign.Name;
                    worksheet.Cell(currentRow, 3).Value = campaign.Status;
                    worksheet.Cell(currentRow, 4).Value = campaign.Type;
                    worksheet.Cell(currentRow, 5).Value = campaign.StartDate;
                    worksheet.Cell(currentRow, 6).Value = campaign.EndDate;
                    worksheet.Cell(currentRow, 7).Value = campaign.CampaignPurpose;
                    worksheet.Cell(currentRow, 8).Value = campaign.LinkToTemplate;
                    worksheet.Cell(currentRow, 9).Value = campaign.RegistrationRequired;
                }

                workbook.SaveAs(stream);
            }

            return await BlobStorageHelper.FileUploadToBlob(stream, $"{Constants.Application.CampaignsBlobName}_{DateTime.Now.ToString("MMddyyyy")}", ConfigReader.Get(Constants.Environment.ContainerNameForCampaigns));
        }
    }
}
