using Godot;

public sealed partial class SimpleRotateY : Node3D
{
	public override void _Process(
		double delta
	)
	{
		float rotation = Speed * (float)delta;
        RotateObjectLocal(
            Vector3.Up,
            Mathf.DegToRad(
                rotation
            )
        );
    }

	[Export]
	public float Speed { get; set; } = 1f;
}