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
        int returnCode = 0;
        RootCommand rootCommand = new RootCommand("Converts a CSV file to JSON");

        var fileOutputOverwriteOption = new Option<bool>(
            name: "--overwrite-existing",
            description: "Whether to overwrite the existing output.",
            getDefaultValue:() => false
        );

        var fileOutputOption = new Option<FileInfo>(
            aliases: new string[] { "--output", "-o" }, 
            description: "The output file to save. Defaults to <inputFile>.json",
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
        );
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
                    var fileInfo = new FileInfo(filePath);
                    var outFile = result.GetValueForOption(fileOutputOption);
                    if(outFile == null)
                    {
                        var isOverwrite = result.GetValueForOption(fileOutputOverwriteOption);
                        var autoOutFileName = Path.GetFileNameWithoutExtension(fileInfo.Name) + ".json";
                        var autoOutFilePath = Path.Combine(fileInfo.DirectoryName, autoOutFileName);
                        if (File.Exists(autoOutFilePath) && !isOverwrite)
                        {
                            result.ErrorMessage = $"Output file {autoOutFilePath} already exists. Use --overwrite-existing option to overwrite the output";
                            return null;
                        }
                        else
                        {
                            return new FileInfo(filePath);
                        }
                    }
                    else
                    {
                        return fileInfo;
                    }
                }
            }
        )
        { IsRequired = true };

        rootCommand.AddOption(fileInputOption);
        rootCommand.AddOption(fileOutputOption);
        rootCommand.AddOption(fileOutputOverwriteOption);
        

        rootCommand.SetHandler(async (fileIn, fileOut) =>
        {
            returnCode = await ProcessCSV(fileIn, fileOut);
        }, fileInputOption, fileOutputOption);

        await rootCommand.InvokeAsync(args);
        
        return returnCode;
    }
    static async Task<int> ProcessCSV(FileInfo fileIn, FileInfo fileOut)
    {
        try
        {
            throw new Exception();
            if (fileOut == null)
            {
                var autoOutFilePath = Path.GetFileNameWithoutExtension(fileIn.Name) + ".json";
                fileOut = new FileInfo(Path.Combine(fileIn.DirectoryName, autoOutFilePath));
            }
            using (var reader = new StreamReader(fileIn.FullName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();
                var json = JsonConvert.SerializeObject(records);
                await File.WriteAllTextAsync(fileOut.FullName, json);
                Console.WriteLine($"Output JSON file: {fileOut.FullName}");
                return 0;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not convert file. {e.Message}");
            return -2;
        }
        
    }
}




