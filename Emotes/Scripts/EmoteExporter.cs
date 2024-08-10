
namespace Emotes
{
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
                path: c_nodeDirectorySubViewportEmote
            );
            m_viewportOutline = GetNode<Viewport>(
                path: c_nodeDirectorySubViewportOutline
            );

            m_exportPath = $"res://{c_directoryNameEmotes}/{EmoteName}/{c_directoryNameRenders}";
            var applicationPath = $"{Directory.GetCurrentDirectory()}\\{c_directoryNameEmotes}\\{EmoteName}\\{c_directoryNameRenders}";
            if (
                Directory.Exists(
                    path: applicationPath
                ) is false
            )
            {
                _ = Directory.CreateDirectory(
                    path:applicationPath
                );
            }
            else
            {
                var files = Directory.GetFiles(
                    path: applicationPath
                );
                foreach (var file in files)
                {
                    File.Delete(
                        path: file
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
            public Image Emote { get; set; } = null;
            public Image Outline { get; set; } = null;

            public ImageLayers(
                Image emote,
                Image outline
            )
            {
                this.Emote = emote;
                this.Outline = outline;
            }
        }

        [Export]
        private string EmoteName { get; set; } = string.Empty;
        [Export]
        private uint TargetFrameCount { get; set; } = 1u;
        [Export]
        private uint TargetFramesPerSecond { get; set; } = 1u;

        private readonly List<ImageLayers> m_imageLayers = new();
        private Viewport m_viewportEmote = null;
        private Viewport m_viewportOutline = null;
        private bool m_canRenderFrames = true;
        private double m_elapsed = 0d;
        private double m_targetElapsed = 0d;
        private string m_exportPath = string.Empty;
        private uint m_numberOfRenderedFrames = 0u;

        private void OnImagesRetrieved()
        {
            _ = Task.Run(
                () =>
                {
                    for (var i = 0; i < m_imageLayers.Count; i++)
                    {
                        var imageEmote = m_imageLayers[i].Emote;
                        var imageOutline = m_imageLayers[i].Outline;
                        var imageOutput = Image.Create(
                            width: c_textureWidth,
                            height: c_textureHeight,
                            useMipmaps: false,
                            format: Image.Format.Rgba8
                        );

                        for (var x = 0; x < c_textureWidth; x++)
                        {
                            for (var y = 0; y < c_textureHeight; y++)
                            {
                                var pixelColorEmote = imageEmote.GetPixel(
                                    x: x,
                                    y: y
                                );
                                var pixelColorOutline = imageOutline.GetPixel(
                                    x: x,
                                    y: y
                                );

                                imageOutput.SetPixel(
                                    x: x,
                                    y: y,
                                    color: pixelColorEmote.Equals(
                                        other: c_imageBackgroundColorEmote
                                    ) ? pixelColorOutline : 
                                        pixelColorEmote
                                );
                            }
                        }

                        _ = imageOutput.SavePng(
                            path: $"{m_exportPath}/{EmoteName}_{i}.png"
                        );
                    }

                    var sceneTree = GetTree();
                    sceneTree.Quit();
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
            var viewportTextureEmote = m_viewportEmote.GetTexture();
            var viewportTextureOutline = m_viewportOutline.GetTexture();

            m_imageLayers.Add(
                item: new(
                    viewportTextureEmote.GetImage(),
                    viewportTextureOutline.GetImage()
                )
            );

            m_numberOfRenderedFrames++;
            if (
                m_numberOfRenderedFrames.Equals(
                    obj: TargetFrameCount    
                ) is true
            )
            {
                m_canRenderFrames = false;
                OnImagesRetrieved();
            }
        }
    }
}