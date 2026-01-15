#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;

namespace TextualGraph.Serialization;

/// <summary>
/// 文本图序列化器注册表，用于管理不同类型的解析器和写入器
/// </summary>
public static class TextGraphSerializerRegistry
{
    private static Dictionary<string, TextParser> _textParsers;
    private static Dictionary<string, ConnectionParser> _nodeSerializers;
    private static Dictionary<string, FragmentWriter> _fragmentWriters;

    /// <summary>
    /// 根据ID获取文本解析器
    /// </summary>
    /// <param name="id">解析器的唯一标识符</param>
    /// <returns>找到的文本解析器，如果不存在则返回null</returns>
    public static TextParser GetTextParser(string id) => _textParsers?.TryGetValue(id, out var parser) ?? false ? parser : null;
    
    /// <summary>
    /// 根据ID获取连接解析器
    /// </summary>
    /// <param name="id">解析器的唯一标识符</param>
    /// <returns>找到的连接解析器，如果不存在则返回null</returns>
    public static ConnectionParser GetConnectionParser(string id) => _nodeSerializers?.TryGetValue(id, out var parser) ?? false ? parser : null;
    
    /// <summary>
    /// 根据ID获取片段写入器
    /// </summary>
    /// <param name="id">写入器的唯一标识符</param>
    /// <returns>找到的片段写入器，如果不存在则返回null</returns>
    public static FragmentWriter GetFragmentWriter(string id) => _fragmentWriters?.TryGetValue(id, out var writer) ?? false ? writer : null;

    /// <summary>
    /// 清理所有已注册的解析器和写入器
    /// </summary>
    public static void Clearup()
    {
        _textParsers = null;
        _nodeSerializers = null;
        _fragmentWriters = null;
    }

    /// <summary>
    /// 扫描当前程序集并注册所有继承自TextParser、ConnectionParser和FragmentWriter的类型
    /// </summary>
    public static void Register()
    {
        _nodeSerializers = [];
        _textParsers = [];
        _fragmentWriters = [];

        var types = typeof(TextGraphSerializerRegistry).Assembly.GetTypes()
                .Where(t => !t.IsAbstract);

        foreach (var type in types)
        {
            if (type.IsSubclassOf(typeof(TextParser)))
            {
                var parser = (TextParser)Activator.CreateInstance(type);
                _textParsers[parser.Id] = parser;             
            }
            else if (type.IsSubclassOf(typeof(ConnectionParser)))
            {
                var serializer = (ConnectionParser)Activator.CreateInstance(type);
                _nodeSerializers[serializer.Id] = serializer;
            }
            else if (type.IsSubclassOf(typeof(FragmentWriter)))
            {
                var serializer = (FragmentWriter)Activator.CreateInstance(type);
                _fragmentWriters[serializer.Id] = serializer;
            }
        }        

    }
}
#endif