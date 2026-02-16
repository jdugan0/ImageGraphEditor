using System;
using Godot;

public partial class DataManager : Control
{
    public Dag dag = new Dag();
    public static DataManager instance;

    [Export]
    PackedScene nodeUI;

    public PortUI currentHover;

    public override void _Ready()
    {
        instance = this;
    }

    public override void _Process(double delta)
    {
        dag.Propegate();
    }

    public void CreateUINode(string operatorTime)
    {
        GraphNode node;
        switch (operatorTime)
        {
            case "ADD":
                node = new AddGraphNode();
                break;
            case "CONSTANT":
                node = new ConstantGraphNode();
                break;
            default:
                throw new Exception();
        }
        Guid id = dag.AddNode(node);
        NodeUI nodeInstance = nodeUI.Instantiate<NodeUI>();
        nodeInstance.id = id;
        nodeInstance.Init();
        AddChild(nodeInstance);
    }
}
