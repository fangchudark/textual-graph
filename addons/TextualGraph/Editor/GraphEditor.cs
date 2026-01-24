#if TOOLS
using System.Collections.Generic;
using System.Linq;
using TextualGraph.Editor.EditorNode;
using TextualGraph.Serialization;
using Godot;

namespace TextualGraph.Editor;

[Tool]
public sealed partial class GraphEditor : GraphEdit, ISerializationListener
{
	// from = output, to = input

	[Export]
	private NodeSelectWindow _nodeSelectWindow;

	[Export]
	private NodeMouseRightWindow _nodeMouseRightWindow;

	[Export]
	private Vector2 _clickPosition;

	[Export]
	private GraphNode _lastSelectedNode;

	[Export]
	private Godot.Collections.Array<GraphNode> _selectedNodes = [];

	[Export]
	private Godot.Collections.Array<GraphNode> _nodes = [];

	[Export]
	private StringName _pendingNodeName;

	[Export]
	private int _pendingPort;

	[Export]
	private bool _pendingIsFrom; // true = from empty，false = to empty


	public void ClearState()
    {
		_pendingNodeName = null;
		_pendingPort = -1;
		_nodes.Clear();
		_selectedNodes.Clear();
		_lastSelectedNode = null;
		_clickPosition = Vector2.Zero;
    }

	public override void _EnterTree()
	{
		_nodeSelectWindow.OnNodeSelected += OnNodeSelected;
		_nodeMouseRightWindow.ActionButtonPressed += OnRightActionPressed;
	}

    public override void _Ready()
    {
		PopupRequest += OnPopupRequest;
        NodeSelected += OnNodeSelected;
		NodeDeselected += OnNodeDeselected;
		ConnectionDragStarted += OnConnectionDragStarted;
		ConnectionFromEmpty += OnConnectionFromEmpty;
		ConnectionToEmpty += OnConnectionToEmpty;
		DeleteNodesRequest += OnDeleteNodesRequest;
		ConnectionRequest += OnConnectionRequest;
		DisconnectionRequest += OnDisconnectionRequest;
		_nodeSelectWindow.PopupHide += OnPopupHide;
    }

    public override void _ExitTree()
    {
		_nodeSelectWindow.OnNodeSelected -= OnNodeSelected;
		_nodeMouseRightWindow.ActionButtonPressed -= OnRightActionPressed;
    }


	public override bool _IsNodeHoverValid(StringName outputNode, int outputPort, StringName toNode, int toPort)
	{
		// 禁止连接自身
		return outputNode != toNode;
	}

	private void OnConnectionDragStarted(StringName nodeName, long port, bool isOutput)
	{
		var node = _nodes.FirstOrDefault(n => n.Name == nodeName);
		if (node is not IGraphNode graphNode)
			return;

		if (isOutput)
		{
			int max = graphNode.GetMaxOutputConnections((int)port);
			if (max >= 0)
				EnsureOutputCapacity(nodeName, (int)port, max);
		}
		else
		{
			int max = graphNode.GetMaxInputConnections((int)port);
			if (max >= 0)
				EnsureInputCapacity(nodeName, (int)port, max);
		}
	}

	private void EnsureOutputCapacity(StringName outputNodeName, int outputPort, int maxConnections)
	{
		var connections = GetConnectionList()
			.Where(dict => dict["from_node"].AsStringName() == outputNodeName &&
						dict["from_port"].AsInt32() == outputPort)
			.ToList();

		if (connections.Count >= maxConnections)
		{
			var last = connections.Last();
			Disconnect(
				last["from_node"].AsStringName(),
				last["from_port"].AsInt32(),
				last["to_node"].AsStringName(),
				last["to_port"].AsInt32()
			);
		}
	}

	private void EnsureInputCapacity(StringName inputNodeName, int inputPort, int maxConnections)
	{
		var connections = GetConnectionList()
			.Where(dict => dict["to_node"].AsStringName() == inputNodeName &&
						dict["to_port"].AsInt32() == inputPort)
			.ToList();

		if (connections.Count >= maxConnections)
		{
			var last = connections.Last();
			Disconnect(
				last["from_node"].AsStringName(),
				last["from_port"].AsInt32(),
				last["to_node"].AsStringName(),
				last["to_port"].AsInt32()
			);
		}
	}

	

