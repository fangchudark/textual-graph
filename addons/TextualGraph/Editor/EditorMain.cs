#if TOOLS
using TextualGraph.Editor;
using TextualGraph.Editor.EditorNode;
using TextualGraph.JsonHelper;
using TextualGraph.Serialization;
using Godot;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TextualGraph;

[Tool]
public sealed partial class EditorMain : Control
{
	/// <summary>
    /// 浮窗模式关闭时发出的事件
    /// </summary>
	public event Action OnWindowClosed;

	/// <summary>
    /// 浮窗模式开启时发出的事件
    /// </summary>
	public event Action OnWindowOpened;

	/// <summary>
    /// 获取当前是否处于浮窗模式
    /// </summary>
	public bool IsWindowMode => IsInstanceValid(_editorWindow);

	private Button _importButton;

	private Button _exportButton;

	private Button _asWindowButton;

	[Export]
	private VBoxContainer _editorContainer;

	[Export]
	private GraphEditor _graphEditor;

	private Window _editorWindow;

	public override void _Ready()
	{
		_asWindowButton = new Button()
		{
			Text = "浮动窗口",
			CustomMinimumSize = new Vector2(64, 32),
			SizeFlagsHorizontal = SizeFlags.ShrinkEnd
		};
		_asWindowButton.Pressed += OnAsWindowPressed;

		_exportButton = new Button()
		{
			Text = "导出为文本",
			CustomMinimumSize = new Vector2(64, 32),
			SizeFlagsHorizontal = SizeFlags.ShrinkEnd
		};
		_exportButton.Pressed += OnExportPressed;

		_importButton = new Button()
		{
			Text = "从文本导入",
			CustomMinimumSize = new Vector2(64, 32),
			SizeFlagsHorizontal = SizeFlags.ShrinkEnd
		};
		_importButton.Pressed += OnImportPressed;

		var menu = _graphEditor.GetMenuHBox();
		menu.AddChild(_asWindowButton);
		menu.AddChild(_exportButton);
		menu.AddChild(_importButton);
    }

	public void ChangeEditorVisibility(bool show)
	{
		if (show)
		{
			if (IsInstanceValid(_editorWindow))
			{
				_editorWindow.GrabFocus();
			}
			else
			{
				Show();
			}
		}
		else
		{
			Hide();
		}
	}
	
	private void OnImportPressed()
    {
		var config = ConfigReader.DeserializeSerializeConfig();
		if (string.IsNullOrEmpty(config.TextParser))
		{
			ReportError("未指定导入所使用的文本解析器ID，请配置序列化配置文件");
			return;
		}

		var textParser = TextGraphSerializerRegistry.GetTextParser(config.TextParser);
		if (textParser == null)
		{
			ReportError($"未找到指定的文本解析器：{config.TextParser}");
			return;
		}

		var connectionParser = TextGraphSerializerRegistry.GetConnectionParser(config.ConnectionParser);
		if (connectionParser == null)
		{
			ReportError($"未找到指定的连接信息解析器：{config.ConnectionParser}");
			return;
		}

		var extensions = config.AllowFileExtensions;

		var exportPath = ProjectSettings.GetSetting(
			"TextualGraph/Serialization/ImportPath",
			ProjectSettings.GetSetting(
				"TextualGraph/Serialization/ExportPath",
				"res://"
		)).AsString();
			
		var currentDir = ProjectSettings.GlobalizePath(exportPath.GetBaseDir());
		var fileName = exportPath.GetFile();
		if (string.IsNullOrEmpty(fileName))
        {
            fileName = "graph.txt";
        }
        

		DisplayServer.FileDialogShow(
			"从文本文件导入",
			currentDir,
			fileName,
			false,
			DisplayServer.FileDialogMode.OpenFile,
			extensions,
			Callable.From<bool, string[], int>((status, selectedPaths, selectedFilterIndex) =>
			{
				if (!status) return;

				try
				{
					var serializer = new TextGraphSerializer(
						GraphNodeFactory.NodeSerializers,
						connectionParser,
						textParser,
						null
					);

					var selectedFilter = extensions[selectedFilterIndex];
					var match = FileFilterRegex().Match(selectedFilter);
					var extension = match.Success ? "." + match.Groups["ext"].Value : ".txt";

					var path = ProjectSettings.GlobalizePath(selectedPaths[0]).GetBaseName() + extension;
					
					ProjectSettings.SetSetting("TextualGraph/Serialization/ImportPath", path);

					using var stream = new StreamReader(path);
					var data = serializer.Deserialize(stream);
					_graphEditor.Restore(data);
				}
				catch (Exception e)
				{
					DisplayServer.DialogShow(
						"导入失败",
						$"导入过程中发生了错误：{e}",
						["关闭"],
						default
					);
					return;
				}
			})
		);
    }

