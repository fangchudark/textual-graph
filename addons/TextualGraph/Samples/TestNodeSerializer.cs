#if TOOLS
using System.Collections.Generic;
using System.Text.Json;
using TextualGraph.Serialization;
using System.Linq;

namespace TextualGraph.Samples;

public class TestNodeSerializer : NodeSerializer
{
    public override string NodeType => TestNode.NodeName;

    public override NodeDeserializeResult Deserialize(string text)
    {
        using JsonDocument doc = JsonDocument.Parse(text);
        return new NodeDeserializeResult(
            new Dictionary<string, object>
            {
                ["text"] = doc.RootElement.GetProperty("text").GetString(),
                ["id"] = doc.RootElement.GetProperty("id").GetInt32()
            },
            null
        );
    }

    public override string Serialize(NodeData node, IGraphSerializationContext context)
    {
        var text = node.CustomData["text"].ToString();
        var id = node.CustomData["id"];
        var nextNodeId = context.GetOutgoing(node).FirstOrDefault()?.InputNodeId;
        var nextId = nextNodeId == null ? null : context.GetNode(nextNodeId).CustomData["id"];
        return $$"""
        {
            "id": {{id}},
            "next": {{(nextId == null ? "\"\"" : (int)nextId)}},
            "text": "{{text}}"            
        }
        """;
    }
}

public class TestNode2Serializer : NodeSerializer
{
    public override string NodeType => TestNode2.NodeName;

    public override NodeDeserializeResult Deserialize(string text)
    {
        using JsonDocument doc = JsonDocument.Parse(text);
        return new NodeDeserializeResult(
            new Dictionary<string, object>
            {
                ["id"] = doc.RootElement.GetProperty("id").GetInt32(),
            },
            null
        );
    }

    public override string Serialize(NodeData node, IGraphSerializationContext context)
    {
        var id = node.CustomData["id"];
        var nextNodeId = context.GetOutgoing(node).FirstOrDefault()?.InputNodeId;
        var nextId = nextNodeId == null ? null : context.GetNode(nextNodeId).CustomData["id"];
        return $$"""
        {
            "id": {{id}},
            "next": {{(nextId == null ? "\"\"" : (int)nextId)}}
        }
        """;
    }
}
#endif