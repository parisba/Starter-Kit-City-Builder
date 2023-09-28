using Godot;
using System;

[Tool]
public partial class DataStructure : Resource
{
    //[Export]
    public Vector2I Position { get; set; }

    [Export]
    public int Orientation { get; set; }

    [Export]
    public int Structure { get; set; }
}
