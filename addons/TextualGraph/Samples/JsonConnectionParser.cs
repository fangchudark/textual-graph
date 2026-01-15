#if TOOLS
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TextualGraph.Serialization;

namespace TextualGraph.Samples;

public class JsonConnectionParser : ConnectionParser
{
    public override string Id => "json";

    public override List<string> Order(GraphData graph, Dictionary<string, string> nodeFragments)
    {
        var orderedIds = new List<string>();
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        
        var allNodeIds = new HashSet<string>(graph.Nodes.Select(n => n.NodeId));
        var nodesWithInput = new HashSet<string>(graph.Connections.Select(c => c.InputNodeId));
        var entryNodes = graph.Nodes
            .Where(n => !nodesWithInput.Contains(n.NodeId))
            .OrderBy(n => n.NodeId)
            .Select(n => n.NodeId);

        foreach (var id in entryNodes)
        {
            if (!visited.Contains(id))
            {
                visited.Add(id);
                queue.Enqueue(id);
            }
        }

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            orderedIds.Add(currentId);
            
            var outgoing = graph.Connections
                .Where(c => c.OutputNodeId == currentId)
                .Select(c => c.InputNodeId);

            foreach (var nextId in outgoing.OrderBy(id => id))
            {
                if (!visited.Contains(nextId))
                {
                    visited.Add(nextId);
                    queue.Enqueue(nextId);
                }
            }
        }
        
        foreach (var id in allNodeIds.Except(orderedIds).OrderBy(id => id))
        {
            orderedIds.Add(id);
        }

        return orderedIds
            .Where(id => nodeFragments.ContainsKey(id))
            .Select(id => nodeFragments[id])
            .ToList();
    }

    public override List<ConnectionData> Restore(List<ParsedNodeFragment> fragments, List<NodeData> nodes)
    {
        var links = new List<ConnectionData>();
        foreach (var fragment in fragments)
        {
            var node = nodes.FirstOrDefault(n => n.NodeId == fragment.NodeId);
            using var doc = JsonDocument.Parse(fragment.Text);
            var nextId = doc.RootElement.GetProperty("next").ToString();
            links.Add(new ConnectionData(
                node.NodeId,
                string.IsNullOrEmpty(nextId) ? -1 : 0, // 示例场景只有一个端口，因此这里是0，实际场景需要结合语义进行推断，或直接从文本获取
                nextId,
                string.IsNullOrEmpty(nextId) ? -1 : 0
            ));
        }
        return links;
    }
}
#endif