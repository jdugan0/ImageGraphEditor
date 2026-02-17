using System;
using System.Collections.Generic;
using Godot;

public partial class DataManager : Control
{
    public Dag dag = new Dag();
    public static DataManager instance;

    [Export]
    Godot.Collections.Dictionary<string, PackedScene> nodeUI = new Godot.Collections.Dictionary<
        string,
        PackedScene
    >();

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
            CreateUINode("ADD");
        }
        if (Input.IsActionJustPressed("X"))
        {
            CreateUINode("CONSTANT");
        }
    }

    public void CreateUINode(string operatorTime)
    {
        GraphNode node;
        NodeUI nodeInstance;
        switch (operatorTime)
        {
            case "ADD":
                nodeInstance = nodeUI["ADD"].Instantiate<NodeUI>();
                node = new AddGraphNode();
                nodeInstance.operatorType = "ADD";
                break;
            case "CONSTANT":
                nodeInstance = nodeUI["CONSTANT"].Instantiate<NodeUI>();
                node = new ConstantGraphNode();
                nodeInstance.operatorType = "CONSTANT";
                break;
            default:
                throw new Exception();
        }
        Guid id = dag.AddNode(node);

        nodeInstance.id = id;
        nodeInstance.Init();
        nodeInstance.Position = GetViewport().GetMousePosition();
        AddChild(nodeInstance);
    }
}
