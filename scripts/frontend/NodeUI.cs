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

    [Export]
    public StyleBoxFlat panelTheme;

    [Export]
    Label errorMessage;

    [Export]
    Label nodeType;

    [Export]
    Panel panel;

    [Export]
    Button dragButton;

    private StyleBoxFlat _panelStyle;
    Vector2? dragDelta = null;

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
        if (operatorType == GraphNodeTypes.CONSTANT)
        {
            ((LineEdit)GetNode(references["text_entry"])).TextChanged += (string text) =>
            {
                dag.MarkDirty(id);
            };
        }
    }

    public override void _Ready()
    {
        _panelStyle = (StyleBoxFlat)panelTheme.Duplicate(true);
        panel.AddThemeStyleboxOverride("panel", _panelStyle);
        _panelStyle.BorderColor = _panelStyle.BgColor;
        nodeType.Text = operatorType.ToString();
        dragButton.ButtonDown += Drag;
        dragButton.ButtonUp += EndDrag;
    }

    public void EndDrag()
    {
        dragDelta = null;
    }

    public void Drag()
    {
        if (dragDelta == null)
        {
            dragDelta = GlobalPosition - GetGlobalMousePosition();
        }
    }

    public override void _Process(double delta)
    {
        if (dragDelta != null)
        {
            GlobalPosition = dragDelta.Value + GetGlobalMousePosition();
        }
    }

    public void RemoveSelf()
    {
        dag.RemoveNode(id);
        QueueFree();
    }

    public void FailedEval(string msg)
    {
        errorMessage.Text = msg;
        _panelStyle.BorderColor = Colors.Red;
    }

    public void SucceedEval()
    {
        _panelStyle.BorderColor = panelTheme.BgColor;
        errorMessage.Text = "";
    }

    public void SetData()
    {
        switch (operatorType)
        {
            case GraphNodeTypes.ADD:
                ((Label)GetNode(references["label"])).Text = (
                    (float)node.data["result"]
                ).ToString();
                break;
            case GraphNodeTypes.CONSTANT:
                float f;
                string text = ((LineEdit)GetNode(references["text_entry"])).Text;
                if (text.IsValidFloat())
                {
                    f = text.ToFloat();
                }
                else
                {
                    node.SetData(dag, new Dict { ["value"] = null });
                    throw new Exception("Not valid float.");
                }
                node.SetData(dag, new Dict { ["value"] = f });
                break;
            default:
                throw new Exception();
        }
    }
}
