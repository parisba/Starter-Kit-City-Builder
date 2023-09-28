using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class DataMap : Resource
{
    [Export]
    public int Cash { get; set; } = 10000;

    [Export]
    public Godot.Collections.Array<DataStructure> Structures { get; set; } = new Godot.Collections.Array<DataStructure>();
}
