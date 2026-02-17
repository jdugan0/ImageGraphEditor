using System;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;
using Dict = System.Collections.Generic.Dictionary<string, object>;

public partial class NodeUI : Control
{
    public Guid id;
    GraphNode node;
    Dag dag;

    [Export]
    Node inputPorts;

    [Export]
    Node outputPorts;

    [Export]
    PackedScene portScene;

    [Export]
    Dictionary<string, NodePath> references;
    public GraphNodeTypes operatorType;

    public void Init()
    {
        dag = DataManager.instance.dag;
        node = dag.nodes[id];
        foreach (var port in node.inputPorts)
        {
            PortUI p = portScene.Instantiate<PortUI>();
            p.id = port;
            p.Init();
            inputPorts.AddChild(p);
        }
        foreach (var port in node.outputPorts)
        {
            PortUI p = portScene.Instantiate<PortUI>();
            p.id = port;
            p.Init();
            outputPorts.AddChild(p);
        }
    }

    public override void _Process(double delta)
    {
        SetData();
    }

    public void SetData()
    {
        switch (operatorType)
        {
            case GraphNodeTypes.ADD:
                if (node.evaluated)
                {
                    ((Label)GetNode(references["label"])).Text = (
                        (float)node.data["result"]
                    ).ToString();
                }
                else
                {
                    ((Label)GetNode(references["label"])).Text = "";
                }
                break;
            case GraphNodeTypes.CONSTANT:
                float f;
                if (references["text_entry"] == null)
                    GD.Print(references.ToString());
                string text = ((LineEdit)GetNode(references["text_entry"])).Text;
                if (text.IsValidFloat())
                {
                    f = text.ToFloat();
                }
                else
                {
                    f = 0;
                }
                node.SetData(dag, new Dict { ["value"] = f });
                break;
            default:
                throw new Exception();
        }
        node.evaluated = false;
    }
}
