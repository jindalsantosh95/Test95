using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Utility.DataProvisioning;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 2 &&
            (args[0].Is("--write-oci-default") || args[0].Is("--write-trips-default")))
        {
            JsonSerializerSettings outputSettings = new();
            outputSettings.Converters.Add(new StringEnumConverter());
            outputSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            outputSettings.NullValueHandling = NullValueHandling.Ignore;
            string outputPath = Path.GetFullPath(args[1]);
            await File.WriteAllTextAsync(
                outputPath,
                JsonConvert.SerializeObject(
                    args[0].Is("--write-oci-default")
                        ? OciDefaultManifestFactory.Create()
                        : TripsDefaultManifestFactory.Create(),
                    Formatting.Indented,
                    outputSettings));
            Console.WriteLine($"Wrote default manifest to {outputPath}.");
            return 0;
        }

        string[] positionalArguments = args.Where(argument => !argument.StartsWith("--", StringComparison.Ordinal)).ToArray();
        string[] options = args.Where(argument => argument.StartsWith("--", StringComparison.Ordinal)).ToArray();
        if (options.Any(option => !option.Is("--validate-only") && !option.Is("--include-passenger-names")) ||
            options.Count(option => option.Is("--validate-only")) > 1 ||
            options.Count(option => option.Is("--include-passenger-names")) > 1 ||
            positionalArguments.Length is < 1 or > 2 ||
            !args[0].Is(positionalArguments[0]) ||
            !File.Exists(positionalArguments[0]))
        {
            Console.Error.WriteLine("Usage: dotnet run --project VAA.EAPI.Automation.Utility.DataProvisioning -- <manifest.json> [settings.json] [--validate-only] [--include-passenger-names]");
            return 2;
        }

        string manifestPath = Path.GetFullPath(args[0]);
        ProvisioningManifest manifest;
        try
        {
            JsonSerializerSettings serializerSettings = new();
            serializerSettings.Converters.Add(new StringEnumConverter { AllowIntegerValues = false });
            manifest = JsonConvert.DeserializeObject<ProvisioningManifest>(
                await File.ReadAllTextAsync(manifestPath),
                serializerSettings) ?? throw new InvalidDataException("Manifest is empty.");
            Validate(manifest);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Invalid manifest: {exception.Message}");
            return 2;
        }

        if (args.Contains("--validate-only", StringComparer.OrdinalIgnoreCase))
        {
            IReadOnlyList<ProvisioningDataSet> validatedDataSets = GetDataSets(manifest);
            Console.WriteLine($"Manifest is valid: {validatedDataSets.Count} data set(s), {validatedDataSets.Sum(dataSet => dataSet.Scenarios.Count)} scenario(s).");
            return 0;
        }

        string invocationDirectory = Directory.GetCurrentDirectory();
        string? settingsPath = args.Skip(1).FirstOrDefault(argument => !argument.StartsWith("--", StringComparison.Ordinal));
        if (settingsPath is not null)
        {
            string absoluteSettingsPath = Path.GetFullPath(settingsPath, invocationDirectory);
            ConfigReader.LoadEnvironmentSettingsFile(absoluteSettingsPath, optional: false);
        }
        else
            ConfigReader.LoadEnvironmentSettingsFile();

        // Legacy request helpers resolve their checked-in templates from the
        // current directory. The utility output contains the merged content
        // from its project references, so make invocation location irrelevant
        // after settings have been resolved relative to the caller.
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        using TokenFixture tokenFixture = new();
        PnrProvisioner provisioner = new(
            tokenFixture,
            options: new PnrProvisionerOptions
            {
                CardPaymentMode = manifest.CardPaymentMode,
                UsePlaywrightForHpp = true,
                HeadlessHpp = true,
            });
        using SemaphoreSlim loggedInScenarioGate = new(1, 1);
        bool includePassengerNames = args.Contains("--include-passenger-names", StringComparer.OrdinalIgnoreCase);
        (ProvisioningDataSet DataSet, PnrScenario Scenario)[] workItems = GetDataSets(manifest)
            .SelectMany(dataSet => dataSet.Scenarios.Select(scenario => (dataSet, scenario)))
            .ToArray();
        ProvisioningResult[] results = new ProvisioningResult[workItems.Length];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, workItems.Length),
            new ParallelOptions { MaxDegreeOfParallelism = manifest.MaxConcurrency },
            async (index, _) =>
            {
                (ProvisioningDataSet dataSet, PnrScenario scenario) = workItems[index];
                results[index] = await RunScenarioAsync(
                    provisioner,
                    loggedInScenarioGate,
                    dataSet,
                    scenario,
                    includePassengerNames);
            });

        Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
        return results.All(result => result.Status is not ProvisioningStatus.Failed) ? 0 : 1;
    }

    private static void Validate(ProvisioningManifest manifest)
    {
        IReadOnlyList<ProvisioningDataSet> dataSets = GetDataSets(manifest);
        if (dataSets.Count == 0 || dataSets.All(dataSet => dataSet.Scenarios.Count == 0))
            throw new InvalidDataException("At least one scenario is required.");
        if (manifest.MaxConcurrency is < 1 or > 8)
            throw new InvalidDataException("MaxConcurrency must be between 1 and 8.");
        if (dataSets.Any(dataSet => dataSet.Name.IsNullOrWhiteSpace()))
            throw new InvalidDataException("Every data set must have a name.");
        if (dataSets.GroupBy(dataSet => dataSet.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new InvalidDataException("Data set names must be unique.");

        IReadOnlyList<PnrScenario> scenarios = dataSets.SelectMany(dataSet => dataSet.Scenarios).ToArray();
        if (scenarios.Any(scenario => scenario.Name.IsNullOrWhiteSpace() || scenario.Name.Is("Unnamed")))
            throw new InvalidDataException("Every scenario must have a name.");
        if (scenarios.GroupBy(scenario => scenario.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new InvalidDataException("Scenario names must be unique.");

        foreach (ProvisioningDataSet dataSet in dataSets)
        {
            foreach (TableOutput output in dataSet.TableOutputs)
            {
                if (!output.Schema.HasValue)
                    throw new InvalidDataException($"Data set '{dataSet.Name}' has a table output without a schema.");
                if (!Enum.IsDefined(output.Schema.Value))
                    throw new InvalidDataException($"Data set '{dataSet.Name}' has unsupported table schema value '{(int)output.Schema.Value}'.");
                if (!System.Text.RegularExpressions.Regex.IsMatch(output.TableName, "^[A-Za-z][A-Za-z0-9]{2,62}$"))
                    throw new InvalidDataException($"Data set '{dataSet.Name}' has invalid Azure table name '{output.TableName}'. Table names must be 3-63 alphanumeric characters and start with a letter.");
            }

            bool requiresStoragePartitionKey = dataSet.TableOutputs.Any(output =>
                output.Schema is PnrTableSchema.Oci or PnrTableSchema.OciMobileApp);
            if (requiresStoragePartitionKey && dataSet.Scenarios.Any(scenario => scenario.StoragePartitionKey.IsNullOrWhiteSpace()))
                throw new InvalidDataException($"Data set '{dataSet.Name}' requires storagePartitionKey on every scenario written with an OCI schema.");
        }

        foreach (PnrScenario scenario in scenarios)
        {
            if (scenario.StoragePartitionKey is string storagePartitionKey &&
                !storagePartitionKey.IsNullOrWhiteSpace() &&
                !IsValidAzureTableKey(storagePartitionKey))
                throw new InvalidDataException($"Scenario '{scenario.Name}' has an invalid storagePartitionKey.");

            if (scenario.Adults < 1)
                throw new InvalidDataException($"Scenario '{scenario.Name}' requires at least one adult.");

            if (scenario.YoungAdults < 0 || scenario.Children < 0 || scenario.Infants < 0)
                throw new InvalidDataException($"Scenario '{scenario.Name}' has a negative passenger count.");

            if (scenario.JourneyType == JourneyType.MultiCity &&
                scenario.MultiCityLocations.Count is not 6 and not 8)
            {
                throw new InvalidDataException($"Scenario '{scenario.Name}' requires 6 or 8 multiCityLocations entries.");
            }

            if (scenario.JourneyType == JourneyType.MultiCity &&
                scenario.OfferSelection is not PnrOfferSelection.Any and not PnrOfferSelection.FareFamilyAndAirline)
            {
                throw new InvalidDataException(
                    $"Scenario '{scenario.Name}' uses offer selection '{scenario.OfferSelection}', which is not supported for multi-city provisioning.");
            }

            if (scenario.OfferSelection == PnrOfferSelection.LongLayover && !scenario.LongLayoverHours.HasValue)
                throw new InvalidDataException($"Scenario '{scenario.Name}' requires longLayoverHours.");

            if (scenario.Airline != FlightType.VS &&
                scenario.OfferSelection is PnrOfferSelection.Direct or
                    PnrOfferSelection.DirectWithStop or
                    PnrOfferSelection.OneStop or
                    PnrOfferSelection.TwoStops or
                    PnrOfferSelection.LongLayover)
            {
                throw new InvalidDataException(
                    $"Scenario '{scenario.Name}' uses VS-only offer selection '{scenario.OfferSelection}' with airline '{scenario.Airline}'.");
            }

            if (!scenario.PaymentType.Is("Card") &&
                !scenario.PaymentType.Is("CreditCard") &&
                !scenario.PaymentType.Is("PayPal"))
            {
                throw new InvalidDataException(
                    $"Scenario '{scenario.Name}' has unsupported paymentType '{scenario.PaymentType}'. Use Card, CreditCard, or PayPal.");
            }

            if (!scenario.FrequentFlyerProgramme.IsNullOrWhiteSpace() &&
                !System.Text.RegularExpressions.Regex.IsMatch(scenario.FrequentFlyerProgramme!, "^[A-Za-z0-9]{2,3}-[A-Za-z0-9]+$"))
            {
                throw new InvalidDataException(
                    $"Scenario '{scenario.Name}' has invalid frequentFlyerProgramme '{scenario.FrequentFlyerProgramme}'. Use PROGRAMME-NUMBER.");
            }

            if (scenario.MinimumLoyaltyPoints is < 0)
                throw new InvalidDataException($"Scenario '{scenario.Name}' has a negative minimumLoyaltyPoints value.");

            if (scenario.UseSeasonalSchedule &&
                !SeasonalRouteDateResolver.IsConfigured(
                    scenario.Origin.ToString(),
                    scenario.Destination.ToString()))
            {
                throw new InvalidDataException(
                    $"Scenario '{scenario.Name}' uses unconfigured seasonal route {scenario.Origin}-{scenario.Destination}.");
            }
        }
    }

    private static bool IsValidAzureTableKey(string value) =>
        value.Length <= 1024 &&
        value.IndexOfAny(['/', '\\', '#', '?']) < 0 &&
        !value.Any(character => character is <= '\u001f' or >= '\u007f' and <= '\u009f');

    private static async Task<ProvisioningResult> RunScenarioAsync(
        PnrProvisioner provisioner,
        SemaphoreSlim loggedInScenarioGate,
        ProvisioningDataSet dataSet,
        PnrScenario scenario,
        bool includePassengerNames)
    {
        if (scenario.UseSeasonalSchedule &&
            (!SeasonalRouteDateResolver.TryResolve(
                 scenario.Origin.ToString(),
                 scenario.Destination.ToString(),
                 DateTime.UtcNow.Date.AddDays(1),
                 out DateTime firstAvailableDate) ||
             !SeasonalRouteDateResolver.TryResolve(
                 scenario.Origin.ToString(),
                 scenario.Destination.ToString(),
                 firstAvailableDate.AddDays(scenario.DepartureOffsetDays ?? 0),
                 out _)))
        {
            return new ProvisioningResult(
                dataSet.Name,
                scenario.Name,
                ProvisioningStatus.SeasonalUnavailable,
                null,
                null,
                null,
                null,
                $"No future service date is configured for seasonal route {scenario.Origin}-{scenario.Destination}.",
                dataSet.TableOutputs.Select(output => output.TableName).ToArray());
        }

        bool loggedInGateAcquired = false;
        try
        {
            if (scenario.LoggedInUser)
            {
                await loggedInScenarioGate.WaitAsync();
                loggedInGateAcquired = true;
            }

            IPnrSink[] sinks = dataSet.TableOutputs
                .Select(output => (IPnrSink)new ConfigurableTablePnrSink(
                    output.Schema ?? throw new InvalidDataException("A validated table output is missing its schema."),
                    output.TableName))
                .ToArray();
            IReadOnlyList<string> storageTables = dataSet.TableOutputs.Select(output => output.TableName).ToArray();
            CreatedPnr? createdPnr = await provisioner.CreateAndStoreAsync(scenario, sinks);
            return createdPnr is null
                ? new ProvisioningResult(dataSet.Name, scenario.Name, ProvisioningStatus.NoInventory, null, null, null, null, null, storageTables)
                : new ProvisioningResult(
                    dataSet.Name,
                    scenario.Name,
                    ProvisioningStatus.Created,
                    createdPnr.PnrCode,
                    includePassengerNames ? createdPnr.LeadPassengerFirstName : null,
                    includePassengerNames ? createdPnr.LeadPassengerLastName : null,
                    null,
                    null,
                    storageTables);
        }
        catch (PnrProvisioningException exception)
        {
            CreatedPnr? createdPnr = exception.CreatedPnr;
            return new ProvisioningResult(
                dataSet.Name,
                scenario.Name,
                ProvisioningStatus.Failed,
                createdPnr?.PnrCode,
                includePassengerNames ? createdPnr?.LeadPassengerFirstName : null,
                includePassengerNames ? createdPnr?.LeadPassengerLastName : null,
                exception.FailedStage.ToString(),
                GetSanitizedError(exception),
                dataSet.TableOutputs.Select(output => output.TableName).ToArray());
        }
        catch (Exception exception)
        {
            return new ProvisioningResult(
                dataSet.Name,
                scenario.Name,
                ProvisioningStatus.Failed,
                null,
                null,
                null,
                PnrStage.Runner.ToString(),
                GetSanitizedError(exception),
                dataSet.TableOutputs.Select(output => output.TableName).ToArray());
        }
        finally
        {
            if (loggedInGateAcquired)
                loggedInScenarioGate.Release();
        }
    }

    private static string GetSanitizedError(Exception exception)
    {
        List<string> exceptionTypes = new();
        Exception? current = exception;
        while (current is not null)
        {
            string exceptionType = current.GetType().Name;
            if (!exceptionTypes.Contains(exceptionType, StringComparer.Ordinal))
                exceptionTypes.Add(exceptionType);
            current = current.InnerException;
        }

        return string.Join(" --> ", exceptionTypes);
    }

    private static IReadOnlyList<ProvisioningDataSet> GetDataSets(ProvisioningManifest manifest)
    {
        if (manifest.DataSets.Count > 0 && manifest.Scenarios.Count > 0)
            throw new InvalidDataException("Use either dataSets or the legacy root scenarios list, not both.");

        return manifest.DataSets.Count > 0
            ? manifest.DataSets
            : manifest.Scenarios.Count > 0
                ? new[] { new ProvisioningDataSet { Name = "default", Scenarios = manifest.Scenarios } }
                : Array.Empty<ProvisioningDataSet>();
    }
}
