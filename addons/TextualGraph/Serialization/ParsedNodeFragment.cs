#if TOOLS
using TextualGraph.Editor.EditorNode;

namespace TextualGraph.Serialization;

/// <summary>
/// 解析节点片段的记录类，包含节点的基本信息
/// </summary>
/// <param name="NodeId">节点的唯一标识符</param>
/// <param name="NodeType">节点的类型, 对应<see cref="IGraphNodeFactory{T}.NodeName"/> 和 <see cref="IGraphNode.NodeType"/></param>
/// <param name="Text">节点的序列化文本内容</param>
public sealed record ParsedNodeFragment(
    string NodeId,
    string NodeType,
    string Text
);

#endif