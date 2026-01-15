#if TOOLS
using System.IO;
using Godot;
using System.Text.Json;
using TextualGraph.Config;
using System.Collections.Generic;

namespace TextualGraph.JsonHelper;

/// <summary>
/// 配置读取器，提供静态方法来反序列化各种配置文件
/// </summary>
public static class ConfigReader
{
    /// <summary>
    /// 节点配置文件路径
    /// </summary>
    public const string NodeConfigPath = "res://addons/TextualGraph/Config/nodes.json";
    
    /// <summary>
    /// 序列化配置文件路径
    /// </summary>
    public const string SerializationConfigPath = "res://addons/TextualGraph/Config/serialization.json";
    
    /// <summary>
    /// 导出文件扩展名配置文件路径
    /// </summary>
    public const string ExportFileExtensionsConfigPath = "res://addons/TextualGraph/Config/export_file_extensions.json";


    /// <summary>
    /// 反序列化节点配置文件
    /// </summary>
    /// <returns>节点配置列表，如果文件不存在则返回空列表</returns>
    public static List<NodeConfig> DeserializeNodeConfig()
    {
        var globalPath = ProjectSettings.GlobalizePath(NodeConfigPath);
        if (!File.Exists(globalPath))
        {
            return [];
        }

        using var stream = File.OpenRead(globalPath);
        return JsonSerializer.Deserialize(stream, PluginConfigJsonContext.Default.ListNodeConfig);
    }

    /// <summary>
    /// 反序列化序列化配置文件
    /// </summary>
    /// <returns>序列化配置对象，如果文件不存在则返回默认配置</returns>
    public static SerializationConfig DeserializeSerializeConfig()
    {
        var globalPath = ProjectSettings.GlobalizePath(SerializationConfigPath);
        if (!File.Exists(globalPath))
        {
            return new SerializationConfig("", "", "", []);
        }

        using var stream = File.OpenRead(globalPath);
        return JsonSerializer.Deserialize(stream, PluginConfigJsonContext.Default.SerializationConfig);
    }
    
    /// <summary>
    /// 反序列化导出文件扩展名配置文件
    /// </summary>
    /// <returns>文件扩展名数组，如果文件不存在则返回空数组</returns>
    public static string[] DeserializeExportFileExtensions()
    {
        var globalPath = ProjectSettings.GlobalizePath(ExportFileExtensionsConfigPath);
        if (!File.Exists(globalPath))
        {
            return [];
        }

        using var stream = File.OpenRead(globalPath);
        return JsonSerializer.Deserialize(stream, PluginConfigJsonContext.Default.StringArray);
    }
}
#endif