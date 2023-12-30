using Godot;

public sealed partial class SimpleTranslateZ : Node3D
{
	public override void _Process(
		double delta
	)
	{
		float translation = Speed * (float)delta;
		TranslateObjectLocal(
			Vector3.Forward * translation
		);
	}

	[Export]
	public float Speed { get; set; } = 1f;
}