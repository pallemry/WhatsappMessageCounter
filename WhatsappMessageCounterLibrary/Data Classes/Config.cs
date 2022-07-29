namespace WhatsappMessageCounterLibrary.Data_Classes;
public class Config
{
    public static readonly string APP_DATA_PATH = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Whatsapp Message Counter\\";
    public string AppDataPath { get; set; }

    public Config(bool shouldPrintToConsole)
    {
        AppDataPath = APP_DATA_PATH;
        ShouldPrintToConsole = shouldPrintToConsole;
    }

    public Action<string> PrintLineMethod { get; set; }
    public Action<string?> PrintMethod { get; set; }
    public Func<string?> ReadLineMethod { get; set; }
    public bool ShouldPrintToConsole { get; set; }
    public Action ClearMethod { get; set; }
    public int DateMaxLength { get; set; } = 15;
    public SaveOption SaveOption { get; set; } = SaveOption.None;
    public bool AllowBracesInDate { get; set; } = false;

    internal static Config GetConfig(bool shouldPrintToConsole) => new(shouldPrintToConsole)
    {
        PrintMethod = Console.Write,
        PrintLineMethod = Console.WriteLine,
        ReadLineMethod = Console.ReadLine,
        ClearMethod = Console.Clear,
        DateMaxLength = 15,
        SaveOption = SaveOption.JsonAndNormal,
    };
}