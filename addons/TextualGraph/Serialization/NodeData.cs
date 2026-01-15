#if TOOLS
using System.Collections.Generic;
using TextualGraph.Editor.EditorNode;
using Godot;

namespace TextualGraph.Serialization;

/// <summary>
/// 节点数据的记录类，包含节点的所有必要信息
/// </summary>
/// <param name="NodeId">节点的唯一标识符</param>
/// <param name="NodeType">节点的类型， 对应<see cref="IGraphNodeFactory{T}.NodeName"/> 和 <see cref="IGraphNode.NodeType"/></param>
/// <param name="Position">节点在编辑器中的位置（可选）</param>
/// <param name="CustomData">节点的自定义数据字典</param>
public sealed record NodeData(
    string NodeId,
    string NodeType,
    Vector2? Position,
    Dictionary<string, object> CustomData
);
#endif