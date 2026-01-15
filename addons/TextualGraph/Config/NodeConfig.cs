#if TOOLS
namespace TextualGraph.Config;

/// <summary>
/// 节点配置记录类，定义了对话编辑器中节点的配置信息
/// </summary>
/// <param name="Name">节点的内部唯一类型名称</param>
/// <param name="DisplayName">节点的显示名称</param>
public record NodeConfig(
    string Name,
    string DisplayName
) : PluginConfig;
#endif