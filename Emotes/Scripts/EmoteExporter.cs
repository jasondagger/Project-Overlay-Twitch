using Godot;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public sealed partial class EmoteExporter : Node
{
    public override void _Process(
        double delta
    )
    {
        if (m_canRenderFrames)
        {
            m_elapsed += delta;
        }
    }

    public override void _Ready()
	{
        m_viewportEmote = GetNode<Viewport>(
            c_nodeDirectorySubViewportEmote
        );
        m_viewportOutline = GetNode<Viewport>(
            c_nodeDirectorySubViewportOutline
        );

        m_exportPath = $"res://{c_directoryNameEmotes}/{EmoteName}/{c_directoryNameRenders}";
        string applicationPath = $"{Directory.GetCurrentDirectory()}\\{c_directoryNameEmotes}\\{EmoteName}\\{c_directoryNameRenders}";
        if (
            !Directory.Exists(
                applicationPath
            )
        )
        {
            Directory.CreateDirectory(
                applicationPath
            );
        }
        else
        {
            // clean up existing renders
            var files = Directory.GetFiles(
                applicationPath
            );
        
            foreach (var file in files)
            {
                File.Delete(
                    file
                );
            }
        }

        m_targetElapsed = 1d / TargetFramesPerSecond;
        RenderingServer.FramePostDraw += OnRendererReady;
	}

    private const string c_directoryNameEmotes = "Emotes";
    private const string c_directoryNameRenders = "Renders";
    private const string c_nodeDirectorySubViewportEmote = "/root/Main/SubViewportContainer/SubViewportEmote";
    private const string c_nodeDirectorySubViewportOutline = "/root/Main/SubViewportContainer/SubViewportOutline";

    private const int c_textureHeight = 1024;
    private const int c_textureWidth = 1024;

    private readonly Color c_imageBackgroundColorEmote = new(0.0f, 0.0f, 0.0f);

    private struct ImageLayers
    {
        public Image emote { get; set; } = null;
        public Image outline { get; set; } = null;

        public ImageLayers(
            Image emote,
            Image outline
        )
        {
            this.emote = emote;
            this.outline = outline;
        }
    }

    [Export]
    private string EmoteName { get; set; } = string.Empty;
    [Export]
    private uint TargetFrameCount { get; set; } = 1u;
    [Export]
    private uint TargetFramesPerSecond { get; set; } = 1u;

    private List<ImageLayers> m_imageLayers = new();
    private Viewport m_viewportEmote = null;
    private Viewport m_viewportOutline = null;
    private bool m_canRenderFrames = true;
    private double m_elapsed = 0d;
    private double m_targetElapsed = 0d;
    private string m_exportPath = string.Empty;
    private uint m_numberOfRenderedFrames = 0u;

    private void OnImagesRetrieved()
    {
        Task.Run(
            () =>
            {
                for (int i = 0; i < m_imageLayers.Count; i++)
                {
                    Image imageEmote = m_imageLayers[i].emote;
                    //Image imageOutline = m_imageLayers[i].outline;
                    //Image imageOutput = Image.Create(
                    //    c_textureWidth,
                    //    c_textureHeight,
                    //    false,
                    //    Image.Format.Rgba8
                    //);
                    //
                    //for (int x = 0; x < c_textureWidth; x++)
                    //{
                    //    for (int y = 0; y < c_textureHeight; y++)
                    //    {
                    //        Color pixelColorEmote = imageEmote.GetPixel(
                    //            x,
                    //            y
                    //        );
                    //        Color pixelColorOutline = imageOutline.GetPixel(
                    //            x,
                    //            y
                    //        );
                    //
                    //        imageOutput.SetPixel(
                    //            x,
                    //            y,
                    //            pixelColorEmote == c_imageBackgroundColorEmote ? pixelColorOutline : pixelColorEmote
                    //        );
                    //    }
                    //}

                    imageEmote.SavePng(
                        $"{m_exportPath}/{EmoteName}_{i}.png"
                    );
                }

                // close application
                GetTree().Quit();
            }
        );
    }

    private void OnRendererReady()
	{
        if (m_elapsed >= m_targetElapsed)
        {
            RetrieveViewportImage();
            m_elapsed = 0d;
        }
    }

    private void RetrieveViewportImage()
    {
        ViewportTexture viewportTextureEmote = m_viewportEmote.GetTexture();
        ViewportTexture viewportTextureOutline = m_viewportOutline.GetTexture();

        m_imageLayers.Add(
            new(
                viewportTextureEmote.GetImage(),
                viewportTextureOutline.GetImage()
            )
        );

        m_numberOfRenderedFrames++;
        if (m_numberOfRenderedFrames == TargetFrameCount)
        {
            m_canRenderFrames = false;
            OnImagesRetrieved();
        }
    }
}