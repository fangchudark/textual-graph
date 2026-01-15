#if TOOLS
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TextualGraph.Serialization;
using Godot;

namespace TextualGraph.Samples;

public class JsonArrayTextParser : TextParser
{
    public override string Id => "json_array";

    public override List<ParsedNodeFragment> Parse(TextReader reader)
    {
        var content = reader.ReadToEnd();
        if (string.IsNullOrEmpty(content))
            return [];

        using var doc = JsonDocument.Parse(content);

        var fragments = new List<ParsedNodeFragment>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var id = element.GetProperty("id");
            var next = element.GetProperty("next");
            if (element.TryGetProperty("text", out var text))
            {
                fragments.Add(new(
                    $"{id}",
                    "dialogue",
                    $$"""
                    {
                        "id":{{id}},
                        "next":{{(string.IsNullOrEmpty(next.ToString()) ? "\"\"" : next)}},
                        "text":"{{text}}"
                    }
                    """
                ));
                continue;
            }
            
            fragments.Add(new(
                $"{id}",
                "choice",
                $$"""
                {
                    "next":{{(string.IsNullOrEmpty(next.ToString()) ? "\"\"" : next)}},
                    "id":{{id}}
                }
                """
            ));
            
        }
        return fragments;
    }
}
#endif