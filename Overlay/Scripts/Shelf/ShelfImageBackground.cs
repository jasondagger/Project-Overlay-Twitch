
namespace Overlay
{
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
                image: m_imageMain
            );
        }

        private void GenerateImageBackground()
        {
            var color = new Color(
                rgba: 0x202020FF
            );
            for (var y = 0; y < c_textureHeight; y++)
            {
                for (var x = 0; x < c_textureWidth; x++)
                {
                    m_imageMain.SetPixel(
                        x: x,
                        y: y,
                        color: color
                    );
                }
            }

            m_textureMain.Update(
                image: m_imageMain
            );
        }

        private void SetShaderMaterial()
        {
            var material = (ShaderMaterial)Get(
                property: "material"
            );
            material.SetShaderParameter(
                param: "textureMain",
                value: m_textureMain
            );
        }
    }
}