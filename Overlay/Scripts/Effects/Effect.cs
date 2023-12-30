using Godot;

public abstract partial class Effect : Node
{
    public abstract void Play();
    public abstract void Stop();

    public abstract bool IsLooping();
    public abstract bool IsPlaying();
}