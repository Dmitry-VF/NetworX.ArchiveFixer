// See https://aka.ms/new-console-template for more information

using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

Console.WriteLine("Enter CSV directory path:");
var input = Console.ReadLine();

if (!Directory.Exists(input))
{
    Console.WriteLine("Invalid path. Enter the valid path and try again!");
    Environment.Exit(1);
}

var files = Directory.GetFiles(input);

foreach (var file in files)
{
    if (Path.GetFileName(file).Contains(".bak")) continue;

    Console.WriteLine($"Checking {Path.GetFileName(file)}");
    var csvEntries = ReadCSV(file);
    var headers = "RecordType,Reference,PropertyName,LastEdited,EditedBy,OldValue,Value,IsLegacyData,BranchCode,IsExported".Split(",");

    //replace headers
    csvEntries[0] = headers;

    for (var i = 1; i < csvEntries.Count; i++)
    {
        var entry = csvEntries[i];

        if (entry is null) continue;

        var rowArray = entry.ToList();

        if (entry.Length < 10 && entry.Length > 0)
        {
            rowArray.Insert(5, "null");
            rowArray.Add("FALSE");
        }
        csvEntries[i] = rowArray.ToArray();
    }

    File.Copy(file, $"{file}.bak", true);

    File.WriteAllText(file, String.Empty);
    
    using (var f = File.CreateText(file))
    {
        foreach (var entry in csvEntries)
        {
            if(entry is null) continue;

            f.WriteLine(string.Join(",", entry));
        }
    }

    Console.WriteLine("OK");
}

Console.WriteLine("Fixed");

List<string[]> ReadCSV(string absolutePath)
{
    var result = new List<string[]>();

    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
    };

    using (var reader = new StreamReader(absolutePath))
    using (var parser = new CsvParser(reader, config))
    {
        while (parser.Read())
        {
            var row = parser.Record;
            result.Add(row);
            if (row == null)
            {
                break;
            }
        }
    }
    return result;
}

