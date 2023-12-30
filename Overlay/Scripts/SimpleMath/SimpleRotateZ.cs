using Godot;

public sealed partial class SimpleRotateZ : Node3D
{
	public override void _Process(
		double delta
	)
	{
		float rotation = Speed * (float)delta;
		RotateObjectLocal(
			Vector3.Forward,
            Mathf.DegToRad(
                rotation
            )
        );
	}

	[Export]
	public float Speed { get; set; } = 1f;
}