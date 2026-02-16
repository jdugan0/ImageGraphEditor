using System;
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
    Dictionary<string, Node> references;
    public string operatorType;

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
            case "ADD":
                ((Label)references["label"]).Text = (string)node.data["result"];
                break;
            case "CONSTANT":
                string text = ((TextEdit)references["text_entry"]).Text;
                float f = text.ToFloat();
                node.SetData(dag, new Dict { ["value"] = text.ToFloat() });
                break;
            default:
                throw new Exception();
        }
    }
}
