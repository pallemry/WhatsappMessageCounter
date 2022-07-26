
# Whatsapp Message Counter
# Brief
Extracts and scans informatin from an exported whatsapp message chat file. Can extract information such as:

 - Total amount of **messages** sent
 - Total amount of **words** sent
 - Who sent the most to least messages
 - Save the results inside a `.json`file and/or a `.txt` file
# Installation
Nuget package - https://www.nuget.org/packages/WhatsappMessageCounterLibrary/

    dotnet add package WhatsappMessageCounterLibrary --version 1.0.0
# Usage
After you've successfully installed the package, 
```c#
    private static void Main()
    {
        AsyncTest().Wait();
    }
```
