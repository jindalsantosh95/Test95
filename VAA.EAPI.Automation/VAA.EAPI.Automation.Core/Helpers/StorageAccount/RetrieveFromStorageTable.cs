using System.Globalization;
using Microsoft.Azure.Cosmos.Table;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.Core.Helpers.StorageAccount
{
    public class RetrieveFromStorageTable
    {
        private static readonly string _env = Config.Environment;
        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");
        public static string notificationStoreConnectionString = Vault.Default.GetSecretValue($"NotificationStoreConnectionString-{_env}");
        private static Random random = new();

        #region New Book Flow PNRs from Storage Table

        public static Dictionary<string, string> RetrieveLoyaltyAccountNumberDetailsFromStorageTable(string tier, int points, bool min = false, bool underSixteen = false, string countryCode = "", string environment = "", int? minimumPoints = null)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new TableStorageService<LoyaltyAccountsTableEntity>();
            tableStorageService.TableName = "LoyaltyAccounts";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> loyaltyAccountNumberDetails = new Dictionary<string, string>();

            try
            {
                string accountEnvironment = environment.IsNullOrWhiteSpace() ? _env : environment;
                DateTime minAdultDob = DateTime.Now.AddYears(-18);
                IEnumerable<LoyaltyAccountsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                              .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                     x.PartitionKey.Is(accountEnvironment) &&
                                     !x.FirstName.IsNullOrEmpty() &&
                                     !x.LastName.IsNullOrEmpty() &&
                                     !x.Tier.IsNullOrEmpty() &&
                                     x.UnderSixteen == underSixteen &&
                                     x.CoBrand == false &&
                                     (min ? x.Points < points : x.Points > points) &&
                                     (!minimumPoints.HasValue || x.Points >= minimumPoints.Value) &&
                                     x.Tier == tier && (countryCode == "US" ? x.CountryAddress == "US" : !string.IsNullOrEmpty(x.CountryAddress)) &&
                                     (!underSixteen ? (DateTime.TryParse(x.DateOfBirth, out DateTime dob) && dob <= minAdultDob) : true));

                int resultsCount = results.Count();

                if (results.Any())
                {
                    int randomIndex = random.Next(0, resultsCount);
                    LoyaltyAccountsTableEntity randomItem = results.ElementAt(randomIndex);

                    loyaltyAccountNumberDetails["rowKey"] = randomItem.RowKey ?? string.Empty;
                    loyaltyAccountNumberDetails["tier"] = randomItem.Tier ?? string.Empty;
                    loyaltyAccountNumberDetails["points"] = randomItem.Points.ToString();
                    loyaltyAccountNumberDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    loyaltyAccountNumberDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    loyaltyAccountNumberDetails["email"] = randomItem.Email ?? string.Empty;
                    loyaltyAccountNumberDetails["dob"] = randomItem.DateOfBirth ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching entity is found
                    loyaltyAccountNumberDetails["error"] = "No matching loyalty account found.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                Console.WriteLine($"Error retrieving loyalty account details: {ex.Message}");
                loyaltyAccountNumberDetails["error"] = "Error retrieving loyalty account details.";
            }

            return loyaltyAccountNumberDetails;
        }

        // Household test data lives in a dedicated table (LoyaltyHouseHoldAccounts), seeded by
        // MyAccount/DataProvider/LoyaltyHouseHoldAccountCreation.cs. Each row is one household:
        // RowKey is the leader (parent) FC number and ChildFCNumbers is the " , "-joined list of
        // linked members. The plain LoyaltyAccounts pool can't satisfy a household lookup because
        // those members aren't household-enrolled. PartitionKey is the env (compared via .Is()).
        public static Dictionary<string, string> RetrieveLoyaltyHouseHoldAccountFromStorageTable(string environment = null)
        {
            TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity> tableStorageService = new TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity>();
            tableStorageService.TableName = "LoyaltyHouseHoldAccounts";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> houseHoldAccountDetails = new Dictionary<string, string>();
            string env = (environment).IsNullOrWhiteSpace() ? _env : environment;

            try
            {
                // RetrieveItemsAsync already returns a materialized collection; ToList() here
                // pins one snapshot so Count/index sampling don't re-enumerate (or risk any
                // deferred-execution surprise from the Where).
                List<LoyaltyHouseHoldandHighFiveAccountsTableEntity> results =
                    tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                              .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                     x.PartitionKey.Is(env) &&
                                     !x.ParentFirstName.IsNullOrEmpty() &&
                                     !x.ParentLastName.IsNullOrEmpty() &&
                                     !x.ChildFCNumbers.IsNullOrEmpty())
                              .ToList();

                if (results.Count > 0)
                {
                    LoyaltyHouseHoldandHighFiveAccountsTableEntity randomItem = results[random.Next(0, results.Count)];

                    houseHoldAccountDetails["rowKey"] = randomItem.RowKey ?? string.Empty;
                    houseHoldAccountDetails["firstName"] = randomItem.ParentFirstName ?? string.Empty;
                    houseHoldAccountDetails["lastName"] = randomItem.ParentLastName ?? string.Empty;
                    houseHoldAccountDetails["tier"] = randomItem.ParentTierCode ?? string.Empty;
                    houseHoldAccountDetails["childFcNumbers"] = randomItem.ChildFCNumbers ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching entity is found
                    houseHoldAccountDetails["error"] = "No matching household loyalty account found.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                Console.WriteLine($"Error retrieving household loyalty account details: {ex.Message}");
                houseHoldAccountDetails["error"] = "Error retrieving household loyalty account details.";
            }

            return houseHoldAccountDetails;
        }

        public static Dictionary<string, string> RetrieveLoyaltyHighFiveAccountFromStorageTable(string environment = null)
        {
            TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity> tableStorageService = new TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity>();
            tableStorageService.TableName = "LoyaltyHouseHoldAccounts";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> highFiveAccountDetails = new Dictionary<string, string>();
            string env = (environment).IsNullOrWhiteSpace() ? _env : environment;

            try
            {
                List<LoyaltyHouseHoldandHighFiveAccountsTableEntity> results =
                    tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                    x.PartitionKey.Is(env) &&
                                    x.RowKey.StartsWith("HighFive -", StringComparison.OrdinalIgnoreCase) &&
                                    !x.ActiveYears.IsNullOrWhiteSpace())
                        .ToList();

                if (results.Count > 0)
                {
                    LoyaltyHouseHoldandHighFiveAccountsTableEntity randomItem = results[Random.Shared.Next(results.Count)];
                    // High Five seed rows store the FC number after the RowKey prefix instead of in MemberNumber.
                    string memberNumber = randomItem.RowKey.Split('-', StringSplitOptions.RemoveEmptyEntries).Last().Trim();

                    highFiveAccountDetails["memberNumber"] = memberNumber;
                    highFiveAccountDetails["activeYears"] = randomItem.ActiveYears ?? string.Empty;
                }
                else
                {
                    highFiveAccountDetails["error"] = "No matching High Five loyalty account found.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving High Five loyalty account details: {ex.Message}");
                highFiveAccountDetails["error"] = "Error retrieving High Five loyalty account details.";
            }

            return highFiveAccountDetails;
        }

        public static Dictionary<string, string> FetchLoyaltyAccountInfoExcludingPoints(string tier, string environment = null)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new TableStorageService<LoyaltyAccountsTableEntity>();
            tableStorageService.TableName = "LoyaltyAccounts";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> loyaltyAccountNumberDetails = new Dictionary<string, string>();
            string env = (environment).IsNullOrWhiteSpace() ? _env : environment;

            try
            {
                DateTime minAdultDob = DateTime.Now.AddYears(-18);
                IEnumerable<LoyaltyAccountsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                              .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                     x.PartitionKey.Is(env) &&
                                     !x.FirstName.IsNullOrEmpty() &&
                                     !x.LastName.IsNullOrEmpty() &&
                                     !x.Tier.IsNullOrEmpty() &&
                                     x.Tier == tier &&
                                     (DateTime.TryParse(x.DateOfBirth, out DateTime dob) && dob <= minAdultDob));

                int resultsCount = results.Count();

                if (results.Any())
                {
                    int randomIndex = random.Next(0, resultsCount);
                    LoyaltyAccountsTableEntity randomItem = results.ElementAt(randomIndex);

                    loyaltyAccountNumberDetails["rowKey"] = randomItem.RowKey ?? string.Empty;
                    loyaltyAccountNumberDetails["tier"] = randomItem.Tier ?? string.Empty;
                    loyaltyAccountNumberDetails["points"] = randomItem.Points.ToString();
                    loyaltyAccountNumberDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    loyaltyAccountNumberDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    loyaltyAccountNumberDetails["email"] = randomItem.Email ?? string.Empty;
                    loyaltyAccountNumberDetails["dob"] = randomItem.DateOfBirth ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching entity is found
                    loyaltyAccountNumberDetails["error"] = "No matching loyalty account found.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                Console.WriteLine($"Error retrieving loyalty account details: {ex.Message}");
                loyaltyAccountNumberDetails["error"] = "Error retrieving loyalty account details.";
            }

            return loyaltyAccountNumberDetails;
        }

        public static Dictionary<string, string> RetrieveCoBrandLoyaltyAccountNumberDetailsFromStorageTable(string tier, string lastName, int points, bool min = false, bool underSixteen = false, string countryCode = "US")
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new TableStorageService<LoyaltyAccountsTableEntity>();
            tableStorageService.TableName = "LoyaltyAccounts";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> loyaltyAccountNumberDetails = new Dictionary<string, string>();

            try
            {
                DateTime minAdultDob = DateTime.Now.AddYears(-18);
                IEnumerable<LoyaltyAccountsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                              .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                     x.PartitionKey.Is(_env) &&
                                     !x.FirstName.IsNullOrEmpty() &&
                                     !x.Tier.IsNullOrEmpty() &&
                                     x.LastName == lastName &&
                                     x.UnderSixteen == underSixteen &&
                                     x.CoBrand == true &&
                                     (min ? x.Points < points : x.Points > points) &&
                                     x.Tier == tier && x.CountryAddress == countryCode &&
                                     (!underSixteen ? (DateTime.TryParse(x.DateOfBirth, out DateTime dob) && dob <= minAdultDob) : true));

                int resultsCount = results.Count();

                if (results.Any())
                {
                    int randomIndex = random.Next(0, resultsCount);
                    LoyaltyAccountsTableEntity randomItem = results.ElementAt(randomIndex);

                    loyaltyAccountNumberDetails["rowKey"] = randomItem.RowKey ?? string.Empty;
                    loyaltyAccountNumberDetails["tier"] = randomItem.Tier ?? string.Empty;
                    loyaltyAccountNumberDetails["points"] = randomItem.Points.ToString();
                    loyaltyAccountNumberDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    loyaltyAccountNumberDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    loyaltyAccountNumberDetails["email"] = randomItem.Email ?? string.Empty;
                    loyaltyAccountNumberDetails["dob"] = randomItem.DateOfBirth ?? string.Empty;
                    loyaltyAccountNumberDetails["CoBrand"] = randomItem.CoBrand?.ToString() ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching entity is found
                    loyaltyAccountNumberDetails["error"] = "No matching loyalty account found.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                Console.WriteLine($"Error retrieving loyalty account details: {ex.Message}");
                loyaltyAccountNumberDetails["error"] = "Error retrieving loyalty account details.";
            }

            return loyaltyAccountNumberDetails;
        }

        public static Dictionary<string, string> RetrieveLoyaltyVoucherAccountDetailsFromStorageTable(
            string tierCode = "",
            string environmentOverride = "",
            string voucherType = "",
            int? minimumPoints = null,
            int? maximumPoints = null,
            DateTime? minimumVoucherExpiryDate = null,
            DateTime? voucherExpiryBeforeDate = null,
            int? exactVoucherCount = null,
            int? minimumVoucherCount = null,
            bool requireAllVouchersInExpiryRange = false,
            bool requireDifferentVoucherExpiryDates = false,
            bool requireAllVouchersMatchType = false)
        {
            Dictionary<string, string> loyaltyVoucherDetails = new Dictionary<string, string>();
            string accountEnvironment = environmentOverride.IsNullOrWhiteSpace() ? _env : environmentOverride;

            if ((connectionString?.Trim()).IsNullOrEmpty())
            {
                loyaltyVoucherDetails["error"] = $"Storage connection string secret 'AzureStoragePnrCreationsConnectionString' was not available for env '{accountEnvironment}' and KeyVaultUrl '{ConfigReader.Get(Constants.Environment.KeyVaultUrl)}'. {Vault.GetLastFailure("AzureStoragePnrCreationsConnectionString")}";
                return loyaltyVoucherDetails;
            }

            try
            {
                string environmentPartitionKey = accountEnvironment.ToUpperInvariant();

                List<LoyaltyVouchersTableEntity> voucherRows = RetrieveItemsInPartition<LoyaltyVouchersTableEntity>("LoyaltyVouchers", environmentPartitionKey)
                    .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                x.PartitionKey.Is(accountEnvironment) &&
                                !x.Vouchers.IsNullOrEmpty() &&
                                VoucherTypeMatches(x.Vouchers, voucherType, requireAllVouchersMatchType) &&
                                VoucherExpiryMatches(
                                    x.Vouchers,
                                    minimumVoucherExpiryDate,
                                    voucherExpiryBeforeDate,
                                    exactVoucherCount,
                                    minimumVoucherCount,
                                    requireAllVouchersInExpiryRange,
                                    requireDifferentVoucherExpiryDates) &&
                                (tierCode.IsNullOrEmpty() || x.TierCode.Is(tierCode)))
                    .ToList();

                if (voucherRows.Count == 0)
                {
                    string voucherCountFilter = exactVoucherCount != null
                        ? $"exactly {exactVoucherCount.Value} voucher(s)"
                        : minimumVoucherCount != null
                            ? $"at least {minimumVoucherCount.Value} voucher(s)"
                            : "any voucher count";
                    List<string> expiryBounds = new List<string>();
                    if (minimumVoucherExpiryDate != null)
                    {
                        expiryBounds.Add($">= {minimumVoucherExpiryDate.Value:yyyy-MM-dd}");
                    }

                    if (voucherExpiryBeforeDate != null)
                    {
                        expiryBounds.Add($"< {voucherExpiryBeforeDate.Value:yyyy-MM-dd}");
                    }

                    string expiryRangeFilter = expiryBounds.Count == 0
                        ? "any voucher expiry date"
                        : $"voucher expiry date {string.Join(" and ", expiryBounds)}";
                    string expiryMatchFilter = expiryBounds.Count == 0
                        ? "no voucher expiry-range filter"
                        : requireAllVouchersInExpiryRange
                            ? "all vouchers within the expiry range"
                            : "at least one voucher within the expiry range";
                    string distinctExpiryFilter = requireDifferentVoucherExpiryDates
                        ? " and at least two different expiry dates"
                        : string.Empty;
                    loyaltyVoucherDetails["error"] = $"No matching loyalty voucher account found for env '{accountEnvironment}', tier '{tierCode}', voucher type '{voucherType}', {voucherCountFilter}, {expiryRangeFilter}, {expiryMatchFilter}{distinctExpiryFilter}.";
                    return loyaltyVoucherDetails;
                }

                List<(LoyaltyVouchersTableEntity VoucherRow, LoyaltyAccountsTableEntity AccountRow)> accountCandidates = new List<(LoyaltyVouchersTableEntity VoucherRow, LoyaltyAccountsTableEntity AccountRow)>();
                List<LoyaltyAccountsTableEntity> loyaltyAccountRows = RetrieveItemsInPartition<LoyaltyAccountsTableEntity>("LoyaltyAccounts", environmentPartitionKey)
                    .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                x.PartitionKey.Is(accountEnvironment))
                    .ToList();

                foreach (LoyaltyVouchersTableEntity voucherRow in voucherRows)
                {
                    LoyaltyAccountsTableEntity loyaltyAccountRow = loyaltyAccountRows.FirstOrDefault(x => IsAdultLoyaltyAccountMatch(x, voucherRow.RowKey));

                    if (loyaltyAccountRow != null &&
                        (minimumPoints == null || loyaltyAccountRow.Points >= minimumPoints.Value) &&
                        (maximumPoints == null || loyaltyAccountRow.Points < maximumPoints.Value))
                    {
                        accountCandidates.Add((voucherRow, loyaltyAccountRow));
                    }
                }

                if (accountCandidates.Count == 0)
                {
                    string minimumPointsFilter = minimumPoints == null ? string.Empty : $"points >= {minimumPoints.Value}";
                    string maximumPointsFilter = maximumPoints == null ? string.Empty : $"points < {maximumPoints.Value}";
                    string pointsFilter = string.Join(" and ", new[] { minimumPointsFilter, maximumPointsFilter }
                        .Where(filter => !filter.IsNullOrEmpty()));
                    pointsFilter = pointsFilter.IsNullOrEmpty() ? "any points balance" : pointsFilter;
                    loyaltyVoucherDetails["error"] = $"No adult loyalty voucher account found for env '{accountEnvironment}', tier '{tierCode}', voucher type '{voucherType}' and {pointsFilter}.";
                    return loyaltyVoucherDetails;
                }

                (LoyaltyVouchersTableEntity selectedVoucherRow, LoyaltyAccountsTableEntity selectedAccountRow) = accountCandidates[Random.Shared.Next(0, accountCandidates.Count)];

                loyaltyVoucherDetails["rowKey"] = selectedVoucherRow.RowKey ?? string.Empty;
                loyaltyVoucherDetails["maskedRowKey"] = MaskMemberNumber(selectedVoucherRow.RowKey);
                loyaltyVoucherDetails["tier"] = !selectedVoucherRow.TierCode.IsNullOrEmpty() ? selectedVoucherRow.TierCode : selectedAccountRow.Tier ?? string.Empty;
                loyaltyVoucherDetails["email"] = !selectedVoucherRow.Email.IsNullOrEmpty() ? selectedVoucherRow.Email : selectedAccountRow.Email ?? string.Empty;
                loyaltyVoucherDetails["firstName"] = !selectedVoucherRow.FirstName.IsNullOrEmpty() ? selectedVoucherRow.FirstName : selectedAccountRow.FirstName ?? string.Empty;
                loyaltyVoucherDetails["lastName"] = !selectedVoucherRow.LastName.IsNullOrEmpty() ? selectedVoucherRow.LastName : selectedAccountRow.LastName ?? string.Empty;
                loyaltyVoucherDetails["vouchers"] = selectedVoucherRow.Vouchers ?? string.Empty;
                loyaltyVoucherDetails["voucherCreatedDate"] = selectedVoucherRow.VoucherCreatedDate?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
                loyaltyVoucherDetails["accountPartitionKey"] = selectedAccountRow.PartitionKey ?? string.Empty;
                loyaltyVoucherDetails["points"] = selectedAccountRow.Points.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                loyaltyVoucherDetails["error"] = $"Error retrieving loyalty voucher account details. {ex.GetType().Name}: {ex.Message}";
            }

            return loyaltyVoucherDetails;
        }

        private static bool VoucherTypeMatches(string voucherPayload, string voucherType, bool requireAllVouchersMatchType)
        {
            if (voucherType.IsNullOrEmpty())
            {
                return true;
            }

            try
            {
                List<Newtonsoft.Json.Linq.JObject> vouchers = Newtonsoft.Json.Linq.JArray.Parse(voucherPayload)
                    .Children<Newtonsoft.Json.Linq.JObject>()
                    .ToList();
                Func<Newtonsoft.Json.Linq.JObject, bool> matchesType = voucher =>
                    voucher.ToString(Newtonsoft.Json.Formatting.None)
                        .IndexOf(voucherType, StringComparison.OrdinalIgnoreCase) >= 0;

                return requireAllVouchersMatchType
                    ? vouchers.Count > 0 && vouchers.All(matchesType)
                    : vouchers.Any(matchesType);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return false;
            }
        }

        private static bool VoucherExpiryMatches(
            string voucherPayload,
            DateTime? minimumExpiryDate,
            DateTime? expiryBeforeDate,
            int? exactVoucherCount,
            int? minimumVoucherCount,
            bool requireAllVouchersInExpiryRange,
            bool requireDifferentVoucherExpiryDates)
        {
            if (minimumExpiryDate == null &&
                expiryBeforeDate == null &&
                exactVoucherCount == null &&
                minimumVoucherCount == null &&
                !requireDifferentVoucherExpiryDates)
            {
                return true;
            }

            try
            {
                List<Newtonsoft.Json.Linq.JObject> vouchers = Newtonsoft.Json.Linq.JArray.Parse(voucherPayload)
                    .Children<Newtonsoft.Json.Linq.JObject>()
                    .ToList();
                if (minimumExpiryDate == null &&
                    expiryBeforeDate == null &&
                    !requireDifferentVoucherExpiryDates)
                {
                    return (!exactVoucherCount.HasValue || vouchers.Count == exactVoucherCount.Value) &&
                        (!minimumVoucherCount.HasValue || vouchers.Count >= minimumVoucherCount.Value);
                }

                List<DateTime> expiryDates = new List<DateTime>();
                foreach (Newtonsoft.Json.Linq.JObject voucher in vouchers)
                {
                    string expiryDate = voucher.GetValue(
                        "expiryDate",
                        StringComparison.OrdinalIgnoreCase)?.ToString();
                    if (!DateTime.TryParse(
                        expiryDate,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime parsedExpiryDate))
                    {
                        return false;
                    }

                    expiryDates.Add(parsedExpiryDate.Date);
                }

                Func<DateTime, bool> isInRange = expiryDate =>
                    (minimumExpiryDate == null || expiryDate >= minimumExpiryDate.Value.Date) &&
                    (expiryBeforeDate == null || expiryDate < expiryBeforeDate.Value.Date);
                List<DateTime> eligibleExpiryDates = expiryDates.Where(isInRange).ToList();

                if (exactVoucherCount != null && eligibleExpiryDates.Count != exactVoucherCount.Value)
                {
                    return false;
                }

                if (minimumVoucherCount != null && eligibleExpiryDates.Count < minimumVoucherCount.Value)
                {
                    return false;
                }

                if (requireDifferentVoucherExpiryDates && eligibleExpiryDates.Distinct().Count() < 2)
                {
                    return false;
                }

                return requireAllVouchersInExpiryRange
                    ? expiryDates.All(isInRange)
                    : eligibleExpiryDates.Count > 0;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return false;
            }
        }

        public static Dictionary<string, string> RetrieveLoyaltyAccountWithoutVouchersFromStorageTable(string tier, int points, bool min = false, string environmentOverride = "")
        {
            Dictionary<string, string> loyaltyAccountNumberDetails = new Dictionary<string, string>();
            string accountEnvironment = environmentOverride.IsNullOrWhiteSpace() ? _env : environmentOverride;

            if ((connectionString?.Trim()).IsNullOrEmpty())
            {
                loyaltyAccountNumberDetails["error"] = $"Storage connection string secret 'AzureStoragePnrCreationsConnectionString' was not available for env '{accountEnvironment}' and KeyVaultUrl '{ConfigReader.Get(Constants.Environment.KeyVaultUrl)}'. {Vault.GetLastFailure("AzureStoragePnrCreationsConnectionString")}";
                return loyaltyAccountNumberDetails;
            }

            try
            {
                string environmentPartitionKey = accountEnvironment.ToUpperInvariant();
                DateTime minAdultDob = DateTime.UtcNow.AddYears(-18);

                HashSet<string> voucherAccountNumbers = RetrieveItemsInPartition<LoyaltyVouchersTableEntity>("LoyaltyVouchers", environmentPartitionKey)
                    .Where(x => !x.RowKey.IsNullOrWhiteSpace() && x.PartitionKey.Is(accountEnvironment))
                    .Select(x => x.RowKey)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                List<LoyaltyAccountsTableEntity> results = RetrieveItemsByColumnInPartition<LoyaltyAccountsTableEntity>("LoyaltyAccounts", environmentPartitionKey, "Tier", tier)
                    .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                                x.PartitionKey.Is(accountEnvironment) &&
                                !x.FirstName.IsNullOrEmpty() &&
                                !x.LastName.IsNullOrEmpty() &&
                                !x.Tier.IsNullOrEmpty() &&
                                x.UnderSixteen == false &&
                                x.CoBrand == false &&
                                (min ? x.Points < points : x.Points > points) &&
                                x.Tier == tier &&
                                !voucherAccountNumbers.Contains(x.RowKey) &&
                                DateTime.TryParse(x.DateOfBirth, out DateTime dob) &&
                                dob <= minAdultDob)
                    .ToList();

                if (results.Count == 0)
                {
                    loyaltyAccountNumberDetails["error"] = $"No matching non-voucher loyalty account found for env '{accountEnvironment}', tier '{tier}' and points rule '{(min ? "<" : ">")} {points}'.";
                    return loyaltyAccountNumberDetails;
                }

                LoyaltyAccountsTableEntity randomItem = results[random.Next(0, results.Count)];

                loyaltyAccountNumberDetails["rowKey"] = randomItem.RowKey ?? string.Empty;
                loyaltyAccountNumberDetails["maskedRowKey"] = MaskMemberNumber(randomItem.RowKey);
                loyaltyAccountNumberDetails["tier"] = randomItem.Tier ?? string.Empty;
                loyaltyAccountNumberDetails["points"] = randomItem.Points.ToString(CultureInfo.InvariantCulture);
                loyaltyAccountNumberDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                loyaltyAccountNumberDetails["lastName"] = randomItem.LastName ?? string.Empty;
                loyaltyAccountNumberDetails["email"] = randomItem.Email ?? string.Empty;
                loyaltyAccountNumberDetails["dob"] = randomItem.DateOfBirth ?? string.Empty;
            }
            catch (Exception ex)
            {
                loyaltyAccountNumberDetails["error"] = $"Error retrieving non-voucher loyalty account details. {ex.GetType().Name}: {ex.Message}";
            }

            return loyaltyAccountNumberDetails;
        }

        private static List<T> RetrieveItemsInPartition<T>(string tableName, string partitionKey) where T : TableEntity, ITableEntity, new()
        {
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            return RetrieveItemsByFilter<T>(tableName, partitionFilter);
        }

        private static List<T> RetrieveItemsByColumnInPartition<T>(string tableName, string partitionKey, string columnName, string columnData) where T : TableEntity, ITableEntity, new()
        {
            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            string columnFilter = TableQuery.GenerateFilterCondition(columnName, QueryComparisons.Equal, columnData);
            string combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, columnFilter);

            return RetrieveItemsByFilter<T>(tableName, combinedFilter);
        }

        private static List<T> RetrieveItemsByFilter<T>(string tableName, string filter) where T : TableEntity, ITableEntity, new()
        {
            List<T> entities = new List<T>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            TableContinuationToken continuationToken = null;
            TableQuery<T> query = new TableQuery<T>().Where(filter);

            do
            {
                TableQuerySegment<T> queryResult = table.ExecuteQuerySegmented(query, continuationToken);
                entities.AddRange(queryResult.Results);
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return entities;
        }

        private static string MaskMemberNumber(string memberNumber)
        {
            if (memberNumber.IsNullOrWhiteSpace())
            {
                return "<empty>";
            }

            int visibleCharacters = Math.Min(4, memberNumber.Length);
            return $"***{memberNumber.Substring(memberNumber.Length - visibleCharacters, visibleCharacters)}";
        }

        private static bool IsLoyaltyAccountMatch(LoyaltyAccountsTableEntity accountRow, string loyaltyAccountNumber)
        {
            if (accountRow == null || loyaltyAccountNumber.IsNullOrWhiteSpace())
            {
                return false;
            }

            string expectedAccountNumber = loyaltyAccountNumber.Trim();

            return accountRow.RowKey?.Trim().Is(expectedAccountNumber) == true ||
                   accountRow.LoyaltyAccountNumber?.Trim().Is(expectedAccountNumber) == true;
        }

        private static bool IsAdultLoyaltyAccountMatch(LoyaltyAccountsTableEntity accountRow, string loyaltyAccountNumber)
        {
            if (!IsLoyaltyAccountMatch(accountRow, loyaltyAccountNumber))
            {
                return false;
            }

            DateTime minAdultDob = DateTime.UtcNow.AddYears(-18);

            return accountRow.UnderSixteen == false &&
                   DateTime.TryParse(accountRow.DateOfBirth, out DateTime dob) &&
                   dob <= minAdultDob;
        }

        #endregion New Book Flow PNRs from Storage Table

        #region OCI PNRs from Storage Table

        private static readonly Dictionary<string, string[]> DestinationGroups = new()
            {
                { "USA", new[] { "JFK", "LAS", "LAX", "SEA", "MIA", "ATL", "SVD" } },
                { "UK", new[] { "LHR", "LGW", "MAN" } },
                { "CAN", new[] { "YYZ", "YVR", "YUL" } },
                {"GOVTFORMSROUTES", new[] { "MLE", "MBJ" } }
                // Add more as needed
            };

        public static Dictionary<string, string> RetrieveOCIPNRDetailsFromStorageTable(string journeyType = null, string reservationType = null, string testScenarioName = null, string origin = null, string destination = null, string firstName = null, string airline = null, string cabinType = null)
        {
            Dictionary<string, string> pnrDetails = new Dictionary<string, string>();
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>
            {
                TableName = "OCIPNRs",
                ConnectionString = connectionString
            };

            try
            {
                DateTime nowUtc = DateTime.UtcNow;
                DateTime minTime = nowUtc.AddMinutes(80); // 1 hour 20 minutes from now
                DateTime maxTime = nowUtc.AddHours(24);   // up to 24 hours ahead

                // Pull all PNRs
                List<CreatedPnrsTableEntity> allItems = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult();

                List<(CreatedPnrsTableEntity Entity, DateTime DepartureDate)> itemsWithParsedDate = allItems
                    .Select(x =>
                    {
                        if ((x.DepartureDate).IsNullOrWhiteSpace())
                            return ((CreatedPnrsTableEntity Entity, DateTime DepartureDate)?)null;

                        // Parse your specific format — note the '.' between HH and mm
                        if (DateTime.TryParseExact(
                                x.DepartureDate,
                                "yyyy-MM-ddTHH:mm:ss",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                out DateTime depDate))
                        {
                            return (Entity: x, DepartureDate: depDate);
                        }

                        // parsing failed
                        return ((CreatedPnrsTableEntity Entity, DateTime DepartureDate)?)null;
                    })
                    // Only keep items where departure is within the next 24 hours but more than 1h away
                    .Where(p => p.HasValue && p.Value.DepartureDate >= minTime && p.Value.DepartureDate <= maxTime)
                    .Select(p => p.Value)
                    .ToList();

                // Determine if no filters were provided
                bool noFiltersProvided = string.IsNullOrEmpty(journeyType)
                                         && string.IsNullOrEmpty(reservationType)
                                         && string.IsNullOrEmpty(testScenarioName)
                                         && string.IsNullOrEmpty(origin)
                                         && string.IsNullOrEmpty(destination)
                                         && string.IsNullOrEmpty(firstName)
                                         && string.IsNullOrEmpty(airline)
                                         && string.IsNullOrEmpty(cabinType);

                IEnumerable<CreatedPnrsTableEntity> filteredEntities;

                if (noFiltersProvided)
                {
                    // Use every valid entity (within time window)
                    filteredEntities = itemsWithParsedDate.Select(p => p.Entity);
                }
                else
                {
                    // Apply the provided filters
                    filteredEntities = itemsWithParsedDate
                        .Where(p =>
                            !string.IsNullOrEmpty(p.Entity.RowKey) &&
                            !string.IsNullOrEmpty(p.Entity.Journey) &&
                            (string.IsNullOrEmpty(journeyType) || p.Entity.Journey.Is(journeyType)) &&
                            !string.IsNullOrEmpty(p.Entity.PassengersCount) &&
                            (
                                (string.IsNullOrEmpty(reservationType)) ||
                                (reservationType.Is("REVENUE") && string.IsNullOrEmpty(p.Entity.FlyingClubNumber)) ||
                                (reservationType.Is("AWARD") && !string.IsNullOrEmpty(p.Entity.FlyingClubNumber))
                            ) &&
                            (string.IsNullOrEmpty(origin) || p.Entity.Origin.Is(origin)) &&
                            (
                                string.IsNullOrEmpty(destination)
                                || (DestinationGroups.TryGetValue(destination.ToUpper(), out string[] destList) && destList.Contains(p.Entity.Destination, StringComparer.OrdinalIgnoreCase))
                                || p.Entity.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)
                            ) &&
                            (string.IsNullOrEmpty(testScenarioName) || p.Entity.PartitionKey.Is(testScenarioName)) && (string.IsNullOrEmpty(firstName) || p.Entity.FirstName.Is(firstName)) && (string.IsNullOrEmpty(airline) || p.Entity.Airline.Is(airline)) && (string.IsNullOrEmpty(cabinType) || p.Entity.OutBoundCabinClass.ContainsCsvValue(cabinType))
                        )
                        .Select(p => p.Entity);
                }

                List<CreatedPnrsTableEntity> finalList = filteredEntities.ToList();

                if (finalList.Any())
                {
                    CreatedPnrsTableEntity selected = finalList[random.Next(finalList.Count)];

                    pnrDetails = new Dictionary<string, string>
                    {
                        ["partitionKey"] = selected.PartitionKey ?? string.Empty,
                        ["pnr"] = selected.RowKey ?? string.Empty,
                        ["testScenario"] = selected.PartitionKey ?? string.Empty,
                        ["origin"] = selected.Origin ?? string.Empty,
                        ["destination"] = selected.Destination ?? string.Empty,
                        ["journeyType"] = selected.Journey ?? string.Empty,
                        ["departureDate"] = selected.DepartureDate ?? string.Empty,
                        ["email"] = selected.Email ?? string.Empty,
                        ["firstName"] = selected.FirstName ?? string.Empty,
                        ["lastName"] = selected.LastName ?? string.Empty,
                        ["currency"] = selected.Currency ?? string.Empty,
                        ["totalPrice"] = selected.TotalPrice ?? string.Empty,
                        ["orderNo"] = selected.OrderNo ?? string.Empty,
                        ["pri"] = selected.PRI ?? string.Empty,
                        ["outboundCabinClass"] = selected.OutBoundCabinClass ?? string.Empty,
                        ["inboundCabinClass"] = selected.InBoundCabinClass ?? string.Empty,
                        ["passengersCount"] = selected.PassengersCount ?? string.Empty,
                        ["timestamp"] = selected.Timestamp.ToString(),
                        ["flyingClubNumber"] = selected.FlyingClubNumber ?? string.Empty
                    };
                }
                else
                {
                    string filterSummary = noFiltersProvided ? "no filters applied" :
                        $"journeyType='{journeyType}', reservationType='{reservationType}', origin='{origin}', destination='{destination}', testScenarioName='{testScenarioName}'";

                    pnrDetails["error"] = $"No matching PNR found ({filterSummary}) in the next 24 hours.";
                }
            }
            catch (Exception ex)
            {
                pnrDetails["error"] = $"Exception occurred while retrieving PNRs: {ex.Message}";
            }

            return pnrDetails;
        }

        public static Dictionary<string, string> RetrieveOCITravelRequirementsPnrs(string testScenarioName = null, string origin = null, string destination = null, int? cutOffWindowTime = 74)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new()
            {
                TableName = "OCIPNRs",
                ConnectionString = connectionString
            };

            DateTime? cutoff = cutOffWindowTime.HasValue ? DateTime.UtcNow.AddHours(cutOffWindowTime.Value) : null;

            List<CreatedPnrsTableEntity> allItems = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult();

            List<CreatedPnrsTableEntity> filteredEntities = allItems
                .Where(x =>
                    !(x.DepartureDate).IsNullOrWhiteSpace() &&
                    DateTime.TryParseExact(x.DepartureDate, "yyyy-MM-ddTHH:mm:ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime depDate) &&
                    depDate > DateTime.UtcNow && (!cutoff.HasValue || depDate > cutoff.Value) &&
                    (origin.IsNullOrWhiteSpace() || x.Origin.Is(origin)) &&
                    (
                        destination.IsNullOrWhiteSpace() ||
                        (DestinationGroups.TryGetValue(destination.ToUpperInvariant(), out string[] destList) &&
                         destList.Any(dest => dest.Is(x.Destination))) ||
                        x.Destination.Is(destination)
                    ) &&
                    (testScenarioName.IsNullOrWhiteSpace() || x.PartitionKey.Is(testScenarioName)))
                .OrderBy(x => DateTime.ParseExact(x.DepartureDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture))
                .ToList();

            if (!filteredEntities.Any())
            {
                return new Dictionary<string, string>
                {
                    ["error"] = "No matching PNR found for travel requirements."
                };
            }

            CreatedPnrsTableEntity item = filteredEntities[Random.Shared.Next(filteredEntities.Count)];

            return new Dictionary<string, string>
            {
                ["pnr"] = item.RowKey ?? string.Empty,
                ["origin"] = item.Origin ?? string.Empty,
                ["destination"] = item.Destination ?? string.Empty,
                ["firstName"] = item.FirstName ?? string.Empty,
                ["lastName"] = item.LastName ?? string.Empty
            };
        }

        public static List<Dictionary<string, string>> RetrieveAllOciPnrs()
        {
            List<Dictionary<string, string>> allPnrDetails = new List<Dictionary<string, string>>();
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>
            {
                TableName = "OCIPNRs",
                ConnectionString = connectionString
            };

            List<CreatedPnrsTableEntity> allItems = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult();

            foreach (CreatedPnrsTableEntity item in allItems)
            {
                Dictionary<string, string> pnrDetails = new Dictionary<string, string>
                {
                    ["pnr"] = item.RowKey ?? string.Empty,
                    ["origin"] = item.Origin ?? string.Empty,
                    ["destination"] = item.Destination ?? string.Empty,
                    ["airline"] = item.Airline ?? string.Empty,
                    ["flightNumber"] = item.FlightNumber ?? string.Empty,
                    ["journeyType"] = item.Journey ?? string.Empty,
                    ["departureDate"] = item.DepartureDate ?? string.Empty,
                    ["email"] = item.Email ?? string.Empty,
                    ["firstName"] = item.FirstName ?? string.Empty,
                    ["lastName"] = item.LastName ?? string.Empty,
                    ["currency"] = item.Currency ?? string.Empty,
                    ["totalPrice"] = item.TotalPrice ?? string.Empty,
                    ["orderNo"] = item.OrderNo ?? string.Empty,
                    ["pri"] = item.PRI ?? string.Empty,
                    ["outboundCabinClass"] = item.OutBoundCabinClass ?? string.Empty,
                    ["inboundCabinClass"] = item.InBoundCabinClass ?? string.Empty,
                    ["passengersCount"] = item.PassengersCount ?? string.Empty,
                    ["timestamp"] = item.Timestamp.ToString(),
                    ["flyingClubNumber"] = item.FlyingClubNumber ?? string.Empty
                };

                allPnrDetails.Add(pnrDetails);
            }

            return allPnrDetails;
        }

        #endregion OCI PNRs from Storage Table

        #region MYTRIPS PNRs from Storage Table

        public static Dictionary<string, string> RetrievePNRDetailsFromStorageTableForCurrentWeek(string journeyType = null, string reservationType = null, string origin = null, string paxCount = null, string flightType = null, string cabinType = null, bool loggedInUser = false, string environment = null)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = origin == "JFK" ? "PNRs" : "MobileAppPnrs";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> pnrDetails = new();
            Random random = new();
            string env = environment.IsNullOrWhiteSpace() ? _env : environment;
            string envUpper = env.ToUpperInvariant();

            try
            {
                // Get current date and find last Monday
                DateTime today = DateTime.UtcNow.Date;
                DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                IEnumerable<CreatedPnrsTableEntity> results = loggedInUser
                    ? tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x =>
                            !string.IsNullOrEmpty(x.RowKey) &&
                            !string.IsNullOrEmpty(x.PartitionKey) &&
                            x.PartitionKey.Is(envUpper) &&
                            !x.Journey.IsNullOrEmpty() &&
                            (string.IsNullOrEmpty(journeyType) || x.Journey == journeyType) &&
                            !x.PassengersCount.IsNullOrEmpty() &&
                            x.Timestamp.Date >= currentWeekMonday &&
                            x.Timestamp.Date <= today &&
                            (string.IsNullOrEmpty(origin) || x.Origin == origin) &&
                            (string.IsNullOrEmpty(paxCount) || x.PassengersCount == paxCount) &&
                            (string.IsNullOrEmpty(flightType) || x.FlightType == flightType) &&
                            (string.IsNullOrEmpty(cabinType) || x.OutBoundCabinClass.Split(',').Any(cabin =>
                                cabin.Trim().Replace(" ", "").Is(cabinType))) &&
                            !string.IsNullOrEmpty(x.FlyingClubNumber) &&
                            !x.FlyingClubNumber.Trim().Is("N/A") &&
                            !string.IsNullOrEmpty(x.TierCode) &&
                            (x.TierCode.Trim().Equals("Red", StringComparison.OrdinalIgnoreCase) ||
                             x.TierCode.Trim().Equals("Silver", StringComparison.OrdinalIgnoreCase) ||
                             x.TierCode.Trim().Equals("Gold", StringComparison.OrdinalIgnoreCase))
                        )
                    : tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x =>
                            !string.IsNullOrEmpty(x.RowKey) &&
                            (tableStorageService.TableName.Is("PNRs") ? x.Env.Is(envUpper) : (!string.IsNullOrEmpty(x.PartitionKey) && x.PartitionKey.Is(envUpper))) &&
                            !x.Journey.IsNullOrEmpty() &&
                            (string.IsNullOrEmpty(journeyType) || x.Journey == journeyType) &&
                            !x.PassengersCount.IsNullOrEmpty() &&
                            x.Timestamp.Date >= currentWeekMonday &&
                            x.Timestamp.Date <= today &&
                            (string.IsNullOrEmpty(origin) || x.Origin == origin) &&
                            (string.IsNullOrEmpty(paxCount) || x.PassengersCount == paxCount) &&
                            (string.IsNullOrEmpty(flightType) || x.FlightType == flightType) &&
                            (string.IsNullOrEmpty(cabinType) || x.OutBoundCabinClass.Split(',').Any(cabin =>
                                cabin.Trim().Replace(" ", "").Is(cabinType))) &&
                            (string.IsNullOrEmpty(reservationType) ||
                                (reservationType.Is("REVENUE") &&
                                    (string.IsNullOrEmpty(x.ReservationType) || x.ReservationType.Is("REVENUE"))) ||
                                (reservationType.Is("AWARD") && x.ReservationType.Is("AWARD")))
                        );

                int resultsCount = results.Count();

                if (results.Any())
                {
                    // Get random PNR from results
                    int randomIndex = random.Next(0, resultsCount);
                    CreatedPnrsTableEntity randomItem = results.ElementAt(randomIndex);

                    // Populate dictionary with PNR details
                    pnrDetails["pnr"] = randomItem.RowKey ?? string.Empty;
                    pnrDetails["origin"] = randomItem.Origin ?? string.Empty;
                    pnrDetails["destination"] = randomItem.Destination ?? string.Empty;
                    pnrDetails["journeyType"] = randomItem.Journey ?? string.Empty;
                    pnrDetails["departureDate"] = randomItem.DepartureDate ?? string.Empty;
                    pnrDetails["email"] = randomItem.Email ?? string.Empty;
                    pnrDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    pnrDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    pnrDetails["currency"] = randomItem.Currency ?? string.Empty;
                    pnrDetails["totalPrice"] = randomItem.TotalPrice ?? string.Empty;
                    pnrDetails["orderNo"] = randomItem.OrderNo ?? string.Empty;
                    pnrDetails["pri"] = randomItem.PRI ?? string.Empty;
                    pnrDetails["outboundCabinClass"] = randomItem.OutBoundCabinClass ?? string.Empty;
                    pnrDetails["inboundCabinClass"] = randomItem.InBoundCabinClass ?? string.Empty;
                    pnrDetails["passengersCount"] = randomItem.PassengersCount ?? string.Empty;
                    pnrDetails["timestamp"] = randomItem.Timestamp.ToString();
                    pnrDetails["flyingClubNumber"] = randomItem.FlyingClubNumber ?? string.Empty;
                    pnrDetails["reservationType"] = randomItem.ReservationType ?? string.Empty;
                    pnrDetails["emailVAC"] = randomItem.EmailFromBasket ?? string.Empty;
                    pnrDetails["Timestamp"] = randomItem.Timestamp.ToString() ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching PNR is found
                    pnrDetails["error"] = $"No matching PNR found for test case pre-requisite for journey type {journeyType}.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                pnrDetails["error"] = $"Error retrieving PNR details: {ex.Message}";
            }
            return pnrDetails;
        }

        public static Dictionary<string, string> RetrievePNRDetailsFromStorageTableForSpecificRoute(string journeyTypeString, string originString, string destinationString, string flightTypeString = null, string cabinType = null)
        {
            List<Dictionary<string, string>> all = RetrieveAllPNRDetailsFromStorageTableForSpecificRoute(journeyTypeString, originString, destinationString, flightTypeString, cabinType);
            return !all.Any()
                ? new Dictionary<string, string> { ["error"] = $"No matching PNR found for test case pre-requisite for journey type {journeyTypeString}, route {originString}-{destinationString}." }
                : all[random.Next(all.Count)];
        }

        public static List<Dictionary<string, string>> RetrieveAllPNRDetailsFromStorageTableForSpecificRoute(string journeyTypeString, string originString, string destinationString, string flightTypeString = null, string cabinType = null)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "MobileAppPnrs";
            tableStorageService.ConnectionString = connectionString;

            try
            {
                DateTime today = DateTime.UtcNow.Date;
                DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                return tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                    .Where(x =>
                        !string.IsNullOrEmpty(x.RowKey) &&
                        !x.PassengersCount.IsNullOrEmpty() &&
                        x.Timestamp.Date >= currentWeekMonday &&
                        x.Timestamp.Date <= today &&
                        (string.IsNullOrEmpty(journeyTypeString) || x.Journey == journeyTypeString) &&
                        (string.IsNullOrEmpty(flightTypeString) || x.FlightType == flightTypeString) &&
                        (string.IsNullOrEmpty(originString) || x.Origin.Equals(originString, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(destinationString) || x.Destination.Equals(destinationString, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(cabinType) || x.OutBoundCabinClass.Split(',').Any(cabin => cabin.Trim().Replace(" ", "").Equals(cabinType, StringComparison.OrdinalIgnoreCase)))
                    )
                    .Select(x => new Dictionary<string, string>
                    {
                        ["pnr"] = x.RowKey ?? string.Empty,
                        ["origin"] = x.Origin ?? string.Empty,
                        ["destination"] = x.Destination ?? string.Empty,
                        ["journeyType"] = x.Journey ?? string.Empty,
                        ["departureDate"] = x.DepartureDate ?? string.Empty,
                        ["email"] = x.Email ?? string.Empty,
                        ["firstName"] = x.FirstName ?? string.Empty,
                        ["lastName"] = x.LastName ?? string.Empty,
                        ["currency"] = x.Currency ?? string.Empty,
                        ["totalPrice"] = x.TotalPrice ?? string.Empty,
                        ["orderNo"] = x.OrderNo ?? string.Empty,
                        ["pri"] = x.PRI ?? string.Empty,
                        ["outboundCabinClass"] = x.OutBoundCabinClass ?? string.Empty,
                        ["inboundCabinClass"] = x.InBoundCabinClass ?? string.Empty,
                        ["passengersCount"] = x.PassengersCount ?? string.Empty,
                        ["timestamp"] = x.Timestamp.ToString(),
                        ["flyingClubNumber"] = x.FlyingClubNumber ?? string.Empty,
                        ["flightType"] = flightTypeString ?? string.Empty
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving PNR details for specific route: {ex.Message}");
                return new List<Dictionary<string, string>>();
            }
        }

        public static Dictionary<string, string> RetrievePNRDetailsFromStorageTableForCurrentWeekWithLoyaltyNumberPresent()
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "MobileAppPnrs";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> pnrDetails = new();
            Random random = new();

            try
            {
                // Get current date and find last Monday
                DateTime today = DateTime.UtcNow.Date;
                DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                IEnumerable<CreatedPnrsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x =>
                                  !string.IsNullOrEmpty(x.RowKey) &&
                                  !x.Journey.IsNullOrEmpty() &&
                                  !x.PassengersCount.IsNullOrEmpty() &&
                                  x.Timestamp.Date >= currentWeekMonday &&
                                  x.Timestamp.Date <= today &&
                                  !x.FlyingClubNumber.IsNullOrEmpty() && !x.FlyingClubNumber.Equals("N/A", StringComparison.OrdinalIgnoreCase)// Ensure FlyingClubNumber is present
                              );

                int resultsCount = results.Count();

                if (results.Any())
                {
                    // Get random PNR from results
                    int randomIndex = random.Next(0, resultsCount);
                    CreatedPnrsTableEntity randomItem = results.ElementAt(randomIndex);

                    // Populate dictionary with PNR details
                    pnrDetails["pnr"] = randomItem.RowKey ?? string.Empty;
                    pnrDetails["origin"] = randomItem.Origin ?? string.Empty;
                    pnrDetails["destination"] = randomItem.Destination ?? string.Empty;
                    pnrDetails["journeyType"] = randomItem.Journey ?? string.Empty;
                    pnrDetails["departureDate"] = randomItem.DepartureDate ?? string.Empty;
                    pnrDetails["email"] = randomItem.Email ?? string.Empty;
                    pnrDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    pnrDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    pnrDetails["currency"] = randomItem.Currency ?? string.Empty;
                    pnrDetails["totalPrice"] = randomItem.TotalPrice ?? string.Empty;
                    pnrDetails["orderNo"] = randomItem.OrderNo ?? string.Empty;
                    pnrDetails["pri"] = randomItem.PRI ?? string.Empty;
                    pnrDetails["outboundCabinClass"] = randomItem.OutBoundCabinClass ?? string.Empty;
                    pnrDetails["inboundCabinClass"] = randomItem.InBoundCabinClass ?? string.Empty;
                    pnrDetails["passengersCount"] = randomItem.PassengersCount ?? string.Empty;
                    pnrDetails["timestamp"] = randomItem.Timestamp.ToString();
                    pnrDetails["flyingClubNumber"] = randomItem.FlyingClubNumber ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching PNR is found
                    pnrDetails["error"] = $"No matching PNR found for test case pre-requisite for flying club number.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                pnrDetails["error"] = "Error retrieving PNR details.";
            }
            return pnrDetails;
        }

        public static Dictionary<string, string> RetrievePNRDetailsFromStorageTableForDirectFlightVSWithSelectedSeats(string flightType)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "MobileAppPnrs";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> pnrDetails = new();
            Random random = new();

            try
            {
                // Get current date and find last Monday for current week filtering
                DateTime today = DateTime.UtcNow.Date;
                DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                // Define allowed origins and destinations based on flight type
                string[] allowedOrigins = flightType == "VS" ? new[] { "LHR" } : new[] { "ATL", "LAS", "JFK", "MIA" };
                string[] allowedDestinations = flightType == "VS" ? new[] { "JFK", "DEL", "ATL", "LAX", "BOM", "GND", "SVD" } : new[] { "LHR" };
                string[] allowedPaxCount = new[] { "1", "2", "3" };

                IEnumerable<CreatedPnrsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x =>
                                  !string.IsNullOrEmpty(x.RowKey) &&
                                  !x.Journey.IsNullOrEmpty() &&
                                  !x.PassengersCount.IsNullOrEmpty() &&
                                  allowedPaxCount.Contains(x.PassengersCount) &&
                                  x.Timestamp.Date >= currentWeekMonday &&
                                  x.Timestamp.Date <= today &&
                                  !string.IsNullOrEmpty(x.Origin) &&
                                  allowedOrigins.Contains(x.Origin.ToUpperInvariant()) &&
                                  !string.IsNullOrEmpty(x.Destination) &&
                                  allowedDestinations.Contains(x.Destination.ToUpperInvariant()) &&
                                  x.IsSeats == true &&
                                  !x.SeatUpdate.IsNullOrEmpty() &&
                                  x.FlightType == flightType
                              );

                int resultsCount = results.Count();

                if (results.Any())
                {
                    // Get random PNR from results
                    int randomIndex = random.Next(0, resultsCount);
                    CreatedPnrsTableEntity randomItem = results.ElementAt(randomIndex);

                    // Populate dictionary with PNR details
                    pnrDetails["pnr"] = randomItem.RowKey ?? string.Empty;
                    pnrDetails["origin"] = randomItem.Origin ?? string.Empty;
                    pnrDetails["destination"] = randomItem.Destination ?? string.Empty;
                    pnrDetails["journeyType"] = randomItem.Journey ?? string.Empty;
                    pnrDetails["departureDate"] = randomItem.DepartureDate ?? string.Empty;
                    pnrDetails["email"] = randomItem.Email ?? string.Empty;
                    pnrDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    pnrDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    pnrDetails["currency"] = randomItem.Currency ?? string.Empty;
                    pnrDetails["totalPrice"] = randomItem.TotalPrice ?? string.Empty;
                    pnrDetails["orderNo"] = randomItem.OrderNo ?? string.Empty;
                    pnrDetails["pri"] = randomItem.PRI ?? string.Empty;
                    pnrDetails["outboundCabinClass"] = randomItem.OutBoundCabinClass ?? string.Empty;
                    pnrDetails["inboundCabinClass"] = randomItem.InBoundCabinClass ?? string.Empty;
                    pnrDetails["passengersCount"] = randomItem.PassengersCount ?? string.Empty;
                    pnrDetails["timestamp"] = randomItem.Timestamp.ToString();
                    pnrDetails["flyingClubNumber"] = randomItem.FlyingClubNumber ?? string.Empty;
                    pnrDetails["isSeats"] = randomItem.IsSeats?.ToString() ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching PNR is found
                    pnrDetails["error"] = $"No matching PNR found for {flightType} flight with IsSeats as True data in the storage table";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                Console.WriteLine($"Error retrieving PNR details for direct flight {flightType} with selected seats: {ex.Message}");
                pnrDetails["error"] = $"Error retrieving PNR details for direct flight {flightType} with selected seats.";
            }
            return pnrDetails;
        }

        public static Dictionary<string, string> RetrievePNRDetailsFromStorageTableForPartnerAirline(string partnerCode)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "MobileAppPnrsForLoyaltyAccounts";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> pnrDetails = new();
            Random random = new();

            try
            {
                // Get current date and find last Monday
                DateTime today = DateTime.UtcNow.Date;
                DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                IEnumerable<CreatedPnrsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x =>
                                  !string.IsNullOrEmpty(x.RowKey) &&
                                  !x.Journey.IsNullOrEmpty() &&
                                  !x.PassengersCount.IsNullOrEmpty() &&
                                   x.Timestamp.Date >= currentWeekMonday &&
                                   x.Timestamp.Date <= today &&
                                  !string.IsNullOrEmpty(x.FQTProgram) &&
                                   x.FQTProgram.StartsWith(partnerCode) // Directly use partnerCode
                              );

                int resultsCount = results.Count();

                if (results.Any())
                {
                    // Get random PNR from results
                    int randomIndex = random.Next(0, resultsCount);
                    CreatedPnrsTableEntity randomItem = results.ElementAt(randomIndex);

                    // Populate dictionary with PNR details
                    pnrDetails["pnr"] = randomItem.RowKey ?? string.Empty;
                    pnrDetails["origin"] = randomItem.Origin ?? string.Empty;
                    pnrDetails["destination"] = randomItem.Destination ?? string.Empty;
                    pnrDetails["journeyType"] = randomItem.Journey ?? string.Empty;
                    pnrDetails["departureDate"] = randomItem.DepartureDate ?? string.Empty;
                    pnrDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    pnrDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    pnrDetails["fQTProgram"] = randomItem.FQTProgram ?? string.Empty;
                    pnrDetails["passengersCount"] = randomItem.PassengersCount ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching PNR is found
                    pnrDetails["error"] = $"No matching PNR found for test case pre-requisite for partnercode {partnerCode}.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                pnrDetails["error"] = "Error retrieving PNR details.";
            }
            return pnrDetails;
        }

        public static Dictionary<string, string> RetrievePNRDetailsFromStorageTableForSeatScenarios(string flightType = null, string cabinType = null, string currency = null, string reservationType = null, string tierCode = null, string passengersCount = null, string origin = null, string destination = null)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "MobileAppPnrsForSeatScenarios";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> pnrDetails = new();
            Random random = new();

            try
            {
                DateTime today = DateTime.UtcNow.Date;
                DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                List<CreatedPnrsTableEntity> allItems = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult();

                IEnumerable<CreatedPnrsTableEntity> filterByTier(string tc)
                {
                    return allItems
                        .Where(x =>
                                  !string.IsNullOrEmpty(x.RowKey) &&
                                  !x.Journey.IsNullOrEmpty() &&
                                  !x.PassengersCount.IsNullOrEmpty() &&
                                  x.Timestamp.Date >= currentWeekMonday &&
                                  x.Timestamp.Date <= today &&
                                  x.IsSeats == false &&
                                  (string.IsNullOrEmpty(flightType) || x.FlightType == flightType) &&
                                  (string.IsNullOrEmpty(reservationType) || x.ReservationType == reservationType) &&
                                  (string.IsNullOrEmpty(tc) || x.TierCode == tc) &&
                                  (string.IsNullOrEmpty(currency) || x.Currency == currency) &&
                                  (string.IsNullOrEmpty(passengersCount) || x.PassengersCount == passengersCount) &&
                                  (string.IsNullOrEmpty(origin) || x.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase)) &&
                                  (string.IsNullOrEmpty(destination) || x.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)) &&
                                  (string.IsNullOrEmpty(cabinType) || x.OutBoundCabinClass.Split(',').Any(cabin => cabin.Trim().Replace(" ", "").Is(cabinType)))
                              );
                }

                string[] tiersToTry = tierCode?.Is("Red") == true ? new[] { tierCode, "Silver", "Gold" } : new[] { tierCode };
                List<CreatedPnrsTableEntity> results = new();
                foreach (string tier in tiersToTry)
                {
                    results = filterByTier(tier).ToList();
                    if (results.Any())
                        break;
                }

                if (results.Any())
                {
                    // Shuffle so callers that iterate get a random order (Fisher–Yates)
                    for (int i = results.Count - 1; i > 0; i--)
                    {
                        int j = random.Next(i + 1);
                        (results[i], results[j]) = (results[j], results[i]);
                    }

                    CreatedPnrsTableEntity selected = results[0];
                    pnrDetails["pnr"] = selected.RowKey ?? string.Empty;
                    pnrDetails["origin"] = selected.Origin ?? string.Empty;
                    pnrDetails["destination"] = selected.Destination ?? string.Empty;
                    pnrDetails["journeyType"] = selected.Journey ?? string.Empty;
                    pnrDetails["departureDate"] = selected.DepartureDate ?? string.Empty;
                    pnrDetails["email"] = selected.Email ?? string.Empty;
                    pnrDetails["firstName"] = selected.FirstName ?? string.Empty;
                    pnrDetails["lastName"] = selected.LastName ?? string.Empty;
                    pnrDetails["currency"] = selected.Currency ?? string.Empty;
                    pnrDetails["totalPrice"] = selected.TotalPrice ?? string.Empty;
                    pnrDetails["orderNo"] = selected.OrderNo ?? string.Empty;
                    pnrDetails["pri"] = selected.PRI ?? string.Empty;
                    pnrDetails["outboundCabinClass"] = selected.OutBoundCabinClass ?? string.Empty;
                    pnrDetails["inboundCabinClass"] = selected.InBoundCabinClass ?? string.Empty;
                    pnrDetails["passengersCount"] = selected.PassengersCount ?? string.Empty;
                    pnrDetails["timestamp"] = selected.Timestamp.ToString();
                    pnrDetails["flyingClubNumber"] = selected.FlyingClubNumber ?? string.Empty;
                    pnrDetails["isSeats"] = selected.IsSeats?.ToString() ?? string.Empty;
                    pnrDetails["reservationType"] = selected.ReservationType ?? string.Empty;
                    // Store a bounded candidate set so callers can iterate without serializing an unbounded table.
                    int maxCandidates = 10;
                    List<CreatedPnrsTableEntity> candidates = results.Take(maxCandidates).ToList();

                    pnrDetails["allCandidatePnrs"] = string.Join(",", candidates.Select(r => r.RowKey));
                    pnrDetails["allCandidateFirstNames"] = string.Join("||", candidates.Select(r => r.FirstName ?? string.Empty));
                    pnrDetails["allCandidateLastNames"] = string.Join("||", candidates.Select(r => r.LastName ?? string.Empty));
                }
                else
                {
                    pnrDetails["error"] = "No matching PNR found for seat scenarios (IsSeats == false).";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving PNR details for seat scenarios: {ex.Message}");

                pnrDetails["error"] = "Error retrieving PNR details for seat scenarios.";
            }
            return pnrDetails;
        }

        public static Dictionary<string, string> RetrievePNRDetailsFromOCIPNRsStorageTable()
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "OCIPNRs";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> pnrDetails = new();
            Random random = new();

            try
            {
                // Get tomorrow's date. POPS coverage requires a VS-operated flight departing tomorrow.
                DateTime today = DateTime.UtcNow.Date;
                DateTime tomorrow = today.AddDays(1);

                IEnumerable<CreatedPnrsTableEntity> results = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult()
                        .Where(x =>
                                  !string.IsNullOrEmpty(x.RowKey) &&
                                  !x.Journey.IsNullOrEmpty() &&
                                  !x.PassengersCount.IsNullOrEmpty() &&
                                  x.Airline == "VS" && // Ensure Airline is VS
                                  DateTime.TryParse(x.DepartureDate, out DateTime departureDate) &&
                                  departureDate.Date == tomorrow
                              );

                int resultsCount = results.Count();

                if (results.Any())
                {
                    // Get random PNR from results
                    int randomIndex = random.Next(0, resultsCount);
                    CreatedPnrsTableEntity randomItem = results.ElementAt(randomIndex);

                    // Populate dictionary with PNR details
                    pnrDetails["pnr"] = randomItem.RowKey ?? string.Empty;
                    pnrDetails["origin"] = randomItem.Origin ?? string.Empty;
                    pnrDetails["destination"] = randomItem.Destination ?? string.Empty;
                    pnrDetails["journeyType"] = randomItem.Journey ?? string.Empty;
                    pnrDetails["departureDate"] = randomItem.DepartureDate ?? string.Empty;
                    pnrDetails["firstName"] = randomItem.FirstName ?? string.Empty;
                    pnrDetails["lastName"] = randomItem.LastName ?? string.Empty;
                    pnrDetails["passengersCount"] = randomItem.PassengersCount ?? string.Empty;
                }
                else
                {
                    // Handle case where no matching PNR is found
                    pnrDetails["error"] = $"No matching PNR found for test case pre-requisite for aircraft details.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception and add an error entry to the dictionary
                pnrDetails["error"] = "Error retrieving PNR details.";
            }
            return pnrDetails;
        }

        #endregion MYTRIPS PNRs from Storage Table

        #region Holidays Bookings from Storage Table

        public static Dictionary<string, string> RetrieveHolidayBookingDetailsFromStorageTable(
                  string destination = null,
                  string[] optionStatus = null,
                  IEnumerable<string> requiredItems = null,
                  string company = null,
                  string departureDate = null,
                  string pastDepartureDate = null,
                  string bookingStatus = null,
                  string genericTypeContains = null)
        {
            TableStorageService<VHolsBookingsTableEntity> tableStorageService = new();
            tableStorageService.TableName = (_env.Is("sit") || _env.Is("dev")) ? "vHolsBookingsTestE" : "vHolsBookingsTestF";
            tableStorageService.ConnectionString = connectionString;

            Dictionary<string, string> bookingDetails = [];

            try
            {
                List<string> requiredItemsList = requiredItems?
                    .Where(i => !(i).IsNullOrWhiteSpace())
                    .Select(i => i.Trim())
                    .ToList();

                List<VHolsBookingsTableEntity> allItems = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult();

                List<VHolsBookingsTableEntity> itemsWithRequiredFields = allItems
                    .Where(x =>
                        !string.IsNullOrEmpty(x.RowKey) &&
                        !x.LeadName.IsNullOrEmpty() &&
                        !x.DateOfBirth.IsNullOrEmpty() &&
                        !x.Items.IsNullOrEmpty())
                    .ToList();

                IEnumerable<VHolsBookingsTableEntity> results = itemsWithRequiredFields
                    .Where(x =>
                        !string.IsNullOrEmpty(x.RowKey) &&
                        !x.LeadName.IsNullOrEmpty() &&
                        !x.DateOfBirth.IsNullOrEmpty() &&
                        !x.Items.IsNullOrEmpty() &&
                        (string.IsNullOrEmpty(destination) || x.Destination.Is(destination) || x.PartitionKey.Is(destination)) &&
                        (optionStatus == null || optionStatus.Length == 0 || optionStatus.Any(s => x.OptionStatus.Is(s))) &&
                        (string.IsNullOrEmpty(company) || x.Company.Is(company)) &&
                        (string.IsNullOrEmpty(bookingStatus) || x.BookingStatus.Is(bookingStatus)) &&
                        (requiredItemsList == null || requiredItemsList.Count == 0 || requiredItemsList.All(requiredItem =>
                            x.Items.Replace(";", ",")
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Any(item => item.Equals(requiredItem, StringComparison.OrdinalIgnoreCase)))) &&
                        (string.IsNullOrEmpty(genericTypeContains) || (!string.IsNullOrEmpty(x.GenericType) && x.GenericType.Contains(genericTypeContains, StringComparison.OrdinalIgnoreCase))) &&
                        (string.IsNullOrEmpty(departureDate) || (!string.IsNullOrEmpty(x.DepartureDate) && x.DepartureDate.CompareTo(departureDate) >= 0)) &&
                        (string.IsNullOrEmpty(pastDepartureDate) || (!string.IsNullOrEmpty(x.DepartureDate) && x.DepartureDate.CompareTo(pastDepartureDate) < 0))
                    );

                int resultsCount = results.Count();

                if (results.Any())
                {
                    int randomIndex = random.Next(0, resultsCount);
                    VHolsBookingsTableEntity randomItem = results.ElementAt(randomIndex);

                    string bookingReference = randomItem.Id.HasValue ? randomItem.Id.Value.ToString() : randomItem.RowKey;

                    bookingDetails["bookingReference"] = bookingReference ?? string.Empty;
                    bookingDetails["leadName"] = randomItem.LeadName ?? string.Empty;
                    bookingDetails["dateOfBirth"] = randomItem.DateOfBirth ?? string.Empty;
                    bookingDetails["items"] = randomItem.Items ?? string.Empty;
                    bookingDetails["destination"] = randomItem.Destination ?? string.Empty;
                    bookingDetails["departureDate"] = randomItem.DepartureDate ?? string.Empty;
                    bookingDetails["inbound"] = randomItem.Inbound ?? string.Empty;
                    bookingDetails["optionStatus"] = randomItem.OptionStatus ?? string.Empty;
                    bookingDetails["rowKey"] = randomItem.RowKey ?? string.Empty;
                    bookingDetails["id"] = randomItem.Id?.ToString() ?? string.Empty;
                    bookingDetails["bookingStatus"] = randomItem.BookingStatus ?? string.Empty;
                    bookingDetails["company"] = randomItem.Company ?? string.Empty;
                    bookingDetails["genericType"] = randomItem.GenericType ?? string.Empty;
                }
                else
                {
                    bookingDetails["error"] = "No matching holiday booking found in VHolsBookings storage table.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving holiday booking details: {ex.Message}");
                bookingDetails["error"] = "Error retrieving holiday booking details.";
            }

            return bookingDetails;
        }

        #endregion Holidays Bookings from Storage Table

        public static IEnumerable<NotificationsTableEntity> RetrieveNotificationRequests(string notificationName, string messageDateTime)
        {
            DateTime from = messageDateTime.Parse<DateTime>();
            Dictionary<string, string> notifications = new();

            TableStorageService<NotificationsTableEntity> tableStorageService = new()
            {
                TableName = "OrchesNotifStore",
                ConnectionString = notificationStoreConnectionString,
            };

            List<NotificationsTableEntity> results = tableStorageService.RetrieveItemsAsync(default)
                                               .GetAwaiter()
                                               .GetResult();

            return results.Where(r => !(r.PartitionKey).IsNullOrWhiteSpace() && r.PartitionKey.Is(notificationName) && r.Timestamp >= from)
                          .OrderByDescending(r => r.Timestamp);
        }
    }
}