	private void OnDisconnectionRequest(StringName outputNodeName, long outputPort, StringName inputNodeName, long inputPort)
    {
        Disconnect(outputNodeName, outputPort, inputNodeName, inputPort);
    }

    private void Disconnect(StringName outputNodeName, long outputPort, StringName inputNodeName, long inputPort)
    {
        DisconnectNode(outputNodeName, (int)outputPort, inputNodeName, (int)inputPort);
        var output = _nodes.FirstOrDefault(n => n.Name == outputNodeName);
        var input = _nodes.FirstOrDefault(n => n.Name == inputNodeName);
        if (output is IConnectionListener outputListener)
        {
            outputListener.OnNodeDisconnected(output, input, false, (int)outputPort, (int)inputPort);
        }
        if (input is IConnectionListener inputListener)
        {
            inputListener.OnNodeDisconnected(input, output, true, (int)inputPort, (int)outputPort);
        }
    }

    private void OnConnectionRequest(StringName outputNodeName, long outputPort, StringName inputNodeName, long inputPort)
	{
		var outputNode = _nodes.FirstOrDefault(n => n.Name == outputNodeName);
		var inputNode = _nodes.FirstOrDefault(n => n.Name == inputNodeName);

		var outputGraph = outputNode as IGraphNode;
		var inputGraph = inputNode as IGraphNode;

		// 语义校验
		if (!outputGraph.CanConnectWhenIsOutput(inputNode, (int)inputPort, out var acceptOutputPort, (int)outputPort))
			return;

		if (acceptOutputPort != outputPort)
			return;
		

		if (!inputGraph.CanConnectWhenIsInput(outputNode, (int)outputPort, out var acceptInputPort, (int)inputPort))
			return;

		if (acceptInputPort != inputPort)
			return;
	

		// 输出端容量检查
		var outputMax = outputGraph.GetMaxOutputConnections((int)outputPort);
		if (outputMax >= 0)
			EnsureOutputCapacity(outputNodeName, (int)outputPort, outputMax);
	

        // 输入端容量检查
		var inputMax = inputGraph.GetMaxInputConnections((int)inputPort);
		if (inputMax >= 0)
			EnsureInputCapacity(inputNodeName, (int)inputPort, inputMax);


		var error = ConnectNode(outputNodeName, (int)outputPort, inputNodeName, (int)inputPort);
		if (error == Error.Ok)
		{
			if (outputNode is IConnectionListener outputListener)
			{
				outputListener.OnNodeConnected(outputNode, inputNode, false, (int)outputPort, (int)inputPort);
			}
			
			if (inputNode is IConnectionListener inputListener)
            {
                inputListener.OnNodeConnected(inputNode, outputNode, true, (int)inputPort, (int)outputPort);
            }
        }
	}


	private void OnPopupHide()
	{
		if (!_nodeSelectWindow.IsOneshot)
			_pendingIsFrom = false;
	}
	
	private List<(StringName neighborNodeName, int selfPort, int neighborPort, bool isNeighborInput)> GetNodeNeighbors(StringName nodeName)
	{
		var neighbors = new List<(StringName, int, int, bool)>();

		var allConnections = GetConnectionList();

		// 查找作为输出节点的连接（邻居是输入节点）
		var outputConnections = allConnections
			.Where(dict => dict["from_node"].AsStringName() == nodeName)
			.ToList();

		foreach (var connection in outputConnections)
		{
			var neighborNodeName = connection["to_node"].AsStringName();
			var selfPort = connection["from_port"].AsInt32();
			var neighborPort = connection["to_port"].AsInt32();

			neighbors.Add((neighborNodeName, selfPort, neighborPort, true)); // 邻居是输入节点
		}

		// 查找作为输入节点的连接（邻居是输出节点）
		var inputConnections = allConnections
			.Where(dict => dict["to_node"].AsStringName() == nodeName)
			.ToList();

		foreach (var connection in inputConnections)
		{
			var neighborNodeName = connection["from_node"].AsStringName();
			var selfPort = connection["to_port"].AsInt32();
			var neighborPort = connection["from_port"].AsInt32();

			neighbors.Add((neighborNodeName, selfPort, neighborPort, false)); // 邻居是输出节点
		}

		return neighbors;
	}
	
