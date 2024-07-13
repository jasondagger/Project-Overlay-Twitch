
namespace Overlay
{
	using Godot;
    using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
    using System.Linq;
	using System.Runtime.Versioning;
    using static Godot.HttpClient;
    using RainbowColorIndexType = PastelInterpolator.RainbowColorIndexType;

    [SupportedOSPlatform(platformName: "windows")]
    public sealed partial class TwitchChatMessage : Node2D
	{
		public Action<TwitchChatMessage> Generated = null;
		public Action Destroyed = null;

        #region GODOT_INTRINSTICS
        public override void _Process(
			double delta
		)
		{
            switch (m_generatedState)
			{
				case GeneratedState.Generated:
					Generated?.Invoke(
						obj: this
					);
					m_generatedState = GeneratedState.Complete;
					break;
				case GeneratedState.Complete:
					HandleTextAnimation(
						delta: (float)delta	
					);
					HandleTextFade(
						delta: (float)delta
					);
					break;
				case GeneratedState.Generating:
				default:
					break;
			}
		}
        #endregion

        public void Generate(
			HttpManager httpManager,
			PastelInterpolator pastelInterpolator,
			string name,
			string nameColor,
			string message,
			string messageColor,
			string emotes,
			string badges,
			bool isSubscriber,
			bool isSmoothGPT
		)
		{
			m_pastelInterpolator = pastelInterpolator;
			m_isSubscriber = isSubscriber;
            m_text =
                $"{c_labelFontSize}" +
                $"{c_labelNameFont}" +
                $"[color=#{(m_isSubscriber ? c_labelSubscriberColor : nameColor)}]" +
                $"{name}" +
                $"[/color]" +
                $"[/font]" +
                $"  " +
                $"{c_labelMessageFont}";

            if (
                messageColor.Equals(
                    value: c_rainbowColorTag
                ) is true
            )
            {
                var trimmedMessage = message.Remove(
                    startIndex: message.Length - 2, 
                    count: 2
                );
                var rainbowifiedMessage = PastelInterpolator.RainbowifyText(
                    text: trimmedMessage    
                );
                m_text += rainbowifiedMessage;
            }
            else
            {
                m_text += 
                    $"{(messageColor.Equals(value: string.Empty) is true ? c_labelMessageColor : messageColor)}" +
				    $"{(isSmoothGPT ? message : message.Remove(startIndex: message.Length - 2, count: 2))}";
            }

			InsertImages(
				httpManager: httpManager,
				message: message,
				emotes: emotes,
				badges: badges
			);
		}

		public int GetLabelHeightInPixels()
		{
			return m_richTextLabel.GetContentHeight();
		}

		public void ShowLabel()
		{
			m_richTextLabel.Visible = true;
		}

        #region INTERNAL_VARIABLES_&_STRUCTURES
        private enum GeneratedState : uint
		{
			Generating = 0u,
			Generated,
			Complete,
		}

		private enum FadeState : uint
		{
			Visible = 0u,
			Fading,
		}

		private const string c_labelSubscriberColor = $"00000000";
		private const string c_labelFontSize = $"[font_size=22]";
		private const string c_labelNameFont = $"[font=res://Overlay/Fonts/Roboto-Black.ttf]";
		private const string c_labelMessageFont = $"[font=res://Overlay/Fonts/Roboto-Bold.ttf]";
		private const string c_labelMessageColor = $"[color=#F2F2F2FF]";

        private const string c_twitchBadgeDirectory = "user://Badges";
        private const string c_twitchEmoteDirectoryAnimated = "user://Emotes/Animated";
        private const string c_twitchEmoteDirectoryStatic = "user://Emotes/Static";
        private const string c_twitchEmoteUrlPrefix = "https://static-cdn.jtvnw.net/emoticons/v2";
        private const string c_twitchEmoteUrlSuffix = "default/light/1.0";

		private const float c_emoteFramesPerSecondInMilliseconds = 0.04167f;
        private const uint c_labelWidth = 678u;
		private const int c_twitchEmoteWidth = 16;
		private const int c_twitchEmoteHeight = 16;

