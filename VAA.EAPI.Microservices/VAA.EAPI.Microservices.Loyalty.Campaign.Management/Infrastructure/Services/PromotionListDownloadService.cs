using ClosedXML.Excel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class PromotionListDownloadService : IPromotionListDownloadService
    {
        private readonly ILogger<PromotionListDownloadService> _logger;

        public PromotionListDownloadService(ILogger<PromotionListDownloadService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> CreateXlStatementAndUploadToBlob(List<PromotionsDownload> promotionList)
        {
            Stream stream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(Constants.Application.PromotionsWorksheetName);
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = Constants.Application.PromoTableFirstColumn;
                worksheet.Cell(currentRow, 2).Value = Constants.Application.PromoTableSecondColumn;
                worksheet.Cell(currentRow, 3).Value = Constants.Application.PromoTableThirdColumn;
                worksheet.Cell(currentRow, 4).Value = Constants.Application.PromoTableFourthColumn;
                worksheet.Cell(currentRow, 5).Value = Constants.Application.PromoTableFifthColumn;
                worksheet.Cell(currentRow, 6).Value = Constants.Application.PromoTableSixthColumn;
                worksheet.Cell(currentRow, 7).Value = Constants.Application.PromoTableLastColumn;

                foreach (var promotion in promotionList)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = promotion.PromotionId;
                    worksheet.Cell(currentRow, 2).Value = promotion.PromotionName;
                    worksheet.Cell(currentRow, 3).Value = promotion.PartnerName;
                    worksheet.Cell(currentRow, 4).Value = promotion.Active;
                    worksheet.Cell(currentRow, 5).Value = promotion.StartDate;
                    worksheet.Cell(currentRow, 6).Value = promotion.EndDate;
                    worksheet.Cell(currentRow, 7).Value = promotion.CampaignLinked;
                }

                workbook.SaveAs(stream);
            }

            var promotionName = Constants.Application.PromotionsBlobName;
            var response = await BlobStorageHelper.FileUploadToBlob(stream, promotionName, ConfigReader.Get(Constants.Environment.ContainerNameForPromotions));

            var result = new Response<string>
            {
                Status = Status.OK,
                Value = response
            };

            return result;
        }
    }
}
