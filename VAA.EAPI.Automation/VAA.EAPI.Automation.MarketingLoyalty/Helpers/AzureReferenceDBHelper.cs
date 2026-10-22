using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public static class AzureReferenceDBHelper
    {
        private static readonly string env = Config.Environment;
        private static readonly string connectionString = env.Is("prod") ? Vault.Default.GetSecretValue("MarketingLoyalty-avapeundbCommonReferenceDataConnectionString") : Vault.Default.GetSecretValue("MarketingLoyalty-avateundbCommonReferenceDataConnectionString");

        public static object RefDbQueryResponse(string query) => SqlHelper.ExecuteScalar(query, connectionString);

        public static object[][] MultiColumnRefDbQueryResponse(string query) => SqlHelper.ExecuteReader(query, connectionString);

        public static IEnumerable<GetPartners> GetPartnerDetails()
        {
            List<GetPartners> partners = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = "Select RowId,PartnerCode,PartnerType,PartnerSubType,Name,PartnerCategory,CodeShareFlag from dbo.PartnerProfile where active = '1' "
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    partners.Add(new GetPartners
                    {
                        PartnerId = (string)reader.GetValue(0),
                        PartnerCode = (string)reader.GetValue(1),
                        PartnerType = (string)reader.GetValue(2),
                        PartnerSubType = (string)reader.GetValue(3),
                        PartnerName = (string)reader.GetValue(4),
                        PartnerCategory = (string)reader.GetValue(5),
                        CodeShareFlag = (string)reader.GetValue(6)
                    });

            return partners;
        }

        public static IEnumerable<GetCountries> GetCountryDetails()
        {
            List<GetCountries> countries = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = "select Value, Name from [dbo].ListOfValues where Type='COUNTRY'  and SortOrder > 1000 ORDER BY Value"
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    countries.Add(new GetCountries
                    {
                        Name = (string)reader.GetValue(0),
                        Code = (string)reader.GetValue(1),
                    });

            return countries;
        }

        public static IEnumerable<GetListOfMediaCodes> GetMediaCodeDetails()
        {
            List<GetListOfMediaCodes> mediaCodeList = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = "select MediaCode,MediaCodeCategory,MediaCodeDescription from [dbo].[MediaCodes]"
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    mediaCodeList.Add(new GetListOfMediaCodes
                    {
                        MediaCode = (string)reader.GetValue(0),
                        MediaCodeCategory = reader.IsDBNull(1) ? string.Empty : (string)reader.GetValue(1),
                        MediaCodeDescription = reader.IsDBNull(2) ? string.Empty : (string)reader.GetValue(2),
                    });

            return mediaCodeList;
        }

        public static IEnumerable<GetMemberStatus> GetMemberStatusDetails()
        {
            List<GetMemberStatus> statusList = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = "select RowID, Value from [dbo].ListOfValues where  Type='LOY_MEMBER_STATUS' and Active='Y' order by Value "
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    statusList.Add(new GetMemberStatus
                    {
                        ProgramId = (string)reader.GetValue(0),
                        StatusCode = (string)reader.GetValue(1),
                    });

            return statusList;
        }

        public static IEnumerable<CampaignPromotionDetails> DuplicateCampaignPromotionQueryResponse(string duplicateCheckQuery)
        {
            List<CampaignPromotionDetails> dupList = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = duplicateCheckQuery
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    dupList.Add(new CampaignPromotionDetails
                    {
                        CampaignId = reader.IsDBNull(0) ? string.Empty : (string)reader.GetValue(0),
                        DuplicateCount = (int)reader.GetValue(1),
                    });

            return dupList;
        }

        public static IEnumerable<GetListOfProducts> GetProductDetails()
        {
            List<GetListOfProducts> productList = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = "Select RowID,ProductName,ProductType,ProductCategory,Description from [dbo].[Product] where active='1' "
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    productList.Add(new GetListOfProducts
                    {
                        ProductId = (string)reader.GetValue(0),
                        ProductName = (string)reader.GetValue(1),
                        ProductType = reader.IsDBNull(2) ? string.Empty : (string)reader.GetValue(2),
                        ProductCategory = reader.IsDBNull(3) ? string.Empty : (string)reader.GetValue(3),
                        ProductDescription = reader.IsDBNull(4) ? string.Empty : (string)reader.GetValue(4),
                    });

            return productList;
        }
    }
}
