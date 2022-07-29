#define DISPLAY_USERS_WHILE_SCAN
using System.Globalization;
using System.Text.Json;

using WhatsappMessageCounterLibrary.Counter;
using WhatsappMessageCounterLibrary.Data_Classes;

namespace WhatsappMessageCounterLibrary;
internal class MessageConsoleCounter
{
    public Config Config { get; set; }
    private string optionalTxtContents = "";
    public MessageConsoleCounter(Config config) => Config = config;

    internal async void Run()
    {
        var appDataPath = Config.AppDataPath;
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        string dataPath;

        do
        {
            Console.WriteLine("Enter a full path and file name to scan: ");
            dataPath = Console.ReadLine() ?? string.Empty;
        } while (!File.Exists(dataPath));

        var messageCounter = new MessageCounter(Config);
        var res = await messageCounter.ScanMessagesAsync(dataPath, "");

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
}