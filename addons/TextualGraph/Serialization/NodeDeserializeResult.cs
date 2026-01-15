#if TOOLS
using System.Collections.Generic;
using Godot;

namespace TextualGraph.Serialization;

/// <summary>
/// 节点反序列化结果的记录类，包含自定义数据和位置提示
/// </summary>
/// <param name="CustomData">节点的自定义数据字典</param>
/// <param name="PositionHint">节点的可选位置提示</param>
public sealed record NodeDeserializeResult(
    Dictionary<string, object> CustomData,
    Vector2? PositionHint
);
    
#endif