		private readonly Dictionary<FadeState, float> c_fadeDelays = new()
		{
			{ FadeState.Visible, 32f },
			{ FadeState.Fading,  2f },
		};

        private readonly string c_rainbowColorTag = PastelInterpolator.GetRainbowColorTag();

        private readonly HashSet<string> m_animatedEmotes = new();
		private readonly Dictionary<string, int> m_animatedEmoteCurrentFrameCounts = new();
        private readonly Dictionary<string, int> m_animatedEmoteMaxFrameCounts = new();

        private PastelInterpolator m_pastelInterpolator = null;
		private RichTextLabel m_richTextLabel = new();
		private GeneratedState m_generatedState = GeneratedState.Generating;
		private FadeState m_fadeState = FadeState.Visible;
		private float m_fadeElapsed = 0f;

		private bool m_hasAnimatedEmotes = false;
        private bool m_isSubscriber = false;
		private float m_elapsedFrameTime = 0f;
        private string m_color = string.Empty;
		private string m_text = string.Empty;
        private uint m_emotesToLoad = 0u;
        #endregion

        #region INTERNAL_GENERATORS
        private void GeneratePngFromStaticEmote(
			byte[] body,
			string emoteName,
			string emoteDirectory
		)
		{
			ApplicationManager.CreateStaticEmoteDirectory(
                emoteName: emoteName
            );

			var image = Godot.Image.Create(
                width: c_twitchEmoteWidth,
                height: c_twitchEmoteHeight,
                useMipmaps: false,
                format: Godot.Image.Format.Rgba8
            );
            _ = image.LoadPngFromBuffer(
                buffer: body
            );
            var imageTexture = ImageTexture.CreateFromImage(
                image: image
            );

			var emotePath = $"{emoteDirectory}\\static_0.res";
            _ = ResourceSaver.Save(
                resource: imageTexture,
                path: emotePath
            );

            m_text = m_text.Replace(
                oldValue: emoteName,
                newValue: $"[img]{emotePath}[/img]"
            );

            m_emotesToLoad--;
            if (m_emotesToLoad is 0u)
            {
                GenerateRichTextLabel();
            }
		}

        private void GeneratePngsFromAnimatedEmote(
			byte[] body,
			string emoteName,
			string emoteDirectory
		)
        {
			ApplicationManager.CreateAnimatedEmoteDirectory(
                emoteName: emoteName
            );

            using var gifMemoryStream = new MemoryStream(
                buffer: body
            );
            using var gifImage = System.Drawing.Image.FromStream(
                stream: gifMemoryStream
            );
            var frameDimension = new FrameDimension(
				guid: gifImage.FrameDimensionsList[0]
            );
			var frameCount = gifImage.GetFrameCount(
				dimension: frameDimension
			);
			var totalFrames = frameCount - 1;

			for (var i = 0; i < frameCount; i++)
			{
				_ = gifImage.SelectActiveFrame(
					dimension: frameDimension, 
					frameIndex: i
				);

				using var frame = new System.Drawing.Bitmap(
					width: gifImage.Width, 
					height: gifImage.Height
				);
                using (
					var graphics = Graphics.FromImage(
						image: frame
					)
				)
                {
                    graphics.DrawImage(
						image: gifImage, 
						point: Point.Empty
					);
                }

                using var frameStream = new MemoryStream();
				frame.Save(
					stream: frameStream,
					format: ImageFormat.Png
				);

                var imageBytes = frameStream.ToArray();
                var image = Godot.Image.Create(
                    width: c_twitchEmoteWidth,
                    height: c_twitchEmoteHeight,
                    useMipmaps: false,
                    format: Godot.Image.Format.Rgba8
                );
                _ = image.LoadPngFromBuffer(
                    buffer: imageBytes
                );
                var imageTexture = ImageTexture.CreateFromImage(
                    image: image
                );
				var emoteFile = $"{emoteDirectory}\\animated_{i}.res";
                _ = ResourceSaver.Save(
                    resource: imageTexture,
                    path: emoteFile
                );
            }

			var emotePath = $"{emoteDirectory}/animated_0.res";
            m_text = m_text.Replace(
				oldValue: emoteName,
				newValue: $"[img]{emotePath}[/img]"
			);

			m_animatedEmotes.Add(
                item: emoteName
            );
            m_animatedEmoteCurrentFrameCounts.Add(
				key: emoteName,
				value: 0
			);
            m_animatedEmoteMaxFrameCounts.Add(
                key: emoteName,
                value: totalFrames
            );
			m_hasAnimatedEmotes = true;

            m_emotesToLoad--;
            if (m_emotesToLoad is 0u)
            {
                GenerateRichTextLabel();
            }
        }

