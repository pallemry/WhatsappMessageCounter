
# Whatsapp Message Counter
# Brief
Extracts and scans informatin from an exported whatsapp message chat file. Can extract information such as:

 - Total amount of **messages** sent
 - Total amount of **words** sent
 - Who sent the most to least messages
 - Save the results inside a `.json` file and/or a `.txt` file

[See how to export chat ->](https://faq.whatsapp.com/196737011380816/?locale=en_US)
# Installation
Nuget package - https://www.nuget.org/packages/WhatsappMessageCounterLibrary/

    dotnet add package WhatsappMessageCounterLibrary --version 1.0.3
# Usage
After you've successfully installed the package, start by importing the the namespaces `WhatsappMessageCounterLibrary.Counter` and `WhatsappMessageCounterLibrary.Data_Classes` by using a [`using statement`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement) creating a new [`Config`](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Data%20Classes/Config.cs) object like this:
```csharp
    Config config = new Config(false or true);
```
The first parameter represents wether it the scanner should print to the console. Generally if you use this app on a console app you'd likely want to set this to true. If set to true, you have to specify the `Console` methods like this (you can just copy & paste this):
```csharp
    config.PrintMethod = Console.Write;
    config.PrintLineMethod = Console.WriteLine;
    config.ClearMethod = Console.Clear;
    config.ReadLineMethod = Console.ReadLine;
```
After you've created your `Config` object. You can create a `MessageCounter` object which is the core of the scan.
```csharp
	MessageCounter counter = new MessageCounter(config);
```
Finally, you can scan your file using the `MessageCounter.ScanMessagesAsync(string, string, [Dictionary<string, string> = null])`method like this:
```csharp
	var result = await counter.ScanMessagesAsync(#FILE_PATH#, #SAVE_RESULT_NAME#, #SPECIAL_NAME_CASES#);
```

> NOTE: 
>The `#SAVE_RESULT_NAME#` is the name of the save file, there's no need for a path or an extension, just the pure name . For example: ``#SAVE_RESULT_NAME# = "Test"``, then if conifgured to be svaed the file will be saved inside `./AppData/Whatsapp Message Counter/Test.json`
# Trouble-shooting
If you're getting nonsense results, try changing the `Config.DateMaxLength` to 16 or 17, and see how it goes. If your contacts are saved in a foreign langauage try to use the `specialCases` in `MessageCounter.ScanMessagesAsync` method to manualy translate them to english.
# Classes & Members.
[**`MessageCounter`**](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Counter/MessageCounter.cs) class:

**Description**: This class is the heart of the package. It is responsible for actually scanning the files.

**Members**:
|Member Type |Member Full Name| Summary | Extra Description|
|--|--|--|--|
|  Constructor| `MessageCounter(Config config)` |Constructs a new `MessageCounter` object with the specified configuration.| -
|Event|`OnProgressChanged : EventHandler<`[`ProgressionInformation`](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Data%20Classes/ProgressionInformation.cs)`>`|Raised every time the scan has progressed by a notable amount. i.e. 1%|The event has `ProgressionInformation` parameter which specifys information about the how for the progression has gone and other information|
|Method|`Task<`[`ScanResults`](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Data%20Classes/ScanResults.cs)`> ScanMessagesAsync(string fileName, string fileToSaveResultsIn, Dictionary<string, string> specialCases = null)`|Scans the `fileName` file, and if configured to do so. Saves the file inside `fileToSaveResultsIn` as [json doucment](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument?view=net-6.0) or txt file|The special cases specifys cases where the app will replace a user's username with the value. For Example: `55-217-8910` sent a message: "hello" and I want him to appear as "John" in the final result of the scan. To do this, there should be an element in the `specialCases` dictionary with the Key: `55-217-8910`, Value: `"John"`. |
|Property|[`Config`](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Data%20Classes/Config.cs) `Config { get; internal set; }`|The configuration which the `ScanMessagesAsync` uses to configure its behavior|-|

[**`Config`**](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Data%20Classes/Config.cs) class:

**Description**: This class is the heart of the package. It is responsible for actually scanning the files.

**Members**: 
|Member Type|Member Full Name|Summary|Extra Description|
|--|--|--|--|
|Constructor|`Config(bool shouldPrintToConsole)`|Constructs a new `Config` object and initializes the `ShouldPrintToConsole` property to the parameter given|-|
|Property|`bool ShouldPrintToConsole { get; set; }`|Wether to print results and progression to the console.|Taken into account in `MessageCounter.ScanMessagesAsync`
|Property|[`SaveOption`](https://github.com/pallemry/WhatsappMessageCounter/blob/main/WhatsappMessageCounterLibrary/Data%20Classes/SaveOption.cs) `SaveOption { get; set; }`|Wether to save the results or not, and in what format (e.g. Json, txt)|Taken into account in `MessageCounter.ScanMessagesAsync`
|Property|`int DateMaxLength { get; set; }`|The max length of a date in a message|For example: `"19.11.2022 19:32 - ..."`: 16, the default value is 15|
|Property|` public string AppDataPath { get; set; }`|The app data path of the program, which will be used to save results in|The Default value is: `$"C:\\Users\\{` [`Environment.UserName`](https://docs.microsoft.com/en-us/dotnet/api/system.environment.username?view=net-6.0) `}\\AppData\\Local\\Whatsapp Message Counter\\"`|
|Property|`Action<string?> PrintMethod { get; set; }`|The method the counter will use to print values|-
|Property|`Action<string> PrintLineMethod { get; set; }`|The method the counter will use to print values in a new line|-
|Property|`Func<string?> ReadLineMethod { get; set; }`|The method the counter will use to read (input) values from the user|-
|Property|`Action ClearMethod { get; set; }`|The method the counter will use clear all the contents of the console/stream|-
|Property|`bool AllowBracesInDate { get; set; }`|Wether to allow(ignore) characters like "{..}", "[..]" and "()" while reading the date from the source file|-

# References

 - https://docs.microsoft.com/en-us/dotnet/api/system.environment.username?view=net-6.0
 - https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument?view=net-6.0
 - https://faq.whatsapp.com/196737011380816/?locale=en_US
 
 Enjoy~ 
 :)
