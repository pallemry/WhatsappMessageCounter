using System.Globalization;

namespace WhatsappMessageCounterLibrary.Data_Classes;

internal class IsValidDateResult
{
    public IsValidDateResult(bool isValid, string rawDate, DateTime? dateTime, string? cultureInfoString)
    {
        RawDate = rawDate;
        IsValid = isValid;
        DateTime = dateTime;
        CultureInfoString = cultureInfoString;
    }
    public string RawDate { get; set; }
    public bool IsValid { get; set; }
    public DateTime? DateTime { get; set; }
    public string? CultureInfoString { get; set; }

    public CultureInfo? CultureInfo => CultureInfoString == null ? null : CultureInfo.GetCultureInfo(CultureInfoString);
}