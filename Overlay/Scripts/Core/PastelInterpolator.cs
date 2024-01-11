using Godot;
using System.Collections.Generic;

public sealed partial class PastelInterpolator : Node
{
	public override void _EnterTree()
	{
        RetrieveResources();
	}

	public override void _Process(
        double delta
    )
	{
        UpdateColor(
            (float)delta
        );
	}

    public Color GetColor()
    {
        return m_currentColor;
    }

    public string GetColorAsHex()
    {
        return m_currentColor.ToHtml();
    }

    private enum ColorInterpolationType : uint
    {
        RedToYellow = 0u,
        YellowToGreen,
        GreenToCyan,
        CyanToBlue,
        BlueToMagenta,
        MagentaToRed
    }

    private enum ColorType : uint
    {
        Red = 0u,
        Yellow,
        Green,
        Cyan,
        Blue,
        Magenta,
    }

    private readonly Dictionary<ColorType, Color> c_colorCodes = new()
    {
        { ColorType.Red,     new(0xF898A4FF) },
        { ColorType.Yellow,  new(0xFDFFB6FF) },
        { ColorType.Green,   new(0xCAFFBFFF) },
        { ColorType.Cyan,    new(0x9BF6FFFF) },
        { ColorType.Blue,    new(0xA0C4FFFF) },
        { ColorType.Magenta, new(0xFFC6FFFF) },
    };

    private const float c_colorInterpolationRate = 0.25f;

    private ColorInterpolationType m_colorInterpolationType = ColorInterpolationType.RedToYellow;
    private Color m_currentColor = new();
    private Color m_fromColor = new();
    private float m_colorInterpolation = 0f;

    private void RetrieveResources()
    {
        m_currentColor = c_colorCodes[ColorType.Red];
        m_fromColor = c_colorCodes[ColorType.Red];
    }

    private void UpdateColor(
        float delta
    )
    {
        m_colorInterpolation += c_colorInterpolationRate * delta;
        switch (m_colorInterpolationType)
        {
            case ColorInterpolationType.RedToYellow:
                m_currentColor = m_fromColor.Lerp(
                    c_colorCodes[ColorType.Yellow],
                    m_colorInterpolation
                );

                if (m_colorInterpolation >= 1f)
                {
                    m_colorInterpolation = 0f;
                    m_currentColor = c_colorCodes[ColorType.Yellow];
                    m_fromColor = c_colorCodes[ColorType.Yellow];
                    m_colorInterpolationType = ColorInterpolationType.YellowToGreen;
                }
                break;

            case ColorInterpolationType.YellowToGreen:
                m_currentColor = m_fromColor.Lerp(
                    c_colorCodes[ColorType.Green],
                    m_colorInterpolation
                );

                if (m_colorInterpolation >= 1f)
                {
                    m_colorInterpolation = 0f;
                    m_currentColor = c_colorCodes[ColorType.Green];
                    m_fromColor = c_colorCodes[ColorType.Green];
                    m_colorInterpolationType = ColorInterpolationType.GreenToCyan;
                }
                break;

            case ColorInterpolationType.GreenToCyan:
                m_currentColor = m_fromColor.Lerp(
                    c_colorCodes[ColorType.Cyan],
                    m_colorInterpolation
                );

                if (m_colorInterpolation >= 1f)
                {
                    m_colorInterpolation = 0f;
                    m_currentColor = c_colorCodes[ColorType.Cyan];
                    m_fromColor = c_colorCodes[ColorType.Cyan];
                    m_colorInterpolationType = ColorInterpolationType.CyanToBlue;
                }
                break;

            case ColorInterpolationType.CyanToBlue:
                m_currentColor = m_fromColor.Lerp(
                    c_colorCodes[ColorType.Blue],
                    m_colorInterpolation
                );

                if (m_colorInterpolation >= 1f)
                {
                    m_colorInterpolation = 0f;
                    m_currentColor = c_colorCodes[ColorType.Blue];
                    m_fromColor = c_colorCodes[ColorType.Blue];
                    m_colorInterpolationType = ColorInterpolationType.BlueToMagenta;
                }
                break;

            case ColorInterpolationType.BlueToMagenta:
                m_currentColor = m_fromColor.Lerp(
                    c_colorCodes[ColorType.Magenta],
                    m_colorInterpolation
                );

                if (m_colorInterpolation >= 1f)
                {
                    m_colorInterpolation = 0f;
                    m_currentColor = c_colorCodes[ColorType.Magenta];
                    m_fromColor = c_colorCodes[ColorType.Magenta];
                    m_colorInterpolationType = ColorInterpolationType.MagentaToRed;
                }
                break;

            case ColorInterpolationType.MagentaToRed:
                m_currentColor = m_fromColor.Lerp(
                    c_colorCodes[ColorType.Red],
                    m_colorInterpolation
                );

                if (m_colorInterpolation >= 1f)
                {
                    m_colorInterpolation = 0f;
                    m_currentColor = c_colorCodes[ColorType.Red];
                    m_fromColor = c_colorCodes[ColorType.Red];
                    m_colorInterpolationType = ColorInterpolationType.RedToYellow;
                }
                break;

            default:
                break;
        }
    }
}
