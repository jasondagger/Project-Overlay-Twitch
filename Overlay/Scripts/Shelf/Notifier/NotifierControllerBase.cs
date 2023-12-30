using Godot;
using System;
using System.Threading.Tasks;

public abstract partial class NotifierControllerBase : Control
{
    public Action CompletedNotification = null;
    
    public async void StartNotification()
    {
        await Task.Run(
            PlayNotification
        );
    }
    
    protected abstract void PlayNotification();
}