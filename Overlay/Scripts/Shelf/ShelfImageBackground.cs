using Godot;
using static Godot.Image;

public sealed partial class ShelfImageBackground : ShelfImage
{
    public override void _EnterTree()
    {
        CreateImageTexture();
        GenerateImageBackground();
        SetShaderMaterial();
    }

    private Image m_imageMain = null;
    private ImageTexture m_textureMain = new();

    private void CreateImageTexture()
    {
        m_imageMain = Create(
            c_textureWidth,
            c_textureHeight,
            false,
            Format.Rgbaf
        );

        m_textureMain.SetImage(
            m_imageMain
        );
    }

    private void GenerateImageBackground()
    {
        Color color = new(
            0x202020FF
        );
        for (int y = 0; y < c_textureHeight; y++)
        {
            for (int x = 0; x < c_textureWidth; x++)
            {
                m_imageMain.SetPixel(
                    x,
                    y,
                    color
                );
            }
        }

        m_textureMain.Update(
            m_imageMain
        );
    }

    private void SetShaderMaterial()
    {
        var material = (ShaderMaterial)Get(
            "material"
        );
        material.SetShaderParameter(
            "textureMain",
            m_textureMain
        );
    }
}