#if TOOLS
using Godot;

namespace TextualGraph.Editor.EditorNode;

/// <summary>
/// 允许在节点连接时做出反应
/// </summary>
public interface IConnectionListener
{
    /// <summary>
    /// 当两个节点连接时调用此方法（用户操作建立连接）
    /// </summary>
    /// <param name="self">该接口类自身的实例</param>
    /// <param name="other">与之建立连接的另一个节点</param>
    /// <param name="selfIsInput">指示自身是否为输入节点，即从<paramref name="other"/>接受连接，如果为false则表示自身节点为输出节点，即向<paramref name="other"/>发起连接</param>
    /// <param name="selfPort">自身连接的节点上的端口号</param>
    /// <param name="otherPort">与之建立连接的节点上的端口号</param>
    void OnNodeConnected(GraphNode self, GraphNode other, bool selfIsInput, int selfPort, int otherPort);

    /// <summary>
    /// 当两个节点断开连接时调用此方法（用户操作断开、超过连接上限断开）
    /// </summary>
    /// <param name="self">该接口类自身的实例</param>
    /// <param name="other">与之断开连接的另一个节点</param>
    /// <param name="selfIsInput">指示自身是否为输入节点，即断开前是从<paramref name="other"/>接受连接，如果为false则表示自身节点为输出节点，即断开前是自身向<paramref name="other"/>发起的连接</param>
    /// <param name="selfPort">自身的节点上的端口号</param>
    /// <param name="otherPort">与之断开连接的节点上的端口号</param>
    void OnNodeDisconnected(GraphNode self, GraphNode other, bool selfIsInput, int selfPort, int otherPort);
}

#endif