#if TOOLS
using Godot;

namespace TextualGraph.Editor.EditorNode;

/// <summary>
/// 允许在节点完成反序列化创建后做出反应
/// </summary>
public interface IDeserializeListener
{
    /// <summary>
    /// 反序列化完成后调用
    /// </summary>
    /// <param name="self">该接口类自身的实例</param>
    /// <param name="other">与之建立连接的另一个节点</param>
    /// <param name="selfIsInput">指示自身是否为输入节点，即从<paramref name="other"/>接受连接，如果为false则表示自身节点为输出节点，即向<paramref name="other"/>发起连接</param>
    /// <param name="selfPort">自身连接的节点上的端口号</param>
    /// <param name="otherPort">与之建立连接的节点上的端口号</param>
    void OnAfterDeserialize(GraphNode self, GraphNode other, bool selfIsInput, int selfPort, int otherPort);
}
#endif