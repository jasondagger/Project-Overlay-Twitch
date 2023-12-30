using Godot;

public sealed partial class SimpleRotateX : Node3D
{
	public override void _Process(
		double delta
	)
	{
		float rotation = Speed * (float)delta;
        RotateObjectLocal(
            Vector3.Right,
            Mathf.DegToRad(
                rotation
            )
        );
    }

	[Export]
	public float Speed { get; set; } = 1f;
}