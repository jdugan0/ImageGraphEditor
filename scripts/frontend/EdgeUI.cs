using System;
using Godot;

public partial class EdgeUI : Control
{
    public Guid id;
    public Edge edge;

    public Vector2 start = Vector2.Zero;
    public Vector2 end = Vector2.Zero;

    public void Init()
    {
        edge = DataManager.instance.dag.edges[id];
    }

    public override void _Draw()
    {
        DrawLine(-start + GlobalPosition, end - GlobalPosition, Colors.Blue, 20);
    }
}