	private void OnDeleteNodesRequest(Godot.Collections.Array<StringName> nodeNames)
	{		
		var copy = _nodes.ToList();
		foreach (var n in copy)
		{
			if (!nodeNames.Contains(n.Name))
			{
				continue;
			}
			var neighbors = GetNodeNeighbors(n.Name);

			// 遍历所有邻居，如果实现了 INodeLifecycleListener 接口，则调用回调
			foreach (var (neighborNodeName, selfPort, neighborPort, isNeighborInput) in neighbors)
			{
				var neighborNode = _nodes.FirstOrDefault(n => n.Name == neighborNodeName);
				if (neighborNode is INodeLifecycleListener lifecycleListener)
				{
					// 调用邻居节点的生命周期回调，告知它连接的节点正在被删除
					// 对于邻居节点来说，如果邻居是输入节点(isNeighborInput=true)，则当前节点是输出节点
					// 如果邻居是输出节点(isNeighborInput=false)，则当前节点是输入节点
					lifecycleListener.OnConnectedNodeDeleting(neighborNode, n, isNeighborInput, neighborPort, selfPort);
				}
			}
			RemoveChild(n);
			n.QueueFree();
            _selectedNodes.Remove(n);
            _nodes.Remove(n);
		}
		
		_lastSelectedNode = null;
	}

	private void OnConnectionFromEmpty(StringName inputNodeName, long inputPort, Vector2 releasePos)
	{
		_pendingIsFrom = true;
		_pendingNodeName = inputNodeName;
		_pendingPort = (int)inputPort;

		var inputNode = _nodes.FirstOrDefault(n => n.Name == inputNodeName);
		_clickPosition = releasePos;
		ShowNodeSelectWindow(
			releasePos,
			((IGraphNode)inputNode).GetValidFromNodeNamesForPort((int)inputPort),
			oneshot: true
		);
	}
	private void OnConnectionToEmpty(StringName outputNodeName, long outputPort, Vector2 releasePos)
    {
		_pendingIsFrom = false;
		_pendingNodeName = outputNodeName;
		_pendingPort = (int)outputPort;

		var outputNode = _nodes.FirstOrDefault(n => n.Name == outputNodeName);
		_clickPosition = releasePos;
		ShowNodeSelectWindow(
			releasePos,
			((IGraphNode)outputNode).GetValidToNodeNamesForPort((int)outputPort),
			oneshot: true
		);
    }
	private void OnRightActionPressed(NodeRightAction action)
	{
		switch (action)
		{
			case NodeRightAction.Delete:
				foreach (var node in _selectedNodes.ToList())
				{
					if (!IsInstanceValid(node))
						continue;

					// 获取当前节点的所有邻居
					var neighbors = GetNodeNeighbors(node.Name);
					
					// 遍历所有邻居，如果实现了 INodeLifecycleListener 接口，则调用回调
					foreach (var (neighborNodeName, selfPort, neighborPort, isNeighborInput) in neighbors)
					{
						var neighborNode = _nodes.FirstOrDefault(n => n.Name == neighborNodeName);
						if (neighborNode is INodeLifecycleListener lifecycleListener)
						{
							// 调用邻居节点的生命周期回调，告知它连接的节点正在被删除
							// 对于邻居节点来说，如果邻居是输入节点(isNeighborInput=true)，则当前节点是输出节点
							// 如果邻居是输出节点(isNeighborInput=false)，则当前节点是输入节点
							lifecycleListener.OnConnectedNodeDeleting(neighborNode, node, isNeighborInput, neighborPort, selfPort);
						}
					}

					RemoveChild(node);
					_selectedNodes.Remove(node);				
					_nodes.Remove(node);

					node.QueueFree();
				}

				_lastSelectedNode = null;
				break;
		}
	}

	private void OnNodeSelected(Node node)
	{
		_lastSelectedNode = (GraphNode)node;
		if (!_selectedNodes.Contains(_lastSelectedNode))
			_selectedNodes.Add(_lastSelectedNode);
	}

	private void OnNodeDeselected(Node node)
	{
		_selectedNodes.Remove((GraphNode)node);
		_lastSelectedNode = _selectedNodes.LastOrDefault();
	}

