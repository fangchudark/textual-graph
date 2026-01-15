#if TOOLS
using System.Collections.Generic;
using System.Linq;

namespace TextualGraph.Serialization;

/// <summary>
/// 图数据的记录类，包含节点列表和连接列表，并实现图序列化上下文接口
/// </summary>
/// <param name="Nodes">图中所有节点的列表</param>
/// <param name="Connections">图中所有连接的列表</param>
public sealed record GraphData(
    List<NodeData> Nodes,
    List<ConnectionData> Connections
) : IGraphSerializationContext
{
    public List<ConnectionData> GetOutgoing(NodeData self)
        => Connections.Where(c => c.OutputNodeId == self.NodeId).ToList();

    public List<ConnectionData> GetIncoming(NodeData self)
        => Connections.Where(c => c.InputNodeId == self.NodeId).ToList();
    
    public List<NodeData> GetConnectedNodes(NodeData self)
    {
        var ids = GetOutgoing(self).Select(c => c.InputNodeId)
            .Concat(GetIncoming(self).Select(c => c.OutputNodeId))
            .Distinct();

        return Nodes.Where(n => ids.Contains(n.NodeId)).ToList();
    }

    public List<NodeData> GetConnectedNodes(string selfId)
    {
        var self = Nodes.FirstOrDefault(n => n.NodeId == selfId);
        if (self == null)
            return [];

        return GetConnectedNodes(self);
    }

    public NodeData GetNode(string nodeId)
        => Nodes.FirstOrDefault(n => n.NodeId == nodeId);
}

#endif