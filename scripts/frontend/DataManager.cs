using System;
using Godot;

public partial class DataManager : Node
{
    Dag dag = new Dag();
    public DataManager instance;

    [Export]
    PackedScene nodeUI;

    public override void _Ready()
    {
        instance = this;
    }

    public void CreateUINode() { }
}
