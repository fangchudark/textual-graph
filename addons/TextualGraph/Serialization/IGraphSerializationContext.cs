#if TOOLS
using System.Collections.Generic;

namespace TextualGraph.Serialization;

/// <summary>
/// 节点图序列化上下文，提供对图中其他节点的访问
/// </summary>
public interface IGraphSerializationContext
{
    /// <summary>
    /// 获取从指定节点向外连接的所有连接数据。
    /// </summary>
    /// <param name="self">当前节点。</param>
    /// <returns>所有以 <paramref name="self"/> 作为输出端点（起点）的 <see cref="ConnectionData"/> 列表。</returns>
    List<ConnectionData> GetOutgoing(NodeData self);

    /// <summary>
    /// 获取指向指定节点的所有连接数据。
    /// </summary>
    /// <param name="self">当前节点。</param>
    /// <returns>所有以 <paramref name="self"/> 作为输入端点（终点）的 <see cref="ConnectionData"/> 列表。</returns>
    List<ConnectionData> GetIncoming(NodeData self);

    /// <summary>
    /// 获取与指定节点直接相连的所有节点（包括前驱和后继）。
    /// </summary>
    /// <param name="self">当前节点。</param>
    /// <returns>
    /// 所有通过连接与 <paramref name="self"/> 相邻的 <see cref="NodeData"/> 节点列表，
    /// 包含上游（输入来源）和下游（输出目标）节点，且不重复。
    /// </returns>
    List<NodeData> GetConnectedNodes(NodeData self);

    /// <summary>
    /// 获取与指定节点直接相连的所有节点（包括前驱和后继）。
    /// </summary>
    /// <param name="selfId">当前节点Id。</param>
    /// <returns>
    /// 所有通过连接与 <paramref name="selfId"/> 相邻的 <see cref="NodeData"/> 节点列表，
    /// 包含上游（输入来源）和下游（输出目标）节点，且不重复。
    /// </returns>
    List<NodeData> GetConnectedNodes(string selfId);

    /// <summary>
    /// 获取指定节点的 <see cref="NodeData"/>。
    /// </summary>
    /// <param name="nodeId">节点Id。</param>
    /// <returns>指定节点的 <see cref="NodeData"/>。</returns>
    NodeData GetNode(string nodeId);
}
    
#endif