	private void OnNodeSelected(string nodeName)
	{
		var node = GraphNodeFactory.Create(nodeName);
		if (node == null)
			return;

		Vector2 canvasPos = (_clickPosition + ScrollOffset) / Zoom;
		// 从输入端拖到空白创建的时候要向左偏移，此时是位置刚好是输出端
		node.PositionOffset = canvasPos + new Vector2(_pendingIsFrom ? -node.Size.X : 0, 0);
		_nodes.Add(node);
		
		node.TreeExiting += () =>
		{
			if (_lastSelectedNode == node)
				_lastSelectedNode = null;

			if (_pendingNodeName == node.Name)
				_pendingNodeName = null;

			_nodes.Remove(node);
			_selectedNodes.Remove(node);
		};

		var newNode = (IGraphNode)node;
		newNode.MetadataChanged += OnNodeMetadataChanged;
		if (newNode is ICanvasMoveRequester requester)
		{
			requester.CanvasMoveRequested += OnCanvasMoveRequested;
		}
		AddChild(node);


		// 从输入输出端到空白处创建节点时，检查是否能自动连接
		if (_pendingNodeName != null)
		{
			var pendingNode = _nodes.FirstOrDefault(n => n.Name == _pendingNodeName);
			if (pendingNode == null)
				goto resetPending;

			if (_pendingIsFrom)
			{
				if (newNode.CanConnectWhenIsOutput(pendingNode, _pendingPort, out var outputPort, -1))
				{
					EmitSignalConnectionRequest(
						node.Name,
						outputPort,
						pendingNode.Name,
						_pendingPort
					);
				}
			}
			else
			{
				if (newNode.CanConnectWhenIsInput(pendingNode, _pendingPort, out var inputPort, -1))
				{
					EmitSignalConnectionRequest(
						pendingNode.Name,
						_pendingPort,
						node.Name,
						inputPort
					);
				}
			}
		}

		resetPending:
		_pendingIsFrom = false;
		_pendingNodeName = null;
		_pendingPort = 0;
	}

	private void OnPopupRequest(Vector2 pos)
	{
		_nodeMouseRightWindow.Hide();
		_nodeSelectWindow.Hide();

		_clickPosition = pos;

		if (_lastSelectedNode == null)
		{
			ShowNodeSelectWindow(pos);
		}
		else
		{
			ShowNodeMouseRightWindow(pos);
		}
	}

	private void ShowNodeMouseRightWindow(Vector2 pos)
	{
		SetPopPosition(_nodeMouseRightWindow, pos);
		
		_nodeMouseRightWindow.Popup();
	}

	private void ShowNodeSelectWindow(Vector2 pos, HashSet<string> availableButtons = null, bool oneshot = false)
	{		
		SetPopPosition(_nodeSelectWindow, pos);
		
		_nodeSelectWindow.ShowSelectMenu(availableButtons, oneshot);
	}

	private void SetPopPosition(Popup pop, Vector2 pos)
	{
		Vector2 globalMousePos = GetScreenPosition() + pos;

		var windowSize = pop.Size;
		var screenSize = DisplayServer.ScreenGetSize();

		float maxX = screenSize.X - windowSize.X;
		float maxY = screenSize.Y - windowSize.Y;

		Vector2I clampedPos = new(
			Mathf.Clamp((int)globalMousePos.X, 0, (int)maxX),
			Mathf.Clamp((int)globalMousePos.Y, 0, (int)maxY)
		);

		pop.Position = clampedPos;
	}

	private void OnNodeMetadataChanged(IGraphNode source, Dictionary<string, object> meta)
	{
		if (!IsNodeHasConnection((GraphNode)source))
			return;
		
		
		var connections = GetConnectionListFromNode(source.Name);
		foreach (var connection in connections)
		{
			var output = connection["from_node"].AsString();
			var input = connection["to_node"].AsString();
			var outputPort = connection["from_port"].AsInt32();
			var inputPort = connection["to_port"].AsInt32();

			if (!string.IsNullOrEmpty(output) && output != source.Name)
			{
				var outputNode = _nodes.FirstOrDefault(n => n.Name == output);
				if (outputNode is IGraphNodeMetadataListener listener)
				{
					listener.OnConnectionNodeMetadataChanged(source, meta, isOutput: false, port: inputPort);
				}
			}

			if (!string.IsNullOrEmpty(input) && input != source.Name)
			{
				var inputNode = _nodes.FirstOrDefault(n => n.Name == input);
				if (inputNode is IGraphNodeMetadataListener listener)
				{
					listener.OnConnectionNodeMetadataChanged(source, meta, isOutput: true, port: outputPort);
				}
			}
		}
	}

