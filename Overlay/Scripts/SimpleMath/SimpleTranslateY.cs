using Godot;

public sealed partial class SimpleTranslateY : Node3D
{
	public override void _Process(
		double delta
	)
	{
		float translation = Speed * (float)delta;
		TranslateObjectLocal(
			Vector3.Up * translation
		);
	}

	[Export]
	public float Speed { get; set; } = 1f;
}