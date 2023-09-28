using Godot;
using System;

[Tool]
public partial class Structure : Resource
{
    [Export(PropertyHint.None, "Model")]
    public PackedScene Model { get; set; } // Model of the structure

    [Export(PropertyHint.None, "Gameplay")]
    public int Price { get; set; } // Price of the structure when building
}
