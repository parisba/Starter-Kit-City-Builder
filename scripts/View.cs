using Godot;
using System;

public partial class View : Node3D
{
    private Vector3 CameraPosition;
    private Vector3 CameraRotation;
    private Camera3D Camera;

    public override void _Ready()
    {
        CameraRotation = RotationDegrees; // Initial rotation
        Camera = GetNode<Camera3D>("Camera");
    }

    public override void _Process(double delta)
    {
        // Set position and rotation to targets
        Position = Position.Lerp(CameraPosition, (float)delta * 8);
        RotationDegrees = RotationDegrees.Lerp(CameraRotation, ((float)delta * 6));

        HandleInput((float)delta);
    }

    private void HandleInput(float delta)
    {
        // Rotation
        var input = new Vector3();

        input.X = Input.GetActionStrength("camera_right") - Input.GetActionStrength("camera_left");
        input.Z = Input.GetActionStrength("camera_forward") - Input.GetActionStrength("camera_back");

        input = input.Rotated(Vector3.Up, Rotation.Y).Normalized();

        CameraPosition += input / 4;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMotion)
        {
            if (Input.IsActionPressed("camera_rotate"))
            {
                CameraRotation += new Vector3(0, -eventMotion.Relative.X / 10, 0);
            }
        }
    }
}
