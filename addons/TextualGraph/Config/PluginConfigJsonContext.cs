#if TOOLS
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TextualGraph.Config;

/// <summary>
/// 插件配置的JSON序列化上下文，定义了序列化选项和可序列化类型
/// </summary>
[JsonSerializable(typeof(NodeConfig))]
[JsonSerializable(typeof(List<NodeConfig>))]
[JsonSerializable(typeof(SerializationConfig))]
[JsonSerializable(typeof(string[]))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
)]
public sealed partial class PluginConfigJsonContext : JsonSerializerContext { }

/// <summary>
/// 插件配置的抽象基类，作为所有插件配置类型的父类
/// </summary>
public abstract record PluginConfig;
#endif