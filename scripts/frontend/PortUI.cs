using System;
using Godot;

public partial class PortUI : Control
{
    public Guid id;
    public Port port;
    bool over = false;
    bool pressed = false;

    EdgeUI currentEdge = null;

    [Export]
    PackedScene edgeScene;

    public void Init()
    {
        port = DataManager.instance.dag.ports[id];
    }

    public void OnHover()
    {
        over = true;
        DataManager.instance.currentHover = this;
    }

    public void OnHoverExit()
    {
        over = false;
        DataManager.instance.currentHover = null;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("CLICK") && over)
        {
            pressed = true;
            currentEdge = edgeScene.Instantiate<EdgeUI>();
            AddChild(currentEdge);
            currentEdge.start = GlobalPosition;
        }
        if (Input.IsActionJustReleased("CLICK") && pressed)
        {
            if (
                DataManager.instance.currentHover != null
                && DataManager.instance.currentHover != this
            )
            {
                try
                {
                    DataManager.instance.dag.TryConnect(
                        DataManager.instance.currentHover.port,
                        port
                    );
                    DataManager.instance.dag.Connect(DataManager.instance.currentHover.id, id);
                }
                catch (Exception e)
                {
                    currentEdge.QueueFree();
                    GD.Print(e.Message);
                }
            }
            else if (currentEdge != null)
            {
                currentEdge.QueueFree();
            }
            currentEdge = null;
            pressed = false;
        }
        if (pressed)
        {
            currentEdge.QueueRedraw();
            if (
                DataManager.instance.currentHover != null
                && DataManager.instance.currentHover != this
            )
            {
                currentEdge.end = DataManager.instance.currentHover.GlobalPosition;
            }
            else
            {
                currentEdge.end = GetGlobalMousePosition();
            }
        }
    }
}
