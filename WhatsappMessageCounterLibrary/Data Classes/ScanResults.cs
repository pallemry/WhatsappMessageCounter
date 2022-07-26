namespace WhatsappMessageCounterLibrary.Data_Classes;

public class ScanResults{
    public IOrderedEnumerable<KeyValuePair<string, MessageInformationCounter>>? SortedRecords { get; internal set; }
    public int TotalMessages { get; internal set; }
    public int TotalWords { get; internal set; }
    public int TotalParticipaints { get; internal set; }
    public int TotalLines { get; internal set; }
    public string OriginalDataSourcePath { get; internal set; } = "";
    public string OptionalFileReadableContents { get; internal set; }
    public Dictionary<string, MessageInformationCounter> OriginalRecords { get; internal set; }
    public string SavedIn { get; internal set; }
}