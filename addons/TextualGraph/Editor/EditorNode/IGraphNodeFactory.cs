#if TOOLS
using Godot;
using System.Reflection;

namespace TextualGraph.Editor.EditorNode;

/// <summary>
/// 图节点工厂接口，用于定义创建图节点所需的方法和属性
/// </summary>
/// <typeparam name="TSelf">实现此接口的具体节点类型</typeparam>
public interface IGraphNodeFactory<TSelf>
    where TSelf : GraphNode, IGraphNode, IGraphNodeFactory<TSelf>, new()
{
    /// <summary>
    /// 节点的唯一类型名称
    /// </summary>
    static abstract string NodeName { get; }

    /// <summary>
    /// 节点的预制体路径
    /// </summary>
    static abstract string PrefabFilePath { get; }

    /// <summary>
    /// 创建节点
    /// </summary>
    /// <returns>返回创建的节点实例</returns>
    static virtual TSelf Create()
    {
        var node = ResourceLoader.Load<PackedScene>(TSelf.PrefabFilePath).Instantiate<GraphNode>();
        var scriptAttr = typeof(TSelf).GetCustomAttribute<ScriptPathAttribute>();
        var script = ResourceLoader.Load<CSharpScript>(scriptAttr.Path);
        var instanceId = node.GetInstanceId();
        node.SetScript(script);
        return (TSelf)GodotObject.InstanceFromId(instanceId);
    }
}
#endif