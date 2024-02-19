using Godot;
using System;
using System.Collections.Generic;

public sealed partial class InputManager : Node
{
	public enum KeyBindType : long
	{
        ApplicationManagerQuit = Key.F12,
    }

	public readonly Dictionary<KeyBindType, Action> KeyBindPressed = new()
	{
		{ KeyBindType.ApplicationManagerQuit,     null },
    };
    public readonly Dictionary<KeyBindType, Action> KeyBindPressing = new()
    {
		{ KeyBindType.ApplicationManagerQuit,     null },
    };
    public readonly Dictionary<KeyBindType, Action> KeyBindReleased = new()
    {
		{ KeyBindType.ApplicationManagerQuit,     null },
    };

	public override void _Process(
        double delta
    )
	{
        foreach (var keyBindState in m_keyBindStates)
        {
            KeyBindType keyBindType = keyBindState.Key;
            bool isKeyPressed = Input.IsPhysicalKeyPressed(
                (Key)keyBindType
            );

            KeyStateType keyStateType = keyBindState.Value;
            switch (keyStateType) 
            {
                case KeyStateType.Pressed:
                    m_keyBindStates[keyBindType] = KeyStateType.Pressing;
                    KeyBindPressing[keyBindType]?.Invoke();
                    break;

                case KeyStateType.Pressing:
                    if (isKeyPressed)
                    {
                        KeyBindPressing[keyBindType]?.Invoke();
                    }
                    else
                    {
                        m_keyBindStates[keyBindType] = KeyStateType.Released;
                        KeyBindReleased[keyBindType]?.Invoke();
                    }
                    break;

                case KeyStateType.Released:
                    if (isKeyPressed)
                    {
                        m_keyBindStates[keyBindType] = KeyStateType.Pressed;
                        KeyBindPressed[keyBindType]?.Invoke();
                    }
                    break;

                default:
                    break;
            }
        }
	}

	private enum KeyStateType : uint
	{
		Pressed = 0u,
		Pressing,
		Released
	}

    private Dictionary<KeyBindType, KeyStateType> m_keyBindStates = new()
    {
        { KeyBindType.ApplicationManagerQuit,     KeyStateType.Released },
    };
}