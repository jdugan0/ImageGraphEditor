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
            currentEdge.start = GetViewport().GetMousePosition();
        }
        if (Input.IsActionJustReleased("CLICK"))
        {
            if (
                DataManager.instance.currentHover != null
                && DataManager.instance.currentHover != this
            )
            {
                try
                {
                    DataManager.instance.dag.TryConnect(
                        port,
                        DataManager.instance.currentHover.port
                    );
                    DataManager.instance.dag.Connect(id, DataManager.instance.currentHover.id);
                }
                catch (Exception e)
                {
                    GD.Print(e.Message);
                }
            }
            else
            {
                currentEdge.QueueFree();
            }
            currentEdge = null;
            pressed = false;
            currentEdge = null;
        }
        if (pressed)
        {
            if (
                DataManager.instance.currentHover != null
                && DataManager.instance.currentHover != this
            )
            {
                currentEdge.end = DataManager.instance.currentHover.Position;
            }
            else
            {
                currentEdge.end = GetViewport().GetMousePosition();
            }
        }
    }
}
