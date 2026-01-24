#if TOOLS
using System.Collections.Generic;
using System.Linq;
using TextualGraph.Editor.EditorNode;
using TextualGraph.JsonHelper;
using Godot;

namespace TextualGraph.Editor;

[Tool]
public sealed partial class NodeSelectWindow : PopupPanel, ISerializationListener
{
    /// <summary>
    /// 选择节点时发出的事件，参数为节点的类型，对应<see cref="IGraphNodeFactory{T}.NodeName"/> 和 <see cref="IGraphNode.NodeType"/>
    /// </summary>
    /// <param name="nodeType">节点的类型，对应<see cref="IGraphNodeFactory{T}.NodeName"/> 和 <see cref="IGraphNode.NodeType"/></param>
    [Signal]
    public delegate void OnNodeSelectedEventHandler(string nodeType);

    /// <summary>
    /// 获取是否为一次性弹窗，如果true则创建一次节点后关闭
    /// </summary>
    public bool IsOneshot => _oneshot;
    
    [Export]
    private LineEdit _searchEdit;
    [Export]
    private VBoxContainer _buttonContainer;
    private bool _oneshot;
    private List<Button> _buttons = [];
    private HashSet<Button> _availableButtons = [];

    private bool _dirty = true;

    public override void _EnterTree()
    {
        if (_dirty)
        {
            RemoveButtons();
            CreateButtons();
            _dirty = false;
        }
        else
        {
            foreach (var b in _buttonContainer.GetChildren().Cast<Button>())
                _buttons.Add(b);
        }
    }
    

    public override void _Ready()
    {
        _searchEdit.TextChanged += OnSearchTextChanged;
    }

    public override void _ExitTree()
    {
        _buttons.Clear();
        _availableButtons.Clear();
    }

    /// <summary>
    /// 显示选择菜单
    /// </summary>
    /// <param name="availableButtons">可供选择的节点类型，为null则所有类型可选</param>
    /// <param name="oneshot">是否在选择节点后即刻关闭弹窗</param>
    public void ShowSelectMenu(HashSet<string> availableButtons = null, bool oneshot = false)
    {
        _oneshot = oneshot;
        if (_dirty)
        {
            RemoveButtons();
            CreateButtons();
            _dirty = false;
        }

        if (availableButtons != null)
        {
            _availableButtons = _buttons.Where(b => availableButtons.Contains(b.Name)).ToHashSet();
            foreach (var b in _buttons)
                b.Visible = _availableButtons.Contains(b);
        }
        else
        {
            _availableButtons = _buttons.ToHashSet();
            _buttons.ForEach(b => b.Visible = true);
        }

        Popup();
    }

    private void CreateButtons()
    {
        var config = ConfigReader.DeserializeNodeConfig();
        var valid = GraphNodeFactory.GetNodeNames();
        foreach (var nodeConfig in config)
        {
            if (!valid.Contains(nodeConfig.Name))
                continue;

            var button = new Button()
            {
                CustomMinimumSize = new Vector2(0, 32),
                Text = nodeConfig.DisplayName,
                Name = nodeConfig.Name
            };
            _buttonContainer.AddChild(button);
            _buttons.Add(button);
            button.Pressed += () =>
            {
                if (_oneshot)
                {
                    Hide();
                    _oneshot = false;
                }
                EmitSignalOnNodeSelected(button.Name);
            };
        }
    }

    private void OnSearchTextChanged(string text)
    {        
        foreach (var child in _availableButtons)
        {
            child.Visible = ((string)child.Name).Contains(text) || child.Text.Contains(text);
        }
    }

    private void RemoveButtons()
    {
        foreach (var child in _buttonContainer.GetChildren())
        {
            _buttonContainer.RemoveChild(child);
            child.QueueFree();
        }
        _buttons.Clear();
        _availableButtons.Clear();
    }

    public void OnBeforeSerialize()
    {
        if (!EditorInterface.Singleton.IsPluginEnabled("TextualGraph"))
            return;

        // 由于不知道为什么插件启用时，每次重载程序集前，都会创建两个幽灵节点
        // 因此这里当自己不在树时移除
        if (IsInstanceValid(this) && !IsInsideTree())
        {
            GD.PushWarning("[NodeSelectWindow] The current instance is not in the tree, and the instance has been released.");
            Free();
            return;
        }
    }

    public void OnAfterDeserialize()
    {
        if (!EditorInterface.Singleton.IsPluginEnabled("TextualGraph"))
            return;

        // 标记为脏，下次打开窗口时更新按钮
        _dirty = true;
    }
}
#endif