        private void GenerateRichTextLabel()
		{
			m_richTextLabel.SetSize(
				size: new(
					x: c_labelWidth,
					y: 0f
				)
			);
			m_richTextLabel.BbcodeEnabled = true;
			m_richTextLabel.FitContent = true;
			m_richTextLabel.Text = m_text;
			m_richTextLabel.Visible = false;

			AddChild(
				node: m_richTextLabel
			);

			m_generatedState = GeneratedState.Generated;
		}
        #endregion

        #region INTERNAL_HANDLERS
        private void HandleTextAnimation(
			float delta
		)
		{
            if (m_hasAnimatedEmotes)
            {
                m_elapsedFrameTime += delta;
                if (m_elapsedFrameTime >= c_emoteFramesPerSecondInMilliseconds)
                {
                    foreach (var animatedEmote in m_animatedEmotes)
                    {
                        var previousFrame = m_animatedEmoteCurrentFrameCounts[animatedEmote];
                        var currentFrame = previousFrame + 1;
                        if (currentFrame > m_animatedEmoteMaxFrameCounts[animatedEmote])
                        {
                            currentFrame = 0;
                        }

                        m_text = m_text.Replace(
                            oldValue: $"{animatedEmote}/animated_{previousFrame}.res",
                            newValue: $"{animatedEmote}/animated_{currentFrame}.res"
                        );

                        m_animatedEmoteCurrentFrameCounts[animatedEmote] = currentFrame;
                    }
                    m_elapsedFrameTime = 0f;
                }
            }

            if (m_isSubscriber)
            {
                var color = m_pastelInterpolator.GetColorAsHex(
                    rainbowColorIndexType: RainbowColorIndexType.Color0    
                );
                m_richTextLabel.Text = m_text.Replace(
                    oldValue: c_labelSubscriberColor,
                    newValue: color
                );
            }
			else if (m_hasAnimatedEmotes)
			{
				m_richTextLabel.Text = m_text;
			}
        }

		private void HandleTextFade(
			float delta
		)
		{
			m_fadeElapsed += delta;
			switch (m_fadeState)
			{
				case FadeState.Visible:
					if (m_fadeElapsed >= c_fadeDelays[FadeState.Visible])
					{
						m_fadeState = FadeState.Fading;
						m_fadeElapsed = 0f;
					}
					break;
				case FadeState.Fading:
                    // todo: transparency
                    var height = m_richTextLabel.GetContentHeight();
					if (m_fadeElapsed >= c_fadeDelays[FadeState.Fading])
					{
						Destroyed?.Invoke();
						QueueFree();
					}
					break;

				default:
					// something went wrong, shouldn't hit
					break;
			}
		}
        #endregion

        #region INTERNAL_INSERTIONS
        private void InsertBadges(
            string badges
        )
        {
            var badgesList = badges.Split(
                separator: ','
            ).Reverse().ToList();
            foreach (var badge in badgesList)
            {
                var badgeData = badge.Split(
                    separator: '/'
                );
                var badgeSet = badgeData[0];
                var badgeVersion = badgeData[1];

                var badgePath = $"{c_twitchBadgeDirectory}\\{badgeSet}\\{badgeVersion}.res";
                m_text = m_text.Insert(
                    startIndex: 0,
                    value: $"[img]{badgePath}[/img]  "
                );
            }
        }

