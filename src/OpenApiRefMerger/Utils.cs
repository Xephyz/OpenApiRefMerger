using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenApiRefMerger;

public static class Utils
{
    public static JsonObject FindRef(JsonObject root, string refPath)
    {
        if (refPath.StartsWith("http"))
        {
            throw new NotSupportedException("Remote references are not supported (yet?).");
        }

        if (string.IsNullOrEmpty(refPath))
        {
            return root.DeepClone().AsObject();
        }

        if (refPath.StartsWith("#"))
        {
            // This is a local reference.
            var parts = refPath.Split('/').Skip(1).ToList();
            var current = root;
            foreach (var part in parts)
            {
                if (current!.TryGetPropertyValue(part, out var next) &&
                    next is JsonObject nextObj)
                {
                    current = nextObj;
                }
                else
                {
                    throw new KeyNotFoundException($"Property '{part}' not found in the JSON structure or was not object type.");
                }
            }

            return current.DeepClone().AsObject();
        }

        var uri = new Uri(Path.GetFullPath(refPath.Split('#')[0]), UriKind.Absolute);
        Debug.Assert(uri.IsAbsoluteUri);

        var refContent = JsonNode.Parse(File.ReadAllText(uri.LocalPath))!.AsObject();
        var fragment = refPath.AsSpan().Slice(refPath.IndexOf('#')).ToString();

        return FindRef(refContent, fragment);
    }

    public static void MergeList(JsonArray l, JsonObject root)
    {
        var cwd = new Uri(Directory.GetCurrentDirectory());

        for (var i = 0; i < l.Count; i++)
        {
            if (l[i] is not JsonObject obj)
            {
                continue;
            }

            if (!obj.TryGetPropertyValue("$ref", out var refProp))
            {
                MergeDict(obj, root);
                continue;
            }

            var refPropValue = refProp!.AsValue().ToString();
            l[i] = FindRef(root, refPropValue);
            var refPath = new Uri(Path.GetFullPath(refPropValue.Split('#')[0]), UriKind.RelativeOrAbsolute);
            Debug.Assert(refPath.IsAbsoluteUri);

            if (string.IsNullOrEmpty(refPath.LocalPath))
            {
                MergeDict(obj, root);
            }
            else
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(refPath.LocalPath)!);
                MergeDict(obj, obj);
                Directory.SetCurrentDirectory(cwd.LocalPath);
            }
        }
    }

    public static void MergeDict(JsonObject dict, JsonObject root)
    {
        var cwd = new Uri(Directory.GetCurrentDirectory());

        foreach (var kvp in dict)
        {
            if (kvp.Value is JsonArray arr)
            {
                MergeList(arr, root);
            }
            else if (kvp.Value is JsonObject nestedObj)
            {
                if (nestedObj.TryGetPropertyValue("$ref", out var refProp))
                {
                    var refPropValue = refProp!.AsValue().ToString();
                    nestedObj.ReplaceWith(FindRef(root, refPropValue));
                    if (refPropValue.StartsWith("#"))
                    {
                        MergeDict(nestedObj, root);
                    }
                    else
                    {
                        var refPath = new Uri(Path.GetFullPath(refPropValue.Split('#')[0]), UriKind.RelativeOrAbsolute);
                        Debug.Assert(refPath.IsAbsoluteUri);
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(refPath.LocalPath)!);
                        MergeDict(nestedObj, nestedObj);
                        Directory.SetCurrentDirectory(cwd.LocalPath);
                    }
                }
                else
                {
                    MergeDict(nestedObj, root);
                }
            }
        }
    }
}
