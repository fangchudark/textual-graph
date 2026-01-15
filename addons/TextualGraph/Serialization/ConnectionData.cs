#if TOOLS
namespace TextualGraph.Serialization;

/// <summary>
/// 连接数据的记录类，表示两个节点之间的连接关系
/// </summary>
/// <param name="OutputNodeId">输出节点的唯一标识符</param>
/// <param name="OutputPort">输出节点的端口索引</param>
/// <param name="InputNodeId">输入节点的唯一标识符</param>
/// <param name="InputPort">输入节点的端口索引</param>
public sealed record ConnectionData(
    string OutputNodeId,
    int OutputPort,
    string InputNodeId,
    int InputPort
);
#endif