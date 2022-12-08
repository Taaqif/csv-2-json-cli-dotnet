using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.CommandLine;

namespace CSV2JSON;

class Program
{
    static async Task<int> Main(string[] args)
    {
        RootCommand rootCommand = new RootCommand("Converts a CSV file to JSON");

        var fileInputOption = new Option<FileInfo>(
            aliases: new string[] { "--input", "-i" },
            description: "The file to read and convert.",
            parseArgument: result =>
            {
                string? filePath = result.Tokens.Single().Value;
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = "File does not exist";
                    return null;
                }
                else
                {
                    return new FileInfo(filePath);
                }
            }
        ){ IsRequired = true };


        var fileOutputOverwriteOption = new Option<bool>(
            name: "--overwrite-existing",
            description: "Whether to overwrite the existing output.",
            getDefaultValue:() => false
        );

        var fileOutputOption = new Option<FileInfo>(
            aliases: new string[] { "--output", "-o" }, 
            description: "The file to read and convert.",
            parseArgument: result =>
            {
                var isOverwrite = result.GetValueForOption(fileOutputOverwriteOption);
                string? filePath = result.Tokens.Single().Value;
                if (File.Exists(filePath) && !isOverwrite)
                {
                    result.ErrorMessage = "Output file already exists. Use --overwrite-existing option to overwrite the output";
                    return null;
                }
                else
                {
                    return new FileInfo(filePath);
                }
            }
        )
        { IsRequired = true };
        
        rootCommand.AddOption(fileInputOption);
        rootCommand.AddOption(fileOutputOption);
        rootCommand.AddOption(fileOutputOverwriteOption);
        

        rootCommand.SetHandler(async (fileIn, fileOut) =>
        {
            await ProcessCSV(fileIn, fileOut);
        }, fileInputOption, fileOutputOption);

        return await rootCommand.InvokeAsync(args);
    }
    static async Task ProcessCSV(FileInfo fileIn, FileInfo fileOut)
    {

        using (var reader = new StreamReader(fileIn.FullName))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>().ToList();
            var json = JsonConvert.SerializeObject(records);
            await File.WriteAllTextAsync(fileOut.FullName, json);
        }
    }
}




