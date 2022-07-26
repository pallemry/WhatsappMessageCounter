namespace WhatsappMessageCounterLibrary.Data_Classes;
public class Config
{
    public string AppDataPath { get; set; }

    public Config(string appDataPath, bool shouldPrintToConsole)
    {
        AppDataPath = appDataPath;
        ShouldPrintToConsole = shouldPrintToConsole;
    }

    public Action<string> PrintLineMethod { get; set; }
    public Action<string?> PrintMethod { get; set; }
    public Func<string?> ReadLineMethod { get; set; }
    public bool ShouldPrintToConsole { get; set; } = false;
    public Action ClearMethod { get; set; }
    public int DateMaxLength { get; set; } = 15;
    public SaveOption SaveOption { get; set; } = SaveOption.None;
}