        private void InsertEmotes(
            HttpManager httpManager,
            string message,
            string emotes
        )
        {
            // split each emote from twitch apis
            var emoteValues = emotes.Split(
                separator: '/'
            );
            foreach (var emoteValue in emoteValues)
            {
                // split emote link & name ranges
                var emoteData = emoteValue.Split(
                    separator: ':'
                );

                var emoteLink = emoteData[0u];
                // retrieve emote name within message string
                var emoteRanges = emoteData[1u].Split(
                    separator: ','
                );
                var emoteIndices = emoteRanges[0u].Split(
                    separator: '-'
                );
                var startIndex = emoteIndices[0u].ToInt();
                var endIndex = emoteIndices[1u].ToInt();
                var emoteName = message.Substring(
                    startIndex: startIndex,
                    length: endIndex - startIndex + 1
                );

                var emotePathStatic = ApplicationManager.GetStaticEmoteDirectory(
                    emoteName: emoteName
                );
                if (
                    Directory.Exists(
                        path: emotePathStatic
                    ) is true
                )
                {
                    var filePath = $"{c_twitchEmoteDirectoryStatic}/{emoteName}/static_0.res";
                    m_text = m_text.Replace(
                        oldValue: emoteName,
                        newValue: $"[img]{filePath}[/img]"
                    );
                    continue;
                }

                var emotePathAnimated = ApplicationManager.GetAnimatedEmoteDirectory(
                    emoteName: emoteName
                );
                if (
                    Directory.Exists(
                        path: emotePathAnimated
                    ) is true
                )
                {
                    var filePath = $"{c_twitchEmoteDirectoryAnimated}/{emoteName}/animated_0.res";
                    m_text = m_text.Replace(
                        oldValue: emoteName,
                        newValue: $"[img]{filePath}[/img]"
                    );

                    m_animatedEmotes.Add(
                        item: emoteName
                    );
                    m_animatedEmoteCurrentFrameCounts.Add(
                        key: emoteName,
                        value: 0
                    );

                    var files = Directory.GetFiles(
                        path: emotePathAnimated
                    );
                    var frameCount = files.Length - 1;
                    m_animatedEmoteMaxFrameCounts.Add(
                        key: emoteName,
                        value: frameCount
                    );

                    m_hasAnimatedEmotes = true;
                    continue;
                }

                m_emotesToLoad++;
                var uri = new Uri(
                    $"{c_twitchEmoteUrlPrefix}/{emoteLink}/{c_twitchEmoteUrlSuffix}"
                );
                httpManager.SendHttpRequest(
                    url: uri.OriginalString,
                    headers: new List<string>(),
                    method: Method.Get,
                    json: string.Empty,
                    requestCompletedHandler:
                    (
                        long result,
                        long responseCode,
                        string[] headers,
                        byte[] body
                    ) =>
                    {
                        // failed web request
                        if (responseCode >= 300u)
                        {
                            QueueFree();
                            return;
                        }

                        var contentTypeHeader = headers[0];
                        if (
                            contentTypeHeader.Contains(
                                value: "png"
                            )
                        )
                        {
                            GeneratePngFromStaticEmote(
                                body: body,
                                emoteName: emoteName,
                                emoteDirectory: emotePathStatic
                            );
                        }
                        else
                        {
                            GeneratePngsFromAnimatedEmote(
                                body: body,
                                emoteName: emoteName,
                                emoteDirectory: emotePathAnimated
                            );
                        }
                    }
                );
            }
        }

        private void InsertImages(
			HttpManager httpManager,
            string message,
            string emotes,
            string badges
        )
		{
            var hasBadges = string.IsNullOrEmpty(
				value: badges
			) is false;
			if (hasBadges)
			{
                InsertBadges(
				    badges: badges
				);
            }

			var hasEmotes = string.IsNullOrEmpty(
				value: emotes
			) is false;
			if (hasEmotes is true)
			{
                InsertEmotes(
                    httpManager: httpManager,
                    message: message,
                    emotes: emotes
                );
            }

            if (m_emotesToLoad is 0u)
            {
                GenerateRichTextLabel();
            }
        }
        #endregion
    }
}