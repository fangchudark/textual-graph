#if TOOLS
using Godot;
using System;

namespace TextualGraph.Editor;

/// <summary>
/// 右键节点动作枚举
/// </summary>
public enum NodeRightAction
{
	/// <summary>
    /// 删除节点
    /// </summary>
	Delete
}

[Tool]
public partial class NodeMouseRightWindow : PopupPanel
{
	/// <summary>
    /// 动作按钮按下事件
    /// </summary>
	/// <param name="action">动作</param>
	[Signal]
	public delegate void ActionButtonPressedEventHandler(NodeRightAction action);

	[Export]
	private Button _delButton;

	public override void _Ready()
	{
		_delButton.Pressed += OnDelButtonPressed;
	}

	private void OnDelButtonPressed()
	{
		EmitSignalActionButtonPressed(NodeRightAction.Delete);
		Hide();
	}
}	
#endif