[![NuGet](https://img.shields.io/nuget/v/OpenApiRefMerger?color=blue)](https://www.nuget.org/packages/OpenApiRefMerger)

# OpenApiRefMerger

OpenApiRefMerger is a .NET 9 command-line tool that recursively resolves and merges all `$ref` references in OpenAPI (Swagger) JSON files. This is especially useful for flattening OpenAPI documents that use local or file-based references, making them easier to consume by tools or services that do not support `$ref` resolution.

## Features

- **Resolves local and file-based `$ref` references** in OpenAPI JSON files.
- **Recursively merges referenced objects** into the main document.
- **Supports pretty-printed output** for improved readability.
- **Command-line interface** for easy integration into build pipelines or manual workflows.
- **Packaged as a .NET global tool** (`oarm`).

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- NuGet packages (automatically restored):
    - [CommandLineParser.Commands](https://www.nuget.org/packages/CommandLineParser.Commands)

## Installation

You can install OpenApiRefMerger as a .NET global tool from NuGet:

```bash
dotnet tool install --global OpenApiRefMerger
```

## Usage

After installation, use the `oarm` command:

```bash
oarm <input-file.json> [--output <output-file.json>] [--pretty]
```

### Arguments

- `<OpenAPI.json>`  
  Path to the OpenAPI JSON file to process. This argument is required.

### Options

- `-o`, `--output <output.json>`  
  Path to save the merged OpenAPI JSON file. If not specified, the merged JSON is printed to stdout.

- `-p`, `--pretty`  
  Format the output JSON with indentation for readability.

### Example

Merge all `$ref` references in `api.json` and save the result to `api.merged.json`:

```bash
oarm api.json --output api.merged.json --pretty
```

## How It Works

- The tool parses the input OpenAPI JSON file.
- It searches for all `$ref` properties, both local (e.g., `#/components/schemas/Model`) and file-based (e.g., `otherfile.json#/components/schemas/Model`).
- For each `$ref`, it loads and merges the referenced object into the main document, recursively resolving nested references.
- Remote (HTTP/HTTPS) references are not supported and will result in an error.
- The merged document is output to the specified file or to the console.

## Limitations

- Only JSON format is supported (YAML is not supported).
- Remote references (e.g., URLs) are not supported.
- Circular references are not explicitly detected and may cause a stack overflow.

## Testing

Unit tests are provided using xUnit. To run tests:

```bash
dotnet test
```

## License

See [LICENSE.txt](../../LICENSE.txt) for license information.

## Contributing

Contributions are welcome! Please open issues or pull requests on GitHub.

## Credits

- [CommandLineParser.Commands](https://github.com/commandlineparser/commandline)

---

*This tool is built with .NET 9 and uses the CommandLineParser library for argument parsing.*