	private bool IsNodeHasConnection(GraphNode node)
		=> Connections.Any(c => c["from_node"].AsString() == node.Name || c["to_node"].AsString() == node.Name);

	private void OnCanvasMoveRequested(GraphNode target)
	{
		ScrollOffset = target.PositionOffset * Zoom - GetMenuHBox().Size;
	}
	
	/// <summary>
	/// 获取编辑器中的节点图数据
	/// </summary>
	/// <returns>节点图数据，包含了编辑器中所有节点连接信息</returns>
	public GraphData GetGraphData()
	{
		var nodes = GetChildren()
				.OfType<IGraphNode>()
				.Select(n => new NodeData(
					n.Name,
					n.NodeType,
					n.Position,
					n.CustomData
				))
				.ToList();

		var connections = Connections
				.Select(c => new ConnectionData(
					c["from_node"].AsString(),
					c["from_port"].AsInt32(),
					c["to_node"].AsString(),
					c["to_port"].AsInt32()
				))
				.ToList();

		return new(nodes, connections);
	}

	/// <summary>
    /// 从图数据重建节点图
    /// </summary>
    /// <param name="data">图数据</param>
	public void Restore(GraphData data)
	{
		_nodes.Clear();
		_selectedNodes.Clear();

		foreach (var node in GetChildren().OfType<GraphNode>())
		{
			RemoveChild(node);
			node.QueueFree();
		}

		var nodes = data.Nodes;
		var connections = data.Connections;

		var nodeIdToName = new Dictionary<string, string>();
		var needSort = new List<GraphNode>();

		foreach (var node in nodes)
		{
			var newNode = GraphNodeFactory.Create(node.NodeType);
			if (newNode == null)
				continue;

			var name = $"{node.NodeType}_{node.NodeId}";
			newNode.Name = name;
			nodeIdToName[node.NodeId] = name;

			if (node.Position.HasValue)
			{
				newNode.PositionOffset = node.Position.Value;
			}
			else
			{
				needSort.Add(newNode);
			}

			newNode.TreeExiting += () =>
			{
				if (_lastSelectedNode == newNode)
					_lastSelectedNode = null;
					
				if (_pendingNodeName == newNode.Name)
					_pendingNodeName = null;

				_nodes.Remove(newNode);
				_selectedNodes.Remove(newNode);
			};

			var graphNode = (IGraphNode)newNode;
			graphNode.CustomData = node.CustomData;
			graphNode.MetadataChanged += OnNodeMetadataChanged;
			if (newNode is ICanvasMoveRequester requester)
            {
                requester.CanvasMoveRequested += OnCanvasMoveRequested;
            }
			AddChild(newNode);
			_nodes.Add(newNode);
		}

		foreach (var connection in connections)
		{
			if (!nodeIdToName.TryGetValue(connection.OutputNodeId, out var outputNodeName) ||
				!nodeIdToName.TryGetValue(connection.InputNodeId, out var inputNodeName))
				continue;
			var error = ConnectNode(outputNodeName, connection.OutputPort, inputNodeName, connection.InputPort);
			if (error == Error.Ok)
			{
				var outputNode = _nodes.FirstOrDefault(n => n.Name == outputNodeName);
				var inputNode = _nodes.FirstOrDefault(n => n.Name == inputNodeName);
				if (outputNode is IDeserializeListener outputListener)
				{
					outputListener.OnAfterDeserialize(outputNode, inputNode, false, connection.OutputPort, connection.InputPort);
				}
				
				if (inputNode is IDeserializeListener inputListener)
				{
					inputListener.OnAfterDeserialize(inputNode, outputNode, true, connection.InputPort, connection.OutputPort);
				}
            }
		}

		needSort.ForEach(n => n.Selected = true);
		ArrangeNodes();
	}
	
    public void OnBeforeSerialize()
	{        
	
    }

	public void OnAfterDeserialize()
	{
		if (!EditorInterface.Singleton.IsPluginEnabled("TextualGraph"))
			return;

		foreach (var n in _nodes)
		{
			((IGraphNode)n).MetadataChanged += OnNodeMetadataChanged;
			if (n is ICanvasMoveRequester requester)
            {
                requester.CanvasMoveRequested += OnCanvasMoveRequested;            
            }
		}
	}

}
#endif
