using System.Text.Json.Nodes;

using OpenApiRefMerger;

var filepathArg = args[0];
var filepath = Path.GetFullPath(filepathArg);
var content = File.ReadAllText(filepath);

if (!content.Contains("$ref"))
{
    Console.WriteLine("No refs to be merged.");
    return;
}

var json = JsonNode.Parse(content)!.AsObject();
Directory.SetCurrentDirectory(Path.GetDirectoryName(filepath)!);
Utils.MergeDict(json, json);

Console.WriteLine(json.ToJsonString());
