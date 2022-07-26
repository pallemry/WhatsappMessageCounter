#define DISPLAY_USERS_WHILE_SCAN
using System.Globalization;
using System.Text.Json;

using WhatsappMessageCounterLibrary.Data_Classes;

namespace WhatsappMessageCounterLibrary;
internal class MessageConsoleCounter
{
    public Config Config { get; set; }
    private string optionalTxtContents = "";
    public MessageConsoleCounter(Config config) => Config = config;

    internal void Start()
    {
        var appDataPath = Config.AppDataPath;
        var PrintLine = Config.PrintLineMethod;
        var Print = Config.PrintMethod;
        var Input = Config.ReadLineMethod;
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        string dataPath;

        do
        {
            PrintLine("Enter a full path and file name to scan: ");
            dataPath = Input() ?? string.Empty;
        } while (!File.Exists(dataPath));

        var lines = File.ReadAllLines(dataPath);
        File.WriteAllText(dataPath, File.ReadAllText(dataPath).Replace(",", ""));
        Dictionary<string, MessageInformationCounter> records = new();
        Dictionary<string, string> specialCases = new();
        specialCases.Add("איל רבינוביץ'", "Eyal");
        specialCases.Add("נטע", "Neta");
        specialCases.Add("יונתן שלגינזר", "Yonatan something");
        specialCases.Add("אוריין", "Oriyan?");
        specialCases.Add("רוני", "Roni");
        specialCases.Add("איילת", "Ayelet");
        specialCases.Add("עלמה ארזי", "Alma ARAZI");
        specialCases.Add("יעל ליסיצין", "Yael Lis..?");
        specialCases.Add("עדי זלברברברברג", "Adi zelbebebebebebbebebeberg");
        specialCases.Add("מאיה", "Maya");
        specialCases.Add("נהורה", "Nehora");
        specialCases.Add("שילת", "Shilat");
        //specialCases.Add("", "");

        var prec = 0;
        var max = lines.Length / 100;
        var currentUser = new MessageInformationCounter { OriginalUserName = "" };
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var date = new DateTime();

            string dateRaw;
            try
            {
                dateRaw = line[..Config.DateMaxLength];
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException)
                    currentUser.TotalWords += line.Split(' ').Length;
                continue;
            }

            if (!IsValidDate(dateRaw.Replace("-", "").Trim()).IsValid)
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

            if (specialCases.ContainsKey(userName))
                userName = specialCases[userName];


            if (!records.ContainsKey(userName))
            {
                currentUser = new MessageInformationCounter { TotalMessages = 1, OriginalUserName = originalUserName };
                records.Add(userName, currentUser);
            }
            else
            {
                records[userName].TotalMessages++;
                currentUser = records[userName];
            }

            var words = line.Replace(originalUserName, "").Split(' ').ToList();
            words.RemoveAll(s => string.IsNullOrEmpty(s.Trim()) || s.Trim().ToLower() is ":" or "-" or "<media" or "omitted>");
            currentUser.TotalWords += words.Count;

            if (i >= max)
            {
                prec++;
                max += lines.Length / 100;
                Console.Clear();

                for (int j = 0; j < prec; j++)
                {
                    Print("|");
                }

                PrintLine($"\n{prec}%");
#if DISPLAY_USERS_WHILE_SCAN
                foreach (var record in records)
                {
                    PrintLine($"{record.Key}: Total messages: {record.Value.TotalMessages}," +
                                      $" Total Words: {record.Value.TotalWords}");
                }
#endif

            }
        }

        Console.Clear();

        for (int j = 0; j < prec; j++)
        {
            Print("|");
        }

        PrintLine($"\n{prec}%");

        var sortedRecords =
            from entry
                in records
            orderby entry.Value.TotalMessages
                descending
            select entry;

        int totalMessages = sortedRecords.Sum(pair => pair.Value.TotalMessages);
        int totalWords = sortedRecords.Sum(pair => pair.Value.TotalWords);
        int totalLines = lines.Length;
        PrintResultLine($"Original data source was extracted from: {dataPath}");
        PrintResultLine($"Total messages: {totalMessages}");
        PrintResultLine($"Total lines: {totalLines}");
        PrintResultLine("Winners: (Sorted by most messages)");

        var currUser = 0;

        foreach (var i in sortedRecords)
        {
            currUser++;
            PrintResultLine($"#{currUser}: ", false);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            PrintResultLine($"{i.Value.TotalMessages}",  false);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            PrintResultLine($" - TM (total messages) \"{i.Key}\"", false);
            PrintResultLine($", and {i.Value.TotalWords} total words!");
        }


        string answer;

        do
        {
            PrintLine("\nWould you like to save the results as JSON (J), \n" +
            "Readable Format (R), As Both (JR), Or Not Save at all (N)? (J/R/N)");
            answer = (Input() ?? string.Empty).ToLower();
            if (answer == "n") return;
        } while (answer != "j" && answer != "r" && answer != "jr");

        var formatToSaveAs = answer;
        var jsonAndNormal = answer == "jr";
        do
        {
            PrintLine("Please enter a file name to save the file as: ");
            answer = Input() ?? string.Empty;
        } while (!ValidName(answer, appDataPath));

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
                OriginalDataSourcePath = dataPath
            };
            var resultFilePath = appDataPath + answer;
            string fileContents = "", fullFormatName = "", fileExtension = "";
            const string jsonFullFormatName = "JSON document";
            const string normalFullFormatName = "Normal text";
            if (jsonAndNormal)
            {
                var jsonContents = JsonSerializer.Serialize(jsonObjectContents, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(appDataPath + answer + ".json", jsonContents);
                var normalContents = optionalTxtContents;
                File.WriteAllText(appDataPath + answer + ".txt", optionalTxtContents);
                PrintLine($"Successfully saved the results inside : {resultFilePath} as "
                + jsonFullFormatName + " and " + normalFullFormatName);
                return;
            }
            else if (formatToSaveAs == "j")
            {
                fullFormatName = jsonFullFormatName;
                fileExtension = ".json";
                fileContents = JsonSerializer.Serialize(jsonObjectContents, new JsonSerializerOptions { WriteIndented = true });
            }
            else if (formatToSaveAs == "r")
            {
                fullFormatName = normalFullFormatName;
                fileExtension = ".txt";
                fileContents = optionalTxtContents;
            }
            File.WriteAllText(resultFilePath + fileExtension, fileContents);
            PrintLine($"Successfully saved the results inside : {resultFilePath} as " + fullFormatName);

        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            PrintLine("FATAL: Failed to save file | REASON: ");
            PrintLine(e.ToString());
            Console.ForegroundColor = ConsoleColor.Black;
        }
    }

     /// <summary>
    /// Prints a new line and saves its contents
    /// </summary>
    /// <param name="line">The line represented as string to print</param>
    /// <param name="newLine">Wether to move to a new line. If true, moves to a new line,
    /// otherwise, stays on the same line</param>
    internal void PrintResultLine(string line, bool newLine = true) {
        optionalTxtContents += line;
        if (newLine)
            optionalTxtContents += "\n";
        if (newLine)
            Config.PrintMethod(line + "\n");
        else
            Config.PrintMethod(line);
    }
    internal static IsValidDateResult IsValidDate(string date)
    {
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