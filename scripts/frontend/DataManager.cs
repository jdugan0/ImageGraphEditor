using System;
using System.Collections.Generic;
using Godot;

public partial class DataManager : Control
{
    public Dag dag = new Dag();
    public static DataManager instance;

    [Export]
    Godot.Collections.Dictionary<GraphNodeTypes, PackedScene> nodeUI =
        new Godot.Collections.Dictionary<GraphNodeTypes, PackedScene>();

    public PortUI currentHover;

    public override void _Ready()
    {
        instance = this;
    }

    public override void _Process(double delta)
    {
        dag.Propegate();
        if (Input.IsActionJustPressed("Z"))
        {
            CreateUINode(GraphNodeTypes.ADD);
        }
        if (Input.IsActionJustPressed("X"))
        {
            CreateUINode(GraphNodeTypes.CONSTANT);
        }
    }

    public void CreateUINode(GraphNodeTypes type)
    {
        GraphNode node;
        NodeUI nodeInstance = nodeUI[type].Instantiate<NodeUI>();
        nodeInstance.operatorType = type;
        switch (type)
        {
            case GraphNodeTypes.ADD:
                node = new AddGraphNode();
                break;
            case GraphNodeTypes.CONSTANT:
                node = new ConstantGraphNode();
                break;
            default:
                throw new Exception();
        }
        Guid id = dag.AddNode(node);

        nodeInstance.id = id;
        nodeInstance.Init();
        nodeInstance.Position = GetViewport().GetMousePosition();
        node.UI = nodeInstance;
        AddChild(nodeInstance);
    }
}
