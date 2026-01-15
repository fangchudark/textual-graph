#if TOOLS
namespace TextualGraph.Config;

/// <summary>
/// 序列化配置记录类，定义了对话编辑器的序列化相关设置
/// </summary>
/// <param name="TextParser">文本解析器的标识符</param>
/// <param name="ConnectionParser">连接解析器的标识符</param>
/// <param name="FragmentWriter">片段写入器的标识符</param>
/// <param name="AllowFileExtensions">允许的文件扩展名数组</param>
public record SerializationConfig(
    string TextParser,
    string ConnectionParser,
    string FragmentWriter,
    string[] AllowFileExtensions
) : PluginConfig;
#endif