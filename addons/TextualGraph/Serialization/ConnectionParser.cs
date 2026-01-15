#if TOOLS
using System.Collections.Generic;

namespace TextualGraph.Serialization;

/// <summary>
/// 连接解析器的抽象基类，用于处理图中节点之间的连接关系
/// </summary>
public abstract class ConnectionParser
{
    /// <summary>
    /// 获取连接解析器的唯一标识符
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// 对节点片段进行排序，确定它们在序列化输出中的顺序
    /// </summary>
    /// <param name="graph">包含节点和连接关系的图数据</param>
    /// <param name="nodeFragments">节点ID到其序列化片段的映射字典</param>
    /// <returns>按正确顺序排列的节点片段列表</returns>
    public abstract List<string> Order(
        GraphData graph,
        Dictionary<string, string> nodeFragments
    );

    /// <summary>
    /// 从解析的节点片段和节点数据恢复连接关系
    /// </summary>
    /// <param name="fragments">从文本解析器提取语义，解析后的节点片段列表</param>
    /// <param name="nodes">节点数据列表</param>
    /// <returns>恢复的连接数据列表</returns>
    public abstract List<ConnectionData> Restore(
        List<ParsedNodeFragment> fragments,
        List<NodeData> nodes
    );
}
    
#endif