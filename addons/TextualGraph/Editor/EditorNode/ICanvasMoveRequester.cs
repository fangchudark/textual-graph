#if TOOLS
using Godot;

namespace TextualGraph.Editor.EditorNode;

/// <summary>
/// 请求画布聚焦
/// </summary>
/// <param name="target">需求聚焦的节点</param>
public delegate void CanvasMoveRequestedEventHandler(GraphNode target);

/// <summary>
/// 允许节点请求画布聚焦
/// </summary>
public interface ICanvasMoveRequester
{
    /// <summary>
    /// 请求画布聚焦
    /// </summary>
    event CanvasMoveRequestedEventHandler CanvasMoveRequested;
}
#endif