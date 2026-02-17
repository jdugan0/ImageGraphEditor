using System;
using Godot;

public partial class LineEditUnFocus : LineEdit
{
    bool hovered = false;

    public override void _Ready()
    {
        MouseEntered += OnHover;
        MouseExited += OnHoverExit;
    }

    public void OnHover()
    {
        hovered = true;
    }

    public void OnHoverExit()
    {
        hovered = false;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("CLICK") && !hovered && HasFocus())
        {
            ReleaseFocus();
        }
    }
}
