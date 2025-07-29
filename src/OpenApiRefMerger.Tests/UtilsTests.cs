using System.Text.Json.Nodes;

namespace OpenApiRefMerger.Tests;

public class UtilsTests
{
    [Fact]
    public void FindRef_FindTestResponseComponent()
    {
        // Arrange
        var root = JsonNode.Parse(TestJson)!.AsObject();
        var refPath = "#/components/schemas/TestResponse";

        // Act
        var result = Utils.FindRef(root, refPath);

        // Assert
        Assert.NotNull(result);

        Assert.True(result.ContainsKey("type"));
        Assert.Equal("object", result["type"]?.ToString());

        Assert.True(result.ContainsKey("properties"));
        Assert.True(result["properties"] is JsonObject);

        Assert.True(result["properties"]!.AsObject().ContainsKey("message"));
        Assert.Equal("string", result["properties"]!.AsObject()["message"]?["type"]?.ToString());
    }

    [Fact]
    public void MergeList_MergesAllReferencesInJsonArray()
    {
        // Arrange
        var root = JsonNode.Parse("""
            {
                "parameters": [
                    {
                        "$ref": "#/a/a"
                    },
                    {
                        "$ref": "#/a/b"
                    }
                ],
                "a":{
                    "a":{"c":{"$ref": "#/a/b"}}, "b":{}
                }
            }
            """)!.AsObject();
        var parameters = root["parameters"]!.AsArray();

        // Act
        Utils.MergeList(parameters, root);

        // Assert
        Assert.Equal(2, parameters.Count);
        Assert.True(parameters[0] is JsonObject);

        var p0 = parameters[0]!.AsObject();
        Assert.False(p0.TryGetPropertyValue("$ref", out var _));
        Assert.True(p0.TryGetPropertyValue("c", out var p0c));
        Assert.True(p0c is JsonObject);

        var c = p0c!.AsObject();
        Assert.False(c.TryGetPropertyValue("$ref", out var _));
    }

    [Fact]
    public void MergeDict_MergesAllReferencesInJsonObject()
    {
        // Arrange
        var root = JsonNode.Parse("""
            {
                "a": {
                    "$ref": "#/b"
                },
                "b": {
                    "c": {
                        "$ref": "#/b/d"
                    },
                    "d": {}
                }
            }
            """)!.AsObject();

        // Act
        Utils.MergeDict(root, root);

        // Assert
        Assert.True(root.ContainsKey("a"));
        Assert.True(root["a"] is JsonObject);

        var a = root["a"]!.AsObject();
        Assert.False(a.TryGetPropertyValue("$ref", out var _));
        Assert.True(a.ContainsKey("c"));
        Assert.True(a["c"] is JsonObject);

        var c = a["c"]!.AsObject();
        Assert.False(c.TryGetPropertyValue("$ref", out var _));
    }

    private const string TestJson = """
    {
        "openapi": "3.1.0",
        "info": {
            "title": "Test API",
            "version": "1.0.0"
        },
        "paths": {
            "/test": {
                "get": {
                    "parameters": [
                        {
                            "$ref": "#/components/parameters/TestParameter"
                        }
                    ],
                    "responses": {
                        "200": {
                            "description": "Success",
                            "content": {
                                "application/json": {
                                    "schema": {
                                        "$ref": "#/components/schemas/TestResponse"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        },
        "components": {
            "schemas": {
                "TestResponse": {
                    "type": "object",
                    "properties": {
                        "message": {
                            "type": "string"
                        }
                    }
                },
                "TestParameter": {
                    "type": "object",
                    "properties": {
                        "id": {
                            "type": "integer"
                        }
                    }
                }
            }
        }
    }
    """;
}
