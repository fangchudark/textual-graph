#if TOOLS
using TextualGraph.Editor.EditorNode;
using TextualGraph.Serialization;
using Godot;
using System;

namespace TextualGraph;

[Tool]
public sealed partial class PluginMain : EditorPlugin, ISerializationListener
{
  	private PackedScene _editorMain = ResourceLoader.Load<PackedScene>(
		"res://addons/TextualGraph/Prefab/Editor/EditorMain.tscn",
		cacheMode: ResourceLoader.CacheMode.Ignore);

	private EditorMain _editorMainInstance;

	public override void _EnterTree()
	{
		// 注册节点
		GraphNodeFactory.UpdateRegistration(true);
		TextGraphSerializerRegistry.Register();


		_editorMainInstance = _editorMain.Instantiate<EditorMain>();
		_editorMainInstance.OnWindowClosed += OnEditorMainWindowCloned;
		_editorMainInstance.OnWindowOpened += OnEditorWindowClosed;
		_editorMainInstance.Visible = false;
		EditorInterface.Singleton.GetEditorMainScreen().AddChild(_editorMainInstance);
	}

	public override void _ExitTree()
	{
		_editorMainInstance.OnWindowClosed -= OnEditorMainWindowCloned;
		_editorMainInstance.OnWindowOpened -= OnEditorWindowClosed;
		_editorMainInstance.QueueFree();
		_editorMainInstance = null;

		// 清理注册信息
		GraphNodeFactory.Clearup();
		TextGraphSerializerRegistry.Clearup();
	}

	public override bool _HasMainScreen() => true;

	public override string _GetPluginName() => "节点图编辑器";

	public override void _MakeVisible(bool visible)
	{
		_editorMainInstance.ChangeEditorVisibility(visible);
		if (_editorMainInstance.IsWindowMode && visible)
			EditorInterface.Singleton.SetMainScreenEditor("2D");
	}

	private void OnEditorMainWindowCloned()
	{
		EditorInterface.Singleton.SetMainScreenEditor(_GetPluginName());
	}
	
	private void OnEditorWindowClosed()
    {
        EditorInterface.Singleton.SetMainScreenEditor("2D");
    }

    public void OnBeforeSerialize()
	{
		// 卸载程序集时清理所有注册信息
		GraphNodeFactory.Clearup();
		TextGraphSerializerRegistry.Clearup();
    }

    public void OnAfterDeserialize()
	{
		// 重新加载程序集时重新扫描并注册所有节点
		GraphNodeFactory.UpdateRegistration(true);
		TextGraphSerializerRegistry.Register();
    }
}
#endif
