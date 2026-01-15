#if TOOLS
using System.Collections.Generic;
using System.IO;

namespace TextualGraph.Serialization;

/// <summary>
/// 文本解析器的抽象基类，用于解析序列化的文本内容为节点片段
/// </summary>
public abstract class TextParser
{
    /// <summary>
    /// 获取解析器的唯一标识符
    /// </summary>
    public abstract string Id { get; }
    
    /// <summary>
    /// 从文本读取器中提取语义，解析节点片段列表
    /// </summary>
    /// <param name="reader">包含序列化内容的文本读取器</param>
    /// <returns>解析出的节点片段列表</returns>
    public abstract List<ParsedNodeFragment> Parse(TextReader reader);
}
#endif