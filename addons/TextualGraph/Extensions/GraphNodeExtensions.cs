using System.Collections.Generic;
using Godot;

namespace TextualGraph.Extension;

/// <summary>
/// 提供对<see cref="GraphNode"/>的拓展方法
/// </summary>
public static class GraphNodeExtensions
{
    /// <summary>
    /// 枚举所有已启用的输入端口与对应的槽位索引
    /// </summary>
    /// <param name="node">要获取的实例</param>
    /// <returns>包含已启用的输入端口索引和对应的槽位索引的枚举器</returns>
    public static IEnumerable<(int enabledIndex, int slot)> EnumerateEnabledInputSlots(this GraphNode node)
    {
        for (int i = 0; i < node.GetInputPortCount(); i++)
            yield return (i, node.GetInputPortSlot(i));
    }

    /// <summary>
    /// 枚举所有已启用的输出端口与对应的槽位索引
    /// </summary>
    /// <param name="node">要获取的实例</param>
    /// <returns>包含已启用的输出端口索引和对应的槽位索引的枚举器</returns>
    public static IEnumerable<(int enabledIndex, int slot)> EnumerateEnabledOutputSlots(this GraphNode node)
    {
        for (int i = 0; i < node.GetOutputPortCount(); i++)
            yield return (i, node.GetOutputPortSlot(i));
    }

    /// <summary>
    /// 获取指定slot的输入端口，如果没有找到返回null
    /// </summary>
    /// <param name="node">要获取的实例</param>
    /// <param name="slot">要查找的槽位索引</param>
    /// <returns>对应的输入端口，没有找到返回null</returns>
    public static int? GetInputPortBySlot(this GraphNode node, int slot)
    {
        for (int i = 0; i < node.GetInputPortCount(); i++)
            if (node.GetInputPortSlot(i) == slot)
                return i;
        return null;
    }

    /// <summary>
    /// 获取指定slot的输出端口，如果没有找到返回null
    /// </summary>
    /// <param name="node">要获取的实例</param>
    /// <param name="slot">要查找的槽位索引</param>
    /// <returns>对应的输出端口，没有找到返回null</returns>
    public static int? GetOutputPortBySlot(this GraphNode node, int slot)
    {
        for (int i = 0; i < node.GetOutputPortCount(); i++)
            if (node.GetOutputPortSlot(i) == slot)
                return i;
        return null;
    }
}