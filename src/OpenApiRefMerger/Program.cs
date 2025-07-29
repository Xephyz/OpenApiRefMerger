using System.Text.Json.Nodes;

using CommandLine;

using OpenApiRefMerger;

var parseResult = Parser.Default.ParseArguments<Options>(args);
if (parseResult is not Parsed<Options> parsed)
{
    Environment.Exit(1);
    return;
}

var options = parsed.Value;

var filepathArg = options.FilePath;
var filepath = Path.GetFullPath(filepathArg);
var content = File.ReadAllText(filepath);

if (!content.Contains("$ref"))
{
    Console.WriteLine("No refs to be merged.");
    return;
}

var cwd = Directory.GetCurrentDirectory();
Directory.SetCurrentDirectory(Path.GetDirectoryName(filepath)!);
var json = JsonNode.Parse(content)!.AsObject();
Utils.MergeDict(json, json);
Directory.SetCurrentDirectory(cwd);

var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
{
    WriteIndented = options.PrettyPrint
};

if (!string.IsNullOrEmpty(options.Output))
{
    var outputPath = Path.GetFullPath(options.Output);
    File.WriteAllText(outputPath, json.ToJsonString(jsonSerializerOptions));
    Console.WriteLine($"Merged OpenAPI JSON saved to: {outputPath}");
}
else
{
    Console.WriteLine(json.ToJsonString(jsonSerializerOptions));
}