	private void OnExportPressed()
	{
		var config = ConfigReader.DeserializeSerializeConfig();
		
		if (string.IsNullOrEmpty(config.ConnectionParser))
		{			
			ReportError("未指定导出所使用的连接信息解析器ID，请配置序列化配置文件");
			return;
		}
		
		var connectionParser = TextGraphSerializerRegistry.GetConnectionParser(config.ConnectionParser);
		if (connectionParser == null)
		{
			ReportError($"未找到指定的连接信息解析器：{config.ConnectionParser}");		
			return;
		}
				
		var fragmentWriter = TextGraphSerializerRegistry.GetFragmentWriter(config.FragmentWriter);
		if (fragmentWriter == null)
		{
			ReportError($"未找到指定的片段写入器：{config.FragmentWriter}");
			return;
		}

		var extensions = ConfigReader.DeserializeExportFileExtensions();
		
		var exportPath = ProjectSettings.GetSetting("TextualGraph/Serialization/ExportPath", "res://").AsString();
		var currentDir = ProjectSettings.GlobalizePath(exportPath.GetBaseDir());
		var fileName = exportPath.GetFile();
		if (string.IsNullOrEmpty(fileName))
        {
            fileName = "graph.txt";
        }
        

		DisplayServer.FileDialogShow(
			"导出为文本文件",
			currentDir,
			fileName,
			false,
			DisplayServer.FileDialogMode.SaveFile,
			extensions,
			Callable.From<bool, string[], int>((status, selectedPaths, selectedFilterIndex) =>
			{
				if (!status) return;

				try
				{
					var serializer = new TextGraphSerializer(
						GraphNodeFactory.NodeSerializers,
						connectionParser,
						null,
						fragmentWriter
					);

					var selectedFilter = extensions[selectedFilterIndex];
					var match = FileFilterRegex().Match(selectedFilter);
					var extension = match.Success ? "." + match.Groups["ext"].Value : ".txt";

					var path = ProjectSettings.GlobalizePath(selectedPaths[0]).GetBaseName() + extension;
					
					ProjectSettings.SetSetting("TextualGraph/Serialization/ExportPath", path);

					using var stream = new StreamWriter(path);
					serializer.Serialize(stream, _graphEditor.GetGraphData());
				}
				catch (Exception e)
				{
					DisplayServer.DialogShow(
						"导出失败",
						$"导出过程中发生了错误：{e}",
						["关闭"],
						default
					);
					return;
				}
			})
		);
    }

	private void ReportError(string message)
	{
		DisplayServer.DialogShow(
			"导出失败",
			message,
			["关闭"],
			Callable.From<int>(index =>
			{
				OS.ShellOpen(ProjectSettings.GlobalizePath(ConfigReader.SerializationConfigPath));
			})
		);
	}

	private void OnAsWindowPressed()
	{
		_editorWindow = new Window()
		{
			Title = "节点图编辑器",
			InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen,
			Size = new Vector2I(1600, 900),
		};
		_editorWindow.CloseRequested += OnWindowClose;
		// reparent会触发子节点的ExitTree和EnterTree
		_editorContainer.Reparent(_editorWindow, false);
		_asWindowButton.Visible = false;
		AddChild(_editorWindow);
		OnWindowOpened?.Invoke();
	}
	
	private void OnWindowClose()
	{
		_editorContainer.Reparent(this, false);
		_editorWindow.QueueFree();
		_editorWindow = null;
		_asWindowButton.Visible = true;
		OnWindowClosed?.Invoke();
    }

    [GeneratedRegex(@"^\*\.(?<ext>\w+)")]
    private static partial Regex FileFilterRegex();

}
#endif
