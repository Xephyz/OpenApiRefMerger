using CommandLine;

namespace OpenApiRefMerger;

public class Options
{
    [Value(0, Required = true, HelpText = "Path to the OpenAPI JSON file.", MetaName = "File path")]
    public required string FilePath { get; set; }

    [Option('o', "output", Required = false, HelpText = "Path to save the merged OpenAPI JSON file.")]
    public string? Output { get; set; } = null;

    [Option('p', "pretty", Required = false, HelpText = "Format the output JSON with indentation for readability.")]
    public bool PrettyPrint { get; set; } = false;
}
