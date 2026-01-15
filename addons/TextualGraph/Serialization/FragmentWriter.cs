#if TOOLS
using System.IO;

namespace TextualGraph.Serialization;

/// <summary>
/// 片段写入器的抽象基类，用于将序列化的节点片段写入文本输出流
/// </summary>
public abstract class FragmentWriter
{
    /// <summary>
    /// 获取片段写入器的唯一标识符
    /// </summary>
    public abstract string Id { get; }
    
    /// <summary>
    /// 开始写入操作，在写入第一个片段之前调用
    /// </summary>
    /// <param name="writer">目标文本写入器</param>
    public abstract void Begin(TextWriter writer);
    
    /// <summary>
    /// 写入单个片段到文本写入器
    /// </summary>
    /// <param name="writer">目标文本写入器</param>
    /// <param name="fragment">要写入的片段文本</param>
    /// <param name="isLast">指示是否为最后一个片段的布尔值</param>
    public abstract void WriteFragment(TextWriter writer, string fragment, bool isLast);
    
    /// <summary>
    /// 结束写入操作，在写入所有片段后调用
    /// </summary>
    /// <param name="writer">目标文本写入器</param>
    public abstract void End(TextWriter writer);
}
#endif