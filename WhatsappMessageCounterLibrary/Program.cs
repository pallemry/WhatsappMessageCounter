﻿using WhatsappMessageCounterLibrary.Counter;
using WhatsappMessageCounterLibrary.Data_Classes;

namespace WhatsappMessageCounterLibrary;

internal static class Program
{
    private static void Main()
    {
        AsyncTest().Wait();
    }

    private static async Task AsyncTest()
    {
        

        MessageCounter counter = new MessageCounter(Config.GetConfig(false));
        counter.OnProgressChanged += (sender, args) =>
        {
            System.Console.Write("|");
            Console.Title = $"{args.Precentage}%";
        };
        var result = await counter.ScanMessagesAsync(Console.ReadLine(), "Test1234");
        var sortedRecords = result.SortedRecords;

        int totalMessages = sortedRecords.Sum(pair => pair.Value.TotalMessages);
        int totalWords = sortedRecords.Sum(pair => pair.Value.TotalWords);
        int totalLines = result.TotalLines;
        Console.WriteLine($"Original data source was extracted from: {result.OriginalDataSourcePath}");
        Console.WriteLine($"Total messages: {totalMessages}");
        Console.WriteLine($"Total lines: {totalLines}");
        Console.WriteLine("Winners: (Sorted by most messages)");

        var currUser = 0;

        foreach (var i in sortedRecords)
        {
            currUser++;
            Console.Write($"#{currUser}: ");
            Console.Write($"{i.Value.TotalMessages}");
            Console.Write($" - TM (total messages) \"{i.Key}\"");
            Console.WriteLine($", and {i.Value.TotalWords} total words!");
        }

        Console.WriteLine();
    }
}
