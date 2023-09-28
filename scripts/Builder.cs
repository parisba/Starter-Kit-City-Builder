using Godot;
using System;
using System.Collections.Generic;

public partial class Builder : Node3D
{
    [Export]
	public Godot.Collections.Array<Structure> Structures = new Godot.Collections.Array<Structure>();

    private DataMap Map;

    private int Index = 0; // Index of structure being built

    [Export]
    public Node3D Selector; // The 'cursor'

    [Export]
    public Node3D SelectorContainer; // Node that holds a preview of the structure

    [Export]
    public Camera3D ViewCamera; // Used for raycasting mouse

    [Export]
    public GridMap Gridmap;

    [Export]
    public Label CashDisplay;

    private Plane Plane; // Used for raycasting mouse

    public override void _Ready()
    {
        Map = new DataMap();
        Plane = new Plane(Vector3.Up, Vector3.Zero);

        // Create new MeshLibrary dynamically, can also be done in the editor
        var meshLibrary = new MeshLibrary();

        foreach (var structure in Structures)
        {
            var id = meshLibrary.GetLastUnusedItemId();

            meshLibrary.CreateItem(id);
        
            meshLibrary.SetItemMesh(id, GetMesh(structure.Model));
            //meshLibrary.SetItemMeshTransform(id, new Transform3D());
        }

        Gridmap.MeshLibrary = meshLibrary;

        UpdateStructure();
        UpdateCash();
    }

    public override void _Process(double delta)
    {
        // Controls
        ActionRotate();
        ActionStructureToggle();
        ActionSave();
        ActionLoad();

        // Map position based on mouse
        var worldPosition = Plane.IntersectsRay(
            ViewCamera.ProjectRayOrigin(GetViewport().GetMousePosition()),
            ViewCamera.ProjectRayNormal(GetViewport().GetMousePosition()));

        if(worldPosition != null) 
		{
			var gridmapPosition = new Vector3I((int)Mathf.Round(worldPosition.GetValueOrDefault().X), 0, (int)Mathf.Round(worldPosition.GetValueOrDefault().Z));
		    Selector.Position = Selector.Position.Lerp(gridmapPosition, (float)delta * 40);

        	ActionBuild(gridmapPosition);
        	ActionDemolish(gridmapPosition);
		}
		

    }

    private Mesh GetMesh(PackedScene packedScene)
    {
        var sceneState = packedScene.GetState();
        for (int i = 0; i < sceneState.GetNodeCount(); i++)
        {
            if (sceneState.GetNodeType(i) == "MeshInstance3D")
            {
                for (int j = 0; j < sceneState.GetNodePropertyCount(i); j++)
                {
                    var propName = sceneState.GetNodePropertyName(i, j);
                    if (propName == "mesh")
                    {
						var mesh = sceneState.GetNodePropertyValue(i, j).As<Mesh>();
                    	return (Mesh)(mesh.Duplicate());
                    }
                }
            }
        }
        return null;
    }

    private void ActionBuild(Vector3I gridmapPosition)
    {
        if (Input.IsActionJustPressed("build"))
        {
            var previousTile = Gridmap.GetCellItem(gridmapPosition);
            Gridmap.SetCellItem(gridmapPosition, Index, Gridmap.GetOrthogonalIndexFromBasis(Selector.Basis));
			GD.Print("Index is " + Index + " and gridMapPosition is " + gridmapPosition);
            if (previousTile != Index)
            {
                Map.Cash -= Structures[Index].Price;
                UpdateCash();
            }
			GD.Print("After placing item " + Index + " at gridmapPosition " + gridmapPosition + " the item at that pos is " + Gridmap.GetCellItem(gridmapPosition));
        }
    }

    private void ActionDemolish(Vector3I gridmapPosition)
    {
        if (Input.IsActionJustPressed("demolish"))
        {
            Gridmap.SetCellItem(gridmapPosition, -1);
        }
    }

    private void ActionRotate()
    {
        if (Input.IsActionJustPressed("rotate"))
        {
            Selector.RotateY(Mathf.DegToRad(90));
        }
    }

    private void ActionStructureToggle()
    {
        if (Input.IsActionJustPressed("structure_next"))
        {
            Index = (Index + 1) % Structures.Count;
        }

        if (Input.IsActionJustPressed("structure_previous"))
        {
            Index = (Index - 1 + Structures.Count) % Structures.Count;
        }

        UpdateStructure();
    }

    private void UpdateStructure()
    {
        // Clear previous structure preview in selector
        foreach (Node n in SelectorContainer.GetChildren())
        {
            SelectorContainer.RemoveChild(n);
        }

        // Create new structure preview in selector
        var model = (Node3D)Structures[Index].Model.Instantiate();
        SelectorContainer.AddChild(model);
        model.Position = new Vector3(0, 0.25f, 0);
    }

    private void UpdateCash()
    {
        CashDisplay.Text = "$" + Map.Cash.ToString();
    }

    private void ActionSave()
    {
        if (Input.IsActionJustPressed("save"))
        {
            GD.Print("Saving map...");

            foreach (Vector3I cell in Gridmap.GetUsedCells())
            {
                var dataStructure = new DataStructure
                {
                    Position = new Vector2I((int)cell.X, (int)cell.Z),
                    Orientation = Gridmap.GetCellItemOrientation(cell),
                    Structure = Gridmap.GetCellItem(cell)
                };

                Map.Structures.Add(dataStructure);
            }

            ResourceSaver.Save(Map, "user://map.res");
        }
    }

    private void ActionLoad()
    {
        if (Input.IsActionJustPressed("load"))
        {
            GD.Print("Loading map...");

            Gridmap.Clear();

            Map = (DataMap)ResourceLoader.Load("user://map.res");

            foreach (var cell in Map.Structures)
            {
                Gridmap.SetCellItem(new Vector3I(cell.Position.X, 0, cell.Position.Y), cell.Structure, cell.Orientation);
            }

            UpdateCash();
        }
    }
}
