using System.Globalization;
using System.Text.Json;

using WhatsappMessageCounterLibrary.Data_Classes;

namespace WhatsappMessageCounterLibrary.Counter;

public class MessageCounter
{
    private string optionalTxtContents = "";
    public ProgressionInformation ProgressionInformation { get; private set; }
    public Config Config { get; internal set; }
    public event EventHandler<ProgressionInformation> OnProgressChanged;
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageCounter"> with the specified <see cref="Config"/>
    /// </summary>
    /// <param name="config">The configuration the message counter will be initialized with</param>
    public MessageCounter(Config config) 
    {
        Config = config;
        ProgressionInformation = new ProgressionInformation();
        if (!Directory.Exists(config.AppDataPath))
            Directory.CreateDirectory(config.AppDataPath);
    }


    /// <summary>
    /// Scans a whatsapp exported chat file, and then returns a result object, containing the results of the scan.
    /// Optionally you can save the file if you specify <see cref="fileToSaveResultsIn"/>
    /// </summary>
    /// <param name="fileName">The full file name and path to extract information from. Must end with *.txt or with no extension at all.</param>
    /// <param name="fileToSaveResultsIn">The name file to save the results in. No extension or path is required.</param>
    /// <param name="specialCases">The special cases, in which if a user with a key name is found,
    /// it is replaced by the value in the return value.</param>
    /// <returns>Returns a <see cref="ScanResults"/> object, representing the data collected in the scan.</returns>
    /// <exception cref="FileNotFoundException">In case the <see cref="fileName"/></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<ScanResults> ScanMessagesAsync(string fileName, string fileToSaveResultsIn,
                                                     Dictionary<string, string> specialCases = null)
    {
        var finalResult = await Task.Run(() =>
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException(nameof(fileName));
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Extension != string.Empty && fileInfo.Extension != ".txt")
                throw new ArgumentException(fileName + " must have the extension \".txt\" or no extension at all.");
            if (specialCases == null) specialCases = new Dictionary<string, string>();
            var lines = File.ReadAllLines(fileName);
            Dictionary<string, MessageInformationCounter> records = new();

            var appDataPath = Config.AppDataPath;
            var PrintLine = Config.PrintLineMethod;
            var Print = Config.PrintMethod;
            var Input = Config.ReadLineMethod;
            var ClearConsole = Config.ClearMethod;

            var prec = 0;
            var max = lines.Length / 100;
            var currentUser = new MessageInformationCounter { OriginalUserName = "" };

            for (var i = 0; i < lines.Length; i++)
            {
                if (i >= max)
                {
                    if (prec < 100) prec++;
                    max += lines.Length / 100;
                    var progressionAsString = records.Aggregate("",
                                                                (str, obj) =>
                                                                {
                                                                    return str +
                                                                           $"{obj.Key}: Total messages: {obj.Value.TotalMessages}," +
                                                                           $" Total Words: {obj.Value.TotalWords}\n";
                                                                });
                    OnProgressChanged?.Invoke(prec,
                                              new ProgressionInformation
                                              { Message = progressionAsString, Precentage = prec });

                    if (Config.ShouldPrintToConsole)
                    {
                        ClearConsole();

                        for (int j = 0; j < prec; j++)
                        {
                            Print("|");
                        }

                        PrintLine($"\n{prec}%");
                        PrintLine(progressionAsString);
                    }
                }

                var line = lines[i];

                var date = new DateTime();
                string dateRaw;

                try
                {
                    dateRaw = line[..Config.DateMaxLength];
                }
                catch (Exception e)
                {
                    if (e is ArgumentOutOfRangeException) currentUser.TotalWords += line.Split(' ').Length;
                    continue;
                }

                if (!IsValidDate(dateRaw.Replace("-", "").Trim(), Config.AllowBracesInDate).IsValid)
                {
                    currentUser.TotalWords += line.Split(' ').Length;
                    continue;
                }

                var incrementer = date.Month.ToString().Length > 1 ? 0 : 1;

                line = line.Replace(dateRaw, "");
                var indexOf = line.IndexOf(":", StringComparison.Ordinal);
                string userName, originalUserName;
                if (indexOf != -1)
                {
                    userName = line.Substring(0, indexOf).Replace("-", "").Trim();
                    originalUserName = userName;
                }
                else
                    continue;

                if (specialCases.ContainsKey(userName)) userName = specialCases[userName];


                if (!records.ContainsKey(userName))
                {
                    currentUser =
                        new MessageInformationCounter { TotalMessages = 1, OriginalUserName = originalUserName };
                    records.Add(userName, currentUser);
                }
                else
                {
                    records[userName].TotalMessages++;
                    currentUser = records[userName];
                }

                var words = line.Replace(originalUserName, "").Split(' ').ToList();

                words.RemoveAll(s => string.IsNullOrEmpty(s.Trim()) ||
                                     s.Trim().ToLower() is ":" or "-" or "<media" or "omitted>");
                currentUser.TotalWords += words.Count;
            }

            if (Config.ShouldPrintToConsole)
            {
                ClearConsole();

                for (int j = 0; j < prec; j++)
                {
                    Print("|");
                }

                PrintLine($"\n{prec}%");
            }


            var sortedRecords =
                from entry
                    in records
                orderby entry.Value.TotalMessages
                    descending
                select entry;

            int totalMessages = sortedRecords.Sum(pair => pair.Value.TotalMessages);
            int totalWords = sortedRecords.Sum(pair => pair.Value.TotalWords);
            int totalLines = lines.Length;
            PrintResultLine($"Original data source was extracted from: {fileName}");
            PrintResultLine($"Total messages: {totalMessages}");
            PrintResultLine($"Total lines: {totalLines}");
            PrintResultLine("Winners: (Sorted by most messages)");

            var currUser = 0;

            foreach (var i in sortedRecords)
            {
                currUser++;
                PrintResultLine($"#{currUser}: ", false);
                PrintResultLine($"{i.Value.TotalMessages}", false);
                PrintResultLine($" - TM (total messages) \"{i.Key}\"", false);
                PrintResultLine($", and {i.Value.TotalWords} total words!");
            }

            try
            {
                var jsonObjectContents = new ScanResults
                {
                    SortedRecords = sortedRecords,
                    TotalMessages = totalMessages,
                    TotalWords = totalWords,
                    TotalParticipaints =
                      sortedRecords.Count(),
                    TotalLines = totalLines,
                    OriginalDataSourcePath = fileName
                };
                var resultFilePath = appDataPath + fileToSaveResultsIn;
                string fileContents = "", fullFormatName = "", fileExtension = "";
                const string jsonFullFormatName = "JSON document";
                const string normalFullFormatName = "Normal text";

                if (Config.SaveOption == SaveOption.JsonAndNormal)
                {
                    var jsonContents =
                        JsonSerializer.Serialize(jsonObjectContents,
                                                 new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(appDataPath + fileToSaveResultsIn + ".json", jsonContents);
                    var normalContents = optionalTxtContents;
                    File.WriteAllText(appDataPath + fileToSaveResultsIn + ".txt", optionalTxtContents);
                    if (Config.ShouldPrintToConsole)
                        PrintLine($"Successfully saved the results inside : {resultFilePath} as "
                                + jsonFullFormatName + " and " + normalFullFormatName);
                    return new ScanResults
                    {
                        TotalLines = totalLines,
                        TotalMessages = totalMessages,
                        TotalWords = totalWords,
                        SortedRecords = sortedRecords,
                        TotalParticipaints = sortedRecords.Count(),
                        OptionalFileReadableContents = optionalTxtContents,
                        OriginalDataSourcePath = fileName,
                        SavedIn = resultFilePath,
                        OriginalRecords = records
                    };
                }
                else if (Config.SaveOption == SaveOption.Json)
                {
                    fullFormatName = jsonFullFormatName;
                    fileExtension = ".json";
                    fileContents =
                        JsonSerializer.Serialize(jsonObjectContents,
                                                 new JsonSerializerOptions { WriteIndented = true });
                }
                else if (Config.SaveOption == SaveOption.Normal)
                {
                    fullFormatName = normalFullFormatName;
                    fileExtension = ".txt";
                    fileContents = optionalTxtContents;
                }

                bool shouldSave = Config.SaveOption != SaveOption.None;
                if (shouldSave) File.WriteAllText(resultFilePath + fileExtension, fileContents);
                if (Config.ShouldPrintToConsole && shouldSave)
                    PrintLine($"Successfully saved the results inside : {resultFilePath} as " + fullFormatName);
                return new ScanResults
                {
                    TotalLines = totalLines,
                    TotalMessages = totalMessages,
                    TotalWords = totalWords,
                    SortedRecords = sortedRecords,
                    TotalParticipaints = sortedRecords.Count(),
                    OptionalFileReadableContents = optionalTxtContents,
                    OriginalDataSourcePath = fileName,
                    SavedIn = !shouldSave ? "Configured to not save" : resultFilePath,
                    OriginalRecords = records
                };

            }
            catch (Exception e)
            {
                if (Config.ShouldPrintToConsole)
                {
                    PrintLine("FATAL: Failed to save file | REASON: ");
                    PrintLine(e.ToString());
                }

                throw new Exception("Unable to save file | REASON: " + e.ToString(), e);
            }
        });
        return finalResult;
    }

    internal void PrintResultLine(string line, bool newLine = true)
    {
        optionalTxtContents += line;
        if (newLine)
            optionalTxtContents += "\n";
        if (newLine && Config.ShouldPrintToConsole)
            Config.PrintMethod(line + "\n");
        else if (Config.ShouldPrintToConsole)
            Config.PrintMethod(line);
    }

    internal static IsValidDateResult IsValidDate(string date, bool allowBraces = false)
    {
        if (allowBraces)
        {
            date = date.Replace("[", "").Replace("]", "");
            date = date.Replace("(", "").Replace(")", "");
            date = date.Replace("{", "").Replace("}", "");
        }
        bool isValidGb, isValidUs;
        DateTime? dt = null;
        try
        {
            dt = DateTime.Parse(date, CultureInfo.GetCultureInfo("en-GB"));
            isValidGb = true;
        }
        catch
        {
            isValidGb = false;
        }
        try
        {
            dt  = DateTime.Parse(date, CultureInfo.GetCultureInfo("en-US"));
            isValidUs = true;
        }
        catch
        {
            isValidUs = false;
        }

        var cultureString = isValidGb ? "en-GB" :
            isValidUs                 ? "en-US" : null;


        return new IsValidDateResult(isValidGb || isValidUs, date, dt, cultureString);
    }

    internal static bool ValidName(string fileName, string sourceFolder) => !string.IsNullOrEmpty(fileName) &&
                                                                            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                                                                            !File.Exists(Path.Combine(sourceFolder, fileName));
}